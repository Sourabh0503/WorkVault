using MediatR;
using Microsoft.Extensions.Logging;
using WorkVault.Application.Common.Exceptions;
using WorkVault.Application.Common.Interfaces;
using WorkVault.Domain.Modules.Employees;
using WorkVault.Domain.Modules.Employees.Enums;
using WorkVault.Domain.Modules.Employees.Interfaces;
using WorkVault.Domain.Modules.Identity;
using WorkVault.Domain.Modules.Identity.Interfaces;
using WorkVault.SharedKernel.Constants;
using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Application.Modules.Employees.Commands.CreateEmployee;

/// <summary>
/// Handles employee creation with user account and invite token.
/// </summary>
/// <remarks>
/// Employee creation flow (atomic transaction):
/// 1. Validate email not already used in company
/// 2. Create User (no password, IsActive=false, Role=Employee)
/// 3. Generate unique EmployeeCode (EMP-{year}-{sequence})
/// 4. Create Employee record linked to User
/// 5. Create InviteToken (48h expiry)
/// 6. Save all changes in single transaction
/// 7. Log invite link (email integration TODO)
///
/// Requires HR or CompanyAdmin role.
/// Throws ConflictException if email already exists.
/// </remarks>
public class CreateEmployeeHandler(
    IUserRepository userRepository,
    IEmployeeRepository employeeRepository,
    IInviteTokenRepository inviteTokenRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    ILogger<CreateEmployeeHandler> logger)
    : IRequestHandler<CreateEmployeeCommand, CreateEmployeeResult>
{
    public async Task<CreateEmployeeResult> Handle(
        CreateEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Check email isn't already used in this company
        var existingUser = await userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUser is not null)
            throw new ConflictException(
                $"A user with email '{request.Email}' already exists in this company.");

        // 2. Create User — no password yet, inactive until invite accepted
        var user = new User
        {
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PasswordHash = string.Empty,           // set later via invite flow
            RoleId = SystemRoles.Employee,
            IsActive = false                       // activated when password is set
            // CompanyId auto-stamped by SaveChangesAsync from JWT
        };
        await userRepository.AddAsync(user, cancellationToken);

        // 3. Generate EmployeeCode: EMP-{year}-{0001}
        var year = DateTime.UtcNow.Year;
        var countSoFar = await employeeRepository.GetCountForYearAsync(year, cancellationToken);
        var employeeCode = $"EMP-{year}-{(countSoFar + 1):D4}";

        // 4. Create Employee record linked to User
        var employee = new Employee
        {
            UserId = user.Id,
            EmployeeCode = employeeCode,
            Phone = request.Phone,
            JoinDate = request.JoinDate,
            DepartmentId = request.DepartmentId,
            DesignationId = request.DesignationId,
            ManagerId = request.ManagerId,
            Status = EmployeeStatus.Pending
        };
        await employeeRepository.AddAsync(employee, cancellationToken);

        // 5. Create invite token (48h expiry from entity defaults)
        var inviteToken = new InviteToken
        {
            UserId = user.Id
            // Token and ExpiresAt have defaults on the entity
        };
        await inviteTokenRepository.AddAsync(inviteToken, cancellationToken);

        // 6. Single SaveChanges — all 3 inserts in one transaction
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // 7. Stub the email — log the link to console for now
        var inviteLink = $"http://localhost:5012/auth/set-password?token={inviteToken.Token}";
        logger.LogInformation(
            "Invite created for {Email}. Link: {InviteLink}",
            user.Email, inviteLink);

        // 8. Return result
        return new CreateEmployeeResult(
            EmployeeId: employee.Id,
            EmployeeCode: employeeCode,
            InviteLink: inviteLink);
    }
}