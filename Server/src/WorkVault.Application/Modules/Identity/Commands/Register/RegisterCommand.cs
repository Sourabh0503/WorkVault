using MediatR;

namespace WorkVault.Application.Modules.Identity.Commands.Register;

/// <summary>
/// Self-serve signup: creates a new company (tenant) together with its first
/// admin user. No password is collected here — the admin is created inactive
/// and receives an email invite to set their password (same flow as employees).
/// </summary>
/// <param name="CompanyName">Display name of the new company/tenant.</param>
/// <param name="Domain">Optional email domain for the company.</param>
/// <param name="Industry">Industry classification (drives future onboarding defaults).</param>
/// <param name="GstNumber">Optional GST registration number.</param>
/// <param name="FirstName">Admin user's first name.</param>
/// <param name="LastName">Admin user's last name.</param>
/// <param name="Email">Admin user's login email (also the tenant owner).</param>
public record RegisterCommand(
    // Company fields
    string CompanyName,
    string? Domain,
    string Industry,
    string? GstNumber,

    // Admin user fields
    string FirstName,
    string LastName,
    string Email
) : IRequest<RegisterResponse>;

/// <summary>
/// Confirmation that registration succeeded and an invite email was sent.
/// No tokens — the admin activates and logs in via the set-password link.
/// </summary>
/// <param name="CompanyId">The newly created tenant id.</param>
/// <param name="UserId">The newly created admin user id.</param>
/// <param name="Email">The email the invite link was sent to.</param>
public record RegisterResponse(
    Guid CompanyId,
    Guid UserId,
    string Email
);