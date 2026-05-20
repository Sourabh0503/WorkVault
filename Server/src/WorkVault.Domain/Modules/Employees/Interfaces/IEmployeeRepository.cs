using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Domain.Modules.Employees.Interfaces;

public interface IEmployeeRepository : IRepository<Employee>
{
    /// <summary>
    /// Counts employees created this year — used to generate the next
    /// EmployeeCode (EMP-2025-{count+1:0000}).
    /// </summary>
    Task<int> GetCountForYearAsync(int year, CancellationToken cancellationToken);
    
    Task<Employee?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}