using Microsoft.EntityFrameworkCore;
using WorkVault.Domain.Modules.Employees;
using WorkVault.Domain.Modules.Employees.Enums;
using WorkVault.Domain.Modules.Employees.Interfaces;
using WorkVault.Infrastructure.Persistence;
using WorkVault.SharedKernel.Constants;

namespace WorkVault.Infrastructure.Modules.Employees;

/// <summary>
/// EF Core data access for <see cref="Employee"/>. All queries are automatically
/// scoped to the current tenant by the global CompanyId query filter (except where
/// <c>IgnoreQueryFilters</c> is used explicitly). Writes never call SaveChanges —
/// the handler commits via <c>IUnitOfWork</c>.
/// </summary>
public class EmployeeRepository(AppDbContext context) : IEmployeeRepository
{
    /// <summary>Stages a new employee for insertion (committed later by the Unit of Work).</summary>
    public async Task AddAsync(Employee employee, CancellationToken cancellationToken)
    {
        await context.Employees.AddAsync(employee, cancellationToken);
        // No SaveChanges — handler controls via IUnitOfWork
    }

    /// <summary>Loads a single employee by id with user (+ role), department, designation, and manager eager-loaded.</summary>
    public async Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Employees
            .Include(e => e.User)
                .ThenInclude(u => u!.Role)
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .Include(e => e.Manager)
            .ThenInclude(m => m!.User)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    /// <summary>
    /// Returns a page of employees plus the total match count. Optionally filters by
    /// department, status, or manager, and by a case-insensitive search across employee
    /// code, email, first name, and last name. Ordered by name.
    /// </summary>
    public async Task<(IReadOnlyList<Employee> Employees, int TotalCount)> GetAllAsync(
        int pageNumber,
        int pageSize,
        Guid? departmentId,
        EmployeeStatus? status,
        Guid? managerId,
        string? search,
        CancellationToken cancellationToken)
    {
        var query = context.Employees
            .Include(e => e.User)
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .AsQueryable();

        // Apply filters
        if (departmentId.HasValue)
            query = query.Where(e => e.DepartmentId == departmentId.Value);

        if (status.HasValue)
            query = query.Where(e => e.Status == status.Value);

        if (managerId.HasValue)
            query = query.Where(e => e.ManagerId == managerId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(e =>
                EF.Functions.ILike(e.EmployeeCode, $"%{search}%") ||
                (e.User != null && (
                    EF.Functions.ILike(e.User.Email, $"%{search}%") ||
                    EF.Functions.ILike(e.User.FirstName, $"%{search}%") ||
                    EF.Functions.ILike(e.User.LastName, $"%{search}%")
                )));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination and ordering
        var employees = await query
            .OrderBy(e => e.User!.FirstName)
            .ThenBy(e => e.User!.LastName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (employees, totalCount);
    }

    /// <summary>
    /// Counts a company's employees created in a given year, ignoring the tenant query
    /// filter (companyId is passed explicitly) — used to generate sequential employee codes.
    /// </summary>
    public async Task<int> GetCountForYearAsync(Guid companyId, int year, CancellationToken cancellationToken)
    {
        return await context.Employees
            .IgnoreQueryFilters()
            .CountAsync(e => e.CompanyId == companyId 
                             && e.CreatedAt.Year == year 
                             && !e.IsDeleted, 
                cancellationToken);
    }

    /// <summary>Finds the employee record linked to a given user id (department, designation, and manager eager-loaded).</summary>
    public async Task<Employee?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await context.Employees
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .Include(e => e.Manager)
                .ThenInclude(m => m!.User)
            .FirstOrDefaultAsync(e => e.UserId == userId, cancellationToken);
    }

    /// <summary>Returns up to 200 employees in a department (user + designation loaded), ordered by name.</summary>
    public async Task<IReadOnlyList<Employee>> GetByDepartmentAsync(
        Guid departmentId, CancellationToken cancellationToken)
    {
        return await context.Employees
            .Include(e => e.User)
            .Include(e => e.Designation)
            .Where(e => e.DepartmentId == departmentId)
            .OrderBy(e => e.User!.FirstName)
            .ThenBy(e => e.User!.LastName)
            .Take(200)
            .ToListAsync(cancellationToken);
    }
    
    /// <summary>Returns employee counts grouped by status for the current company (one query, dashboard use).</summary>
    public async Task<Dictionary<EmployeeStatus, int>> GetStatusCountsAsync(
        CancellationToken cancellationToken)
    {
        // One grouped query — tenant filter auto-scopes to current company
        var counts = await context.Employees
            .GroupBy(e => e.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return counts.ToDictionary(x => x.Status, x => x.Count);
    }
    
    /// <summary>True if any employee is assigned to the given department (blocks deletion).</summary>
    public async Task<bool> HasMembersInDepartmentAsync(Guid departmentId, CancellationToken cancellationToken)
    {
        return await context.Employees
            .AnyAsync(e => e.DepartmentId == departmentId, cancellationToken);
    }

    /// <summary>True if any employee reports to the given manager (blocks deletion until reassigned).</summary>
    public async Task<bool> HasDirectReportsAsync(Guid managerId, CancellationToken cancellationToken)
    {
        return await context.Employees
            .AnyAsync(e => e.ManagerId == managerId, cancellationToken);
    }

    /// <summary>Returns Active, Manager-role employees in a department (user loaded), ordered by name.</summary>
    public async Task<IReadOnlyList<Employee>> GetManagersByDepartmentAsync(
        Guid departmentId, CancellationToken cancellationToken)
    {
        return await context.Employees
            .Include(e => e.User)
            .Where(e => e.DepartmentId == departmentId
                        && e.Status == EmployeeStatus.Active
                        && e.User!.RoleId == SystemRoles.Manager)
            .OrderBy(e => e.User!.FirstName)
            .ThenBy(e => e.User!.LastName)
            .ToListAsync(cancellationToken);
    }
}