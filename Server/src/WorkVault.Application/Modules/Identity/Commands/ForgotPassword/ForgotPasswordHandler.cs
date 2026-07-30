using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WorkVault.Application.Common.Interfaces;
using WorkVault.Application.Common.Messaging;
using WorkVault.Domain.Modules.Identity;
using WorkVault.Domain.Modules.Identity.Interfaces;
using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Application.Modules.Identity.Commands.ForgotPassword;

/// <summary>
/// Issues a password reset token for a registered, active user.
/// </summary>
/// <remarks>
/// Flow:
/// 1. Look up the user by email.
/// 2. Silently return (still 200) if not found or inactive — never leak whether an email exists.
/// 3. Invalidate any existing active reset token for the user.
/// 4. Create a new reset token (2h expiry).
/// 5. Save.
/// 6. Log the reset link (real email delivery is a future enhancement).
/// </remarks>
public class ForgotPasswordHandler(
    IUserRepository userRepository,
    IPasswordResetTokenRepository resetTokenRepository,
    IEmailPublisher emailPublisher,
    IConfiguration configuration,
    IUnitOfWork unitOfWork,
    ILogger<ForgotPasswordHandler> logger)
    : IRequestHandler<ForgotPasswordCommand>
{
    /// <summary>Generates (or refreshes) the reset token for the requested email.</summary>
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

        // 6. send the link
        var frontendUrl = configuration["AppSettings:FrontendUrl"];
        var resetLink = $"{frontendUrl}/reset-password?token={newToken.Token}";
        await emailPublisher.PublishAsync(new EmailMessage(
            To: user.Email,
            Subject: "Reset your WorkVault password",
            Body: $"Hi {user.FirstName},\n\n" +
                  $"We received a request to reset your WorkVault password. " +
                  $"Click the link below to choose a new one:\n\n{resetLink}\n\n" +
                  $"This link expires in 2 hours. If you didn't request this, you can safely ignore this email.",
            Type: EmailType.PasswordReset
        ), cancellationToken);
    }
}