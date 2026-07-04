using MediatR;

namespace WorkVault.Application.Modules.Identity.Queries.ValidatePasswordReset;

public record ValidatePasswordResetQuery(Guid Token) : IRequest<bool>;