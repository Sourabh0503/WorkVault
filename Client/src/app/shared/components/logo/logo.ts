import {
  Component,
  booleanAttribute,
  computed,
  input,
  numberAttribute,
} from '@angular/core';

/** What the logo shows alongside the mark. */
export type LogoVariant = 'icon' | 'full' | 'tenant';

/**
 * Brand logo. Renders the WorkVault mark (inline SVG) plus optional wordmark,
 * tenant name and tagline. Colours are driven by CSS custom properties so the
 * same component covers every context via the `mono` / `inverse` flags.
 *
 *   <app-logo variant="icon" />                        just the mark
 *   <app-logo variant="full" />                        mark + "workVault"
 *   <app-logo variant="full" tagline />                mark + wordmark + "Workplace OS"
 *   <app-logo variant="tenant" tenant="Acme Corp" />   mark + wordmark + tenant name
 *   <app-logo mono />                                   single-colour (currentColor)
 *   <app-logo inverse />                                light treatment for dark / brand bgs
 *   <app-logo [height]="48" />                          scale everything off the mark height
 */
@Component({
  selector: 'app-logo',
  templateUrl: './logo.html',
  styleUrl: './logo.scss',
  host: {
    role: 'img',
    '[attr.aria-label]': 'ariaLabel()',
    '[class.is-mono]': 'mono()',
    '[class.is-inverse]': 'inverse()',
    '[style.--logo-size.px]': 'height()',
  },
})
export class Logo {
  /** Which pieces to render. */
  readonly variant = input<LogoVariant>('full');

  /** Tenant / organisation name, shown when `variant === 'tenant'`. */
  readonly tenant = input<string>('');

  /** Show the "Workplace OS" tagline beneath the wordmark. */
  readonly tagline = input(false, { transform: booleanAttribute });

  /** Render in a single colour (currentColor) — for print, badges, watermarks. */
  readonly mono = input(false, { transform: booleanAttribute });

  /** Light treatment for placing the logo on dark or brand-coloured surfaces. */
  readonly inverse = input(false, { transform: booleanAttribute });

  /** Height of the mark in pixels; the wordmark and tagline scale off it. */
  readonly height = input(40, { transform: numberAttribute });

  readonly showText = computed(() => this.variant() !== 'icon');
  readonly showTenant = computed(
    () => this.variant() === 'tenant' && this.tenant().trim().length > 0,
  );

  readonly ariaLabel = computed(() => {
    if (this.variant() === 'icon') return 'WorkVault';
    return this.showTenant() ? `WorkVault — ${this.tenant()}` : 'WorkVault';
  });
}
