// =============================================================================
// Composition root for the WorkVault API.
//
// Wires up, in order: MVC controllers + Swagger, rate limiting (auth /
// forgot-password / api / authenticated policies), the Application and
// Infrastructure layers (MediatR, EF Core, repositories), JWT bearer
// authentication, and CORS. Then builds the request pipeline — exception
// handling → CORS → rate limiter → authN → authZ → Swagger (dev) → controllers.
// Pipeline order is deliberate; see the inline sections below.
// =============================================================================

using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using WorkVault.API.Middleware;
using WorkVault.API.Services;
using WorkVault.Application;
using WorkVault.Application.Common.Interfaces;
using WorkVault.Infrastructure;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

// ============================================================
// Rate Limiting Configuration
// ============================================================
builder.Services.AddRateLimiter(options =>
{
    // Return 429 Too Many Requests with Retry-After header
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "application/json";

        var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retry)
            ? retry.TotalSeconds
            : 60;

        context.HttpContext.Response.Headers.RetryAfter = retryAfter.ToString();

        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            type = "RateLimitExceeded",
            title = "Too many requests. Please try again later.",
            status = 429,
            retryAfterSeconds = retryAfter
        }, cancellationToken);
    };

    // Policy: "auth" - Strict limits for login/register (brute force protection)
    // 5 requests per minute per IP address
    options.AddPolicy("auth", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0 // No queueing - reject immediately
            }));
    
    // Policy: "forgot-password" - Strict limits for forgot-password (sends email)
    // 2 requests per minute per IP address
    options.AddPolicy("forgot-password", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromHours(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0 // No queueing - reject immediately
            }));

    // Policy: "api" - General API rate limit
    // 100 requests per minute per IP address
    options.AddPolicy("api", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 2
            }));

    // Policy: "authenticated" - Higher limits for authenticated users
    // 200 requests per minute, partitioned by user ID
    options.AddPolicy("authenticated", context =>
    {
        var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        // Fall back to IP if not authenticated
        var partitionKey = userId ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: partitionKey,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 200,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 5
            });
    });
});

// ============================================================
// Services DI
// ============================================================
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("JwtSettings");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!))
        };
    });

// ============================================================
// CORS Configuration
// ============================================================
var allowedOrigins = builder.Configuration
    .GetSection("CorsSettings:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            // Production: Only allow configured origins
            policy.WithOrigins(allowedOrigins)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials(); // Required for cookies/auth headers
        }
        else
        {
            // Fallback for misconfiguration - deny all cross-origin requests
            // This forces proper configuration in production
            policy.SetIsOriginAllowed(_ => false);
        }
    });

    // Development-only policy (can be used explicitly if needed)
    if (builder.Environment.IsDevelopment())
    {
        options.AddPolicy("Development", policy =>
        {
            policy.SetIsOriginAllowed(_ => true) // Allow any origin in dev
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
    }
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();   // trust Render's proxy
    options.KnownProxies.Clear();
});

var app = builder.Build();

// ============================================================
// Middleware Pipeline (order matters!)
// ============================================================
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors("DefaultPolicy");
app.UseRateLimiter();  // Rate limiting before auth
app.UseAuthentication();
app.UseAuthorization();

// Swagger - only in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.MapControllers();
app.Run();

// Exposes the implicit Program class to the integration test project
public partial class Program
{
    
}