using WorkVault.Domain.Modules.Identity;

namespace WorkVault.Application.Common.Interfaces;

/// <summary>
/// Service for generating JWT access tokens and refresh tokens.
/// </summary>
/// <remarks>
/// Implemented by JwtTokenService in the Infrastructure layer.
/// Configuration values come from JwtSettings in appsettings.json.
/// </remarks>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a signed JWT access token containing user claims.
    /// </summary>
    /// <param name="user">The user to generate the token for.</param>
    /// <param name="roleName">The user's role name to include in claims.</param>
    /// <returns>A signed JWT string.</returns>
    /// <remarks>
    /// Token includes claims: NameIdentifier (UserId), Email, CompanyId,
    /// Role, FirstName, LastName.
    /// Expires after JwtSettings.AccessTokenExpirationMinutes.
    /// </remarks>
    string GenerateAccessToken(User user, string roleName);

    /// <summary>
    /// Generates a cryptographically secure random refresh token.
    /// </summary>
    /// <returns>A base64-encoded string (64 random bytes).</returns>
    /// <remarks>
    /// Refresh tokens are stored in the database and used to obtain
    /// new access tokens without re-authentication.
    /// </remarks>
    string GenerateRefreshToken();
}