using MediatR;

namespace WorkVault.Application.Modules.Departments.Commands.CreateDepartment;

/// <summary>Creates a department within the current company.</summary>
/// <param name="Name">Department name (unique within the company).</param>
/// <param name="Description">Optional description.</param>
/// <param name="ParentDepartmentId">Optional parent department (for a hierarchy).</param>
/// <param name="HeadEmployeeId">Optional department head.</param>
public record CreateDepartmentCommand(
    string Name,
    string? Description,
    Guid? ParentDepartmentId,
    Guid? HeadEmployeeId
) : IRequest<CreateDepartmentResult>;

/// <summary>Result of creating a department.</summary>
/// <param name="Id">The new department id.</param>
/// <param name="Name">The department name.</param>
public record CreateDepartmentResult(Guid Id, string Name);
