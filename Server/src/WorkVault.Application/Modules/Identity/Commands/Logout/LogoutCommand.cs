using MediatR;

namespace WorkVault.Application.Modules.Identity.Commands.Logout;

public record LogoutCommand(string RefreshToken) : IRequest<Unit>;