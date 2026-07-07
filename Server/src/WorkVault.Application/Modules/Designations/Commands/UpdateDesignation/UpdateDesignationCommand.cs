using MediatR;

namespace WorkVault.Application.Modules.Designations.Commands.UpdateDesignation;

/// <summary>Updates a designation's title, level, and department.</summary>
/// <param name="Id">The designation to update.</param>
/// <param name="Title">New title (unique within the department).</param>
/// <param name="Level">Seniority level (1–10).</param>
/// <param name="DepartmentId">The department this designation belongs to.</param>
public record UpdateDesignationCommand(
    Guid Id,
    string Title,
    int Level,
    Guid DepartmentId
) : IRequest<UpdateDesignationResult>;

/// <summary>Result of updating a designation.</summary>
/// <param name="Id">The designation id.</param>
/// <param name="Title">The updated title.</param>
public record UpdateDesignationResult(Guid Id, string Title);
