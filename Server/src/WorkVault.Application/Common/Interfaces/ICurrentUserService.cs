namespace WorkVault.Application.Common.Interfaces;

/// <summary>
/// Provides access to the current authenticated user's information.
/// Values are extracted from JWT claims in the HTTP request.
/// </summary>
/// <remarks>
/// Implemented by CurrentUserService in the API layer.
/// Injected into AppDbContext for automatic tenant filtering and audit fields.
///
/// All properties return null for unauthenticated requests (public endpoints).
/// </remarks>
public interface ICurrentUserService
{
    /// <summary>
    /// The authenticated user's ID (from JWT NameIdentifier claim).
    /// Null for unauthenticated requests.
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// The user's company/tenant ID (from JWT CompanyId claim).
    /// Used by EF Core global query filters for tenant isolation.
    /// Null for unauthenticated requests.
    /// </summary>
    Guid? CompanyId { get; }

    /// <summary>
    /// The user's role name (from JWT Role claim).
    /// Values: "SuperAdmin", "CompanyAdmin", "HR", "Manager", "Employee".
    /// </summary>
    string? Role { get; }

    /// <summary>
    /// Whether the current request has a valid JWT token.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Whether the current user is a SuperAdmin (platform-wide access).
    /// SuperAdmins bypass tenant filters and can see all companies.
    /// </summary>
    bool IsSuperAdmin { get; }
}