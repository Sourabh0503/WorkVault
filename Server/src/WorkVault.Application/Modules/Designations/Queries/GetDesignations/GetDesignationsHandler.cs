using MediatR;
using WorkVault.Domain.Modules.Employees.Interfaces;

namespace WorkVault.Application.Modules.Designations.Queries.GetDesignations;

/// <summary>Loads designations (optionally by department) and projects them to list rows.</summary>
public class GetDesignationsHandler(IDesignationRepository repository)
    : IRequestHandler<GetDesignationsQuery, IReadOnlyList<DesignationListDto>>
{
    /// <summary>Returns the matching designations with their department names.</summary>
    public async Task<IReadOnlyList<DesignationListDto>> Handle(
        GetDesignationsQuery request,
        CancellationToken cancellationToken)
    {
        var designations = await repository.GetAllAsync(request.DepartmentId, cancellationToken);

        return designations.Select(d => new DesignationListDto(
            Id: d.Id,
            Title: d.Title,
            Level: d.Level,
            DepartmentId: d.DepartmentId,
            DepartmentName: d.Department?.Name ?? string.Empty
        )).ToList();
    }
}
