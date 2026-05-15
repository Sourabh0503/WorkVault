using Microsoft.EntityFrameworkCore;
using WorkVault.Domain.Modules.Identity;
using WorkVault.Domain.Modules.Identity.Interfaces;
using WorkVault.Infrastructure.Persistence;

namespace WorkVault.Infrastructure.Modules.Identity;

public class InviteTokenRepository(AppDbContext context) : IInviteTokenRepository
{
    public async Task AddAsync(InviteToken inviteToken, CancellationToken cancellationToken)
    {
        await context.InviteTokens.AddAsync(inviteToken, cancellationToken);
        // No SaveChanges — handler controls via IUnitOfWork
    }

    public async Task<InviteToken?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.InviteTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<InviteToken?> GetByTokenAsync(Guid token, CancellationToken cancellationToken)
    {
        // Look up by the public Token field (not the Id field)
        return await context.InviteTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token, cancellationToken);
    }
}