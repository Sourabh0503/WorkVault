using MediatR;

namespace WorkVault.Application.Modules.Identity.Commands.RefreshTokens;

/// <summary>Exchanges a valid refresh token for a fresh access + refresh token pair.</summary>
/// <param name="RefreshToken">The current (unexpired, unrevoked) refresh token.</param>
public record RefreshTokenCommand(
    string RefreshToken
) : IRequest<RefreshTokenResponse?>;

/// <summary>The rotated token pair returned by a successful refresh.</summary>
/// <param name="AccessToken">New short-lived JWT access token.</param>
/// <param name="RefreshToken">New refresh token (the old one is rotated out).</param>
public record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken
);
