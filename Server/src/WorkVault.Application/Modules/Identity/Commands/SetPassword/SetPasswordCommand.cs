using MediatR;

namespace WorkVault.Application.Modules.Identity.Commands.SetPassword;

/// <summary>
/// Accepts an invite: sets the invited user's first password, activates the
/// account, and auto-logs them in.
/// </summary>
/// <param name="Token">The invite token (from the invite link).</param>
/// <param name="Password">The plaintext password to set (hashed before storage).</param>
public record SetPasswordCommand(
    Guid Token,
    string Password
) : IRequest<SetPasswordResult?>;

/// <summary>Identifiers and tokens returned after the invite is accepted (auto-login).</summary>
/// <param name="UserId">The activated user's id.</param>
/// <param name="CompanyId">The tenant the user belongs to.</param>
/// <param name="CompanyName">Display name of the tenant, for UI branding.</param>
/// <param name="AccessToken">Short-lived JWT access token.</param>
/// <param name="RefreshToken">Long-lived refresh token.</param>
public record SetPasswordResult(
    Guid UserId,
    Guid CompanyId,
    string CompanyName,
    string AccessToken,
    string RefreshToken
);