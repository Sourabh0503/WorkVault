using WorkVault.Domain.Modules.Identity;

namespace WorkVault.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user, string roleName);
    string GenerateRefreshToken();
}