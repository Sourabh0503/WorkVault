using MediatR;
using WorkVault.Domain.Modules.Identity.Interfaces;
using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Application.Modules.Identity.Commands.Logout;

/// <summary>
/// Handles user logout by revoking the refresh token.
/// </summary>
/// <remarks>
/// Security considerations:
/// - Always returns success (Unit) even if token is invalid/already revoked
/// - This prevents attackers from discovering valid token states
/// - Only revokes the specific token, not all user tokens (allows multi-device)
///
/// Requires authentication (user must have valid JWT).
/// </remarks>
public class LogoutHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<LogoutCommand, Unit>
{
    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var token = await refreshTokenRepository.GetByTokenAsync(
            request.RefreshToken, cancellationToken);

        // Silently succeed even if token doesn't exist or is already revoked.
        // Don't leak info about whether the token was valid.
        if (token is null || token.IsRevoked)
            return Unit.Value;

        await refreshTokenRepository.RevokeAsync(token, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}