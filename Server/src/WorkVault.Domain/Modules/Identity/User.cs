using WorkVault.SharedKernel;

namespace WorkVault.Domain.Modules.Identity;

/// <summary>
/// Represents an authenticated user in the system.
/// </summary>
/// <remarks>
/// Users are linked to exactly one company (tenant) via <see cref="BaseEntity.CompanyId"/>.
/// Each user has exactly one role, and optionally links to an <see cref="Employees.Employee"/> record.
///
/// User lifecycle:
/// 1. Created by HR via employee invite (IsActive=false, no password)
/// 2. User accepts invite and sets password (IsActive=true)
/// 3. Or: Created during company registration (IsActive=true, has password)
///
/// Unique constraint: (Email, CompanyId) - same email can exist in different companies.
/// </remarks>
public class User : BaseEntity
{
    private string _email = string.Empty;

    /// <summary>
    /// User's email address. Used for login and communication.
    /// Stored trimmed and lowercased so lookups and the (Email, CompanyId) uniqueness
    /// constraint are effectively case-insensitive. Unique per company.
    /// </summary>
    public string Email
    {
        get => _email;
        set => _email = value?.Trim().ToLowerInvariant() ?? string.Empty;
    }

    /// <summary>
    /// BCrypt-hashed password. Empty string for users who haven't set password yet
    /// (pending invite acceptance).
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>User's first/given name.</summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>User's last/family name.</summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>Foreign key to the user's assigned role.</summary>
    public Guid RoleId { get; set; }

    /// <summary>Navigation property to the user's role (always loaded via Include).</summary>
    public Role Role { get; set; } = null!;

    /// <summary>
    /// Whether the user can log in. False for:
    /// - New employees who haven't accepted invite
    /// - Manually deactivated users
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// UTC timestamp of the user's most recent successful login.
    /// Null if user has never logged in.
    /// </summary>
    public DateTime? LastLogin { get; set; }
}