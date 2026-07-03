using FluentValidation;
using WorkVault.Domain.Modules.Employees.Enums;

namespace WorkVault.Application.Modules.Employees.Commands.UpdateEmployee;

public class UpdateEmployeeCommandValidator : AbstractValidator<UpdateEmployeeCommand>
{
    public UpdateEmployeeCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Employee ID is required.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Phone)
            .MaximumLength(20)
            .Matches(@"^\+?[0-9\s\-]+$")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone))
            .WithMessage("Phone must contain only digits, spaces, hyphens, and optional + prefix.");

        RuleFor(x => x.PhotoUrl)
            .MaximumLength(500)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.PhotoUrl))
            .WithMessage("Photo URL must be a valid URL.");

        RuleFor(x => x.JoinDate)
            .NotEmpty().WithMessage("Join date is required.");

        RuleFor(x => x.ResignationDate)
            .GreaterThanOrEqualTo(x => x.JoinDate)
            .When(x => x.ResignationDate.HasValue)
            .WithMessage("Resignation date must be on or after join date.");

        RuleFor(x => x.LastWorkingDay)
            .GreaterThanOrEqualTo(x => x.ResignationDate)
            .When(x => x.LastWorkingDay.HasValue && x.ResignationDate.HasValue)
            .WithMessage("Last working day must be on or after resignation date.");

        // Status transition validations
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid employee status.");

        RuleFor(x => x.ResignationDate)
            .NotNull()
            .When(x => x.Status == EmployeeStatus.OnNotice)
            .WithMessage("Resignation date is required when status is OnNotice.");

        RuleFor(x => x.LastWorkingDay)
            .NotNull()
            .When(x => x.Status == EmployeeStatus.OffBoarded)
            .WithMessage("Last working day is required when status is Offboarded.");
    }
}
