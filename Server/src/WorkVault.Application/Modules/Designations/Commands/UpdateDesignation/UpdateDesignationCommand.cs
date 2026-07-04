using MediatR;

namespace WorkVault.Application.Modules.Designations.Commands.UpdateDesignation;

public record UpdateDesignationCommand(
    Guid Id,
    string Title,
    int Level,
    Guid DepartmentId
) : IRequest<UpdateDesignationResult>;

public record UpdateDesignationResult(Guid Id, string Title);
