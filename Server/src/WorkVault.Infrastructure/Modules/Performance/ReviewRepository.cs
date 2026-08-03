using Microsoft.EntityFrameworkCore;
using WorkVault.Domain.Modules.Performance;
using WorkVault.Domain.Modules.Performance.Interfaces;
using WorkVault.Infrastructure.Persistence;

namespace WorkVault.Infrastructure.Modules.Performance;

/// <summary>
/// EF Core data access for <see cref="Review"/>, tenant-scoped by the global query filter.
/// Writes stage changes only; the handler commits via <c>IUnitOfWork</c>.
/// </summary>
public class ReviewRepository(AppDbContext context) : IReviewRepository
{
    /// <summary>Stages a new review for insertion.</summary>
    public async Task AddAsync(Review review, CancellationToken cancellationToken)
    {
        await context.Reviews.AddAsync(review, cancellationToken);
    }

    /// <summary>Loads a single review by id.</summary>
    public async Task<Review?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Reviews
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    /// <summary>All of an employee's reviews, newest first (CreatedAt breaks same-date ties).</summary>
    public async Task<IReadOnlyList<Review>> GetByEmployeeAsync(
        Guid employeeId, CancellationToken cancellationToken)
    {
        return await context.Reviews
            .Where(r => r.EmployeeId == employeeId)
            .OrderByDescending(r => r.ReviewDate)
            .ThenByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>The employee's most recent review that carries a salary — their current salary.</summary>
    public async Task<Review?> GetLatestWithSalaryAsync(
        Guid employeeId, CancellationToken cancellationToken)
    {
        return await context.Reviews
            .Where(r => r.EmployeeId == employeeId && r.NewSalary != null)
            .OrderByDescending(r => r.ReviewDate)
            .ThenByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>True if the employee already has at least one review.</summary>
    public async Task<bool> HasAnyAsync(Guid employeeId, CancellationToken cancellationToken)
    {
        return await context.Reviews
            .AnyAsync(r => r.EmployeeId == employeeId, cancellationToken);
    }
}
