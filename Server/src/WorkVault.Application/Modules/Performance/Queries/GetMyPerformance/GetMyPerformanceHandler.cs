using MediatR;
using WorkVault.Application.Common.Interfaces;
using WorkVault.Application.Modules.Performance.Queries.GetEmployeeReviews;
using WorkVault.Domain.Modules.Employees.Interfaces;
using WorkVault.Domain.Modules.Performance.Interfaces;

namespace WorkVault.Application.Modules.Performance.Queries.GetMyPerformance;

/// <summary>
/// Loads the caller's own reviews. Salary is always visible (it's their own).
/// Returns an empty result if the caller has no employee record.
/// </summary>
public class GetMyPerformanceHandler(
    IEmployeeRepository employeeRepository,
    IReviewRepository reviewRepository,
    ICurrentUserService currentUser)
    : IRequestHandler<GetMyPerformanceQuery, IReadOnlyList<ReviewDto>>
{
    public async Task<IReadOnlyList<ReviewDto>> Handle(
        GetMyPerformanceQuery request,
        CancellationToken cancellationToken)
    {
        // Endpoint is authorized, but guard anyway.
        if (currentUser.UserId is not { } userId)
            return [];

        // Resolve the caller's own employee record. No record → no reviews.
        var me = await employeeRepository.GetByUserIdAsync(userId, cancellationToken);
        if (me is null)
            return [];

        var reviews = await reviewRepository.GetByEmployeeAsync(me.Id, cancellationToken);

        // Own performance: salary always visible, with derived hike%.
        return ReviewMapper.ToDtos(reviews, salaryVisible: true, DateTime.UtcNow);
    }
}
