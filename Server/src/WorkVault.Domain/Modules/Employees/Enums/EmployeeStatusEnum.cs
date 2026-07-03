namespace WorkVault.Domain.Modules.Employees.Enums;

/// <summary>
/// Represents the current state in an employee's lifecycle.
/// </summary>
/// <remarks>
/// Status transitions:
/// <code>
/// Pending → Active (invite accepted)
/// Active → OnNotice (resignation submitted)
/// Active → Suspended (HR action)
/// OnNotice → OffBoarded (last day passed)
/// Suspended → Active (reinstated)
/// Suspended → OffBoarded (terminated)
/// </code>
/// </remarks>
public enum EmployeeStatus
{
    /// <summary>
    /// Employee record created, invite sent, but not yet accepted.
    /// User.IsActive = false at this stage.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Normal working employee with full system access.
    /// </summary>
    Active = 1,

    /// <summary>
    /// Employee has resigned and is serving notice period.
    /// Still has access but may have restricted permissions.
    /// </summary>
    OnNotice = 2,

    /// <summary>
    /// Temporarily deactivated by HR/Admin.
    /// No system access until reinstated.
    /// </summary>
    Suspended = 3,

    /// <summary>
    /// Employee has left the company.
    /// No system access. Record preserved for historical data.
    /// </summary>
    OffBoarded = 4
}