using MediatR;
using WorkVault.Application.Common.Exceptions;
using WorkVault.Domain.Modules.Employees.Interfaces;
using WorkVault.Domain.Modules.Performance;
using WorkVault.Domain.Modules.Performance.Interfaces;
using WorkVault.SharedKernel.Interfaces;

namespace WorkVault.Application.Modules.Performance.Commands.CreateReview;

/// <summary>
/// Creates a performance review — or the salary baseline — for an employee.
/// </summary>
/// <remarks>
/// Access (HR/Admin only) is enforced at the controller. Business rules here:
/// <list type="bullet">
///   <item>The employee's FIRST entry must be the baseline (starting salary). A regular
///   review can't be added until a baseline exists.</item>
///   <item>There's exactly one baseline per employee.</item>
///   <item>A baseline has no rating — it's stored as 0 and never shown.</item>
/// </list>
/// </remarks>
public class CreateReviewHandler(
    IEmployeeRepository employeeRepository,
    IReviewRepository reviewRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateReviewCommand, CreateReviewResult>
{
    public async Task<CreateReviewResult> Handle(
        CreateReviewCommand request,
        CancellationToken cancellationToken)
    {
        // Employee must exist in this tenant (repository is tenant-filtered).
        var employee = await employeeRepository.GetByIdAsync(request.EmployeeId, cancellationToken);
        if (employee is null)
            throw new NotFoundException($"Employee with ID '{request.EmployeeId}' not found.");

        var hasReviews = await reviewRepository.HasAnyAsync(request.EmployeeId, cancellationToken);

        if (request.IsBaseline)
        {
            // The baseline is the very first entry, and there's only ever one.
            if (hasReviews)
                throw new BusinessRuleException(
                    "This employee already has records — a starting salary can only be their first entry.");
        }
        else
        {
            // A regular review needs the starting salary in place first.
            if (!hasReviews)
                throw new BusinessRuleException(
                    "Add the employee's starting salary before recording a review.");
        }

        var review = new Review
        {
            EmployeeId = request.EmployeeId,
            ReviewName = request.ReviewName,
            ReviewDate = request.ReviewDate,
            // A baseline carries no rating — store 0, never shown.
            Rating = request.IsBaseline ? 0m : request.Rating,
            NewSalary = request.NewSalary,
            Summary = request.Summary,
            IsBaseline = request.IsBaseline
        };

        await reviewRepository.AddAsync(review, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateReviewResult(review.Id);
    }
}
