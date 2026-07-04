using MediatR;
using WorkVault.Domain.Modules.Employees.Interfaces;

namespace WorkVault.Application.Modules.Designations.Queries.GetDesignationById;

public class GetDesignationByIdHandler(IDesignationRepository repository)
    : IRequestHandler<GetDesignationByIdQuery, DesignationDto?>
{
    public async Task<DesignationDto?> Handle(
        GetDesignationByIdQuery request,
        CancellationToken cancellationToken)
    {
        var designation = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (designation is null) return null;

        return new DesignationDto(
            Id: designation.Id,
            Title: designation.Title,
            Level: designation.Level,
            DepartmentId: designation.DepartmentId,
            DepartmentName: designation.Department?.Name ?? string.Empty,
            CreatedAt: designation.CreatedAt
        );
    }
}
