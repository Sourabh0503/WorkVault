using MediatR;

namespace WorkVault.Application.Modules.Identity.Commands.SetPassword;

public record SetPasswordCommand(
    Guid Token,
    string Password
) : IRequest<SetPasswordResult?>;

public record SetPasswordResult(
    Guid UserId,
    Guid CompanyId,
    string AccessToken,
    string RefreshToken
);