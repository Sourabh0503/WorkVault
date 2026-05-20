using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using WorkVault.Application.Common.Interfaces;
using WorkVault.SharedKernel.Constants;

namespace WorkVault.API.Services;

/// <summary>
/// Extracts current user information from JWT claims in the HTTP request.
/// </summary>
/// <remarks>
/// This service reads from HttpContext.User (populated by JWT middleware).
/// Injected into AppDbContext for:
/// - Automatic tenant filtering (CompanyId)
/// - Audit field population (CreatedBy)
///
/// All properties return null for unauthenticated requests.
/// </remarks>
public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{

    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var value = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public Guid? CompanyId
    {
        get
        {
            var value = User?.FindFirstValue("CompanyId");
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public string? Role => User?.FindFirstValue(ClaimTypes.Role);

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public bool IsSuperAdmin => Role == SystemRoles.SuperAdminRole;
}