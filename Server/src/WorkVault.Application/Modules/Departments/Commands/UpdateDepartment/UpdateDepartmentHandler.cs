using MediatR;
using WorkVault.Application.Common.Exceptions;
using WorkVault.Domain.Modules.Employees.Interfaces;
using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Application.Modules.Departments.Commands.UpdateDepartment;

public class UpdateDepartmentHandler(
    IDepartmentRepository departmentRepository,
    IEmployeeRepository employeeRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateDepartmentCommand, UpdateDepartmentResult>
{
    public async Task<UpdateDepartmentResult> Handle(
        UpdateDepartmentCommand request,
        CancellationToken cancellationToken)
    {
        var department = await departmentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (department is null)
            throw new NotFoundException($"Department not found.");

        // Check for duplicate name (excluding current)
        if (await departmentRepository.ExistsByNameExcludingAsync(request.Name, request.Id, cancellationToken))
            throw new ConflictException($"A department with name '{request.Name}' already exists.");

        // Validate parent department exists if provided (tenant-filtered)
        // 3. Validate + guard the parent
        if (request.ParentDepartmentId.HasValue)
        {
            var parentId = request.ParentDepartmentId.Value;

            // 3a. Can't be its own parent
            if (parentId == request.Id)
                throw new BusinessRuleException("A department cannot be its own parent.");

            // 3b. Parent must exist in this company
            var parent = await departmentRepository.GetByIdAsync(parentId, cancellationToken);
            if (parent is null)
                throw new BusinessRuleException("Parent department not found in this company.");

            // 3c. Circular chain check — the chosen parent must not be a descendant
            //     of this department. Walk UP from the proposed parent; if we hit
            //     this department, it's a cycle.
            var cursor = parent;
            while (cursor?.ParentDepartmentId is not null)
            {
                if (cursor.ParentDepartmentId == request.Id)
                    throw new BusinessRuleException(
                        "That parent would create a circular department structure.");

                cursor = await departmentRepository.GetByIdAsync(
                    cursor.ParentDepartmentId.Value, cancellationToken);
            }
        }

        // Validate head employee exists if provided (tenant-filtered)
        if (request.HeadEmployeeId.HasValue)
        {
            var head = await employeeRepository.GetByIdAsync(request.HeadEmployeeId.Value, cancellationToken);
            if (head is null)
                throw new NotFoundException($"Employee with ID '{request.HeadEmployeeId}' not found.");
        }

        department.Name = request.Name;
        department.Description = request.Description;
        department.ParentDepartmentId = request.ParentDepartmentId;
        department.HeadEmployeeId = request.HeadEmployeeId;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateDepartmentResult(department.Id, department.Name);
    }
}
