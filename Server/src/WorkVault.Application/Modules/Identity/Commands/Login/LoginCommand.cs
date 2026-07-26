using MediatR;

namespace WorkVault.Application.Modules.Identity.Commands.Login;

/// <summary>Credentials submitted to authenticate a user.</summary>
/// <param name="Email">The user's login email.</param>
/// <param name="Password">The plaintext password (verified against the BCrypt hash).</param>
public record LoginCommand(
    string Email,
    string Password
) : IRequest<LoginResponse?>;

/// <summary>Tokens and identifiers returned after a successful login.</summary>
/// <param name="UserId">The authenticated user's id.</param>
/// <param name="CompanyId">The tenant the user belongs to.</param>
/// <param name="CompanyName">Display name of the tenant, for UI branding.</param>
/// <param name="AccessToken">Short-lived JWT access token.</param>
/// <param name="RefreshToken">Long-lived refresh token used to mint new access tokens.</param>
public record LoginResponse(
    Guid UserId,
    Guid CompanyId,
    string CompanyName,
    string AccessToken,
    string RefreshToken
);
