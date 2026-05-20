using WorkVault.SharedKernel;

namespace WorkVault.Domain.Modules.Employees;

/// <summary>
/// Represents a job title/designation within a department.
/// </summary>
/// <remarks>
/// Designations define the job titles available in each department.
/// They include a seniority level for org chart ordering and salary bands.
///
/// Examples:
/// - Engineering: Junior Dev (L1), Senior Dev (L3), Tech Lead (L4), VP Engineering (L5)
/// - Sales: Sales Rep (L1), Account Manager (L2), Sales Director (L4)
/// </remarks>
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