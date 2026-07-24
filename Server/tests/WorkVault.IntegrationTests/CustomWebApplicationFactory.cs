using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
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
        // Disable rate limiting so tests can make rapid successive requests
        builder.UseSetting("RateLimiting:Enabled", "false");

        // The test fixture migrates its own container in InitializeAsync;
        // don't let the app also auto-migrate on startup.
        builder.UseSetting("Database:AutoMigrate", "false");

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
