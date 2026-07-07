using MediatR;
using WorkVault.Application.Common.Exceptions;
using WorkVault.Domain.Modules.Employees.Enums;
using WorkVault.Domain.Modules.Employees.Interfaces;
using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Application.Modules.Employees.Commands.UpdateEmployee;

/// <summary>
/// Handles updating an existing employee's details.
/// </summary>
/// <remarks>
/// SECURITY: All FK references (DepartmentId, DesignationId, ManagerId) are validated
/// via repository lookups that apply tenant filtering. This prevents cross-tenant
/// data injection where a valid GUID from Company B could be assigned to an
/// employee in Company A.
/// </remarks>
public class UpdateEmployeeHandler(
    IEmployeeRepository employeeRepository,
    IDepartmentRepository departmentRepository,
    IDesignationRepository designationRepository,
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
        
        // 3. Update User details (name only - email is not changeable)
        if (employee.User is not null)
        {
            employee.User.FirstName = request.FirstName;
            employee.User.LastName = request.LastName;
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
