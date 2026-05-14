using WorkVault.SharedKernel;

namespace WorkVault.Domain.Modules.Employees;

public class Department : BaseEntity
{
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
}