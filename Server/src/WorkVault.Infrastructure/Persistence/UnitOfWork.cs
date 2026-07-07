using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Infrastructure.Persistence;

/// <summary>
/// Wraps <see cref="AppDbContext"/> as the single commit boundary. Handlers mutate
/// tracked entities, then call <see cref="SaveChangesAsync"/> once to persist them in
/// one transaction.
/// </summary>
public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    /// <summary>Persists all tracked changes and returns the number of affected rows.</summary>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        => context.SaveChangesAsync(cancellationToken);
}