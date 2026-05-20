using FluentValidation;
using MediatR;

namespace WorkVault.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that runs FluentValidation validators before handlers.
/// </summary>
/// <remarks>
/// This behavior intercepts all MediatR requests and:
/// 1. Finds all registered validators for the request type
/// 2. Runs them in parallel for performance
/// 3. Throws <see cref="ValidationException"/> if any fail
/// 4. Otherwise, continues to the actual handler
///
/// Validators are auto-discovered and registered via:
/// <code>services.AddValidatorsFromAssembly(assembly);</code>
///
/// The ValidationException is caught by ExceptionHandlingMiddleware
/// and converted to a 400 response with grouped error messages.
/// </remarks>
/// <typeparam name="TRequest">The MediatR request type.</typeparam>
/// <typeparam name="TResponse">The MediatR response type.</typeparam>
public class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // No validators registered for this command? Skip and continue.
        if (!validators.Any())
            return await next(cancellationToken);

        var context = new ValidationContext<TRequest>(request);

        // Run all validators in parallel
        var results = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // Collect all failures from all validators
        var failures = results
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);

        // All good — let the request continue to the handler
        return await next(cancellationToken);
    }
}