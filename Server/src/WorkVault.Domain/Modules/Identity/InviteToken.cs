using WorkVault.SharedKernel;

namespace WorkVault.Domain.Modules.Identity;

/// <summary>
/// Represents an employee invitation token for onboarding.
/// </summary>
/// <remarks>
/// Lifecycle:
/// 1. HR creates employee → InviteToken created (48h expiry)
/// 2. Email sent with link containing Token GUID
/// 3. Employee clicks link → Token validated via GetByTokenAsync
/// 4. Employee sets password → Token marked with UsedAt
///
/// Security notes:
/// - Token field is separate from Id to prevent primary key enumeration
/// - Expired or used tokens return null from repository queries
/// - HR can resend invite to generate a new token
/// </remarks>
public class InviteToken : BaseEntity
{
    /// <summary>
    /// Which user is being invited? Links to the User created
    /// by HR when adding a new employee.
    /// </summary>
    public Guid UserId { get; set; }
    public User? User { get; set; }

    /// <summary>
    /// The random token sent in the invite email URL.
    /// Different from this entity's Id — so the URL token
    /// can't be used to guess the database record's primary key.
    /// </summary>
    public Guid Token { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Token expiry — 48 hours from creation by default.
    /// After this, the invite link stops working.
    /// HR can resend to generate a fresh token.
    /// </summary>
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddHours(48);

    /// <summary>
    /// When the employee clicked the link and successfully set their password.
    /// Null = token is still active and unused.
    /// </summary>
    public DateTime? UsedAt { get; set; }

    /// <summary>
    /// Convenience computed property — true if token is still valid.
    /// Not stored in DB (no setter on the calculation).
    /// </summary>
    public bool IsValid => UsedAt == null && DateTime.UtcNow < ExpiresAt;
}