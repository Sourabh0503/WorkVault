using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WorkVault.Application.Common.Interfaces;
using WorkVault.Application.Common.Settings;
using WorkVault.Domain.Modules.Identity;

namespace WorkVault.Infrastructure.Auth;

/// <summary>
/// Implementation of <see cref="IJwtTokenService"/> for JWT generation.
/// </summary>
/// <remarks>
/// Access tokens contain the following claims:
/// - NameIdentifier: User.Id
/// - Email: User.Email
/// - CompanyId: User.CompanyId (custom claim for tenant isolation)
/// - Role: Role name (for [Authorize(Roles = ...)])
/// - FirstName, LastName: For display purposes
///
/// Tokens are signed using HS256 (HMAC SHA-256) with the secret from JwtSettings.
/// </remarks>
public class JwtTokenService(IOptions<JwtSettings> settings) : IJwtTokenService
{
    private readonly JwtSettings _settings = settings.Value;
    
    public string GenerateAccessToken(User user, string roleName)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new("CompanyId", user.CompanyId.ToString()),
            new(ClaimTypes.Role, roleName),
            new("FirstName", user.FirstName),
            new("LastName", user.LastName)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_settings.SecretKey));

        var credentials = new SigningCredentials(
            key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                _settings.AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}