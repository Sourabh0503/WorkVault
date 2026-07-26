using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Domain.Modules.Employees.Interfaces;

/// <summary>
/// Repository interface for <see cref="Department"/> entity operations.
/// </summary>
public interface IDepartmentRepository : IRepository<Department>
{
    /// <summary>
    /// Retrieves all departments for the current tenant.
    /// </summary>
    Task<IReadOnlyList<Department>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Checks if a department with the given name exists in the current tenant.
    /// </summary>
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken);

    /// <summary>
    /// Checks if another department — excluding the given id — has this name.
    /// Used on update so a department can keep its own name.
    /// </summary>
    Task<bool> ExistsByNameExcludingAsync(string name, Guid excludeId, CancellationToken cancellationToken);

    /// <summary>
    /// Checks if any employees are assigned to this department.
    /// </summary>
    Task<bool> HasEmployeesAsync(Guid departmentId, CancellationToken cancellationToken);

    /// <summary>
    /// Counts all departments in the current tenant.
    /// </summary>
    Task<int> CountAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Checks if the department has any child (sub-)departments.
    /// Used to block deleting a department that still has descendants.
    /// </summary>
    Task<bool> HasSubDepartmentsAsync(Guid id, CancellationToken cancellationToken);
}
