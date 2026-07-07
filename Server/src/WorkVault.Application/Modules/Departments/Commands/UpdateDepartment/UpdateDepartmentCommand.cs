using MediatR;

namespace WorkVault.Application.Modules.Departments.Commands.UpdateDepartment;

/// <summary>Updates an existing department's details, parent, and head.</summary>
/// <param name="Id">The department to update.</param>
/// <param name="Name">New name (unique within the company).</param>
/// <param name="Description">Optional description.</param>
/// <param name="ParentDepartmentId">Optional parent (cannot be self or a descendant).</param>
/// <param name="HeadEmployeeId">Optional department head.</param>
public record UpdateDepartmentCommand(
    Guid Id,
    string Name,
    string? Description,
    Guid? ParentDepartmentId,
    Guid? HeadEmployeeId
) : IRequest<UpdateDepartmentResult>;

/// <summary>Result of updating a department.</summary>
/// <param name="Id">The department id.</param>
/// <param name="Name">The updated name.</param>
public record UpdateDepartmentResult(Guid Id, string Name);
