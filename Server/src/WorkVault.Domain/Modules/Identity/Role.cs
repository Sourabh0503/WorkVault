using WorkVault.SharedKernel;

namespace WorkVault.Domain.Modules.Identity;

/// <summary>
/// Represents a role that can be assigned to users for authorization.
/// </summary>
/// <remarks>
/// System roles (SuperAdmin, CompanyAdmin, HR, Manager, Employee) are seeded
/// in the database and cannot be deleted. Companies can create custom roles
/// with <see cref="IsSystemRole"/> = false.
///
/// Use <see cref="SharedKernel.Constants.SystemRoles"/> constants when referencing
/// system role IDs or names instead of hardcoding values.
/// </remarks>
public class Role : BaseEntity
{
    /// <summary>
    /// Display name of the role (e.g., "SuperAdmin", "HR", "Employee").
    /// Used in JWT claims and [Authorize] attribute checks.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if this is a system-defined role that cannot be deleted.
    /// True for the 5 default roles, false for company-created custom roles.
    /// </summary>
    public bool IsSystemRole { get; set; } = true;
}