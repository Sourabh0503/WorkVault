namespace WorkVault.Domain.Modules.Identity.Interfaces;

/// <summary>
/// Repository interface for <see cref="RefreshToken"/> entity operations.
/// </summary>
/// <remarks>
/// Does not extend IRepository because RefreshToken doesn't extend BaseEntity.
/// </remarks>
public interface IRefreshTokenRepository
{
    /// <summary>
    /// Adds a new refresh token to the database.
    /// </summary>
    /// <param name="refreshToken">The refresh token to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken);

    /// <summary>
    /// Finds a refresh token by its value.
    /// </summary>
    /// <param name="token">The token string to search for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The token with User and Role included, or null if not found/revoked.</returns>
    /// <remarks>
    /// Only returns non-revoked tokens. Includes User.Role for JWT generation.
    /// </remarks>
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken);

    /// <summary>
    /// Marks a refresh token as revoked (soft delete for tokens).
    /// </summary>
    /// <param name="refreshToken">The token to revoke.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <remarks>
    /// Used during token rotation and logout. Revoked tokens cannot be used.
    /// </remarks>
    Task RevokeAsync(RefreshToken refreshToken, CancellationToken cancellationToken);

    /// <summary>
    /// Revokes all active refresh tokens for a user.
    /// </summary>
    /// <param name="userId">The user whose tokens should be revoked.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <remarks>
    /// Use when user changes password or is deactivated to force re-authentication
    /// on all devices.
    /// </remarks>
    Task RevokeAllForUserAsync(Guid userId, CancellationToken cancellationToken);
}
