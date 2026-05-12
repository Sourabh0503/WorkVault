using MediatR;

namespace WorkVault.Application.Modules.Identity.Commands.RefreshTokens;

public record RefreshTokenCommand(
    string RefreshToken
) : IRequest<RefreshTokenResponse?>;

public record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken
);
