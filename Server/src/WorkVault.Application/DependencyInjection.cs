using Microsoft.Extensions.DependencyInjection;
using WorkVault.Application.Modules.Identity.Commands.RegisterCompany;

namespace WorkVault.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(
                typeof(RegisterCompanyCommand).Assembly
            ));

        return services;
    }
}