import { Chart, registerables } from 'chart.js';

/**
 * Chart.js setup shared by the performance charts: one-time registration of the built-in
 * controllers/scales/elements, and a helper that resolves the app's CSS design tokens to
 * concrete colours (canvas can't read CSS custom properties directly). Reading them at
 * render time keeps the charts correct in both light and dark themes.
 */

let registered = false;

/** Register Chart.js built-ins exactly once (idempotent across chart instances). */
export function ensureChartsRegistered(): void {
  if (registered) return;
  Chart.register(...registerables);
  registered = true;
}

export interface ChartTheme {
  primary: string;
  text: string;
  grid: string;
  surface: string;
}

/** Resolve chart colours from the current theme's CSS custom properties. */
export function readChartTheme(): ChartTheme {
  const s = getComputedStyle(document.documentElement);
  const v = (name: string, fallback: string) => s.getPropertyValue(name).trim() || fallback;
  return {
    primary: v('--color-primary', '#059669'),
    text: v('--color-ink-muted', '#64748B'),
    grid: v('--color-border', '#E9EEF4'),
    surface: v('--color-surface', '#FFFFFF'),
  };
}

/** Turn a #RRGGBB colour into rgba() at the given alpha (for fills). Non-hex passes through. */
export function withAlpha(color: string, alpha: number): string {
  const m = /^#?([0-9a-f]{6})$/i.exec(color.trim());
  if (!m) return color;
  const int = parseInt(m[1], 16);
  const r = (int >> 16) & 255;
  const g = (int >> 8) & 255;
  const b = int & 255;
  return `rgba(${r}, ${g}, ${b}, ${alpha})`;
}

/** Format a "yyyy-MM-dd" review date as a short month-year label, e.g. "Mar 2026". */
export function monthYearLabel(isoDate: string): string {
  const d = new Date(isoDate);
  if (isNaN(d.getTime())) return isoDate;
  return d.toLocaleDateString(undefined, { month: 'short', year: 'numeric' });
}
