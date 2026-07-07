using MediatR;

namespace WorkVault.Application.Modules.Designations.Queries.GetDesignations;

/// <summary>Lists designations in the current company, optionally filtered to one department.</summary>
/// <param name="DepartmentId">Optional department filter; null returns all designations.</param>
public record GetDesignationsQuery(Guid? DepartmentId = null) : IRequest<IReadOnlyList<DesignationListDto>>;

/// <summary>Summary row for a designation in the list view.</summary>
/// <param name="Id">Designation id.</param>
/// <param name="Title">Designation title.</param>
/// <param name="Level">Seniority level.</param>
/// <param name="DepartmentId">Owning department id.</param>
/// <param name="DepartmentName">Owning department name.</param>
public record DesignationListDto(
    Guid Id,
    string Title,
    int Level,
    Guid DepartmentId,
    string DepartmentName
);
