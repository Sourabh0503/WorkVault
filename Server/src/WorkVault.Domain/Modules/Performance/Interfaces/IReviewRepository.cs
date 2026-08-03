using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Domain.Modules.Performance.Interfaces;

/// <summary>
/// Repository interface for <see cref="Review"/> entity operations.
/// </summary>
/// <remarks>
/// Inherits <see cref="IRepository{Review}.AddAsync"/> and
/// <see cref="IRepository{Review}.GetByIdAsync"/> (both tenant-filtered). All reads here
/// are already scoped to the current company by the global query filter.
/// </remarks>
public interface IReviewRepository : IRepository<Review>
{
    /// <summary>
    /// Returns all of an employee's reviews, newest first — the source for the history
    /// list and both charts.
    /// </summary>
    /// <param name="employeeId">The employee whose reviews to load.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The employee's reviews ordered by review date descending.</returns>
    Task<IReadOnlyList<Review>> GetByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken);

    /// <summary>
    /// Returns the employee's most recent review that has a <see cref="Review.NewSalary"/> —
    /// i.e. their <em>current</em> (derived) salary. Null if they have no salaried review.
    /// </summary>
    /// <param name="employeeId">The employee to resolve current salary for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The latest salaried review, or null.</returns>
    Task<Review?> GetLatestWithSalaryAsync(Guid employeeId, CancellationToken cancellationToken);

    /// <summary>
    /// True if the employee already has at least one review. Used to enforce the
    /// "salary is required on the FIRST review" rule when creating.
    /// </summary>
    /// <param name="employeeId">The employee to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if any review exists for the employee.</returns>
    Task<bool> HasAnyAsync(Guid employeeId, CancellationToken cancellationToken);
}
