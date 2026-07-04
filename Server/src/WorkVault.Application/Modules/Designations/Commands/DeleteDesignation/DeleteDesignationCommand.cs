using MediatR;

namespace WorkVault.Application.Modules.Designations.Commands.DeleteDesignation;

public record DeleteDesignationCommand(Guid Id) : IRequest;
