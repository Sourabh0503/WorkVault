using MediatR;

namespace WorkVault.Application.Modules.Employees.Commands.CreateEmployee;

/// <summary>
/// Creates a new employee (and its linked pending user) and issues an invite so the
/// person can set a password and join.
/// </summary>
/// <param name="FirstName">Employee's first name.</param>
/// <param name="LastName">Employee's last name.</param>
/// <param name="Email">Work email — also the login and invite target.</param>
/// <param name="Phone">Optional contact phone.</param>
/// <param name="JoinDate">Date the employee joins.</param>
/// <param name="DepartmentId">Optional department assignment.</param>
/// <param name="DesignationId">Optional designation/title assignment.</param>
/// <param name="ManagerId">Optional reporting manager.</param>
/// <param name="RoleId">Access role to grant (HR, Manager, or Employee).</param>
public record CreateEmployeeCommand(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    DateOnly JoinDate,
    Guid? DepartmentId,
    Guid? DesignationId,
    Guid? ManagerId,
    Guid RoleId
) : IRequest<CreateEmployeeResult>;

/// <summary>Result of creating an employee, including the shareable invite link.</summary>
/// <param name="EmployeeId">The new employee's id.</param>
/// <param name="EmployeeCode">The generated employee code.</param>
/// <param name="InviteLink">Link the employee uses to set their password.</param>
public record CreateEmployeeResult(
    Guid EmployeeId,
    string EmployeeCode,
    string InviteLink
);