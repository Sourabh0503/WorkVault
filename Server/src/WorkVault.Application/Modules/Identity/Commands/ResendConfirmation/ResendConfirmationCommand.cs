using MediatR;

namespace WorkVault.Application.Modules.Identity.Commands.ResendConfirmation;

/// <summary>
/// Public request from the login page to resend the account-confirmation
/// (set-password) email to a self-registered company admin who never activated.
/// </summary>
/// <param name="Email">The admin's email address.</param>
public record ResendConfirmationCommand(string Email) : IRequest;
