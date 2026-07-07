using MediatR;
using WorkVault.Application.Common.Exceptions;
using WorkVault.Domain.Modules.Employees.Interfaces;
using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Application.Modules.Departments.Commands.DeleteDepartment;

public class DeleteDepartmentHandler(
    IDepartmentRepository departmentRepository,
    IEmployeeRepository employeeRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteDepartmentCommand>
{
    public async Task Handle(DeleteDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await departmentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (department is null)
            throw new NotFoundException("Department not found.");

        // Block if it has members
        var hasMembers = await employeeRepository.HasMembersInDepartmentAsync(
            request.Id, cancellationToken);
        if (hasMembers)
            throw new BusinessRuleException(
                "Cannot delete a department with employees. Reassign them first.");

        // Block if it has sub-departments
        var hasChildren = await departmentRepository.HasSubDepartmentsAsync(
            request.Id, cancellationToken);
        if (hasChildren)
            throw new BusinessRuleException(
                "Cannot delete a department with sub-departments. Remove or reassign them first.");

        // Soft delete
        department.IsDeleted = true;
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}