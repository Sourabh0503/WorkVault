using MediatR;
using WorkVault.Application.Common.Exceptions;
using WorkVault.Domain.Modules.Performance;
using WorkVault.Domain.Modules.Performance.Interfaces;
using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Application.Modules.Performance.Commands.UpdateReview;

/// <summary>
/// Edits an existing review (or the salary baseline).
/// </summary>
/// <remarks>
/// Access (HR/Admin) is enforced at the controller. Business rules enforced here:
/// <list type="bullet">
///   <item>The review must still be within its 30-day mutable window.</item>
///   <item>A baseline's salary can't be cleared (it's the starting salary), and its
///   rating stays 0 — a baseline never carries a rating.</item>
/// </list>
/// </remarks>
public class UpdateReviewHandler(
    IReviewRepository reviewRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateReviewCommand>
{
    public async Task Handle(
        UpdateReviewCommand request,
        CancellationToken cancellationToken)
    {
        var review = await reviewRepository.GetByIdAsync(request.ReviewId, cancellationToken);
        // Not found, or the review doesn't belong to the employee in the route — same 404
        // either way (don't leak the existence of another employee's review).
        if (review is null || review.EmployeeId != request.EmployeeId)
            throw new NotFoundException($"Review with ID '{request.ReviewId}' not found.");

        if (!review.IsMutableAt(DateTime.UtcNow))
            throw new BusinessRuleException(
                $"This review can no longer be edited (older than {Review.MutableWindowDays} days).");

        // The baseline must keep its starting salary.
        if (review.IsBaseline && request.NewSalary is null)
            throw new BusinessRuleException("A starting-salary entry must include a salary.");

        review.ReviewName = request.ReviewName;
        review.ReviewDate = request.ReviewDate;
        // A baseline never carries a rating; ignore any incoming value and keep it at 0.
        review.Rating = review.IsBaseline ? 0m : request.Rating;
        review.NewSalary = request.NewSalary;
        review.Summary = request.Summary;

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
