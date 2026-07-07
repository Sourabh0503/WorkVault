using MediatR;

namespace WorkVault.Application.Modules.Designations.Commands.CreateDesignation;

/// <summary>Creates a designation (job title) within a department.</summary>
/// <param name="Title">Designation title (unique within the department).</param>
/// <param name="Level">Seniority level (1–10).</param>
/// <param name="DepartmentId">The department this designation belongs to.</param>
public record CreateDesignationCommand(
    string Title,
    int Level,
    Guid DepartmentId
) : IRequest<CreateDesignationResult>;

/// <summary>Result of creating a designation.</summary>
/// <param name="Id">The new designation id.</param>
/// <param name="Title">The designation title.</param>
public record CreateDesignationResult(Guid Id, string Title);
