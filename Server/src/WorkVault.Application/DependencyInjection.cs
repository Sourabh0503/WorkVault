using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using WorkVault.Application.Common.Behaviors;

namespace WorkVault.Application;

/// <summary>Registers the Application layer's services (MediatR, FluentValidation, pipeline behaviors).</summary>
public static class DependencyInjection
{
    /// <summary>
    /// Scans this assembly to register all MediatR handlers and FluentValidation validators,
    /// and wires the <see cref="ValidationBehavior{TRequest,TResponse}"/> into the MediatR pipeline.
    /// </summary>
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(assembly));
        
        services.AddValidatorsFromAssembly(assembly);
        
        services.AddTransient(typeof(IPipelineBehavior<,>),typeof(ValidationBehavior<,>));
        
        return services;
    }
}