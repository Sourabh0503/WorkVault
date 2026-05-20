using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Domain.Modules.Employees.Interfaces;

/// <summary>
/// Repository interface for <see cref="Employee"/> entity operations.
/// </summary>
public interface IEmployeeRepository : IRepository<Employee>
{
    /// <summary>
    /// Counts employees created in a specific year within the current tenant.
    /// </summary>
    /// <param name="year">The year to count employees for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The count of employees created that year.</returns>
    /// <remarks>
    /// Used to generate sequential EmployeeCode: EMP-{year}-{count+1:D4}
    /// Note: Has a potential race condition under concurrent requests.
    /// </remarks>
    Task<int> GetCountForYearAsync(int year, CancellationToken cancellationToken);

    /// <summary>
    /// Finds an employee by their linked User account ID.
    /// </summary>
    /// <param name="userId">The User.Id to search for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The employee if found, or null.</returns>
    /// <remarks>
    /// Used in SetPasswordHandler to activate the employee record
    /// when the user accepts their invite.
    /// </remarks>
    Task<Employee?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}