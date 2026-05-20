using MediatR;

namespace WorkVault.Application.Modules.Employees.Commands.ResendInvite;

public record ResendInviteCommand(Guid EmployeeId) : IRequest<ResendInviteResult>;

public record ResendInviteResult(
    Guid EmployeeId,
    string Email,
    string InviteLink
);