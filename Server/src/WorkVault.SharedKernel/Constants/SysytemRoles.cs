namespace WorkVault.SharedKernel.Constants;

public static class SystemRoles
{
    public static readonly Guid SuperAdmin = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid CompanyAdmin = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid HR = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid Manager = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid Employee = Guid.Parse("55555555-5555-5555-5555-555555555555");
}