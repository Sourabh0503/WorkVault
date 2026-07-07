using Microsoft.EntityFrameworkCore;
using WorkVault.Domain.Modules.Employees;
using WorkVault.Domain.Modules.Employees.Interfaces;
using WorkVault.Infrastructure.Persistence;

namespace WorkVault.Infrastructure.Modules.Employees;

/// <summary>
/// EF Core data access for <see cref="Designation"/>, tenant-scoped by the global query
/// filter. Writes stage changes only; the handler commits via <c>IUnitOfWork</c>.
/// </summary>
public class DesignationRepository(AppDbContext context) : IDesignationRepository
{
    /// <summary>Stages a new designation for insertion.</summary>
    public async Task AddAsync(Designation designation, CancellationToken cancellationToken)
    {
        await context.Designations.AddAsync(designation, cancellationToken);
    }

    /// <summary>Loads a single designation by id (department eager-loaded).</summary>
    public async Task<Designation?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Designations
            .Include(d => d.Department)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    /// <summary>Lists designations (optionally for one department), ordered by department, level, then title.</summary>
    public async Task<IReadOnlyList<Designation>> GetAllAsync(Guid? departmentId, CancellationToken cancellationToken)
    {
        var query = context.Designations
            .Include(d => d.Department)
            .AsQueryable();

        if (departmentId.HasValue)
            query = query.Where(d => d.DepartmentId == departmentId.Value);

        return await query
            .OrderBy(d => d.Department!.Name)
            .ThenBy(d => d.Level)
            .ThenBy(d => d.Title)
            .ToListAsync(cancellationToken);
    }

    /// <summary>Case-insensitive check for a duplicate title within a department; optionally excludes one id (for updates).</summary>
    public async Task<bool> ExistsByTitleAsync(string title, Guid departmentId, Guid? excludeId, CancellationToken cancellationToken)
    {
        var query = context.Designations
            .Where(d => d.Title.ToLower() == title.ToLower() && d.DepartmentId == departmentId);

        if (excludeId.HasValue)
            query = query.Where(d => d.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    /// <summary>True if any employee holds this designation (blocks deletion).</summary>
    public async Task<bool> HasEmployeesAsync(Guid designationId, CancellationToken cancellationToken)
    {
        return await context.Employees.AnyAsync(e => e.DesignationId == designationId, cancellationToken);
    }
}
