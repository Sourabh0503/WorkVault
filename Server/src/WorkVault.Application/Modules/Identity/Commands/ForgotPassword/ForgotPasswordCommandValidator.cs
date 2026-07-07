using FluentValidation;

namespace WorkVault.Application.Modules.Identity.Commands.ForgotPassword;

/// <summary>Validates <see cref="ForgotPasswordCommand"/>: email present, well-formed, ≤256 chars.</summary>
public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Please enter a valid email.")
            .MaximumLength(256);
    }
}