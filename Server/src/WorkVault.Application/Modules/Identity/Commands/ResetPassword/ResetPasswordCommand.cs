using MediatR;

namespace WorkVault.Application.Modules.Identity.Commands.ResetPassword;

public record ResetPasswordCommand(Guid Token, string Password) : IRequest;