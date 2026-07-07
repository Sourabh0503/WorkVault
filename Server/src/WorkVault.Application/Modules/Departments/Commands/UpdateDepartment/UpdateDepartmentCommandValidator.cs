using FluentValidation;

namespace WorkVault.Application.Modules.Departments.Commands.UpdateDepartment;

/// <summary>
/// Validates <see cref="UpdateDepartmentCommand"/>: id + name required (name ≤100),
/// description ≤500, and parent may not equal the department itself.
/// </summary>
public class UpdateDepartmentCommandValidator : AbstractValidator<UpdateDepartmentCommand>
{
    public UpdateDepartmentCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Department ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Department name is required.")
            .MaximumLength(100).WithMessage("Department name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");

        // Prevent setting parent to self
        RuleFor(x => x.ParentDepartmentId)
            .NotEqual(x => x.Id)
            .When(x => x.ParentDepartmentId.HasValue)
            .WithMessage("A department cannot be its own parent.");
    }
}
