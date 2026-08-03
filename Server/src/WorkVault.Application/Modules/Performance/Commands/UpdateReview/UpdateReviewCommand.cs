using MediatR;

namespace WorkVault.Application.Modules.Performance.Commands.UpdateReview;

/// <summary>
/// Edits an existing review (HR/Admin only, within the 30-day mutable window).
/// </summary>
/// <param name="EmployeeId">The employee the review belongs to (from the route).</param>
/// <param name="ReviewId">The review to edit (from the route).</param>
/// <param name="ReviewName">Label for the review cycle.</param>
/// <param name="ReviewDate">The date the review applies to (can't be in the future).</param>
/// <param name="Rating">Performance rating, 0–5.</param>
/// <param name="NewSalary">New salary. Required if this is the employee's first review.</param>
/// <param name="Summary">Optional free-text summary.</param>
public record UpdateReviewCommand(
    Guid EmployeeId,
    Guid ReviewId,
    string ReviewName,
    DateOnly ReviewDate,
    decimal Rating,
    decimal? NewSalary,
    string? Summary
) : IRequest;
