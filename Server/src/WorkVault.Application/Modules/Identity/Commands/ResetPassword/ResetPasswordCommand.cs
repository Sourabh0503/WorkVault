using MediatR;

namespace WorkVault.Application.Modules.Identity.Commands.ResetPassword;

/// <summary>
/// Completes a password reset using a valid reset token. Does not auto-login;
/// the user signs in again with the new password.
/// </summary>
/// <param name="Token">The reset token from the reset link.</param>
/// <param name="Password">The new plaintext password (hashed before storage).</param>
public record ResetPasswordCommand(Guid Token, string Password) : IRequest;