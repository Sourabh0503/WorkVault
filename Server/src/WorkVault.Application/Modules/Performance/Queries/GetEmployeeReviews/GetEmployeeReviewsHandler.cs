using MediatR;
using WorkVault.Application.Common.Exceptions;
using WorkVault.Application.Common.Interfaces;
using WorkVault.Domain.Modules.Employees.Interfaces;
using WorkVault.Domain.Modules.Performance.Interfaces;
using WorkVault.SharedKernel.Constants;

namespace WorkVault.Application.Modules.Performance.Queries.GetEmployeeReviews;

/// <summary>
/// Lists an employee's reviews with role-based shaping. THIS is where salary visibility
/// is decided — never the frontend.
/// </summary>
/// <remarks>
/// Access matrix for <c>GET /api/employees/{id}/reviews</c>:
/// <list type="bullet">
///   <item><b>HR / Admin</b> (and SuperAdmin): any employee, salary included.</item>
///   <item><b>Own record</b> (caller is the employee): own reviews, own salary — same as
///   <c>/me/performance</c>.</item>
///   <item><b>Manager</b>: a colleague in the SAME department, ratings only — salary is
///   stripped (nulled) here on the server.</item>
///   <item>Anyone else: 403.</item>
/// </list>
/// </remarks>
public class GetEmployeeReviewsHandler(
    IEmployeeRepository employeeRepository,
    IReviewRepository reviewRepository,
    ICurrentUserService currentUser)
    : IRequestHandler<GetEmployeeReviewsQuery, IReadOnlyList<ReviewDto>>
{
    public async Task<IReadOnlyList<ReviewDto>> Handle(
        GetEmployeeReviewsQuery request,
        CancellationToken cancellationToken)
    {
        // Target must exist in this tenant (repository is tenant-filtered).
        var employee = await employeeRepository.GetByIdAsync(request.EmployeeId, cancellationToken);
        if (employee is null)
            throw new NotFoundException($"Employee with ID '{request.EmployeeId}' not found.");

        // Whether salary may be seen is resolved server-side; it never leaves as a flag.
        // For a manager it's false, so the mapper nulls salary + hike — a raw API call
        // gets nulls, not the figures. The client infers "hide the salary chart" from the
        // data (reviews present but no salaried entry), not from any visibility flag.
        var salaryVisible = await ResolveSalaryVisibility(employee, cancellationToken);

        var reviews = await reviewRepository.GetByEmployeeAsync(request.EmployeeId, cancellationToken);

        return ReviewMapper.ToDtos(reviews, salaryVisible, DateTime.UtcNow);
    }

    /// <summary>
    /// Decides whether the caller may view salary — and, for non-privileged callers,
    /// whether they may view the reviews at all (throws 403 if not).
    /// </summary>
    private async Task<bool> ResolveSalaryVisibility(
        Domain.Modules.Employees.Employee employee, CancellationToken cancellationToken)
    {
        var role = currentUser.Role;

        // HR / Admin / SuperAdmin: full access to any employee, salary included.
        if (role == SystemRoles.HRRole
            || role == SystemRoles.CompanyAdminRole
            || role == SystemRoles.SuperAdminRole)
            return true;

        // Own record: you always see your own salary (matches /me/performance).
        if (currentUser.UserId is { } userId && employee.UserId == userId)
            return true;

        // Manager viewing a colleague in the same department: ratings only, salary stripped.
        if (role == SystemRoles.ManagerRole && currentUser.UserId is { } managerUserId)
        {
            var me = await employeeRepository.GetByUserIdAsync(managerUserId, cancellationToken);
            if (me?.DepartmentId is { } deptId && employee.DepartmentId == deptId)
                return false;
        }

        // Anyone else has no business seeing this employee's reviews.
        throw new ForbiddenException("You don't have access to this employee's reviews.");
    }
}
