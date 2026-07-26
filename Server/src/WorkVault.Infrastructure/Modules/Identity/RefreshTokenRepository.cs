using Microsoft.EntityFrameworkCore;
using WorkVault.Domain.Modules.Identity;
using WorkVault.Domain.Modules.Identity.Interfaces;
using WorkVault.Infrastructure.Persistence;

namespace WorkVault.Infrastructure.Modules.Identity;

/// <summary>
/// Repository implementation for <see cref="RefreshToken"/> operations.
/// </summary>
/// <remarks>
/// Security considerations:
/// - GetByTokenAsync only returns non-revoked tokens
/// - RevokeAllForUserAsync uses ExecuteUpdateAsync for performance
/// - Tokens include User and Role for JWT generation without extra queries
/// </remarks>
public class RefreshTokenRepository(AppDbContext context) : IRefreshTokenRepository
{
    /// <summary>Stages a new refresh token for insertion (committed later by the Unit of Work).</summary>
    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
    {
        await context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
    }

    /// <summary>Loads a non-revoked refresh token by its value (user + role eager-loaded for JWT generation).</summary>
    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken)
    {
        return await context.RefreshTokens
            .Include(rt => rt.User)
                .ThenInclude(u => u.Role)
            .FirstOrDefaultAsync(rt => rt.Token == token && !rt.IsRevoked, cancellationToken);
    }

    /// <summary>Marks a single refresh token as revoked (persisted by the Unit of Work).</summary>
    public Task RevokeAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
    {
        refreshToken.IsRevoked = true;
        return Task.CompletedTask;
    }

    /// <summary>Revokes all of a user's active refresh tokens in one round-trip (logout-everywhere / password change).</summary>
    public async Task RevokeAllForUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        await context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ExecuteUpdateAsync(s => s.SetProperty(rt => rt.IsRevoked, true), cancellationToken);
    }
}
