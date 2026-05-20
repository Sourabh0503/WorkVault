using MediatR;
using Microsoft.Extensions.Options;
using WorkVault.Application.Common.Interfaces;
using WorkVault.Application.Common.Settings;
using WorkVault.Domain.Modules.Identity.Interfaces;
using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Application.Modules.Identity.Commands.RefreshTokens;

/// <summary>
/// Handles token refresh with rotation for security.
/// </summary>
/// <remarks>
/// Token rotation flow:
/// 1. Validate existing refresh token (not expired, not revoked)
/// 2. Revoke the old token (prevents reuse)
/// 3. Generate new access token and refresh token
/// 4. Save new refresh token to database
///
/// Security benefits of token rotation:
/// - Stolen tokens can only be used once
/// - Reduces window of opportunity for attackers
/// - If old token is reused, it indicates theft (already revoked)
///
/// Returns null for invalid tokens (controller returns 401).
/// This is a public endpoint - the refresh token itself is the credential.
/// </remarks>
public class RefreshTokenHandler(
    IJwtTokenService jwtTokenService,
    IUnitOfWork unitOfWork,
    IRefreshTokenRepository refreshTokenRepository,
    IOptions<JwtSettings> jwtSettings)
    : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse?>
{
    public async Task<RefreshTokenResponse?> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var existingToken = await refreshTokenRepository.GetByTokenAsync(
            request.RefreshToken, cancellationToken);

        if (existingToken is null)
            return null;

        if (existingToken.ExpiresAt < DateTime.UtcNow)
        {
            await refreshTokenRepository.RevokeAsync(existingToken, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return null;
        }

        var user = existingToken.User;
        if (!user.IsActive)
            return null;

        // Revoke old token (token rotation)
        await refreshTokenRepository.RevokeAsync(existingToken, cancellationToken);

        // Generate new tokens
        var accessToken = jwtTokenService.GenerateAccessToken(user, user.Role.Name);
        var newRefreshTokenString = jwtTokenService.GenerateRefreshToken();

        // Save new refresh token
        var newRefreshToken = new Domain.Modules.Identity.RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddDays(jwtSettings.Value.RefreshTokenExpirationDays)
        };
        await refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new RefreshTokenResponse(accessToken, newRefreshTokenString);
    }
}
