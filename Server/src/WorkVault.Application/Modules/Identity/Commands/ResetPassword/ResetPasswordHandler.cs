using MediatR;
using WorkVault.Application.Common.Exceptions;
using WorkVault.Domain.Modules.Identity.Interfaces;
using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Application.Modules.Identity.Commands.ResetPassword;

public class ResetPasswordHandler(
    IPasswordResetTokenRepository resetTokenRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ResetPasswordCommand>
{
    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        // 1. Look up reset token (User + Role loaded via .Include chain)
        var resetToken = await resetTokenRepository.GetByTokenAsync(
            request.Token, cancellationToken);

        // 2. Validate token — throw NotFound to hide "why" from the user
        if (resetToken is null || !resetToken.IsValid)
            throw new NotFoundException("Reset link is invalid or expired.");

        var user = resetToken.User;
        if (user is null || !user.IsActive)
            throw new NotFoundException("Reset link is invalid or expired.");

        // 3. Update password + audit
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // 4. Mark reset token as used (single-use enforcement)
        resetToken.UsedAt = DateTime.UtcNow;

        // 5. SECURITY: revoke ALL refresh tokens for this user.
        //    If an attacker had a stolen refresh token, this kicks them out.
        //    The legitimate user will need to log in again with the new password.
        await refreshTokenRepository.RevokeAllForUserAsync(user.Id, cancellationToken);

        // 6. Single transaction save
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}