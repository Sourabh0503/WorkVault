using MediatR;

namespace WorkVault.Application.Modules.Performance.Commands.DeleteReview;

/// <summary>
/// Soft-deletes a review (HR/Admin only, within the 30-day mutable window).
/// </summary>
/// <param name="EmployeeId">The employee the review belongs to (from the route).</param>
/// <param name="ReviewId">The review to delete (from the route).</param>
public record DeleteReviewCommand(
    Guid EmployeeId,
    Guid ReviewId
) : IRequest;
