using MediatR;

namespace WorkVault.Application.Modules.Employees.Commands.ResendInvite;

/// <summary>Reissues an invite link for a still-pending employee (revoking the previous one).</summary>
/// <param name="EmployeeId">The pending employee to re-invite.</param>
public record ResendInviteCommand(Guid EmployeeId) : IRequest<ResendInviteResult>;

/// <summary>Result of resending an invite.</summary>
/// <param name="EmployeeId">The employee's id.</param>
/// <param name="Email">The email the invite targets.</param>
/// <param name="InviteLink">The freshly generated invite link.</param>
public record ResendInviteResult(
    Guid EmployeeId,
    string Email,
    string InviteLink
);