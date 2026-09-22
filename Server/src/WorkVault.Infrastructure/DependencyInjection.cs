using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkVault.Application.Common.Interfaces;
using WorkVault.Application.Common.Settings;
using WorkVault.Domain.Modules.Employees.Interfaces;
using WorkVault.Domain.Modules.Identity.Interfaces;
using WorkVault.Domain.Modules.Performance.Interfaces;
using WorkVault.Infrastructure.Auth;
using WorkVault.Infrastructure.Messaging;
using WorkVault.Infrastructure.Modules.Employees;
using WorkVault.Infrastructure.Modules.Identity;
using WorkVault.Infrastructure.Modules.Performance;
using WorkVault.Infrastructure.Persistence;
using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Infrastructure;

/// <summary>Registers the Infrastructure layer: the EF Core DbContext, JWT service, Unit of Work, and repositories.</summary>
public static class DependencyInjection
{
    /// <summary>
    /// Wires up PostgreSQL via <see cref="AppDbContext"/>, binds <c>JwtSettings</c>, and registers
    /// the JWT token service, Unit of Work, and all module repositories as scoped services.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        RegisterMessagingServices(services,configuration);
        
        // Database
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection")
            ));

        // JWT
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Repositories
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IInviteTokenRepository, InviteTokenRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IDesignationRepository, DesignationRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();

        return services;
    }

    private static void RegisterMessagingServices(IServiceCollection services , IConfiguration configuration)
    {
        //Services
        services.AddSingleton<IEmailPublisher, RabbitMqEmailPublisher>();
        
        // Email settings from config
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

        // The email sender. Brevo's HTTP API (443) is used in place of SMTP because
        // the host (Render free plan) blocks outbound SMTP ports (25/465/587).
        // AddHttpClient registers it as a typed client with a pooled HttpClient.
        // SmtpEmailSender is kept in the codebase as a fallback but is no longer wired up.
        services.AddHttpClient<IEmailSender, BrevoApiEmailSender>();

        // The background consumer that drains the queue
        services.AddHostedService<EmailConsumerService>();
    }
    
}