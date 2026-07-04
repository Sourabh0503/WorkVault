using MediatR;
using WorkVault.Application.Common.Exceptions;
using WorkVault.Domain.Modules.Employees.Interfaces;
using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Application.Modules.Designations.Commands.DeleteDesignation;

public class DeleteDesignationHandler(
    IDesignationRepository designationRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteDesignationCommand>
{
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
