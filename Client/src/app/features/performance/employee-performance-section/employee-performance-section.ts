import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  effect,
  inject,
} from '@angular/core';
import { RatingBarChart } from '../charts/rating-bar-chart/rating-bar-chart';
import { SalaryLineChart } from '../charts/salary-line-chart/salary-line-chart';
import { RecentReviewCard } from '../recent-review-card/recent-review-card';
import { ReviewHistoryList } from '../review-history-list/review-history-list';
import { ReviewForm } from '../review-form/review-form';
import { SalaryGrowthChip } from '../salary-growth-chip/salary-growth-chip';
import { ReviewsStore } from '../reviews-store';

/**
 * Performance section for the employee detail page. Presentational: it renders the shared
 * {@link ReviewsStore} (recent card, charts, history, add/edit form, empty/error states)
 * and forwards user actions to it. The store is provided at the page, so the hero and this
 * section share one instance — no duplicate fetch, no reaching across components.
 */
@Component({
  selector: 'app-employee-performance-section',
  imports: [RatingBarChart, SalaryLineChart, RecentReviewCard, ReviewHistoryList, ReviewForm, SalaryGrowthChip],
  templateUrl: './employee-performance-section.html',
  styleUrl: './employee-performance-section.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EmployeePerformanceSection {
  readonly store = inject(ReviewsStore);
  private host = inject(ElementRef<HTMLElement>);

  constructor() {
    // Bring the form into view when it opens — edit is triggered from the history below,
    // and Add review from the hero above. Purely a view concern, so it lives here.
    effect(() => {
      if (this.store.formOpen()) {
        requestAnimationFrame(() =>
          this.host.nativeElement.scrollIntoView({ behavior: 'smooth', block: 'start' }),
        );
      }
    });
  }
}
