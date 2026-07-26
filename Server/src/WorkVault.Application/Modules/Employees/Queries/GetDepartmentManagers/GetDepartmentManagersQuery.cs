using MediatR;

namespace WorkVault.Application.Modules.Employees.Queries.GetDepartmentManagers;

/// <summary>
/// Lists the Active, Manager-role employees in a department — used to populate the
/// reporting-manager dropdown on the employee create/edit form.
/// </summary>
/// <param name="DepartmentId">The department whose managers to list.</param>
public record GetDepartmentManagersQuery(Guid DepartmentId)
    : IRequest<IReadOnlyList<ManagerOptionDto>>;

/// <summary>A selectable reporting manager.</summary>
/// <param name="Id">The manager employee's id.</param>
/// <param name="FullName">The manager's display name.</param>
/// <param name="EmployeeCode">The manager's employee code (disambiguates same names).</param>
public record ManagerOptionDto(Guid Id, string FullName, string EmployeeCode);
