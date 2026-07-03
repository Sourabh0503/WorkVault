using Microsoft.EntityFrameworkCore;
using WorkVault.Domain.Modules.Employees;
using WorkVault.Domain.Modules.Employees.Enums;
using WorkVault.Domain.Modules.Employees.Interfaces;
using WorkVault.Infrastructure.Persistence;

namespace WorkVault.Infrastructure.Modules.Employees;

public class EmployeeRepository(AppDbContext context) : IEmployeeRepository
{
    public async Task AddAsync(Employee employee, CancellationToken cancellationToken)
    {
        await context.Employees.AddAsync(employee, cancellationToken);
        // No SaveChanges — handler controls via IUnitOfWork
    }

    public async Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Employees
            .Include(e => e.User)
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .Include(e => e.Manager)
                .ThenInclude(m => m!.User)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

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
            var searchLower = search.ToLower();
            query = query.Where(e =>
                e.EmployeeCode.ToLower().Contains(searchLower) ||
                (e.User != null && (
                    e.User.Email.ToLower().Contains(searchLower) ||
                    e.User.FirstName.ToLower().Contains(searchLower) ||
                    e.User.LastName.ToLower().Contains(searchLower)
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

    public async Task<int> GetCountForYearAsync(int year, CancellationToken cancellationToken)
    {
        return await context.Employees
            .CountAsync(e => e.CreatedAt.Year == year, cancellationToken);
    }

    public async Task<Employee?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await context.Employees
            .FirstOrDefaultAsync(e => e.UserId == userId, cancellationToken);
    }
}