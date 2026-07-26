using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using WorkVault.Infrastructure.Persistence;

namespace WorkVault.IntegrationTests;

/// <summary>
/// WebApplicationFactory that overrides the DbContext to run on a PostgreSQL Testcontainer.
/// Spun up dynamically in Docker during integration test runs.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:16")
        .WithDatabase("workvaultdb_test")
        .WithUsername("workvault")
        .WithPassword("workvault123")
        .Build();

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        // Accessing Services builds the host, then apply migrations against the container
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Inject test config as an in-memory source. This is appended after
        // appsettings*.json, so it reliably reaches app.Configuration and overrides
        // any local dev values — unlike UseSetting, which didn't gate the rate limiter.
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                // Disable rate limiting so tests can make rapid successive requests
                // (register/login are otherwise capped at 5/min per IP → 429 across tests).
                ["RateLimiting:Enabled"] = "false",

                // The test fixture migrates its own container in InitializeAsync;
                // don't let the app also auto-migrate on startup.
                ["Database:AutoMigrate"] = "false",

                // Hermetic JWT config — appsettings.Development.json isn't discovered on
                // the CI runner, which left SecretKey null and 500'd every request.
                ["JwtSettings:SecretKey"] = "WorkVault-Integration-Test-Secret-Key-With-At-Least-32-Characters",
                ["JwtSettings:Issuer"] = "WorkVault",
                ["JwtSettings:Audience"] = "WorkVault",
                ["JwtSettings:AccessTokenExpirationMinutes"] = "15",
                ["JwtSettings:RefreshTokenExpirationDays"] = "7",
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove the existing AppDbContext database configuration
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            // Register DbContext with the Testcontainer connection string
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
            });
        });
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _dbContainer.DisposeAsync();
    }
}
