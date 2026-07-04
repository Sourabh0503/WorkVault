using FluentValidation;

namespace WorkVault.Application.Modules.Designations.Commands.CreateDesignation;

public class CreateDesignationCommandValidator : AbstractValidator<CreateDesignationCommand>
{
    public CreateDesignationCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Designation title is required.")
            .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");

        RuleFor(x => x.Level)
            .InclusiveBetween(1, 10).WithMessage("Level must be between 1 and 10.");

        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithMessage("Department is required.");
    }
}
