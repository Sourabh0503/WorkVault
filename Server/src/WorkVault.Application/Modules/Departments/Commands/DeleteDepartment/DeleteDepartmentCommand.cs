using MediatR;

namespace WorkVault.Application.Modules.Departments.Commands.DeleteDepartment;

/// <summary>Soft-deletes a department (only when it has no members or sub-departments).</summary>
/// <param name="Id">The department to delete.</param>
public record DeleteDepartmentCommand(Guid Id) : IRequest;
