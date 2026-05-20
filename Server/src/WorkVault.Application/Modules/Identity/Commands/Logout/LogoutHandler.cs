using MediatR;
using WorkVault.Domain.Modules.Identity.Interfaces;
using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Application.Modules.Identity.Commands.Logout;

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