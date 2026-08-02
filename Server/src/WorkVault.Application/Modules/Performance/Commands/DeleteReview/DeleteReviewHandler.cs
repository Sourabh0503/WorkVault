using MediatR;
using WorkVault.Application.Common.Exceptions;
using WorkVault.Domain.Modules.Performance;
using WorkVault.Domain.Modules.Performance.Interfaces;
using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Application.Modules.Performance.Commands.DeleteReview;

/// <summary>
/// Soft-deletes a review.
/// </summary>
/// <remarks>
/// Access (HR/Admin) is enforced at the controller. Business rules here:
/// <list type="bullet">
///   <item>Within the 30-day mutable window — past that a review is permanent.</item>
///   <item>The baseline can't be deleted while other reviews exist — it's the salary
///   anchor their hikes are measured against.</item>
/// </list>
/// </remarks>
public class DeleteReviewHandler(
    IReviewRepository reviewRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteReviewCommand>
{
    public async Task Handle(
        DeleteReviewCommand request,
        CancellationToken cancellationToken)
    {
        var review = await reviewRepository.GetByIdAsync(request.ReviewId, cancellationToken);
        if (review is null || review.EmployeeId != request.EmployeeId)
            throw new NotFoundException($"Review with ID '{request.ReviewId}' not found.");

        if (!review.IsMutableAt(DateTime.UtcNow))
            throw new BusinessRuleException(
                $"This review can no longer be deleted (older than {Review.MutableWindowDays} days).");

        // The baseline anchors every later review's hike — don't remove it out from under them.
        if (review.IsBaseline)
        {
            var reviews = await reviewRepository.GetByEmployeeAsync(review.EmployeeId, cancellationToken);
            if (reviews.Any(r => r.Id != review.Id))
                throw new BusinessRuleException(
                    "Can't delete the starting salary while the employee has reviews. Delete the reviews first.");
        }

        review.IsDeleted = true;
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
