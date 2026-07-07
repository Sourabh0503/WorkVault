using Microsoft.EntityFrameworkCore;
using WorkVault.Domain.Modules.Employees;
using WorkVault.Domain.Modules.Employees.Interfaces;
using WorkVault.Infrastructure.Persistence;

namespace WorkVault.Infrastructure.Modules.Employees;

/// <summary>
/// EF Core data access for <see cref="Department"/>, tenant-scoped by the global query
/// filter. Writes stage changes only; the handler commits via <c>IUnitOfWork</c>.
/// </summary>
public class DepartmentRepository(AppDbContext context) : IDepartmentRepository
{
    /// <summary>Stages a new department for insertion.</summary>
    public async Task AddAsync(Department department, CancellationToken cancellationToken)
    {
        await context.Departments.AddAsync(department, cancellationToken);
    }

    /// <summary>Loads a single department by id.</summary>
    public async Task<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Departments
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    /// <summary>Lists all departments with parent, head (+ user), and members eager-loaded, ordered by name.</summary>
    public async Task<IReadOnlyList<Department>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await context.Departments
            .Include(d => d.ParentDepartment)
            .Include(d => d.HeadEmployee)
                .ThenInclude(e => e!.User)
            .Include(d => d.Employees)
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>Case-insensitive check for an existing department with the given name.</summary>
    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken)
    {
        var query = context.Departments.Where(d => d.Name.ToLower() == name.ToLower());

        return await query.AnyAsync(cancellationToken);
    }

    /// <summary>Like <see cref="ExistsByNameAsync"/> but ignores one id — for uniqueness checks on update.</summary>
    public async Task<bool> ExistsByNameExcludingAsync(
        string name, Guid excludeId, CancellationToken cancellationToken)
    {
        return await context.Departments
            .AnyAsync(d => d.Id != excludeId && d.Name.ToLower() == name.ToLower(), cancellationToken);
    }

    /// <summary>True if any employee belongs to the department (blocks deletion).</summary>
    public async Task<bool> HasEmployeesAsync(Guid departmentId, CancellationToken cancellationToken)
    {
        return await context.Employees.AnyAsync(e => e.DepartmentId == departmentId, cancellationToken);
    }

    /// <summary>Total number of departments in the current company (dashboard use).</summary>
    public async Task<int> CountAsync(CancellationToken cancellationToken)
    {
        return await context.Departments.CountAsync(cancellationToken);
    }

    /// <summary>True if the department has child departments (blocks deletion).</summary>
    public async Task<bool> HasSubDepartmentsAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Departments
            .AnyAsync(d => d.ParentDepartmentId == id, cancellationToken);
    }
}
