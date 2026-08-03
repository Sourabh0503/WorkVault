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
///   <item>The baseline (starting salary) can never be deleted — it's the permanent
///   anchor every later review's hike is measured against.</item>
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

        // The starting salary (baseline) is the permanent anchor — it can never be deleted.
        if (review.IsBaseline)
            throw new BusinessRuleException("The starting salary can't be deleted.");

        if (!review.IsMutableAt(DateTime.UtcNow))
            throw new BusinessRuleException(
                $"This review can no longer be deleted (older than {Review.MutableWindowDays} days).");

        review.IsDeleted = true;
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
