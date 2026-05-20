namespace WorkVault.SharedKernel.Interfaces;

/// <summary>
/// Generic repository interface defining basic CRUD operations for domain entities.
/// Implementations are in the Infrastructure layer.
/// </summary>
/// <typeparam name="T">Entity type that extends <see cref="BaseEntity"/>.</typeparam>
/// <remarks>
/// Important: Repository methods do NOT call SaveChanges. Use <see cref="IUnitOfWork"/>
/// to persist changes after all operations are complete.
///
/// Example usage in a handler:
/// <code>
/// await userRepository.AddAsync(user, ct);
/// await employeeRepository.AddAsync(employee, ct);
/// await unitOfWork.SaveChangesAsync(ct); // Single DB round-trip
/// </code>
/// </remarks>
public interface IRepository<T> where T : BaseEntity
{
    /// <summary>
    /// Marks the entity for insertion into the database.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <remarks>
    /// Does NOT persist immediately. Call <see cref="IUnitOfWork.SaveChangesAsync"/>
    /// to commit to the database.
    /// </remarks>
    Task AddAsync(T entity, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves an entity by its primary key.
    /// </summary>
    /// <param name="id">The entity's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    /// <remarks>
    /// Automatically applies tenant isolation (CompanyId filter) and soft delete filter.
    /// </remarks>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}