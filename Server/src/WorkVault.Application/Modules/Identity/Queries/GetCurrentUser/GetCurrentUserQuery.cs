using MediatR;
using WorkVault.Domain.Modules.Employees.Enums;

namespace WorkVault.Application.Modules.Identity.Queries.GetCurrentUser;

/// <summary>
/// Returns the authenticated user's own profile — account (name, email, role, tenant)
/// plus their employee record (code, department, designation, status) when one exists.
/// </summary>
public record GetCurrentUserQuery : IRequest<CurrentUserDto?>;

/// <summary>The signed-in user's profile.</summary>
/// <param name="UserId">The user's id.</param>
/// <param name="Email">Login email.</param>
/// <param name="FirstName">First name.</param>
/// <param name="LastName">Last name.</param>
/// <param name="RoleName">Access role (e.g. CompanyAdmin, HR, Manager, Employee).</param>
/// <param name="CompanyId">The tenant id.</param>
/// <param name="CompanyName">The tenant display name.</param>
/// <param name="Employee">The linked employee profile, or null if the user has no employee record.</param>
public record CurrentUserDto(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string RoleName,
    Guid CompanyId,
    string CompanyName,
    EmployeeProfileDto? Employee
);

/// <summary>Employment details shown on the profile page.</summary>
/// <param name="EmployeeCode">Human-readable employee code.</param>
/// <param name="Phone">Contact phone, if set.</param>
/// <param name="JoinDate">Date the employee joined.</param>
/// <param name="DepartmentName">Department name, if assigned.</param>
/// <param name="DesignationTitle">Designation/title, if assigned.</param>
/// <param name="Status">Current employment status.</param>
/// <param name="Manager">The reporting manager, if assigned.</param>
public record EmployeeProfileDto(
    string EmployeeCode,
    string? Phone,
    DateOnly JoinDate,
    string? DepartmentName,
    string? DesignationTitle,
    EmployeeStatus Status,
    ManagerSummaryDto? Manager
);

/// <summary>A brief reference to the employee's reporting manager.</summary>
/// <param name="Id">The manager employee's id.</param>
/// <param name="EmployeeCode">The manager's employee code.</param>
/// <param name="FullName">The manager's display name.</param>
public record ManagerSummaryDto(Guid Id, string EmployeeCode, string FullName);
