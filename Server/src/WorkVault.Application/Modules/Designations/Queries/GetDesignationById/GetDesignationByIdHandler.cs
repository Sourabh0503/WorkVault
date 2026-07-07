using MediatR;
using WorkVault.Domain.Modules.Employees.Interfaces;

namespace WorkVault.Application.Modules.Designations.Queries.GetDesignationById;

/// <summary>Loads a designation by id and maps it to a <see cref="DesignationDto"/> (null if missing).</summary>
public class GetDesignationByIdHandler(IDesignationRepository repository)
    : IRequestHandler<GetDesignationByIdQuery, DesignationDto?>
{
    /// <summary>Returns the designation detail, or null when no designation matches the id.</summary>
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
