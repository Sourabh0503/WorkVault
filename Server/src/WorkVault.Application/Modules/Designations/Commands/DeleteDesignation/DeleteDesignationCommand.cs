using MediatR;

namespace WorkVault.Application.Modules.Designations.Commands.DeleteDesignation;

/// <summary>Soft-deletes a designation (only when no employees are assigned to it).</summary>
/// <param name="Id">The designation to delete.</param>
public record DeleteDesignationCommand(Guid Id) : IRequest;
