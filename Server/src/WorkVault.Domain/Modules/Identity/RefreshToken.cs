namespace WorkVault.Domain.Modules.Identity;

/// <summary>
/// Represents a refresh token used for JWT token rotation.
/// </summary>
/// <remarks>
/// Note: This entity does NOT extend BaseEntity because:
/// - Refresh tokens are tied to users, not directly to companies
/// - They don't need soft delete (revoked tokens stay for audit)
/// - Cascade delete handles cleanup when user is deleted
///
/// Security considerations:
/// - Tokens are currently stored in plain text (TODO: hash before storage)
/// - Token rotation: old token is revoked when new one is issued
/// - Multiple devices = multiple active tokens per user
/// </remarks>
public class RefreshToken
{
    /// <summary>Primary key for the refresh token record.</summary>
    public Guid Id { get; set; }

    /// <summary>Foreign key to the user who owns this token.</summary>
    public Guid UserId { get; set; }

    /// <summary>Navigation property to the owning user.</summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// The actual refresh token string (64 random bytes, base64 encoded).
    /// Sent to client and used to obtain new access tokens.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// UTC timestamp when this token expires.
    /// Configured via JwtSettings.RefreshTokenExpirationDays (default: 7 days).
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Whether this token has been revoked (logout or token rotation).
    /// Revoked tokens cannot be used even if not expired.
    /// </summary>
    public bool IsRevoked { get; set; } = false;
}