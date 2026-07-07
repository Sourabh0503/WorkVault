using Microsoft.EntityFrameworkCore;
using WorkVault.Domain.Modules.Identity;
using WorkVault.Domain.Modules.Identity.Interfaces;
using WorkVault.Infrastructure.Persistence;

namespace WorkVault.Infrastructure.Modules.Identity;

/// <summary>
/// EF Core data access for <see cref="InviteToken"/> (employee invite links). Writes
/// stage changes only; the handler commits via <c>IUnitOfWork</c>.
/// </summary>
public class InviteTokenRepository(AppDbContext context) : IInviteTokenRepository
{
    /// <summary>Stages a new invite token for insertion.</summary>
    public async Task AddAsync(InviteToken inviteToken, CancellationToken cancellationToken)
    {
        await context.InviteTokens.AddAsync(inviteToken, cancellationToken);
        // No SaveChanges — handler controls via IUnitOfWork
    }

    /// <summary>Loads an invite token by primary key (user eager-loaded).</summary>
    public async Task<InviteToken?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.InviteTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    /// <summary>Loads an invite by its public <c>Token</c> value (user + role eager-loaded).</summary>
    public async Task<InviteToken?> GetByTokenAsync(Guid token, CancellationToken cancellationToken)
    {
        // Look up by the public Token field (not the Id field)
        return await context.InviteTokens
            .Include(t => t.User)
                .ThenInclude(user => user!.Role)
            .FirstOrDefaultAsync(t => t.Token == token, cancellationToken);
    }

    /// <summary>Returns the user's current unused, unexpired invite token, if any.</summary>
    public async Task<InviteToken?> GetActiveTokenForUserAsync(
        Guid userId, CancellationToken cancellationToken)
    {
        return await context.InviteTokens
            .Where(t => t.UserId == userId 
                        && t.UsedAt == null 
                        && t.ExpiresAt > DateTime.UtcNow)
            .FirstOrDefaultAsync(cancellationToken);
    }
}