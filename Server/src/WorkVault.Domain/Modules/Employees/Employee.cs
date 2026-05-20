using WorkVault.Domain.Modules.Employees.Enums;
using WorkVault.Domain.Modules.Identity;
using WorkVault.SharedKernel;

namespace WorkVault.Domain.Modules.Employees;

/// <summary>
/// Represents an employee record with HR-related data.
/// </summary>
/// <remarks>
/// Employee is separate from User to support:
/// - HR data (department, designation, dates) vs auth data (email, password)
/// - Employees who don't have login access
/// - Historical records after user accounts are deactivated
///
/// Lifecycle:
/// 1. HR creates employee → Status=Pending, User.IsActive=false
/// 2. Employee accepts invite → Status=Active, User.IsActive=true
/// 3. Resignation → Status=OnNotice, set ResignationDate
/// 4. Last day passes → Status=Offboarded, set LastWorkingDay
/// </remarks>
public class Employee : BaseEntity
{
    /// <summary>
    /// Human-readable code shown on ID cards. Format: EMP-{YYYY}-{XXXX}
    /// e.g., EMP-2025-0042 (the 42nd employee added in 2025).
    /// Auto-generated when the employee is created.
    /// </summary>
    public string EmployeeCode { get; set; } = string.Empty;

    // ---- Link to login account ----

    /// <summary>
    /// One Employee = one User. The User holds login credentials,
    /// Employee holds HR data.
    /// </summary>
    public Guid UserId { get; set; }
    public User? User { get; set; }

    #region Personal Info

    /// <summary>Employee's phone number for contact purposes.</summary>
    public string? Phone { get; set; }

    /// <summary>URL to the employee's profile photo.</summary>
    public string? PhotoUrl { get; set; }

    /// <summary>Employee's date of birth (for birthday reminders, age calculations).</summary>
    public DateOnly? DateOfBirth { get; set; }

    #endregion

    #region Organization Placement

    /// <summary>FK to the department the employee belongs to.</summary>
    public Guid? DepartmentId { get; set; }
    /// <summary>Navigation property to the employee's department.</summary>
    public Department? Department { get; set; }

    /// <summary>FK to the employee's job title/designation.</summary>
    public Guid? DesignationId { get; set; }
    /// <summary>Navigation property to the employee's designation.</summary>
    public Designation? Designation { get; set; }

    #endregion

    /// <summary>
    /// Self-referencing: who does this employee report to?
    /// Building the org chart — every employee points to their manager.
    /// CEO/founder has ManagerId = null.
    /// </summary>
    public Guid? ManagerId { get; set; }
    public Employee? Manager { get; set; }

    /// <summary>
    /// Employees who report to this one. Inverse side of the Manager relationship.
    /// </summary>
    public ICollection<Employee> DirectReports { get; set; } = new List<Employee>();

    #region Employment Lifecycle

    /// <summary>Date when the employee joined/will join the company.</summary>
    public DateOnly JoinDate { get; set; }

    /// <summary>Date when the employee submitted their resignation. Null if not resigned.</summary>
    public DateOnly? ResignationDate { get; set; }

    /// <summary>
    /// The employee's last working day (after notice period).
    /// Used for offboarding and access revocation scheduling.
    /// </summary>
    public DateOnly? LastWorkingDay { get; set; }

    /// <summary>
    /// Current status in the employee lifecycle.
    /// See <see cref="EmployeeStatus"/> for possible values.
    /// </summary>
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Pending;

    #endregion
}