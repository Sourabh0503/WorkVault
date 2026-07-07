using MediatR;
using WorkVault.Application.Common.Exceptions;
using WorkVault.Domain.Modules.Employees.Interfaces;
using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Application.Modules.Designations.Commands.DeleteDesignation;

/// <summary>
/// Soft-deletes a designation after ensuring no employees are assigned to it
/// (throws <see cref="BusinessRuleException"/> otherwise).
/// </summary>
public class DeleteDesignationHandler(
    IDesignationRepository designationRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteDesignationCommand>
{
    /// <summary>Validates the designation has no assignees, then soft-deletes it.</summary>
    public async Task Handle(
        DeleteDesignationCommand request,
        CancellationToken cancellationToken)
    {
        var designation = await designationRepository.GetByIdAsync(request.Id, cancellationToken);
        if (designation is null)
            throw new NotFoundException($"Designation with ID '{request.Id}' not found.");

        // Check if designation has employees
        if (await designationRepository.HasEmployeesAsync(request.Id, cancellationToken))
            throw new BusinessRuleException("Cannot delete a designation that has employees assigned. Reassign employees first.");

        // Soft delete
        designation.IsDeleted = true;

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
