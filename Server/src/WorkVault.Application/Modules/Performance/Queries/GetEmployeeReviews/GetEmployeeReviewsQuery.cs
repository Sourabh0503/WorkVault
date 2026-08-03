using MediatR;

namespace WorkVault.Application.Modules.Performance.Queries.GetEmployeeReviews;

/// <summary>
/// Lists an employee's reviews for the history list and charts, newest first. The data is
/// shaped by the caller's role (see <see cref="GetEmployeeReviewsHandler"/>) — a manager's
/// results have every <see cref="ReviewDto.NewSalary"/> and hike nulled, so salary never
/// leaves the server for them, even to a raw API call.
/// </summary>
/// <param name="EmployeeId">The employee whose reviews to load.</param>
public record GetEmployeeReviewsQuery(Guid EmployeeId) : IRequest<IReadOnlyList<ReviewDto>>;
