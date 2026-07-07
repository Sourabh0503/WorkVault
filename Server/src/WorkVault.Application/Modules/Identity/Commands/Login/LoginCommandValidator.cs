using FluentValidation;

namespace WorkVault.Application.Modules.Identity.Commands.Login;

/// <summary>Validates <see cref="LoginCommand"/>: email present and well-formed, password present.</summary>
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email format is invalid.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
