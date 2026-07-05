using WorkVault.SharedKernel;

namespace WorkVault.Domain.Modules.Employees;

/// <summary>
/// Represents an organizational department within a company.
/// </summary>
/// <remarks>
/// Supports hierarchical structure via <see cref="ParentDepartmentId"/> for nested departments
/// (e.g., "Frontend" under "Engineering" under "Product").
///
/// Each department can have:
/// - A head/lead (<see cref="HeadEmployeeId"/>)
/// - Multiple designations (job titles specific to this department)
/// - Multiple employees assigned to it
/// </remarks>
public class Department : BaseEntity
{
    /// <summary>Department name (e.g., "Engineering", "Sales", "Human Resources").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description, e.g. "Handles all customer-facing engineering work"
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Who leads this department? Links to an Employee.
    /// Nullable because new departments may not have a head assigned yet.
    /// </summary>
    public Guid? HeadEmployeeId { get; set; }
    
    public Employee? HeadEmployee { get; set; }

    /// <summary>
    /// Allows nested departments. e.g. "Frontend" under "Engineering".
    /// Null = top-level department.
    /// </summary>
    public Guid? ParentDepartmentId { get; set; }
    public Department? ParentDepartment { get; set; }

    /// <summary>
    /// Sub-departments under this one. EF Core uses ParentDepartmentId
    /// to figure out the relationship.
    /// </summary>
    public ICollection<Department> SubDepartments { get; set; } = new List<Department>();
    
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}