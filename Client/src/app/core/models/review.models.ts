/**
 * Performance/Review domain types for the Performance module. Each interface mirrors its
 * backend counterpart (noted inline).
 *
 * Security note: there is no `salaryVisible` flag. The server enforces salary access by
 * NULLING `newSalary` + `incrementPercent` for callers who may not see it (managers), so
 * even a raw API response carries nulls. The salary chart's visibility is derived from the
 * data (see `hasVisibleSalary` below), never from a flag.
 */

// Mirrors ReviewDto (WorkVault.Application, GetEmployeeReviews).
export interface Review {
  id: string;
  reviewName: string;
  reviewDate: string; // DateOnly serializes as "yyyy-MM-dd"
  rating: number; // 0–5; meaningless when isBaseline
  isBaseline: boolean; // starting-salary entry (no rating, anchors hikes)
  newSalary: number | null; // null if none, OR caller can't see salary
  incrementPercent: number | null; // derived hike; null for base / no-salary / manager
  summary: string | null;
  isMutable: boolean; // within the 30-day edit/delete window
  createdAt: string; // ISO datetime
}

// Mirrors CreateReviewRequest (ReviewsController). EmployeeId comes from the route.
export interface CreateReviewRequest {
  reviewName: string;
  reviewDate: string;
  rating: number; // ignored server-side when isBaseline
  newSalary: number | null; // required when isBaseline
  summary: string | null;
  isBaseline: boolean;
}

// Mirrors UpdateReviewRequest (ReviewsController). isBaseline is immutable, so it's absent.
export interface UpdateReviewRequest {
  reviewName: string;
  reviewDate: string;
  rating: number;
  newSalary: number | null;
  summary: string | null;
}

// Result of POST /api/employees/{id}/reviews.
export interface CreateReviewResult {
  reviewId: string;
}

/**
 * Whether the salary chart/fields should render for this set of reviews.
 *
 * Because every allowed viewer always has at least the baseline salary, "reviews exist but
 * every newSalary is null" unambiguously means the caller can't see salary (manager) — so
 * we hide the salary UI. Zero reviews => nothing to show either.
 */
export function hasVisibleSalary(reviews: Review[]): boolean {
  return reviews.some((r) => r.newSalary != null);
}
