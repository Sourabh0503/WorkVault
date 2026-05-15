using WorkVault.Domain.Modules.Employees.Enums;
using WorkVault.Domain.Modules.Identity;
using WorkVault.SharedKernel;

namespace WorkVault.Domain.Modules.Employees;

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

    // ---- Personal info ----

    public string? Phone { get; set; }
    public string? PhotoUrl { get; set; }
    public DateOnly? DateOfBirth { get; set; }

    // ---- Organization placement ----

    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public Guid? DesignationId { get; set; }
    public Designation? Designation { get; set; }

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

    // ---- Employment lifecycle ----
    public DateOnly JoinDate { get; set; }
    public DateOnly? ResignationDate { get; set; }
    public DateOnly? LastWorkingDay { get; set; }

    public EmployeeStatus Status { get; set; } = EmployeeStatus.Pending;
}