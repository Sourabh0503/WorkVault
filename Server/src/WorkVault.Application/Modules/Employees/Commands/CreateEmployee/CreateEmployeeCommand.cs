using MediatR;

namespace WorkVault.Application.Modules.Employees.Commands.CreateEmployee;

public record CreateEmployeeCommand(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    DateOnly JoinDate,
    Guid? DepartmentId,
    Guid? DesignationId,
    Guid? ManagerId
) : IRequest<CreateEmployeeResult>;

public record CreateEmployeeResult(
    Guid EmployeeId,
    string EmployeeCode,
    string InviteLink
);