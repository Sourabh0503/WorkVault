using WorkVault.Domain.Modules.Employees.Enums;

namespace WorkVault.Application.Modules.Employees.Queries.GetEmployeeById;

/// <summary>
/// Data transfer object for employee details.
/// </summary>
public record EmployeeDto(
    Guid Id,
    Guid UserId,
    string EmployeeCode,
    string Email,
    string FirstName,
    string LastName,
    string? Phone,
    string? PhotoUrl,
    DateOnly? DateOfBirth,
    DateOnly JoinDate,
    DateOnly? ResignationDate,
    DateOnly? LastWorkingDay,
    EmployeeStatus Status,
    DepartmentInfo? Department,
    DesignationInfo? Designation,
    ManagerInfo? Manager,
    RoleInfo Role,
    DateTime CreatedAt
);

/// <summary>The employee's department (id + name).</summary>
public record DepartmentInfo(Guid Id, string Name);

/// <summary>The employee's designation (id, title, seniority level).</summary>
public record DesignationInfo(Guid Id, string Title, int Level);

/// <summary>The employee's reporting manager (id, code, name).</summary>
public record ManagerInfo(Guid Id, string EmployeeCode, string FullName);

/// <summary>The employee's access role (id + name).</summary>
public record RoleInfo(Guid Id, string Name);
