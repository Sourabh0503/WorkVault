using MediatR;

namespace WorkVault.Application.Modules.Performance.Commands.CreateReview;

/// <summary>
/// Records a new performance review for an employee (HR/Admin only).
/// </summary>
/// <param name="EmployeeId">The employee being reviewed (from the route).</param>
/// <param name="ReviewName">Label for the review cycle, e.g. "H1 2026 Review".</param>
/// <param name="ReviewDate">The date the review applies to (can't be in the future).</param>
/// <param name="Rating">Performance rating, 0–5. Ignored for a baseline entry.</param>
/// <param name="NewSalary">New salary. Required for a baseline; optional on a regular review.</param>
/// <param name="Summary">Optional free-text summary.</param>
/// <param name="IsBaseline">
/// True to record the employee's starting salary (their first, salary-only entry).
/// A baseline has no rating and must be the employee's first entry.
/// </param>
public record CreateReviewCommand(
    Guid EmployeeId,
    string ReviewName,
    DateOnly ReviewDate,
    decimal Rating,
    decimal? NewSalary,
    string? Summary,
    bool IsBaseline
) : IRequest<CreateReviewResult>;

/// <summary>Result of creating a review.</summary>
/// <param name="ReviewId">The new review's id.</param>
public record CreateReviewResult(Guid ReviewId);
