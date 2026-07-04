using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Domain.Modules.Identity.Interfaces;

public interface IPasswordResetTokenRepository : IRepository<PasswordResetToken>
{
    /// <summary>
    /// Look up a reset request by its public token (URL value).
    /// Loads User + Role — needed for the reset handler.
    /// </summary>
    Task<PasswordResetToken?> GetByTokenAsync(Guid token, CancellationToken cancellationToken);

    /// <summary>
    /// Find any live (unused, unexpired) reset token for a user.
    /// Used by forgot-password to invalidate stale requests before issuing new ones.
    /// </summary>
    Task<PasswordResetToken?> GetActiveTokenForUserAsync(Guid userId, CancellationToken cancellationToken);
}