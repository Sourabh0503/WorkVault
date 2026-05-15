namespace WorkVault.SharedKernel.Interfaces;

/// <summary>
/// Coordinates saving changes across multiple repositories in a single
/// transaction. Inject this into handlers that modify data in multiple
/// tables — call SaveChangesAsync once at the end.
/// 
/// Why not call SaveChanges inside each repository?
/// - Multi-step operations would commit halfway through on failure
/// - Lost ability to roll back across repositories
/// - Performance: one DB round-trip instead of many
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}