using MediatR;
using WorkVault.Application.Common.Exceptions;
using WorkVault.Domain.Modules.Employees.Interfaces;
using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Application.Modules.Designations.Commands.UpdateDesignation;

public class UpdateDesignationHandler(
    IDesignationRepository designationRepository,
    IDepartmentRepository departmentRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateDesignationCommand, UpdateDesignationResult>
{
    public async Task<UpdateDesignationResult> Handle(
        UpdateDesignationCommand request,
        CancellationToken cancellationToken)
    {
        var designation = await designationRepository.GetByIdAsync(request.Id, cancellationToken);
        if (designation is null)
            throw new NotFoundException($"Designation with ID '{request.Id}' not found.");

        // Validate department exists
        var department = await departmentRepository.GetByIdAsync(request.DepartmentId, cancellationToken);
        if (department is null)
            throw new NotFoundException($"Department with ID '{request.DepartmentId}' not found.");

        // Check for duplicate title in department (excluding current)
        if (await designationRepository.ExistsByTitleAsync(request.Title, request.DepartmentId, request.Id, cancellationToken))
            throw new ConflictException($"A designation with title '{request.Title}' already exists in this department.");

        designation.Title = request.Title;
        designation.Level = request.Level;
        designation.DepartmentId = request.DepartmentId;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateDesignationResult(designation.Id, designation.Title);
    }
}
