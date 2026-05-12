using MediatR;

namespace WorkVault.Application.Modules.Identity.Commands.Login;

public record LoginCommand(
    string Email,
    string Password
) : IRequest<LoginResponse?>;

public record LoginResponse(
    Guid UserId,
    Guid CompanyId,
    string AccessToken,
    string RefreshToken
);
