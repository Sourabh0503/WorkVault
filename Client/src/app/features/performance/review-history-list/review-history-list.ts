import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { Review } from '../../../core/models/review.models';
import { formatInr } from '../../../shared/utils/currency';
import { formatHike, hikeDirection } from '../../../shared/utils/hike';
import { monthYearLabel } from '../charts/chart-setup';

/**
 * The full review history, newest first. Each row shows the rating (a "Base" chip for the
 * baseline), the date and salary/hike, and the summary. Edit/delete actions appear only
 * when the viewer can manage reviews AND the row is still within its 30-day window;
 * otherwise a lock icon indicates it's immutable.
 */
@Component({
  selector: 'app-review-history-list',
  templateUrl: './review-history-list.html',
  styleUrl: './review-history-list.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ReviewHistoryList {
  /** All reviews, newest first. */
  readonly reviews = input.required<Review[]>();

  /** Whether the viewer may edit/delete (HR/Admin). Backend still enforces. */
  readonly canManage = input(false);

  readonly edit = output<Review>();
  readonly remove = output<Review>();

  dateLabel(review: Review): string {
    return monthYearLabel(review.reviewDate);
  }

  ratingChip(review: Review): string {
    return review.isBaseline ? 'Base' : review.rating.toString();
  }

  /** Salary text for the meta line, e.g. "₹12,00,000" (baseline adds "· starting"). */
  salaryText(review: Review): string {
    if (review.newSalary == null) return '';
    const salary = formatInr(review.newSalary);
    return review.isBaseline ? `${salary} · starting` : salary;
  }

  /** Hike label for the chip, or empty when there's none. */
  hikeText(review: Review): string {
    return review.incrementPercent != null ? formatHike(review.incrementPercent) : '';
  }

  hikeDir(review: Review): 'up' | 'down' | 'flat' {
    return hikeDirection(review.incrementPercent ?? 0);
  }

  canModify(review: Review): boolean {
    return this.canManage() && review.isMutable;
  }

  /** The baseline (starting salary) can be edited within its window but never deleted. */
  canDelete(review: Review): boolean {
    return this.canModify(review) && !review.isBaseline;
  }

  isLocked(review: Review): boolean {
    return this.canManage() && !review.isMutable;
  }
}
