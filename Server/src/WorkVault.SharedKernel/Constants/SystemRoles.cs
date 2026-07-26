namespace WorkVault.SharedKernel.Constants;

/// <summary>
/// Enumeration of system-defined roles.
/// These roles are seeded in the database and cannot be deleted.
/// </summary>
public enum RoleType
{
    /// <summary>Platform-wide administrator with access to all companies.</summary>
    SuperAdmin,
    /// <summary>Administrator for a single company/tenant.</summary>
    CompanyAdmin,
    /// <summary>Human Resources role - can manage employees, departments.</summary>
    HR,
    /// <summary>Team manager - can view and manage their direct reports.</summary>
    Manager,
    /// <summary>Standard employee with basic access.</summary>
    Employee
}

/// <summary>
/// Provides constants and utilities for system-defined roles.
/// Use these constants instead of magic strings/GUIDs throughout the codebase.
/// </summary>
/// <remarks>
/// Roles are seeded in the database with fixed GUIDs (see AppDbContext.SeedRolesData).
///
/// Usage examples:
/// <code>
/// // In [Authorize] attributes:
/// [Authorize(Roles = SystemRoles.HRRole)]
/// [Authorize(Roles = $"{SystemRoles.HRRole},{SystemRoles.CompanyAdminRole}")]
///
/// // When assigning roles to users:
/// user.RoleId = SystemRoles.Employee;
/// </code>
/// </remarks>
public static class SystemRoles
{
    #region Role IDs (Guids for database operations)

    /// <summary>SuperAdmin role ID - platform-wide access.</summary>
    public static readonly Guid SuperAdmin = Guid.Parse("11111111-1111-1111-1111-111111111111");

    /// <summary>CompanyAdmin role ID - company-wide access.</summary>
    public static readonly Guid CompanyAdmin = Guid.Parse("22222222-2222-2222-2222-222222222222");

    /// <summary>HR role ID - employee management access.</summary>
    public static readonly Guid HR = Guid.Parse("33333333-3333-3333-3333-333333333333");

    /// <summary>Manager role ID - team management access.</summary>
    public static readonly Guid Manager = Guid.Parse("44444444-4444-4444-4444-444444444444");

    /// <summary>Employee role ID - basic employee access.</summary>
    public static readonly Guid Employee = Guid.Parse("55555555-5555-5555-5555-555555555555");

    #endregion

    #region Role Names (strings for [Authorize] attributes)

    /// <summary>SuperAdmin role name for authorization attributes.</summary>
    public const string SuperAdminRole = nameof(RoleType.SuperAdmin);

    /// <summary>CompanyAdmin role name for authorization attributes.</summary>
    public const string CompanyAdminRole = nameof(RoleType.CompanyAdmin);

    /// <summary>HR role name for authorization attributes.</summary>
    public const string HRRole = nameof(RoleType.HR);

    /// <summary>Manager role name for authorization attributes.</summary>
    public const string ManagerRole = nameof(RoleType.Manager);

    /// <summary>Employee role name for authorization attributes.</summary>
    public const string EmployeeRole = nameof(RoleType.Employee);

    #endregion

    #region Utility Methods

    /// <summary>
    /// Gets the database GUID for a role type.
    /// </summary>
    /// <param name="role">The role type enum value.</param>
    /// <returns>The corresponding GUID used in the database.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if role is not a valid RoleType.</exception>
    public static Guid GetId(RoleType role) => role switch
    {
        RoleType.SuperAdmin => SuperAdmin,
        RoleType.CompanyAdmin => CompanyAdmin,
        RoleType.HR => HR,
        RoleType.Manager => Manager,
        RoleType.Employee => Employee,
        _ => throw new ArgumentOutOfRangeException(nameof(role))
    };

    /// <summary>
    /// Gets the string name for a role type.
    /// </summary>
    /// <param name="role">The role type enum value.</param>
    /// <returns>The role name as a string.</returns>
    public static string GetName(RoleType role) => role.ToString();

    /// <summary>
    /// Converts a role GUID back to its enum representation.
    /// </summary>
    /// <param name="id">The role GUID from the database.</param>
    /// <returns>The corresponding RoleType, or null if not a system role.</returns>
    public static RoleType? FromId(Guid id)
    {
        if (id == SuperAdmin) return RoleType.SuperAdmin;
        if (id == CompanyAdmin) return RoleType.CompanyAdmin;
        if (id == HR) return RoleType.HR;
        if (id == Manager) return RoleType.Manager;
        if (id == Employee) return RoleType.Employee;
        return null;
    }

    /// <summary>
    /// Roles HR/CompanyAdmin may assign when creating or editing an employee.
    /// Excludes <see cref="SuperAdmin"/> (platform-level) and <see cref="CompanyAdmin"/>
    /// (the founder, established at company registration — never granted via the employee form).
    /// </summary>
    public static readonly IReadOnlySet<Guid> AssignableEmployeeRoles =
        new HashSet<Guid> { HR, Manager, Employee };

    #endregion
}
