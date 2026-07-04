using MediatR;

namespace WorkVault.Application.Modules.Departments.Queries.GetDepartmentById;

public record GetDepartmentByIdQuery(Guid Id) : IRequest<DepartmentDto?>;

public record DepartmentDto(
    Guid Id,
    string Name,
    string? Description,
    Guid? HeadEmployeeId,
    Guid? ParentDepartmentId,
    string? ParentDepartmentName,
    DateTime CreatedAt
);
