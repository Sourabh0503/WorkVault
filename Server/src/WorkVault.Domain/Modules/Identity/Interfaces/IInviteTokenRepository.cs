using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Domain.Modules.Identity.Interfaces;

/// <summary>
/// Repository interface for <see cref="InviteToken"/> entity operations.
/// </summary>
/// <remarks>
/// Invite tokens are used in the employee onboarding flow:
/// 1. HR creates employee → InviteToken generated
/// 2. Employee receives email with link containing token
/// 3. Employee clicks link → token validated
/// 4. Employee sets password → token marked as used
/// </remarks>
public interface IInviteTokenRepository : IRepository<InviteToken>
{
    /// <summary>
    /// Finds an invite token by its public token value (sent in email URL).
    /// </summary>
    /// <param name="token">The GUID token from the invite URL.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// The invite token with User and User.Role included, or null if not found.
    /// </returns>
    /// <remarks>
    /// Note: This looks up by the Token field, not the Id field.
    /// The Token is a separate GUID to prevent ID enumeration attacks.
    /// </remarks>
    Task<InviteToken?> GetByTokenAsync(Guid token, CancellationToken cancellationToken);

    /// <summary>
    /// Finds an active (unused, non-expired) invite token for a user.
    /// </summary>
    /// <param name="userId">The user ID to search for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The active token if found, or null.</returns>
    /// <remarks>
    /// Used by resend-invite to check if there's already a valid token
    /// (to invalidate it before creating a new one).
    /// </remarks>
    Task<InviteToken?> GetActiveTokenForUserAsync(Guid userId, CancellationToken cancellationToken);
}