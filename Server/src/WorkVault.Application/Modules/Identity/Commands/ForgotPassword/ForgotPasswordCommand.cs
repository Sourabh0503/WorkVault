using MediatR;

namespace WorkVault.Application.Modules.Identity.Commands.ForgotPassword;

/// <summary>
/// Requests a password reset link for the given email. Always succeeds (200) —
/// it never reveals whether the email is registered.
/// </summary>
/// <param name="Email">The email to send the reset link to (if it exists).</param>
public record ForgotPasswordCommand(string Email) : IRequest;