using MediatR;

namespace WorkVault.Application.Modules.Departments.Queries.GetDepartments;

public record GetDepartmentsQuery : IRequest<IReadOnlyList<DepartmentListDto>>;

public record DepartmentListDto(
    Guid Id,
    string Name,
    string? Description,
    Guid? ParentDepartmentId,
    string? ParentDepartmentName
);
