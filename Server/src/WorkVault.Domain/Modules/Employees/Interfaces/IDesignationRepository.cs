using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Domain.Modules.Employees.Interfaces;

/// <summary>
/// Repository interface for <see cref="Designation"/> entity operations.
/// </summary>
public interface IDesignationRepository : IRepository<Designation>
{
    /// <summary>
    /// Retrieves all designations, optionally filtered by department.
    /// </summary>
    Task<IReadOnlyList<Designation>> GetAllAsync(Guid? departmentId, CancellationToken cancellationToken);

    /// <summary>
    /// Checks if a designation with the given title exists in the department.
    /// </summary>
    Task<bool> ExistsByTitleAsync(string title, Guid departmentId, Guid? excludeId, CancellationToken cancellationToken);

    /// <summary>
    /// Checks if any employees have this designation.
    /// </summary>
    Task<bool> HasEmployeesAsync(Guid designationId, CancellationToken cancellationToken);
}
