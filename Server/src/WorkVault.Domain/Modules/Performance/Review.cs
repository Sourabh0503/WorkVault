using WorkVault.Domain.Modules.Employees;
using WorkVault.SharedKernel;

namespace WorkVault.Domain.Modules.Performance;

/// <summary>
/// A performance review recorded for an employee at a point in time.
/// </summary>
/// <remarks>
/// One employee has many reviews. Each review captures a rating and, optionally, the
/// employee's new salary — the review is the ONLY entry point for salary in the system
/// ("Model B"). An employee's <em>current salary</em> is therefore not stored on the
/// employee; it's derived as the <see cref="NewSalary"/> of their most recent review
/// that has one.
///
/// Business rules (enforced in the Application layer, not here):
/// - Salary is required on an employee's FIRST review, optional on later ones.
/// - Edit/delete is only allowed within 30 days of <see cref="BaseEntity.CreatedAt"/>;
///   the review is immutable after that.
/// - Rating is 0–5, salary is ≥ 0, and the review date can't be in the future.
///
/// Access is role-based and backend-enforced: managers can see a team member's ratings
/// but never their salary (the list query nulls <see cref="NewSalary"/> for them).
/// </remarks>
public class Review : BaseEntity
{
    /// <summary>
    /// Number of days after creation during which a review may be edited or deleted.
    /// Past this window the review is immutable (a permanent record).
    /// </summary>
    public const int MutableWindowDays = 30;

    /// <summary>
    /// Whether this review is still within its edit/delete window, measured from
    /// <see cref="BaseEntity.CreatedAt"/>. Callers pass "now" so the check stays testable.
    /// </summary>
    public bool IsMutableAt(DateTime utcNow) =>
        (utcNow - CreatedAt).TotalDays <= MutableWindowDays;

    /// <summary>FK to the employee this review belongs to.</summary>
    public Guid EmployeeId { get; set; }

    /// <summary>Navigation property to the reviewed employee.</summary>
    public Employee? Employee { get; set; }

    /// <summary>
    /// Marks the salary <em>baseline</em> — the employee's starting salary, recorded as
    /// their very first entry. A baseline carries a salary but no real appraisal:
    /// <see cref="Rating"/> is not collected for it (stored as 0 and never shown), and it's
    /// excluded from the rating chart. Every employee has exactly one, and it must be their
    /// first entry — it's the anchor the first regular review's hike is measured against.
    /// </summary>
    public bool IsBaseline { get; set; }

    /// <summary>Human-readable label for the review cycle, e.g. "H1 2026 Review".</summary>
    public string ReviewName { get; set; } = string.Empty;

    /// <summary>
    /// The date the review applies to. Displayed month-and-year (e.g. "March 2026")
    /// and used as the x-axis for the rating and salary charts.
    /// </summary>
    public DateOnly ReviewDate { get; set; }

    /// <summary>Performance rating on a 0–5 scale (one decimal place shown as bars).</summary>
    public decimal Rating { get; set; }

    /// <summary>
    /// The employee's new salary as of this review. Nullable: required on the first
    /// review, optional afterwards (a null here means "unchanged since the last review").
    /// </summary>
    public decimal? NewSalary { get; set; }

    /// <summary>Free-text summary of the review.</summary>
    public string? Summary { get; set; }
}
