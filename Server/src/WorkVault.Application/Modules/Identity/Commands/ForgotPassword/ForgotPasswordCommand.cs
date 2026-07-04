using MediatR;

namespace WorkVault.Application.Modules.Identity.Commands.ForgotPassword;

public record ForgotPasswordCommand(string Email) : IRequest;