using FluentValidation;

namespace WorkVault.Application.Modules.Identity.Commands.RefreshTokens;

/// <summary>Validates <see cref="RefreshTokenCommand"/>: the refresh token must be present.</summary>
public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}
