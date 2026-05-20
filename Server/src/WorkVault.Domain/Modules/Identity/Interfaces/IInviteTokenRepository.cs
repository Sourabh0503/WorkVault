using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Domain.Modules.Identity.Interfaces;

public interface IInviteTokenRepository : IRepository<InviteToken>
{
    /// <summary>
    /// Look up an invitation by its public token (the value sent in the email URL).
    /// Used by the set-password endpoint to validate before letting the user
    /// set their password.
    /// </summary>
    Task<InviteToken?> GetByTokenAsync(Guid token, CancellationToken cancellationToken);
    
    Task<InviteToken?> GetActiveTokenForUserAsync(Guid userId, CancellationToken cancellationToken);
}