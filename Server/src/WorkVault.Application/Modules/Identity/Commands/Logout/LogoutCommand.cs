using MediatR;

namespace WorkVault.Application.Modules.Identity.Commands.Logout;

/// <summary>Revokes the given refresh token so the session can no longer be renewed.</summary>
/// <param name="RefreshToken">The refresh token to revoke.</param>
public record LogoutCommand(string RefreshToken) : IRequest<Unit>;