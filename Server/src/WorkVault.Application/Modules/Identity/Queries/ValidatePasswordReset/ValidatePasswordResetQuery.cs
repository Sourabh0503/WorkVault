using MediatR;

namespace WorkVault.Application.Modules.Identity.Queries.ValidatePasswordReset;

/// <summary>
/// Checks a reset token's validity before showing the reset form. Returns null
/// (→ 404) if invalid/expired/used, otherwise personalisation details.
/// </summary>
/// <param name="Token">The reset token to validate.</param>
public record ValidatePasswordResetQuery(Guid Token) : IRequest<ValidatePasswordResetResult?>;

/// <summary>Personalisation shown on the reset-password screen for a valid token.</summary>
/// <param name="Email">The account's email.</param>
/// <param name="FirstName">The user's first name (for the greeting).</param>
/// <param name="CompanyName">The user's company name.</param>
public record ValidatePasswordResetResult(
    string Email,
    string FirstName,
    string CompanyName
);