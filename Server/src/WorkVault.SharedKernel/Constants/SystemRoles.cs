namespace WorkVault.SharedKernel.Constants;

public enum RoleType
{
    SuperAdmin,
    CompanyAdmin,
    HR,
    Manager,
    Employee
}

public static class SystemRoles
{
    // Role IDs
    public static readonly Guid SuperAdmin = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid CompanyAdmin = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid HR = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid Manager = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid Employee = Guid.Parse("55555555-5555-5555-5555-555555555555");

    // Role Names (for use in [Authorize] attributes)
    public const string SuperAdminRole = nameof(RoleType.SuperAdmin);
    public const string CompanyAdminRole = nameof(RoleType.CompanyAdmin);
    public const string HRRole = nameof(RoleType.HR);
    public const string ManagerRole = nameof(RoleType.Manager);
    public const string EmployeeRole = nameof(RoleType.Employee);

    public static Guid GetId(RoleType role) => role switch
    {
        RoleType.SuperAdmin => SuperAdmin,
        RoleType.CompanyAdmin => CompanyAdmin,
        RoleType.HR => HR,
        RoleType.Manager => Manager,
        RoleType.Employee => Employee,
        _ => throw new ArgumentOutOfRangeException(nameof(role))
    };

    public static string GetName(RoleType role) => role.ToString();

    public static RoleType? FromId(Guid id)
    {
        if (id == SuperAdmin) return RoleType.SuperAdmin;
        if (id == CompanyAdmin) return RoleType.CompanyAdmin;
        if (id == HR) return RoleType.HR;
        if (id == Manager) return RoleType.Manager;
        if (id == Employee) return RoleType.Employee;
        return null;
    }
}
