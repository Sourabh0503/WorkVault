using MediatR;
using WorkVault.Application.Common.Exceptions;
using WorkVault.Domain.Modules.Identity.Interfaces;

namespace WorkVault.Application.Modules.Identity.Queries.ValidatePasswordReset;

/// <summary>
/// Validates a password-reset token and returns the user + company details used to
/// personalise the reset screen. Returns null for any invalid/expired/used token.
/// </summary>
public class ValidatePasswordResetHandler(
    IPasswordResetTokenRepository resetTokenRepository,
    ICompanyRepository companyRepository)
    : IRequestHandler<ValidatePasswordResetQuery, ValidatePasswordResetResult?>
{
    /// <summary>Resolves the token to email/first-name/company, or null if it can't be used.</summary>
    public async Task<ValidatePasswordResetResult?> Handle(
        ValidatePasswordResetQuery request,
        CancellationToken cancellationToken)
    {
        var token = await resetTokenRepository.GetByTokenAsync(
            request.Token, cancellationToken);

        if (token is null || !token.IsValid)
            return null;

        var user = token.User;
        if (user is null || !user.IsActive)
            return null;

        var company = await companyRepository.GetByIdAsync(
            user.CompanyId, cancellationToken);

        if (company is null)
            return null;

        return new ValidatePasswordResetResult(
            Email: user.Email,
            FirstName: user.FirstName,
            CompanyName: company.Name
        );
    }
}