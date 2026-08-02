using WorkVault.Domain.Modules.Performance;

namespace WorkVault.Application.Modules.Performance.Queries.GetEmployeeReviews;

/// <summary>
/// Maps <see cref="Review"/> entities to <see cref="ReviewDto"/>, applying salary
/// visibility and computing the derived hike% in one place — shared by the employee
/// reviews and "my performance" queries.
/// </summary>
public static class ReviewMapper
{
    /// <summary>
    /// Projects reviews (newest-first) to DTOs. When <paramref name="salaryVisible"/> is
    /// false, salary and the derived increment are both nulled (managers see ratings only).
    /// </summary>
    /// <param name="reviewsNewestFirst">The employee's reviews, ordered newest first.</param>
    /// <param name="salaryVisible">Whether the caller may see salary figures.</param>
    /// <param name="utcNow">Current time, for the 30-day mutable-window flag.</param>
    public static List<ReviewDto> ToDtos(
        IReadOnlyList<Review> reviewsNewestFirst, bool salaryVisible, DateTime utcNow)
    {
        // Hike% compares each salaried review to the previous salaried one, so we need to
        // walk oldest → newest tracking the last salary. Only meaningful if salary is visible.
        var increments = salaryVisible
            ? ComputeIncrements(reviewsNewestFirst)
            : new Dictionary<Guid, decimal?>();

        return reviewsNewestFirst
            .Select(r => new ReviewDto(
                Id: r.Id,
                ReviewName: r.ReviewName,
                ReviewDate: r.ReviewDate,
                Rating: r.Rating,
                IsBaseline: r.IsBaseline,
                NewSalary: salaryVisible ? r.NewSalary : null,
                IncrementPercent: increments.GetValueOrDefault(r.Id),
                Summary: r.Summary,
                IsMutable: r.IsMutableAt(utcNow),
                CreatedAt: r.CreatedAt))
            .ToList();
    }

    /// <summary>
    /// Builds a map of review id → hike% vs. the previous salaried review. Reviews without
    /// a salary, and the first salaried review, are absent (treated as null by callers).
    /// </summary>
    private static Dictionary<Guid, decimal?> ComputeIncrements(IReadOnlyList<Review> reviewsNewestFirst)
    {
        var result = new Dictionary<Guid, decimal?>();
        decimal? previousSalary = null;

        // Oldest first so "previous" is well-defined as we move forward in time.
        foreach (var review in reviewsNewestFirst.Reverse())
        {
            if (review.NewSalary is not { } salary)
                continue; // no salary change on this review — leave hike% null

            // Need a prior, non-zero salary to compute a percentage.
            if (previousSalary is { } prev && prev != 0)
                result[review.Id] = Math.Round((salary - prev) / prev * 100m, 2);

            previousSalary = salary;
        }

        return result;
    }
}
