using MediatR;
using WorkVault.Application.Common.Interfaces;
using WorkVault.Domain.Modules.Identity.Interfaces;

namespace WorkVault.Application.Modules.Identity.Commands.RefreshTokens;

public class RefreshTokenHandler(
    IJwtTokenService jwtTokenService,
    IRefreshTokenRepository refreshTokenRepository)
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
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        await refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);

        return new RefreshTokenResponse(accessToken, newRefreshTokenString);
    }
}
