/**
 * Indian-rupee formatting helpers, shared by the performance charts and review cards.
 */

/** Full INR with lakh/crore grouping, e.g. 1200000 → "₹12,00,000". */
export function formatInr(value: number): string {
  return `₹${Math.round(value).toLocaleString('en-IN')}`;
}

/**
 * Compact INR for chart axes, e.g. 1200000 → "₹12L", 25000000 → "₹2.5Cr".
 * Falls back to grouped digits below one lakh.
 */
export function formatInrShort(value: number): string {
  const abs = Math.abs(value);
  if (abs >= 1_00_00_000) return `₹${trim(value / 1_00_00_000)}Cr`;
  if (abs >= 1_00_000) return `₹${trim(value / 1_00_000)}L`;
  return `₹${Math.round(value).toLocaleString('en-IN')}`;
}

/** Drop a trailing ".0" so "12.0" reads "12" but "12.5" stays. */
function trim(n: number): string {
  return n.toFixed(1).replace(/\.0$/, '');
}
