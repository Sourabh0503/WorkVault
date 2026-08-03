import { ChangeDetectionStrategy, Component, computed, input, signal } from '@angular/core';
import { Review } from '../../../core/models/review.models';
import { latestHikeLabel, totalGrowthLabel, yearlyGrowthLabel } from '../review-stats';

type GrowthPeriod = '1yr' | 'total' | 'latest';

/**
 * The salary-growth stat shown on the "Salary progression" card: a small period selector
 * (last 1 year / total / latest hike) plus the resulting % chip. Self-contained — it owns
 * the selected period and derives the label from the reviews it's given.
 */
@Component({
  selector: 'app-salary-growth-chip',
  templateUrl: './salary-growth-chip.html',
  styleUrl: './salary-growth-chip.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SalaryGrowthChip {
  readonly reviews = input.required<Review[]>();
  readonly period = signal<GrowthPeriod>('1yr');

  readonly options: { value: GrowthPeriod; label: string }[] = [
    { value: '1yr', label: 'Last 1 year' },
    { value: 'total', label: 'Total' },
    { value: 'latest', label: 'Latest hike' },
  ];

  readonly label = computed<string | null>(() => {
    const reviews = this.reviews();
    switch (this.period()) {
      case '1yr': return yearlyGrowthLabel(reviews);
      case 'total': return totalGrowthLabel(reviews);
      case 'latest': return latestHikeLabel(reviews);
    }
  });

  setPeriod(value: string): void {
    this.period.set(value as GrowthPeriod);
  }
}
