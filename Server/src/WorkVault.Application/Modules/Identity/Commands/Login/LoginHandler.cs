using MediatR;
using Microsoft.Extensions.Options;
using WorkVault.Application.Common.Interfaces;
using WorkVault.Application.Common.Settings;
using WorkVault.Domain.Modules.Identity;
using WorkVault.Domain.Modules.Identity.Interfaces;
using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Application.Modules.Identity.Commands.Login;

/// <summary>
/// Handles user authentication via email and password.
/// </summary>
/// <remarks>
/// Login flow:
/// 1. Find user by email (includes Role via eager loading)
/// 2. Verify user exists and is active
/// 3. Verify password with BCrypt
/// 4. Generate JWT access token and refresh token
/// 5. Update user's LastLogin timestamp
/// 6. Save refresh token to database
///
/// Returns null for invalid credentials (controller returns 401).
/// This is a public endpoint - no authentication required.
/// </remarks>
public class LoginHandler(
    IUserRepository userRepository,
    ICompanyRepository companyRepository,
    IUnitOfWork unitOfWork,
    IJwtTokenService jwtTokenService,
    IRefreshTokenRepository refreshTokenRepository,
    IOptions<JwtSettings> jwtSettings)
    : IRequestHandler<LoginCommand, LoginResponse?>
{
    public async Task<LoginResponse?> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null || !user.IsActive)
            return null;

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        // Generate tokens
        var accessToken = jwtTokenService.GenerateAccessToken(user, user.Role.Name);
        var refreshTokenString = jwtTokenService.GenerateRefreshToken();

        // Save refresh token to database
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddDays(jwtSettings.Value.RefreshTokenExpirationDays)
        };
        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        user.LastLogin = DateTime.UtcNow; // Update last login
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Resolve the tenant display name for UI branding. An authenticated user always
        // has a valid CompanyId (FK), and at login the tenant filter is permissive (no
        // CompanyId claim yet) — so a null here means a corrupt tenant reference. Fail loud (500).
        var company = await companyRepository.GetByIdAsync(user.CompanyId, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Company '{user.CompanyId}' not found for authenticated user '{user.Id}'.");

        return new LoginResponse(
            user.Id,
            user.CompanyId,
            company.Name,
            accessToken,
            refreshTokenString);
    }
}
