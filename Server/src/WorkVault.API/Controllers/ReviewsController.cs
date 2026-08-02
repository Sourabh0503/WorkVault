using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WorkVault.Application.Modules.Performance.Commands.CreateReview;
using WorkVault.Application.Modules.Performance.Commands.DeleteReview;
using WorkVault.Application.Modules.Performance.Commands.UpdateReview;
using WorkVault.Application.Modules.Performance.Queries.GetEmployeeReviews;
using WorkVault.Application.Modules.Performance.Queries.GetMyPerformance;
using WorkVault.SharedKernel.Constants;

namespace WorkVault.API.Controllers;

/// <summary>
/// Performance review endpoints, nested under an employee
/// (<c>api/employees/{employeeId}/reviews</c>), plus the caller's own
/// <c>api/me/performance</c>.
/// </summary>
/// <remarks>
/// Write operations (create/edit/delete) require HR or CompanyAdmin. Reads are open to
/// any authenticated user; the query handler enforces who may see which employee's
/// reviews — and strips salary for managers. Rate limited to the authenticated policy.
/// </remarks>
[ApiController]
[Route("api/employees/{employeeId:guid}/reviews")]
[Authorize]
[EnableRateLimiting("authenticated")]
public class ReviewsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Lists an employee's reviews for the history list and charts.
    /// </summary>
    /// <remarks>
    /// Access is role-based (enforced in the handler): HR/Admin see any employee with
    /// salary; the employee sees their own with salary; a manager sees a same-department
    /// colleague's ratings only (salary stripped server-side); anyone else gets 403.
    /// </remarks>
    /// <param name="employeeId">The employee whose reviews to list.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">The employee's reviews (salary included only if permitted).</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Not allowed to view this employee's reviews.</response>
    /// <response code="404">Employee not found.</response>
    [HttpGet]
    public async Task<IActionResult> GetForEmployee(
        Guid employeeId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetEmployeeReviewsQuery(employeeId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a performance review for an employee (HR/Admin only).
    /// </summary>
    /// <param name="employeeId">The employee being reviewed.</param>
    /// <param name="request">Review details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="201">Review created.</response>
    /// <response code="400">Validation failed, or salary missing on the first review.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Not authorized (requires HR or CompanyAdmin).</response>
    /// <response code="404">Employee not found.</response>
    [HttpPost]
    [Authorize(Roles = $"{SystemRoles.HRRole},{SystemRoles.CompanyAdminRole}")]
    public async Task<IActionResult> Create(
        Guid employeeId,
        [FromBody] CreateReviewRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateReviewCommand(
            employeeId,
            request.ReviewName,
            request.ReviewDate,
            request.Rating,
            request.NewSalary,
            request.Summary,
            request.IsBaseline);

        var result = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetForEmployee), new { employeeId }, result);
    }

    /// <summary>
    /// Edits an existing review (HR/Admin only, within 30 days of creation).
    /// </summary>
    /// <param name="employeeId">The employee the review belongs to.</param>
    /// <param name="reviewId">The review to edit.</param>
    /// <param name="request">Updated review details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="204">Review updated.</response>
    /// <response code="400">Validation failed, outside the 30-day window, or salary cleared on first review.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Not authorized (requires HR or CompanyAdmin).</response>
    /// <response code="404">Review not found.</response>
    [HttpPut("{reviewId:guid}")]
    [Authorize(Roles = $"{SystemRoles.HRRole},{SystemRoles.CompanyAdminRole}")]
    public async Task<IActionResult> Update(
        Guid employeeId,
        Guid reviewId,
        [FromBody] UpdateReviewRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateReviewCommand(
            employeeId,
            reviewId,
            request.ReviewName,
            request.ReviewDate,
            request.Rating,
            request.NewSalary,
            request.Summary);

        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Soft-deletes a review (HR/Admin only, within 30 days of creation).
    /// </summary>
    /// <param name="employeeId">The employee the review belongs to.</param>
    /// <param name="reviewId">The review to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="204">Review deleted.</response>
    /// <response code="400">Outside the 30-day window.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Not authorized (requires HR or CompanyAdmin).</response>
    /// <response code="404">Review not found.</response>
    [HttpDelete("{reviewId:guid}")]
    [Authorize(Roles = $"{SystemRoles.HRRole},{SystemRoles.CompanyAdminRole}")]
    public async Task<IActionResult> Delete(
        Guid employeeId,
        Guid reviewId,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteReviewCommand(employeeId, reviewId), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Returns the current user's own reviews (ratings + own salary), read-only.
    /// </summary>
    /// <remarks>
    /// Absolute route (<c>/api/me/performance</c>) — it isn't nested under an employee id.
    /// Available to any authenticated user; returns an empty result if they have no
    /// employee record.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">The caller's reviews.</response>
    /// <response code="401">Not authenticated.</response>
    [HttpGet("/api/me/performance")]
    public async Task<IActionResult> GetMyPerformance(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetMyPerformanceQuery(), cancellationToken);
        return Ok(result);
    }
}

/// <summary>Request body for creating a review (employee id comes from the route).</summary>
/// <param name="ReviewName">Review cycle label.</param>
/// <param name="ReviewDate">Date the review applies to.</param>
/// <param name="Rating">Rating 0–5 (ignored for a baseline).</param>
/// <param name="NewSalary">New salary (required for a baseline).</param>
/// <param name="Summary">Optional summary.</param>
/// <param name="IsBaseline">True to record the starting salary as the employee's first entry.</param>
public record CreateReviewRequest(
    string ReviewName,
    DateOnly ReviewDate,
    decimal Rating,
    decimal? NewSalary,
    string? Summary,
    bool IsBaseline);

/// <summary>Request body for editing a review (route supplies employee + review ids).</summary>
/// <param name="ReviewName">Review cycle label.</param>
/// <param name="ReviewDate">Date the review applies to.</param>
/// <param name="Rating">Rating 0–5.</param>
/// <param name="NewSalary">New salary (required if this is the first review).</param>
/// <param name="Summary">Optional summary.</param>
public record UpdateReviewRequest(
    string ReviewName,
    DateOnly ReviewDate,
    decimal Rating,
    decimal? NewSalary,
    string? Summary);
