using MediatR;
using WorkVault.Application.Common;
using WorkVault.Domain.Modules.Identity;
using WorkVault.Domain.Modules.Identity.Interfaces;

namespace WorkVault.Application.Modules.Identity.Commands.Login;

public class LoginHandler(
    IUserRepository userRepository,
    IJwtTokenService jwtTokenService,
    IRefreshTokenRepository refreshTokenRepository)
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
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

        // Update last login
        user.LastLogin = DateTime.UtcNow;

        return new LoginResponse(user.Id, user.CompanyId, accessToken, refreshTokenString);
    }
}
