import { Review } from '../../core/models/review.models';
import { formatInr } from '../../shared/utils/currency';
import { formatHike } from '../../shared/utils/hike';

/**
 * Pure derivations over a review list (newest-first), shared by the reviews store and the
 * My Performance page so the "current salary / avg rating / latest hike" logic lives once.
 */

/** Average rating across real appraisals (baselines excluded), 1 decimal — or null. */
export function averageRating(reviews: Review[]): string | null {
  const rated = reviews.filter((r) => !r.isBaseline);
  if (rated.length === 0) return null;
  const mean = rated.reduce((sum, r) => sum + r.rating, 0) / rated.length;
  return (Math.round(mean * 10) / 10).toString();
}

/** Current (derived) salary = the most recent salaried review's salary — or null. */
export function currentSalaryLabel(reviews: Review[]): string | null {
  const salary = reviews.find((r) => r.newSalary != null)?.newSalary;
  return salary != null ? formatInr(salary) : null;
}

/**
 * Salary growth over the trailing 12 months, as a signed % chip label.
 * Compares the current salary to the salary that was in effect one year before the latest
 * salaried review. If the employee has under a year of salary history, it falls back to
 * their earliest salaried entry (growth since they started). Null when it can't be computed.
 */
export function yearlyGrowthLabel(reviews: Review[]): string | null {
  const salaried = reviews.filter((r) => r.newSalary != null); // newest-first
  if (salaried.length < 2) return null;

  const latest = salaried[0];
  const cutoff = new Date(latest.reviewDate);
  cutoff.setFullYear(cutoff.getFullYear() - 1);

  // Salary in effect a year ago = most recent salaried review on/before the cutoff;
  // otherwise (history < 1 year) the earliest salaried entry.
  const prior = salaried.find((r) => new Date(r.reviewDate) <= cutoff) ?? salaried[salaried.length - 1];
  return growthBetween(prior, latest);
}

/** Total salary growth from the earliest salaried entry (baseline) to the current salary. */
export function totalGrowthLabel(reviews: Review[]): string | null {
  const salaried = reviews.filter((r) => r.newSalary != null); // newest-first
  if (salaried.length < 2) return null;
  return growthBetween(salaried[salaried.length - 1], salaried[0]);
}

/** Most recent single hike as a signed % chip label — or null when there's none. */
export function latestHikeLabel(reviews: Review[]): string | null {
  const withHike = reviews.find((r) => r.incrementPercent != null);
  return withHike ? formatHike(withHike.incrementPercent!) : null;
}

/** % change between two salaried reviews, or null if they're the same / from is zero. */
function growthBetween(from: Review, to: Review): string | null {
  if (from.id === to.id || from.newSalary === 0) return null;
  return formatHike(((to.newSalary! - from.newSalary!) / from.newSalary!) * 100);
}

/** Current (derived) salary as a raw number — the most recent salaried review's salary. */
export function currentSalaryValue(reviews: Review[]): number | null {
  return reviews.find((r) => r.newSalary != null)?.newSalary ?? null;
}

/**
 * The salary in effect immediately before a given review (the next-older salaried entry),
 * as a raw number — the base a hike% is measured against when editing that review.
 */
export function salaryBefore(reviews: Review[], reviewId: string): number | null {
  const idx = reviews.findIndex((r) => r.id === reviewId);
  if (idx < 0) return null;
  for (let i = idx + 1; i < reviews.length; i++) {
    if (reviews[i].newSalary != null) return reviews[i].newSalary!;
  }
  return null;
}
