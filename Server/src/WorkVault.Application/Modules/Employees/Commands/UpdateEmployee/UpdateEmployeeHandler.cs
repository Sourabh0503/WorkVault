using MediatR;
using WorkVault.Application.Common.Exceptions;
using WorkVault.Application.Common.Interfaces;
using WorkVault.Domain.Modules.Employees.Enums;
using WorkVault.Domain.Modules.Employees.Interfaces;
using WorkVault.SharedKernel.Constants;
using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Application.Modules.Employees.Commands.UpdateEmployee;

/// <summary>
/// Handles updating an existing employee's details.
/// </summary>
/// <remarks>
/// SECURITY: All FK references (DepartmentId, DesignationId, ManagerId) are validated
/// via repository lookups that apply tenant filtering. This prevents cross-tenant
/// data injection where a valid GUID from Company B could be assigned to an
/// employee in Company A. A user also cannot change their own role (which would let an
/// admin self-demote and lock the company out) — that's blocked up front. Likewise a
/// Company Admin's employment status can't be changed (no offboarding/suspending an
/// admin), so a tenant can never be left without an active admin.
/// </remarks>
public class UpdateEmployeeHandler(
    IEmployeeRepository employeeRepository,
    IDepartmentRepository departmentRepository,
    IDesignationRepository designationRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateEmployeeCommand, UpdateEmployeeResult>
{
    public async Task<UpdateEmployeeResult> Handle(
        UpdateEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Find the employee (tenant-filtered)
        var employee = await employeeRepository.GetByIdAsync(request.Id, cancellationToken);
        if (employee is null)
            throw new NotFoundException($"Employee with ID '{request.Id}' not found.");

        // A user can't change their OWN role — that's how the sole CompanyAdmin could
        // self-demote and lock the company out. Editing their other details is fine.
        if (employee.UserId == currentUserService.UserId && request.RoleId != employee.User?.RoleId)
            throw new BusinessRuleException(
                "You can't change your own role. Ask another admin to do it.");

        // Only a company admin can manage another company admin — HR can view an admin's
        // profile but can't edit or demote them. (Editing your own record is handled above.)
        if (employee.User?.RoleId == SystemRoles.CompanyAdmin
            && employee.UserId != currentUserService.UserId
            && currentUserService.Role != SystemRoles.CompanyAdminRole)
            throw new BusinessRuleException("Only a company admin can edit an admin's record.");

        // Only a company admin can grant the CompanyAdmin role (promote someone to admin).
        if (request.RoleId == SystemRoles.CompanyAdmin
            && employee.User?.RoleId != SystemRoles.CompanyAdmin
            && currentUserService.Role != SystemRoles.CompanyAdminRole)
            throw new BusinessRuleException("Only a company admin can grant the Company Admin role.");

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
            if (request.ManagerId == employee.Id)
                throw new BusinessRuleException("An employee cannot be their own manager.");
        }

        // A Company Admin's employment status can't be changed. Offboarding/suspending
        // an admin — including yourself — could leave the tenant with no active admin.
        // If an admin is truly leaving, reassign their role to a non-admin first, then
        // update their status.
        if (employee.User?.RoleId == SystemRoles.CompanyAdmin && request.Status != employee.Status)
            throw new BusinessRuleException(
                "A company admin's status can't be changed. Reassign their role first, then update status.");

        // Status-dependent field requirements. Enforced here (rather than in the validator)
        // so the admin-status guard above wins first — otherwise editing a company admin would
        // surface a generic "Validation Failed" instead of the specific admin rule.
        if (request.Status == EmployeeStatus.OnNotice && request.ResignationDate is null)
            throw new BusinessRuleException("Resignation date is required when status is OnNotice.");

        if (request.Status == EmployeeStatus.OffBoarded && request.LastWorkingDay is null)
            throw new BusinessRuleException("Last working day is required when status is Offboarded.");

        // Block transitions that break the lifecycle model
        if (employee.Status == EmployeeStatus.OffBoarded && request.Status != EmployeeStatus.OffBoarded)
            throw new BusinessRuleException("Cannot change status of an offBoarded employee.");

        if (request.Status == EmployeeStatus.Pending && employee.Status != EmployeeStatus.Pending)
            throw new BusinessRuleException("Employees cannot be reverted to Pending status.");
        
        // A pending employee can only leave Pending by accepting their invite.
        // HR cannot manually change their status.
        if (employee.Status == EmployeeStatus.Pending && request.Status != EmployeeStatus.Pending)
            throw new BusinessRuleException(
                "This employee hasn't accepted their invite yet. Status will update automatically once they set their password.");
        
        // 3. Update User details (name + access role; email is not changeable)
        if (employee.User is not null)
        {
            employee.User.FirstName = request.FirstName;
            employee.User.LastName = request.LastName;
            employee.User.RoleId = request.RoleId;   // validated to HR/Manager/Employee
        }

        // 4. Update personal info
        employee.Phone = request.Phone;
        employee.PhotoUrl = request.PhotoUrl;
        employee.DateOfBirth = request.DateOfBirth;

        // 5. Update organizational placement (now validated)
        employee.DepartmentId = request.DepartmentId;
        employee.DesignationId = request.DesignationId;
        employee.ManagerId = request.ManagerId;

        // 6. Update employment lifecycle
        employee.JoinDate = request.JoinDate;
        employee.ResignationDate = request.ResignationDate;
        employee.LastWorkingDay = request.LastWorkingDay;
        employee.Status = request.Status;

        // 7. Save changes
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateEmployeeResult(employee.Id, employee.EmployeeCode);
    }
}
