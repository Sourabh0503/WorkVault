using WorkVault.SharedKernel;

namespace WorkVault.Domain.Modules.Employees;

public class Designation : BaseEntity
{
    /// <summary>
    /// Job title, e.g. "Senior Software Engineer", "Tech Lead", "VP of Sales"
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Seniority level (1 = entry, 2 = mid, 3 = senior, 4 = lead, 5 = director...).
    /// Used for org chart ordering and reporting hierarchy.
    /// </summary>
    public int Level { get; set; } = 1;

    /// <summary>
    /// Which department does this designation belong to?
    /// e.g., "Senior Frontend Dev" belongs to "Engineering".
    /// </summary>
    public Guid DepartmentId { get; set; }
    public Department? Department { get; set; }
}