namespace WorkVault.Application.Common.Settings;

/// <summary>
/// Configuration settings for JWT authentication.
/// Bound from appsettings.json "JwtSettings" section.
/// </summary>
/// <remarks>
/// Example configuration:
/// <code>
/// "JwtSettings": {
///     "SecretKey": "your-secret-key-at-least-32-characters-long",
///     "Issuer": "WorkVault",
///     "Audience": "WorkVault",
///     "AccessTokenExpirationMinutes": 15,
///     "RefreshTokenExpirationDays": 7
/// }
/// </code>
/// </remarks>
public class JwtSettings
{
    /// <summary>
    /// Secret key for signing JWT tokens (HS256).
    /// Must be at least 32 characters for security.
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// The issuer claim (iss) for generated tokens.
    /// Validated during token verification.
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// The audience claim (aud) for generated tokens.
    /// Validated during token verification.
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Lifetime of access tokens in minutes.
    /// Recommended: 15 minutes for security.
    /// </summary>
    public int AccessTokenExpirationMinutes { get; set; }

    /// <summary>
    /// Lifetime of refresh tokens in days.
    /// Recommended: 7 days. Tokens are rotated on each use.
    /// </summary>
    public int RefreshTokenExpirationDays { get; set; }
}