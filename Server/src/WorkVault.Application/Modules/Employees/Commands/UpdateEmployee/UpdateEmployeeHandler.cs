using MediatR;
using WorkVault.Application.Common.Exceptions;
using WorkVault.Domain.Modules.Employees.Interfaces;
using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Application.Modules.Employees.Commands.UpdateEmployee;

/// <summary>
/// Handles updating an existing employee's details.
/// </summary>
public class UpdateEmployeeHandler(
    IEmployeeRepository employeeRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateEmployeeCommand, UpdateEmployeeResult>
{
    public async Task<UpdateEmployeeResult> Handle(
        UpdateEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Find the employee
        var employee = await employeeRepository.GetByIdAsync(request.Id, cancellationToken);
        if (employee is null)
            throw new NotFoundException($"Employee with ID '{request.Id}' not found.");

        // 2. Update User details (name only - email is not changeable)
        if (employee.User is not null)
        {
            employee.User.FirstName = request.FirstName;
            employee.User.LastName = request.LastName;
        }

        // 3. Update personal info
        employee.Phone = request.Phone;
        employee.PhotoUrl = request.PhotoUrl;
        employee.DateOfBirth = request.DateOfBirth;

        // 4. Update organizational placement
        employee.DepartmentId = request.DepartmentId;
        employee.DesignationId = request.DesignationId;
        employee.ManagerId = request.ManagerId;

        // 5. Update employment lifecycle
        employee.JoinDate = request.JoinDate;
        employee.ResignationDate = request.ResignationDate;
        employee.LastWorkingDay = request.LastWorkingDay;
        employee.Status = request.Status;

        // 6. Save changes
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateEmployeeResult(employee.Id, employee.EmployeeCode);
    }
}
