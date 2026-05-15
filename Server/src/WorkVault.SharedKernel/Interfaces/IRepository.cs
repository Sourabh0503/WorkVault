namespace WorkVault.SharedKernel.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    /// <summary>
    /// Marks the entity for insertion. Does NOT save to the database.
    /// Caller must call SaveChangesAsync (or use IUnitOfWork) to persist.
    /// </summary>
    Task AddAsync(T entity, CancellationToken cancellationToken);
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}