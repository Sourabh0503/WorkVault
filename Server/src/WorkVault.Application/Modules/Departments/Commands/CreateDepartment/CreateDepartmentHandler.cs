using MediatR;
using WorkVault.Application.Common.Exceptions;
using WorkVault.Domain.Modules.Employees;
using WorkVault.Domain.Modules.Employees.Interfaces;
using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Application.Modules.Departments.Commands.CreateDepartment;

public class CreateDepartmentHandler(
    IDepartmentRepository departmentRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateDepartmentCommand, CreateDepartmentResult>
{
    public async Task<CreateDepartmentResult> Handle(
        CreateDepartmentCommand request,
        CancellationToken cancellationToken)
    {
        // Check for duplicate name
        if (await departmentRepository.ExistsByNameAsync(request.Name, null, cancellationToken))
            throw new ConflictException($"A department with name '{request.Name}' already exists.");

        // Validate parent department exists if provided
        if (request.ParentDepartmentId.HasValue)
        {
            var parent = await departmentRepository.GetByIdAsync(request.ParentDepartmentId.Value, cancellationToken);
            if (parent is null)
                throw new NotFoundException($"Parent department with ID '{request.ParentDepartmentId}' not found.");
        }

        var department = new Department
        {
            Name = request.Name,
            Description = request.Description,
            ParentDepartmentId = request.ParentDepartmentId,
            HeadEmployeeId = request.HeadEmployeeId
        };

        await departmentRepository.AddAsync(department, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateDepartmentResult(department.Id, department.Name);
    }
}
