import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { Review } from '../../../core/models/review.models';
import { formatInr } from '../../../shared/utils/currency';
import { formatHike, hikeDirection } from '../../../shared/utils/hike';
import { monthYearLabel } from '../charts/chart-setup';

/**
 * Highlights an employee's most recent review: name, month-year, rating with a progress
 * bar (hidden for a baseline), summary, and salary with a hike chip or "Starting salary"
 * badge. Salary shows only when the review carries one (nulled server-side otherwise).
 */
@Component({
  selector: 'app-recent-review-card',
  templateUrl: './recent-review-card.html',
  styleUrl: './recent-review-card.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RecentReviewCard {
  readonly review = input.required<Review>();

  readonly dateLabel = computed(() => monthYearLabel(this.review().reviewDate));
  readonly showRating = computed(() => !this.review().isBaseline);
  readonly ratingPct = computed(() => Math.max(0, Math.min(100, (this.review().rating / 5) * 100)));
  readonly showSalary = computed(() => this.review().newSalary != null);
  readonly salaryLabel = computed(() => formatInr(this.review().newSalary!));

  readonly hike = computed(() => {
    const pct = this.review().incrementPercent;
    return pct == null ? null : { label: formatHike(pct), direction: hikeDirection(pct) };
  });
}
