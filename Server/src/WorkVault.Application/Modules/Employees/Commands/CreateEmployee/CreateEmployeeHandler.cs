using MediatR;
using Microsoft.Extensions.Logging;
using WorkVault.Application.Common.Exceptions;
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
/// 2. Validate FK references belong to same tenant (SECURITY)
/// 3. Create User (no password, IsActive=false, Role=Employee)
/// 4. Generate unique EmployeeCode (EMP-{year}-{sequence})
/// 5. Create Employee record linked to User
/// 6. Create InviteToken (48h expiry)
/// 7. Save all changes in single transaction
/// 8. Log invite link (email integration TODO)
///
/// Requires HR or CompanyAdmin role.
/// Throws ConflictException if email already exists.
/// </remarks>
public class CreateEmployeeHandler(
    IUserRepository userRepository,
    IEmployeeRepository employeeRepository,
    IDepartmentRepository departmentRepository,
    IDesignationRepository designationRepository,
    IInviteTokenRepository inviteTokenRepository,
    IUnitOfWork unitOfWork,
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

        // 2. Validate FK references belong to same tenant
        // Repository queries apply CompanyId filter, so cross-tenant IDs return null
        if (request.DepartmentId.HasValue)
        {
            var dept = await departmentRepository.GetByIdAsync(request.DepartmentId.Value, cancellationToken);
            if (dept is null)
                throw new NotFoundException($"Department with ID '{request.DepartmentId}' not found.");
        }

        if (request.DesignationId.HasValue)
        {
            var designation = await designationRepository.GetByIdAsync(request.DesignationId.Value, cancellationToken);
            if (designation is null)
                throw new NotFoundException($"Designation with ID '{request.DesignationId}' not found.");
        }

        if (request.ManagerId.HasValue)
        {
            var manager = await employeeRepository.GetByIdAsync(request.ManagerId.Value, cancellationToken);
            if (manager is null)
                throw new NotFoundException($"Manager with ID '{request.ManagerId}' not found.");
        }

        // 3. Create User — no password yet, inactive until invite accepted
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

        // 4. Generate EmployeeCode: EMP-{year}-{0001}
        var year = DateTime.UtcNow.Year;
        var countSoFar = await employeeRepository.GetCountForYearAsync(year, cancellationToken);
        var employeeCode = $"EMP-{year}-{(countSoFar + 1):D4}";

        // 5. Create Employee record linked to User
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

        // 6. Create invite token (48h expiry from entity defaults)
        var inviteToken = new InviteToken
        {
            UserId = user.Id
            // Token and ExpiresAt have defaults on the entity
        };
        await inviteTokenRepository.AddAsync(inviteToken, cancellationToken);

        // 7. Single SaveChanges — all 3 inserts in one transaction
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // 8. Stub the email — log the link to console for now
        var inviteLink = $"http://localhost:4200/set-password?token={inviteToken.Token}";
        logger.LogInformation(
            "Invite created for {Email}. Link: {InviteLink}",
            user.Email, inviteLink);

        // 9. Return result
        return new CreateEmployeeResult(
            EmployeeId: employee.Id,
            EmployeeCode: employeeCode,
            InviteLink: inviteLink);
    }
}