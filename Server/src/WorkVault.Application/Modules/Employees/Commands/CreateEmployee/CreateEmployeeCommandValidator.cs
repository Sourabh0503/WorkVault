using FluentValidation;
using WorkVault.SharedKernel.Constants;

namespace WorkVault.Application.Modules.Employees.Commands.CreateEmployee;

/// <summary>
/// Validates <see cref="CreateEmployeeCommand"/>: required names/email with length
/// limits, optional phone format, and a join date within a sane window (≤1yr future, ≤50yr past).
/// </summary>
public class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email format is invalid.")
            .MaximumLength(256);

        RuleFor(x => x.Phone)
            .MaximumLength(20)
            .Matches(@"^\+?[0-9\s\-]+$")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone))
            .WithMessage("Phone must contain only digits, spaces, hyphens, and optional + prefix.");

        RuleFor(x => x.JoinDate)
            .NotEmpty().WithMessage("Join date is required.")
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow).AddYears(1))
            .WithMessage("Join date cannot be more than 1 year in the future.")
            .GreaterThan(DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-50))
            .WithMessage("Join date is too far in the past.");

        RuleFor(x => x.RoleId)
            .Must(SystemRoles.AssignableEmployeeRoles.Contains)
            .WithMessage("Role must be HR, Manager, or Employee.");
    }
}