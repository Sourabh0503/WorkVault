using MediatR;
using Microsoft.Extensions.Options;
using WorkVault.Application.Common.Exceptions;
using WorkVault.Application.Common.Interfaces;
using WorkVault.Application.Common.Settings;
using WorkVault.Domain.Modules.Identity;
using WorkVault.Domain.Modules.Identity.Interfaces;
using WorkVault.SharedKernel.Constants;
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

        // Unknown email → generic failure (don't reveal whether the email exists).
        if (user is null)
            return null;

        // Registered but not activated. A self-registered company admin has no one
        // to resend their invite, so we surface a distinct signal that lets the
        // login page offer a "resend confirmation" action. Invited employees fall
        // through to the generic failure — HR resends their invite for them.
        if (!user.IsActive)
        {
            if (user.RoleId == SystemRoles.CompanyAdmin)
                throw new AccountNotActivatedException(
                    "Your account isn't activated yet. Please confirm your email to continue.");

            return null;
        }

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

        // Display profile (name, company) is sourced from /auth/me, not duplicated here.
        return new LoginResponse(
            user.Id,
            user.CompanyId,
            accessToken,
            refreshTokenString);
    }
}
