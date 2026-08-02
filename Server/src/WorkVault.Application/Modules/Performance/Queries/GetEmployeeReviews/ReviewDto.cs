namespace WorkVault.Application.Modules.Performance.Queries.GetEmployeeReviews;

/// <summary>
/// A single review as returned to clients.
/// </summary>
/// <remarks>
/// <see cref="NewSalary"/> is null either because the review genuinely carries no salary
/// OR because the caller isn't allowed to see it (managers). Clients must not infer
/// salary visibility from this field — use <see cref="EmployeeReviewsResult.SalaryVisible"/>.
/// </remarks>
/// <param name="Id">Review id.</param>
/// <param name="ReviewName">Review cycle label.</param>
/// <param name="ReviewDate">Date the review applies to.</param>
/// <param name="Rating">Rating 0–5. Meaningless for a baseline (see <paramref name="IsBaseline"/>).</param>
/// <param name="IsBaseline">
/// True if this is the starting-salary entry: show a "Starting salary" badge, hide the
/// rating, and exclude it from the rating chart (it's still the salary chart's first point).
/// </param>
/// <param name="NewSalary">Salary as of this review (nulled for callers who can't see salary).</param>
/// <param name="IncrementPercent">
/// Percentage change from the previous salaried review, e.g. 8.5 for +8.5%. Derived, not
/// stored. Null when it can't be computed: the first salaried review (no prior figure),
/// a review that carries no salary, or a caller who can't see salary (managers).
/// </param>
/// <param name="Summary">Free-text summary.</param>
/// <param name="IsMutable">Whether the review is still within its 30-day edit/delete window.</param>
/// <param name="CreatedAt">When the review was recorded.</param>
public record ReviewDto(
    Guid Id,
    string ReviewName,
    DateOnly ReviewDate,
    decimal Rating,
    bool IsBaseline,
    decimal? NewSalary,
    decimal? IncrementPercent,
    string? Summary,
    bool IsMutable,
    DateTime CreatedAt);
