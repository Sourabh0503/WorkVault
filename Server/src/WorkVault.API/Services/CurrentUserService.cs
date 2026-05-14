using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using WorkVault.Application.Common.Interfaces;
using WorkVault.SharedKernel.Constants;

namespace WorkVault.API.Services;

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