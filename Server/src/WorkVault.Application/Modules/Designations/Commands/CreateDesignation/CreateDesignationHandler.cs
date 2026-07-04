using MediatR;
using WorkVault.Application.Common.Exceptions;
using WorkVault.Domain.Modules.Employees;
using WorkVault.Domain.Modules.Employees.Interfaces;
using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Application.Modules.Designations.Commands.CreateDesignation;

public class CreateDesignationHandler(
    IDesignationRepository designationRepository,
    IDepartmentRepository departmentRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateDesignationCommand, CreateDesignationResult>
{
    public async Task<CreateDesignationResult> Handle(
        CreateDesignationCommand request,
        CancellationToken cancellationToken)
    {
        // Validate department exists
        var department = await departmentRepository.GetByIdAsync(request.DepartmentId, cancellationToken);
        if (department is null)
            throw new NotFoundException($"Department with ID '{request.DepartmentId}' not found.");

        // Check for duplicate title in department
        if (await designationRepository.ExistsByTitleAsync(request.Title, request.DepartmentId, null, cancellationToken))
            throw new ConflictException($"A designation with title '{request.Title}' already exists in this department.");

        var designation = new Designation
        {
            Title = request.Title,
            Level = request.Level,
            DepartmentId = request.DepartmentId
        };

        await designationRepository.AddAsync(designation, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateDesignationResult(designation.Id, designation.Title);
    }
}
