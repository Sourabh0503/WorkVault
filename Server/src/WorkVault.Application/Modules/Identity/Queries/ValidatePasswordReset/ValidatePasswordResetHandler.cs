using MediatR;
using WorkVault.Application.Common.Exceptions;
using WorkVault.Domain.Modules.Identity.Interfaces;

namespace WorkVault.Application.Modules.Identity.Queries.ValidatePasswordReset;

public class ValidatePasswordResetHandler(
    IPasswordResetTokenRepository resetTokenRepository,
    ICompanyRepository companyRepository)
    : IRequestHandler<ValidatePasswordResetQuery, ValidatePasswordResetResult?>
{
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