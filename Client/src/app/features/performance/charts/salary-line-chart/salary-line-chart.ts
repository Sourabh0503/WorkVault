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
import { formatInr, formatInrShort } from '../../../../shared/utils/currency';
import {
  ChartTheme,
  ensureChartsRegistered,
  monthYearLabel,
  readChartTheme,
  withAlpha,
} from '../chart-setup';

/**
 * Salary progression as a line chart (x = review date). Only reviews that carry a salary
 * are plotted — the baseline is the first point, and cycles without a raise are skipped
 * (the line spans the gap). Renders a placeholder when there's no salary to show; the
 * container decides whether to mount this at all (managers never see it).
 */
@Component({
  selector: 'app-salary-line-chart',
  templateUrl: './salary-line-chart.html',
  styleUrl: './salary-line-chart.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SalaryLineChart {
  /** The employee's reviews, newest first (as returned by the API). */
  readonly reviews = input.required<Review[]>();

  private readonly canvas = viewChild<ElementRef<HTMLCanvasElement>>('canvas');
  private chart?: Chart;

  // Only salaried entries, oldest→newest for the x-axis.
  private readonly points = computed(() =>
    this.reviews()
      .filter((r) => r.newSalary != null)
      .slice()
      .reverse(),
  );

  readonly hasData = computed(() => this.points().length > 0);

  constructor() {
    ensureChartsRegistered();
    inject(DestroyRef).onDestroy(() => this.chart?.destroy());

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
      type: 'line',
      data: {
        labels: points.map((p) => monthYearLabel(p.reviewDate)),
        datasets: [
          {
            label: 'Salary',
            data: points.map((p) => p.newSalary),
            borderColor: theme.primary,
            backgroundColor: withAlpha(theme.primary, 0.12),
            pointBackgroundColor: theme.primary,
            pointRadius: 4,
            pointHoverRadius: 6,
            borderWidth: 2,
            tension: 0.3,
            fill: true,
            spanGaps: true,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        // A single point would otherwise sit flush against the axis; padding gives it room.
        layout: { padding: points.length === 1 ? { left: 24, right: 24 } : 0 },
        scales: {
          y: {
            ticks: { color: theme.text, callback: (v: number) => formatInrShort(v) },
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
              label: (item: any) => formatInr(item.raw),
            },
          },
        },
      },
    };
  }
}
