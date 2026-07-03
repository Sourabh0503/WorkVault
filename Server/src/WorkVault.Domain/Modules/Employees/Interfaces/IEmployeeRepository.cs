using WorkVault.Domain.Modules.Employees.Enums;
using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Domain.Modules.Employees.Interfaces;

/// <summary>
/// Repository interface for <see cref="Employee"/> entity operations.
/// </summary>
public interface IEmployeeRepository : IRepository<Employee>
{
    /// <summary>
    /// Retrieves a paginated list of employees with optional filters.
    /// </summary>
    /// <param name="pageNumber">1-based page number.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="departmentId">Filter by department.</param>
    /// <param name="status">Filter by employee status.</param>
    /// <param name="managerId">Filter by manager (direct reports).</param>
    /// <param name="search">Search term for name, email, or employee code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tuple of employees list and total count.</returns>
    
    Task<(IReadOnlyList<Employee> Employees, int TotalCount)> GetAllAsync(
        int pageNumber,
        int pageSize,
        Guid? departmentId,
        EmployeeStatus? status,
        Guid? managerId,
        string? search,
        CancellationToken cancellationToken);

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