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
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WorkVault.API.Middleware;
using WorkVault.API.Services;
using WorkVault.Application;
using WorkVault.Application.Common.Interfaces;
using WorkVault.Infrastructure;
using WorkVault.Infrastructure.Persistence;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Production containers are immutable — don't watch config files (avoids
// inotify exhaustion on constrained hosts like Render).
builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables();

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

// Apply pending EF Core migrations on startup so a fresh production database
// (e.g. Render's managed Postgres) gets its schema without a manual step.
// Safe for single-instance deploys; disabled in integration tests, which
// create and migrate their own throwaway database explicitly.
if (app.Configuration.GetValue("Database:AutoMigrate", true))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

// ============================================================
// Middleware Pipeline (order matters!)
// ============================================================
app.UseForwardedHeaders();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors("DefaultPolicy");
// Rate limiting before auth; can be disabled via configuration
// (integration tests set RateLimiting:Enabled=false)
if (app.Configuration.GetValue("RateLimiting:Enabled", true))
{
    app.UseRateLimiter();
}
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
// Lightweight, anonymous liveness endpoint for Render's health checks.
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.MapControllers();
app.Run();

// Exposes the implicit Program class to the integration test project
public partial class Program
{
    
}