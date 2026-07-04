using MediatR;

namespace WorkVault.Application.Modules.Designations.Commands.CreateDesignation;

public record CreateDesignationCommand(
    string Title,
    int Level,
    Guid DepartmentId
) : IRequest<CreateDesignationResult>;

public record CreateDesignationResult(Guid Id, string Title);
