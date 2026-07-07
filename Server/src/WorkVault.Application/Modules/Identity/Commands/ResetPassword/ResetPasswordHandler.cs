using MediatR;
using WorkVault.Application.Common.Exceptions;
using WorkVault.Domain.Modules.Identity.Interfaces;
using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Application.Modules.Identity.Commands.ResetPassword;

/// <summary>
/// Applies a new password from a valid reset token and revokes all existing sessions.
/// </summary>
/// <remarks>
/// Flow:
/// 1. Look up the reset token (user eagerly loaded).
/// 2. Throw <see cref="NotFoundException"/> if the token is invalid/expired/used or the user is inactive.
/// 3. Hash and store the new password.
/// 4. Mark the token used (single-use).
/// 5. Revoke ALL of the user's refresh tokens (kicks out any stolen sessions).
/// 6. Save in a single transaction.
/// </remarks>
public class ResetPasswordHandler(
    IPasswordResetTokenRepository resetTokenRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ResetPasswordCommand>
{
    /// <summary>Validates the token, updates the password, and revokes existing sessions.</summary>
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