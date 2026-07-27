using MediatR;

namespace WorkVault.Application.Modules.Employees.Queries.GetDepartmentMembers;

/// <summary>
/// Lists the Active employees in a department (any role) — the reporting-manager
/// candidates for the employee create/edit form.
/// </summary>
/// <param name="DepartmentId">The department whose members to list.</param>
public record GetDepartmentMembersQuery(Guid DepartmentId)
    : IRequest<IReadOnlyList<DepartmentMemberDto>>;

/// <summary>A selectable department member (a candidate reporting manager).</summary>
/// <param name="Id">The employee's id.</param>
/// <param name="FullName">The member's display name.</param>
/// <param name="EmployeeCode">The member's employee code (disambiguates same names).</param>
public record DepartmentMemberDto(Guid Id, string FullName, string EmployeeCode);
