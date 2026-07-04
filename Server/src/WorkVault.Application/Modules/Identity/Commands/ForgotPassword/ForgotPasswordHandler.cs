using MediatR;
using Microsoft.Extensions.Logging;
using WorkVault.Domain.Modules.Identity;
using WorkVault.Domain.Modules.Identity.Interfaces;
using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Application.Modules.Identity.Commands.ForgotPassword;

public class ForgotPasswordHandler(
    IUserRepository userRepository,
    IPasswordResetTokenRepository resetTokenRepository,
    IUnitOfWork unitOfWork,
    ILogger<ForgotPasswordHandler> logger)
    : IRequestHandler<ForgotPasswordCommand>
{
    public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        // 1. Look up the user by email
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);

        // 2. SECURITY: silently succeed if user doesn't exist or is inactive.
        //    We do NOT reveal whether the email is registered.
        if (user is null || !user.IsActive)
        {
            logger.LogInformation(
                "Password reset requested for non-existent or inactive email: {Email}",
                request.Email);
            return; // caller gets 200 OK
        }

        // 3. Invalidate any existing active reset token for this user.
        //    Prevents accumulation of multiple valid tokens.
        var existingToken = await resetTokenRepository
            .GetActiveTokenForUserAsync(user.Id, cancellationToken);

        if (existingToken is not null)
            existingToken.UsedAt = DateTime.UtcNow;

        // 4. Create new reset token (defaults: 2h expiry, v7 Guid)
        var newToken = new PasswordResetToken
        {
            UserId = user.Id,
            CompanyId = user.CompanyId,  // explicit — user isn't logged in, no auto-stamp available
            CreatedBy = user.Id
        };
        await resetTokenRepository.AddAsync(newToken, cancellationToken);

        // 5. Save changes
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // 6. Log the link (real email later)
        var resetLink = $"http://localhost:4200/reset-password?token={newToken.Token}";
        logger.LogInformation(
            "Password reset link generated for {Email}. Link: {ResetLink}",
            user.Email, resetLink);
    }
}