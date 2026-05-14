namespace WorkVault.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    Guid? CompanyId { get; }   // ← filter needs
    string? Role { get; }
    bool IsAuthenticated { get; }
    bool IsSuperAdmin { get; } // SuperAdmins can see ALL companies
}