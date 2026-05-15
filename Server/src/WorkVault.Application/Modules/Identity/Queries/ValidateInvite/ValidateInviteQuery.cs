using MediatR;

namespace WorkVault.Application.Modules.Identity.Queries.ValidateInvite;

public record ValidateInviteQuery(Guid Token) : IRequest<ValidateInviteResult?>;

public record ValidateInviteResult(
    string Email,
    string FirstName,
    string LastName,
    string CompanyName
);