using MediatR;
using WorkVault.Domain.Modules.Identity.Interfaces;

namespace WorkVault.Application.Modules.Identity.Queries.ValidatePasswordReset;

public class ValidatePasswordResetHandler(
    IPasswordResetTokenRepository resetTokenRepository)
    : IRequestHandler<ValidatePasswordResetQuery, bool>
{
    public async Task<bool> Handle(
        ValidatePasswordResetQuery request,
        CancellationToken cancellationToken)
    {
        var token = await resetTokenRepository.GetByTokenAsync(
            request.Token, cancellationToken);

        if (token is null || !token.IsValid)
            return false;

        var user = token.User;
        if (user is null || !user.IsActive)
            return false;

        return true;
    }
}