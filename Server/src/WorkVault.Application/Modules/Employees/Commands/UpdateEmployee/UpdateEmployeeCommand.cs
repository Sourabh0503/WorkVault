using MediatR;
using WorkVault.Domain.Modules.Employees.Enums;

namespace WorkVault.Application.Modules.Employees.Commands.UpdateEmployee;

/// <summary>
/// Command to update an existing employee's details.
/// </summary>
/// <remarks>
/// Note: Email cannot be changed through this command as it affects authentication.
/// </remarks>
public record UpdateEmployeeCommand : IRequest<UpdateEmployeeResult>
{
    /// <summary>Employee ID to update.</summary>
    public Guid Id { get; init; }

    // User details
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;

    // Personal info
    public string? Phone { get; init; }
    public string? PhotoUrl { get; init; }
    public DateOnly? DateOfBirth { get; init; }

    // Organization
    public Guid? DepartmentId { get; init; }
    public Guid? DesignationId { get; init; }
    public Guid? ManagerId { get; init; }

    /// <summary>Access role (HR, Manager, or Employee).</summary>
    public Guid RoleId { get; init; }

    // Employment lifecycle
    public DateOnly JoinDate { get; init; }
    public DateOnly? ResignationDate { get; init; }
    public DateOnly? LastWorkingDay { get; init; }
    public EmployeeStatus Status { get; init; }
}

public record UpdateEmployeeResult(Guid Id, string EmployeeCode);
