using Microsoft.EntityFrameworkCore;
using WorkVault.Domain.Modules.Employees;
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
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<int> GetCountForYearAsync(int year, CancellationToken cancellationToken)
    {
        return await context.Employees
            .CountAsync(e => e.CreatedAt.Year == year, cancellationToken);
    }
}