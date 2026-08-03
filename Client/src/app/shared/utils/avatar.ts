/**
 * Deterministic avatar helpers — a stable colour and initials from an identity key,
 * so the same person always renders the same avatar.
 */

const AVATAR_PALETTE = [
  '#6366F1', '#0EA5E9', '#059669', '#D97706',
  '#DC2626', '#7C3AED', '#DB2777', '#0891B2',
];

/** Pick a stable palette colour for a key (e.g. an employee code or id). */
export function avatarColor(key: string): string {
  let hash = 0;
  for (const ch of key) hash = (hash * 31 + ch.charCodeAt(0)) >>> 0;
  return AVATAR_PALETTE[hash % AVATAR_PALETTE.length];
}

/** Two-letter uppercase initials from first + last name. */
export function initials(firstName: string, lastName: string): string {
  return `${firstName.charAt(0)}${lastName.charAt(0)}`.toUpperCase();
}
