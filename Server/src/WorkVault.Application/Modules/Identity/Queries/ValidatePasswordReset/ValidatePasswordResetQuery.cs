using MediatR;

namespace WorkVault.Application.Modules.Identity.Queries.ValidatePasswordReset;

public record ValidatePasswordResetQuery(Guid Token) : IRequest<ValidatePasswordResetResult?>;

public record ValidatePasswordResetResult(
    string Email,
    string FirstName,
    string CompanyName
);