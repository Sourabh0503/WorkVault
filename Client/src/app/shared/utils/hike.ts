/**
 * Formatting for the derived salary hike (incrementPercent) shown on review cards.
 */

/** e.g. 8.5 → "+8.5%", -3 → "−3%" (true minus sign). Null-safe caller responsibility. */
export function formatHike(percent: number): string {
  const sign = percent > 0 ? '+' : percent < 0 ? '−' : '';
  const magnitude = Math.abs(percent).toFixed(1).replace(/\.0$/, '');
  return `${sign}${magnitude}%`;
}

/** Direction class for styling the hike chip. */
export function hikeDirection(percent: number): 'up' | 'down' | 'flat' {
  if (percent > 0) return 'up';
  if (percent < 0) return 'down';
  return 'flat';
}
