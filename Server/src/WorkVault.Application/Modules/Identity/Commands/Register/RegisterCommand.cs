using MediatR;

namespace WorkVault.Application.Modules.Identity.Commands.Register;

/// <summary>
/// Self-serve signup: creates a new company (tenant) together with its first
/// admin user in a single operation.
/// </summary>
/// <param name="CompanyName">Display name of the new company/tenant.</param>
/// <param name="Domain">Optional email domain for the company.</param>
/// <param name="Industry">Industry classification (drives future onboarding defaults).</param>
/// <param name="GstNumber">Optional GST registration number.</param>
/// <param name="FirstName">Admin user's first name.</param>
/// <param name="LastName">Admin user's last name.</param>
/// <param name="Email">Admin user's login email (also the tenant owner).</param>
/// <param name="Password">Admin user's plaintext password (hashed before storage).</param>
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

/// <summary>Identifiers and tokens returned after a successful company + admin registration.</summary>
/// <param name="CompanyId">The newly created tenant id.</param>
/// <param name="UserId">The newly created admin user id.</param>
/// <param name="AccessToken">Short-lived JWT access token (user is auto-logged-in).</param>
/// <param name="RefreshToken">Long-lived refresh token.</param>
public record RegisterResponse(
    Guid CompanyId,
    Guid UserId,
    string AccessToken,
    string RefreshToken
);