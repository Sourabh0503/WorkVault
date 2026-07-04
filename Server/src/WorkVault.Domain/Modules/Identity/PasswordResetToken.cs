using WorkVault.SharedKernel;

namespace WorkVault.Domain.Modules.Identity;

/// <summary>
/// One-time token issued when a user requests a password reset.
/// Similar to InviteToken but for existing users forgetting their password.
/// 
/// Security properties:
/// - Cryptographically random Guid (v7 — via BaseEntity generation pattern)
/// - Single-use (UsedAt marks consumption)
/// - Short expiry (2h — shorter than invite because it's more sensitive)
/// - Password reset revokes ALL refresh tokens for the user
/// </summary>
public class PasswordResetToken : BaseEntity
{
    /// <summary>
    /// Which user is resetting their password? Links to the User table.
    /// </summary>
    public Guid UserId { get; set; }
    public User? User { get; set; }

    /// <summary>
    /// The random token sent in the reset email URL.
    /// Different from this entity's Id — same design as InviteToken.
    /// The URL token can't be used to guess the DB record's primary key.
    /// </summary>
    public Guid Token { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Token expiry — 2 hours from creation.
    /// Short window minimizes attack surface if the reset link leaks
    /// (email forward, screenshot, browser history).
    /// Users who don't reset within 2h can request a new link.
    /// </summary>
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddHours(2);

    /// <summary>
    /// When the user clicked the link and successfully set a new password.
    /// Null = token is still active and unused.
    /// </summary>
    public DateTime? UsedAt { get; set; }

    /// <summary>
    /// Convenience computed property — true if token is still usable.
    /// Not stored in the DB (no setter, just a getter).
    /// </summary>
    public bool IsValid => UsedAt == null && DateTime.UtcNow < ExpiresAt;
}