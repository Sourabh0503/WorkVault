namespace WorkVault.SharedKernel.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task AddAsync(T entity, CancellationToken cancellationToken);
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}