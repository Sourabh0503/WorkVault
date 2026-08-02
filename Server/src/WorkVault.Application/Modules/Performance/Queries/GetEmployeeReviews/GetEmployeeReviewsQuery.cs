using MediatR;

namespace WorkVault.Application.Modules.Performance.Queries.GetEmployeeReviews;

/// <summary>
/// Lists an employee's reviews for the history list and charts. The data returned is
/// shaped by the caller's role (see <see cref="GetEmployeeReviewsHandler"/>).
/// </summary>
/// <param name="EmployeeId">The employee whose reviews to load.</param>
public record GetEmployeeReviewsQuery(Guid EmployeeId) : IRequest<EmployeeReviewsResult>;

/// <summary>
/// An employee's reviews plus whether the caller may see salary figures.
/// </summary>
/// <remarks>
/// <see cref="SalaryVisible"/> is the authoritative signal for the UI: managers get
/// <c>false</c> (and every <see cref="ReviewDto.NewSalary"/> nulled), so the client hides
/// the salary chart entirely rather than showing an empty one.
/// </remarks>
/// <param name="SalaryVisible">True if the caller is allowed to see salary.</param>
/// <param name="Reviews">The employee's reviews, newest first.</param>
public record EmployeeReviewsResult(
    bool SalaryVisible,
    IReadOnlyList<ReviewDto> Reviews);
