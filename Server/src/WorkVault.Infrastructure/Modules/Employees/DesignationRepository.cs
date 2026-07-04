using Microsoft.EntityFrameworkCore;
using WorkVault.Domain.Modules.Employees;
using WorkVault.Domain.Modules.Employees.Interfaces;
using WorkVault.Infrastructure.Persistence;

namespace WorkVault.Infrastructure.Modules.Employees;

public class DesignationRepository(AppDbContext context) : IDesignationRepository
{
    public async Task AddAsync(Designation designation, CancellationToken cancellationToken)
    {
        await context.Designations.AddAsync(designation, cancellationToken);
    }

    public async Task<Designation?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Designations
            .Include(d => d.Department)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

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

    public async Task<bool> ExistsByTitleAsync(string title, Guid departmentId, Guid? excludeId, CancellationToken cancellationToken)
    {
        var query = context.Designations
            .Where(d => d.Title.ToLower() == title.ToLower() && d.DepartmentId == departmentId);

        if (excludeId.HasValue)
            query = query.Where(d => d.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> HasEmployeesAsync(Guid designationId, CancellationToken cancellationToken)
    {
        return await context.Employees.AnyAsync(e => e.DesignationId == designationId, cancellationToken);
    }
}
