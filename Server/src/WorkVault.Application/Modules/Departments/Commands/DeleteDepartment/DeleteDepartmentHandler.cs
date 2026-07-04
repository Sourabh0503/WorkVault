using MediatR;
using WorkVault.Application.Common.Exceptions;
using WorkVault.Domain.Modules.Employees.Interfaces;
using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Application.Modules.Departments.Commands.DeleteDepartment;

public class DeleteDepartmentHandler(
    IDepartmentRepository departmentRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteDepartmentCommand>
{
    public async Task Handle(
        DeleteDepartmentCommand request,
        CancellationToken cancellationToken)
    {
        var department = await departmentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (department is null)
            throw new NotFoundException($"Department with ID '{request.Id}' not found.");

        // Check if department has employees
        if (await departmentRepository.HasEmployeesAsync(request.Id, cancellationToken))
            throw new BusinessRuleException("Cannot delete a department that has employees assigned. Reassign employees first.");

        // Soft delete
        department.IsDeleted = true;

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
