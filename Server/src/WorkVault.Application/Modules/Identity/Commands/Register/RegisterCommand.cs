using MediatR;

namespace WorkVault.Application.Modules.Identity.Commands.Register;

public record RegisterCommand(
    // Company fields
    string CompanyName,
    string? Domain,
    string Industry,
    string? GstNumber,
    
    // Admin user fields
    string FirstName,
    string LastName,
    string Email,
    string Password
) : IRequest<RegisterResponse>;

public record RegisterResponse(
    Guid CompanyId,
    Guid UserId,
    string AccessToken,
    string RefreshToken
);