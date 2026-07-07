using MediatR;

namespace WorkVault.Application.Modules.Identity.Commands.RegisterCompany;

/// <summary>
/// Provisions a standalone company (tenant) without an admin user — used by the
/// SuperAdmin flow. Returns the new company's id.
/// </summary>
/// <param name="Name">Display name of the company.</param>
/// <param name="Domain">Optional email domain.</param>
/// <param name="Industry">Industry classification.</param>
/// <param name="GstNumber">Optional GST registration number.</param>
/// <param name="Timezone">IANA timezone; defaults to Asia/Kolkata.</param>
public record RegisterCompanyCommand(
    string Name,
    string? Domain,
    string Industry,
    string? GstNumber,
    string Timezone = "Asia/Kolkata"
) : IRequest<Guid>;