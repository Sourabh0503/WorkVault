using Microsoft.EntityFrameworkCore;
using WorkVault.Domain.Modules.Identity;
using WorkVault.Domain.Modules.Identity.Interfaces;
using WorkVault.Infrastructure.Persistence;

namespace WorkVault.Infrastructure.Modules.Identity;

public class RefreshTokenRepository(AppDbContext context) : IRefreshTokenRepository
{
    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
    {
        await context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken)
    {
        return await context.RefreshTokens
            .Include(rt => rt.User)
                .ThenInclude(u => u.Role)
            .FirstOrDefaultAsync(rt => rt.Token == token && !rt.IsRevoked, cancellationToken);
    }

    public Task RevokeAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
    {
        refreshToken.IsRevoked = true;
        return Task.CompletedTask;
    }

    public async Task RevokeAllForUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        await context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ExecuteUpdateAsync(s => s.SetProperty(rt => rt.IsRevoked, true), cancellationToken);
    }
}
