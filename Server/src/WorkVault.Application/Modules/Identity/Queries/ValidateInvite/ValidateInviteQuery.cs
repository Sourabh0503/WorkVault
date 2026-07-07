using MediatR;

namespace WorkVault.Application.Modules.Identity.Queries.ValidateInvite;

/// <summary>
/// Checks an invite token before showing the set-password form. Returns null
/// (→ 404) if invalid/expired/used or the user is already active.
/// </summary>
/// <param name="Token">The invite token to validate.</param>
public record ValidateInviteQuery(Guid Token) : IRequest<ValidateInviteResult?>;

/// <summary>Details shown on the set-password screen for a valid invite.</summary>
/// <param name="Email">The invited user's email.</param>
/// <param name="FirstName">The invited user's first name.</param>
/// <param name="LastName">The invited user's last name.</param>
/// <param name="CompanyName">The company the user is joining.</param>
public record ValidateInviteResult(
    string Email,
    string FirstName,
    string LastName,
    string CompanyName
);