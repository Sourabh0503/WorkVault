import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  computed,
  effect,
  inject,
  input,
  viewChild,
} from '@angular/core';
import { Chart } from 'chart.js';
import { Review } from '../../../../core/models/review.models';
import {
  ChartTheme,
  ensureChartsRegistered,
  monthYearLabel,
  readChartTheme,
  withAlpha,
} from '../chart-setup';

/**
 * Rating over time as a 0–5 bar chart (x = review date). Baselines are excluded — a
 * starting-salary entry has no real rating. Renders a placeholder when there's nothing
 * to plot (e.g. only the baseline exists yet).
 */
@Component({
  selector: 'app-rating-bar-chart',
  templateUrl: './rating-bar-chart.html',
  styleUrl: './rating-bar-chart.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RatingBarChart {
  /** The employee's reviews, newest first (as returned by the API). */
  readonly reviews = input.required<Review[]>();

  private readonly canvas = viewChild<ElementRef<HTMLCanvasElement>>('canvas');
  private chart?: Chart;

  // Baselines carry no rating — drop them; oldest→newest for the x-axis.
  private readonly points = computed(() =>
    this.reviews()
      .filter((r) => !r.isBaseline)
      .slice()
      .reverse(),
  );

  readonly hasData = computed(() => this.points().length > 0);

  constructor() {
    ensureChartsRegistered();
    inject(DestroyRef).onDestroy(() => this.chart?.destroy());

    // Rebuild whenever the data or the canvas (mounted only when hasData) changes.
    effect(() => {
      const canvas = this.canvas()?.nativeElement;
      const points = this.points();
      this.chart?.destroy();
      if (!canvas || points.length === 0) return;
      this.chart = new Chart(canvas, this.buildConfig(points, readChartTheme()));
    });
  }

  private buildConfig(points: Review[], theme: ChartTheme): any {
    return {
      type: 'bar',
      data: {
        labels: points.map((p) => monthYearLabel(p.reviewDate)),
        datasets: [
          {
            label: 'Rating',
            data: points.map((p) => p.rating),
            backgroundColor: withAlpha(theme.primary, 0.85),
            hoverBackgroundColor: theme.primary,
            borderRadius: 6,
            maxBarThickness: 44,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        scales: {
          y: {
            min: 0,
            max: 5,
            ticks: { stepSize: 1, color: theme.text },
            grid: { color: theme.grid },
          },
          x: {
            ticks: { color: theme.text },
            grid: { display: false },
          },
        },
        plugins: {
          legend: { display: false },
          tooltip: {
            callbacks: {
              title: (items: any) => points[items[0].dataIndex]?.reviewName ?? '',
              label: (item: any) => `Rating: ${item.formattedValue} / 5`,
            },
          },
        },
      },
    };
  }
}
