using MediatR;

namespace WorkVault.Application.Modules.Departments.Queries.GetDepartments;

/// <summary>Lists all departments in the current company (with parent/head/member-count summary).</summary>
public record GetDepartmentsQuery : IRequest<IReadOnlyList<DepartmentListDto>>;

/// <summary>Summary row for a department in the list view.</summary>
/// <param name="Id">Department id.</param>
/// <param name="Name">Department name.</param>
/// <param name="Description">Description, if any.</param>
/// <param name="ParentDepartmentId">Parent department id, if any.</param>
/// <param name="ParentDepartmentName">Parent department name, if any.</param>
/// <param name="HeadEmployeeId">Department head id, if any.</param>
/// <param name="HeadEmployeeName">Department head full name, if any.</param>
/// <param name="MemberCount">Number of employees in the department.</param>
public record DepartmentListDto(
    Guid Id,
    string Name,
    string? Description,
    Guid? ParentDepartmentId,
    string? ParentDepartmentName,
    Guid? HeadEmployeeId,
    string? HeadEmployeeName,
    int MemberCount
);
