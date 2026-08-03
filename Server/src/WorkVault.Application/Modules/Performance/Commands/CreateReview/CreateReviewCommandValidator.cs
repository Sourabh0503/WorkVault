using FluentValidation;

namespace WorkVault.Application.Modules.Performance.Commands.CreateReview;

/// <summary>
/// Validates <see cref="CreateReviewCommand"/>: required name, a non-future review date,
/// rating in 0–5 for a regular review, and a salary that's present (baseline) or
/// non-negative when given.
/// </summary>
/// <remarks>
/// Ordering rules — a baseline must be the first entry, and a regular review can't be the
/// first — need a repository lookup and live in <see cref="CreateReviewHandler"/>.
/// </remarks>
public class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
{
    public CreateReviewCommandValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("Employee ID is required.");

        RuleFor(x => x.ReviewName)
            .NotEmpty().WithMessage("Review name is required.")
            .MaximumLength(200);

        RuleFor(x => x.ReviewDate)
            .NotEmpty().WithMessage("Review date is required.")
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Review date can't be in the future.");

        // Rating only applies to a real appraisal — a baseline carries no rating.
        RuleFor(x => x.Rating)
            .InclusiveBetween(0m, 5m)
            .When(x => !x.IsBaseline)
            .WithMessage("Rating must be between 0 and 5.");

        // A baseline is the starting salary, so it must carry one.
        RuleFor(x => x.NewSalary)
            .NotNull()
            .When(x => x.IsBaseline)
            .WithMessage("A starting-salary entry must include a salary.");

        RuleFor(x => x.NewSalary)
            .GreaterThanOrEqualTo(0m)
            .When(x => x.NewSalary.HasValue)
            .WithMessage("Salary must be zero or greater.");
    }
}
