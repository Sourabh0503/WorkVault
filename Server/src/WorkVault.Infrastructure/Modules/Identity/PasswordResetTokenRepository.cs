using Microsoft.EntityFrameworkCore;
using WorkVault.Domain.Modules.Identity;
using WorkVault.Domain.Modules.Identity.Interfaces;
using WorkVault.Infrastructure.Persistence;

namespace WorkVault.Infrastructure.Modules.Identity;

/// <summary>
/// EF Core data access for <see cref="PasswordResetToken"/>. Writes stage changes only;
/// the handler commits via <c>IUnitOfWork</c>.
/// </summary>
public class PasswordResetTokenRepository(AppDbContext context) : IPasswordResetTokenRepository
{
    /// <summary>Stages a new password-reset token for insertion.</summary>
    public async Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken)
    {
        await context.PasswordResetTokens.AddAsync(token, cancellationToken);
        // No SaveChanges — handler drives IUnitOfWork
    }

    /// <summary>Loads a reset token by primary key (user + role eager-loaded).</summary>
    public async Task<PasswordResetToken?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.PasswordResetTokens
            .Include(t => t.User)
            .ThenInclude(u => u!.Role)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    /// <summary>Loads a reset token by its public <c>Token</c> value (user + role eager-loaded).</summary>
    public async Task<PasswordResetToken?> GetByTokenAsync(Guid token, CancellationToken cancellationToken)
    {
        return await context.PasswordResetTokens
            .Include(t => t.User)
            .ThenInclude(u => u!.Role)
            .FirstOrDefaultAsync(t => t.Token == token, cancellationToken);
    }

    /// <summary>Returns the user's current unused, unexpired reset token, if any.</summary>
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