using MediatR;
using WorkVault.Application.Modules.Performance.Queries.GetEmployeeReviews;

namespace WorkVault.Application.Modules.Performance.Queries.GetMyPerformance;

/// <summary>
/// Returns the current user's own reviews — ratings plus their own salary — for the
/// read-only "My Performance" page. Reuses <see cref="EmployeeReviewsResult"/>.
/// </summary>
public record GetMyPerformanceQuery : IRequest<EmployeeReviewsResult>;
