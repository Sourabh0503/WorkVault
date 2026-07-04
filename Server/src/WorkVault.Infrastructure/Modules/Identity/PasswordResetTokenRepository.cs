using Microsoft.EntityFrameworkCore;
using WorkVault.Domain.Modules.Identity;
using WorkVault.Domain.Modules.Identity.Interfaces;
using WorkVault.Infrastructure.Persistence;

namespace WorkVault.Infrastructure.Modules.Identity;

public class PasswordResetTokenRepository(AppDbContext context) : IPasswordResetTokenRepository
{
    public async Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken)
    {
        await context.PasswordResetTokens.AddAsync(token, cancellationToken);
        // No SaveChanges — handler drives IUnitOfWork
    }

    public async Task<PasswordResetToken?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.PasswordResetTokens
            .Include(t => t.User)
            .ThenInclude(u => u!.Role)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<PasswordResetToken?> GetByTokenAsync(Guid token, CancellationToken cancellationToken)
    {
        return await context.PasswordResetTokens
            .Include(t => t.User)
            .ThenInclude(u => u!.Role)
            .FirstOrDefaultAsync(t => t.Token == token, cancellationToken);
    }

    public async Task<PasswordResetToken?> GetActiveTokenForUserAsync(
        Guid userId, CancellationToken cancellationToken)
    {
        return await context.PasswordResetTokens
            .Where(t => t.UserId == userId
                        && t.UsedAt == null
                        && t.ExpiresAt > DateTime.UtcNow)
            .FirstOrDefaultAsync(cancellationToken);
    }
}