using FluentValidation;

namespace WorkVault.Application.Modules.Performance.Commands.UpdateReview;

/// <summary>
/// Validates <see cref="UpdateReviewCommand"/>: required name, a non-future review date,
/// rating in 0–5, and a non-negative salary when present. The 30-day mutable-window and
/// first-review-salary rules need repository/entity state and live in
/// <see cref="UpdateReviewHandler"/>.
/// </summary>
public class UpdateReviewCommandValidator : AbstractValidator<UpdateReviewCommand>
{
    public UpdateReviewCommandValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("Employee ID is required.");

        RuleFor(x => x.ReviewId)
            .NotEmpty().WithMessage("Review ID is required.");

        RuleFor(x => x.ReviewName)
            .NotEmpty().WithMessage("Review name is required.")
            .MaximumLength(200);

        RuleFor(x => x.ReviewDate)
            .NotEmpty().WithMessage("Review date is required.")
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Review date can't be in the future.");

        RuleFor(x => x.Rating)
            .InclusiveBetween(0m, 5m).WithMessage("Rating must be between 0 and 5.");

        RuleFor(x => x.NewSalary)
            .GreaterThanOrEqualTo(0m)
            .When(x => x.NewSalary.HasValue)
            .WithMessage("Salary must be zero or greater.");
    }
}
