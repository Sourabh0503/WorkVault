using MediatR;
using Microsoft.Extensions.Configuration;
using WorkVault.Application.Common.Interfaces;
using WorkVault.Application.Common.Messaging;
using WorkVault.Domain.Modules.Identity;
using WorkVault.Domain.Modules.Identity.Interfaces;
using WorkVault.SharedKernel.Constants;
using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Application.Modules.Identity.Commands.ResendConfirmation;

/// <summary>
/// Resends the account-confirmation email to a self-registered company admin who
/// hasn't activated yet. Rotates the invite token so the new link is the only valid one.
/// </summary>
/// <remarks>
/// This is a public, unauthenticated endpoint. To avoid abuse it only acts when the
/// email belongs to an <b>inactive CompanyAdmin</b>; for anything else it's a silent
/// no-op. The controller always returns 204, so the endpoint can't be used to probe
/// account existence beyond what the login page already reveals.
/// </remarks>
public class ResendConfirmationHandler(
    IUserRepository userRepository,
    IInviteTokenRepository inviteTokenRepository,
    IEmailPublisher emailPublisher,
    IConfiguration configuration,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ResendConfirmationCommand>
{
    public async Task Handle(
        ResendConfirmationCommand request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);

        // Only resend for a not-yet-activated company admin. Everything else
        // (unknown email, active user, invited employee) is a silent no-op.
        if (user is null || user.IsActive || user.RoleId != SystemRoles.CompanyAdmin)
            return;

        // Rotate the invite token: revoke any active one, issue a fresh 48h token.
        var existingInvite = await inviteTokenRepository
            .GetActiveTokenForUserAsync(user.Id, cancellationToken);
        if (existingInvite is not null)
            existingInvite.UsedAt = DateTime.UtcNow;

        var newInviteToken = new InviteToken
        {
            UserId = user.Id
            // Token and ExpiresAt have defaults on the entity
        };
        await inviteTokenRepository.AddAsync(newInviteToken, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var frontendUrl = configuration["AppSettings:FrontendUrl"];
        var inviteLink = $"{frontendUrl}/register/{newInviteToken.Token}";
        await emailPublisher.PublishAsync(new EmailMessage(
            To: user.Email,
            Subject: "Confirm your WorkVault account",
            Body: $"Hi {user.FirstName},\n\n" +
                  "Here's a fresh link to activate your WorkVault admin account. " +
                  $"Set your password to sign in:\n\n{inviteLink}\n\n" +
                  "This link expires in 48 hours.",
            Type: EmailType.Invite
        ), cancellationToken);
    }
}
