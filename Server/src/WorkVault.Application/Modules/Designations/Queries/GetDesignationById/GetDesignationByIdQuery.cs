using MediatR;

namespace WorkVault.Application.Modules.Designations.Queries.GetDesignationById;

/// <summary>Fetches a single designation by id. Returns null if not found.</summary>
/// <param name="Id">The designation id.</param>
public record GetDesignationByIdQuery(Guid Id) : IRequest<DesignationDto?>;

/// <summary>Detail read model for a single designation.</summary>
/// <param name="Id">Designation id.</param>
/// <param name="Title">Designation title.</param>
/// <param name="Level">Seniority level.</param>
/// <param name="DepartmentId">Owning department id.</param>
/// <param name="DepartmentName">Owning department name.</param>
/// <param name="CreatedAt">When the designation was created (UTC).</param>
public record DesignationDto(
    Guid Id,
    string Title,
    int Level,
    Guid DepartmentId,
    string DepartmentName,
    DateTime CreatedAt
);
