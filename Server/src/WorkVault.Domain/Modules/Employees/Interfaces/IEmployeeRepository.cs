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
    /// Counts employees created in a specific year for the given company.
    /// </summary>
    /// <param name="companyId">The company to count for; the global tenant query filter is bypassed and this value is applied explicitly.</param>
    /// <param name="year">The year to count employees for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The count of employees created that year.</returns>
    /// <remarks>
    /// Used to generate sequential EmployeeCode: EMP-{year}-{count+1:D4}
    /// Note: Has a potential race condition under concurrent requests.
    /// </remarks>
    Task<int> GetCountForYearAsync(Guid companyId, int year, CancellationToken cancellationToken);

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
    
    /// <summary>
    /// Returns the employees assigned to a department (capped for list display), ordered by name.
    /// </summary>
    /// <param name="departmentId">The department to load members for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The department's employees.</returns>
    Task<IReadOnlyList<Employee>> GetByDepartmentAsync(Guid departmentId, CancellationToken cancellationToken);

    /// <summary>
    /// Returns employee counts grouped by <see cref="EmployeeStatus"/> for the current company.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A map of status to headcount (used by the dashboard).</returns>
    Task<Dictionary<EmployeeStatus, int>> GetStatusCountsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Checks whether any employee is assigned to the given department.
    /// Used to block deleting a department that still has members.
    /// </summary>
    /// <param name="departmentId">The department to check for members.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if at least one employee belongs to the department.</returns>
    Task<bool> HasMembersInDepartmentAsync(Guid departmentId, CancellationToken cancellationToken);

    /// <summary>
    /// Checks whether any employee reports to the given manager.
    /// Used to block deleting a manager who still has direct reports.
    /// </summary>
    /// <param name="managerId">The manager (employee) to check for reports.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if at least one employee has this manager.</returns>
    Task<bool> HasDirectReportsAsync(Guid managerId, CancellationToken cancellationToken);

    /// <summary>
    /// Returns Active employees with the Manager role in the given department.
    /// Used to populate the reporting-manager dropdown on the employee form.
    /// </summary>
    /// <param name="departmentId">The department whose managers to load.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The department's Active manager-role employees (user eager-loaded), ordered by name.</returns>
    Task<IReadOnlyList<Employee>> GetManagersByDepartmentAsync(Guid departmentId, CancellationToken cancellationToken);

    /// <summary>
    /// Counts employees by their join month within a year (for the dashboard headcount trend).
    /// </summary>
    /// <param name="year">The calendar year to bucket by month.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Map of month number (1–12) to the count of employees who joined that month.</returns>
    Task<Dictionary<int, int>> GetMonthlyJoinCountsAsync(int year, CancellationToken cancellationToken);
}