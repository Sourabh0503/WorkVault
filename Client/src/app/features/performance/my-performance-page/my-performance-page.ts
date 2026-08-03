import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { Review, hasVisibleSalary } from '../../../core/models/review.models';
import { ReviewService } from '../../../core/services/review.service';
import { RatingBarChart } from '../charts/rating-bar-chart/rating-bar-chart';
import { SalaryLineChart } from '../charts/salary-line-chart/salary-line-chart';
import { RecentReviewCard } from '../recent-review-card/recent-review-card';
import { ReviewHistoryList } from '../review-history-list/review-history-list';
import { SalaryGrowthChip } from '../salary-growth-chip/salary-growth-chip';
import { averageRating, currentSalaryLabel } from '../review-stats';

/**
 * Read-only "My Performance" page: the current user's own ratings, salary progression, and
 * review history. Same presentational pieces as the employee performance section, but no
 * add/edit — data comes from GET /api/me/performance (own salary always included).
 */
@Component({
  selector: 'app-my-performance-page',
  imports: [RatingBarChart, SalaryLineChart, RecentReviewCard, ReviewHistoryList, SalaryGrowthChip],
  templateUrl: './my-performance-page.html',
  styleUrl: './my-performance-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MyPerformancePage implements OnInit {
  private reviewService = inject(ReviewService);

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly reviews = signal<Review[]>([]);

  readonly mostRecent = computed<Review | null>(() => this.reviews()[0] ?? null);
  readonly isEmpty = computed(() => this.reviews().length === 0);
  readonly showSalaryChart = computed(() => hasVisibleSalary(this.reviews()));
  readonly avgRating = computed(() => averageRating(this.reviews()));
  readonly currentSalary = computed(() => currentSalaryLabel(this.reviews()));

  ngOnInit(): void {
    this.reviewService.getMyPerformance().subscribe({
      next: (reviews) => {
        this.reviews.set(reviews);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load your performance data.');
        this.loading.set(false);
      },
    });
  }
}
