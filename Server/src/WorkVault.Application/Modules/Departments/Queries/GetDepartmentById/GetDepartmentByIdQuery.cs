using MediatR;

namespace WorkVault.Application.Modules.Departments.Queries.GetDepartmentById;

/// <summary>Fetches a single department by id. Returns null if not found.</summary>
/// <param name="Id">The department id.</param>
public record GetDepartmentByIdQuery(Guid Id) : IRequest<DepartmentDto?>;

/// <summary>Detail read model for a single department.</summary>
/// <param name="Id">Department id.</param>
/// <param name="Name">Department name.</param>
/// <param name="Description">Description, if any.</param>
/// <param name="HeadEmployeeId">Department head id, if any.</param>
/// <param name="ParentDepartmentId">Parent department id, if any.</param>
/// <param name="ParentDepartmentName">Parent department name, if any.</param>
/// <param name="CreatedAt">When the department was created (UTC).</param>
public record DepartmentDto(
    Guid Id,
    string Name,
    string? Description,
    Guid? HeadEmployeeId,
    Guid? ParentDepartmentId,
    string? ParentDepartmentName,
    DateTime CreatedAt
);
