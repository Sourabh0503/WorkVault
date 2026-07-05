using Microsoft.EntityFrameworkCore;
using WorkVault.Domain.Modules.Employees;
using WorkVault.Domain.Modules.Employees.Interfaces;
using WorkVault.Infrastructure.Persistence;

namespace WorkVault.Infrastructure.Modules.Employees;

public class DepartmentRepository(AppDbContext context) : IDepartmentRepository
{
    public async Task AddAsync(Department department, CancellationToken cancellationToken)
    {
        await context.Departments.AddAsync(department, cancellationToken);
    }

    public async Task<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Departments
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

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

    public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId, CancellationToken cancellationToken)
    {
        var query = context.Departments.Where(d => d.Name.ToLower() == name.ToLower());

        if (excludeId.HasValue)
            query = query.Where(d => d.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> HasEmployeesAsync(Guid departmentId, CancellationToken cancellationToken)
    {
        return await context.Employees.AnyAsync(e => e.DepartmentId == departmentId, cancellationToken);
    }
}
