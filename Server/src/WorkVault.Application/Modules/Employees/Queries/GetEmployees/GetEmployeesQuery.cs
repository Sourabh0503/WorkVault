using MediatR;
using WorkVault.Domain.Modules.Employees.Enums;
using WorkVault.SharedKernel.Models;

namespace WorkVault.Application.Modules.Employees.Queries.GetEmployees;

/// <summary>
/// Query to retrieve a paginated list of employees with optional filters.
/// </summary>
public record GetEmployeesQuery : IRequest<PagedResult<EmployeeListDto>>
{
    /// <summary>Page number (1-based). Default: 1</summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>Items per page. Default: 20, Max: 100</summary>
    public int PageSize { get; init; } = 20;

    /// <summary>Filter by department ID.</summary>
    public Guid? DepartmentId { get; init; }

    /// <summary>Filter by employee status.</summary>
    public EmployeeStatus? Status { get; init; }

    /// <summary>Filter by manager ID (direct reports).</summary>
    public Guid? ManagerId { get; init; }

    /// <summary>Search by name, email, or employee code.</summary>
    public string? Search { get; init; }
}

/// <summary>
/// Lightweight DTO for employee list views.
/// </summary>
public record EmployeeListDto(
    Guid Id,
    string EmployeeCode,
    string FullName,
    string Email,
    string? DepartmentName,
    string? DesignationTitle,
    EmployeeStatus Status,
    DateOnly JoinDate
);