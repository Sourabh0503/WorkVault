using MediatR;
using WorkVault.Application.Common.Exceptions;
using WorkVault.Domain.Modules.Employees.Enums;
using WorkVault.Domain.Modules.Employees.Interfaces;
using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Application.Modules.Employees.Commands.DeleteEmployee;

/// <summary>
/// Deletes a <b>pending</b> employee — one whose invite was never accepted (a data-entry
/// mistake or a no-show). Once an employee has onboarded they are a legal/audit record
/// and can only leave via lifecycle transitions (suspend, offboard), never a delete.
/// </summary>
/// <remarks>
/// Guards:
/// <list type="bullet">
///   <item>Only <see cref="EmployeeStatus.Pending"/> employees may be deleted.</item>
///   <item>Blocks deletion while the employee still has direct reports (they would be
///   orphaned — their ManagerId would point at a hidden record).</item>
/// </list>
/// The linked User is soft-deleted and deactivated so the invite can no longer be used.
/// </remarks>
public class DeleteEmployeeHandler(
    IEmployeeRepository employeeRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteEmployeeCommand>
{
    public async Task Handle(
        DeleteEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await employeeRepository.GetByIdAsync(request.Id, cancellationToken);
        if (employee is null)
            throw new NotFoundException($"Employee with ID '{request.Id}' not found.");

        // Only invites that were never accepted can be deleted. An onboarded employee
        // is an audit record — HR must suspend or offboard them instead.
        if (employee.Status != EmployeeStatus.Pending)
            throw new BusinessRuleException(
                "Only a pending employee (invite not yet accepted) can be deleted. " +
                "Suspend or offboard an onboarded employee instead.");

        if (await employeeRepository.HasDirectReportsAsync(employee.Id, cancellationToken))
            throw new BusinessRuleException(
                "Cannot delete an employee who still has direct reports. Reassign their reports first.");

        // Soft-delete the employee and disable the linked (not-yet-activated) login account.
        employee.IsDeleted = true;
        if (employee.User is not null)
        {
            employee.User.IsActive = false;
            employee.User.IsDeleted = true;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
