using MediatR;
using WorkVault.Domain.Modules.Identity.Interfaces;

namespace WorkVault.Application.Modules.Identity.Queries.ValidateInvite;

/// <summary>
/// Validates an invite token and returns the invited user + company details for the
/// set-password screen. Returns null if the token is unusable or the user is already active.
/// </summary>
public class ValidateInviteHandler(
    IInviteTokenRepository inviteTokenRepository,
    ICompanyRepository companyRepository)
    : IRequestHandler<ValidateInviteQuery, ValidateInviteResult?>
{
    /// <summary>Resolves the invite token to the invited user's details, or null if invalid.</summary>
    public async Task<ValidateInviteResult?> Handle(
        ValidateInviteQuery request,
        CancellationToken cancellationToken)
    {
        // Look up the token (includes User via .Include in repo)
        var inviteToken = await inviteTokenRepository.GetByTokenAsync(
            request.Token, cancellationToken);

        // Token doesn't exist
        if (inviteToken is null)
            return null;

        // Token already used or expired
        if (!inviteToken.IsValid)
            return null;

        var user = inviteToken.User;
        if (user is null || user.IsActive)
            return null;  // safety: user already activated somehow

        // Load company for the welcome message
        var company = await companyRepository.GetByIdAsync(
            user.CompanyId, cancellationToken);

        if (company is null)
            return null;  // company deleted? edge case, fail safely

        return new ValidateInviteResult(
            Email: user.Email,
            FirstName: user.FirstName,
            LastName: user.LastName,
            CompanyName: company.Name
        );
    }
}