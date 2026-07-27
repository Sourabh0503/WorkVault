using MediatR;
using WorkVault.Application.Common.Interfaces;
using WorkVault.Domain.Modules.Employees.Interfaces;
using WorkVault.Domain.Modules.Identity.Interfaces;

namespace WorkVault.Application.Modules.Identity.Queries.GetCurrentUser;

/// <summary>
/// Loads the authenticated user's profile from the current-user context: their account
/// (with role and tenant name) plus their employee record when one exists.
/// </summary>
public class GetCurrentUserHandler(
    ICurrentUserService currentUserService,
    IUserRepository userRepository,
    ICompanyRepository companyRepository,
    IEmployeeRepository employeeRepository)
    : IRequestHandler<GetCurrentUserQuery, CurrentUserDto?>
{
    public async Task<CurrentUserDto?> Handle(
        GetCurrentUserQuery request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (userId is null)
            return null; // no authenticated context (unreachable under [Authorize]) → 401

        // A null user means the account was removed while a token is still live. Signal the
        // controller to return 401 so the client tears down the (now stale) session.
        var user = await userRepository.GetByIdAsync(userId.Value, cancellationToken);
        if (user is null)
            return null;

        // The user's CompanyId is a non-null FK, so a missing company is a corrupt tenant
        // reference — fail loud (500) rather than silently blanking the name.
        var company = await companyRepository.GetByIdAsync(user.CompanyId, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Company '{user.CompanyId}' not found for authenticated user '{user.Id}'.");

        var employee = await employeeRepository.GetByUserIdAsync(user.Id, cancellationToken);

        var employeeProfile = employee is null
            ? null
            : new EmployeeProfileDto(
                EmployeeCode: employee.EmployeeCode,
                Phone: employee.Phone,
                DateOfBirth: employee.DateOfBirth,
                JoinDate: employee.JoinDate,
                DepartmentName: employee.Department?.Name,
                DesignationTitle: employee.Designation?.Title,
                Status: employee.Status,
                Manager: employee.Manager is null
                    ? null
                    : new ManagerSummaryDto(
                        employee.Manager.Id,
                        employee.Manager.EmployeeCode,
                        $"{employee.Manager.User?.FirstName} {employee.Manager.User?.LastName}".Trim()));

        return new CurrentUserDto(
            UserId: user.Id,
            Email: user.Email,
            FirstName: user.FirstName,
            LastName: user.LastName,
            RoleName: user.Role?.Name ?? string.Empty,
            CompanyId: user.CompanyId,
            CompanyName: company.Name,
            Employee: employeeProfile);
    }
}
