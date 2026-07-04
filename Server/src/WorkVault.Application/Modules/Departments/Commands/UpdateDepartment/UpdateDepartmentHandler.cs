using MediatR;
using WorkVault.Application.Common.Exceptions;
using WorkVault.Domain.Modules.Employees.Interfaces;
using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Application.Modules.Departments.Commands.UpdateDepartment;

public class UpdateDepartmentHandler(
    IDepartmentRepository departmentRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateDepartmentCommand, UpdateDepartmentResult>
{
    public async Task<UpdateDepartmentResult> Handle(
        UpdateDepartmentCommand request,
        CancellationToken cancellationToken)
    {
        var department = await departmentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (department is null)
            throw new NotFoundException($"Department with ID '{request.Id}' not found.");

        // Check for duplicate name (excluding current)
        if (await departmentRepository.ExistsByNameAsync(request.Name, request.Id, cancellationToken))
            throw new ConflictException($"A department with name '{request.Name}' already exists.");

        // Validate parent department exists if provided
        if (request.ParentDepartmentId.HasValue)
        {
            var parent = await departmentRepository.GetByIdAsync(request.ParentDepartmentId.Value, cancellationToken);
            if (parent is null)
                throw new NotFoundException($"Parent department with ID '{request.ParentDepartmentId}' not found.");
        }

        department.Name = request.Name;
        department.Description = request.Description;
        department.ParentDepartmentId = request.ParentDepartmentId;
        department.HeadEmployeeId = request.HeadEmployeeId;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateDepartmentResult(department.Id, department.Name);
    }
}
