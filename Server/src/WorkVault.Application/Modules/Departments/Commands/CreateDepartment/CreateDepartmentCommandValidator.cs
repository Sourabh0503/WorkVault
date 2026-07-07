using FluentValidation;

namespace WorkVault.Application.Modules.Departments.Commands.CreateDepartment;

/// <summary>Validates <see cref="CreateDepartmentCommand"/>: name required (≤100), description ≤500.</summary>
public class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Department name is required.")
            .MaximumLength(100).WithMessage("Department name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
    }
}
