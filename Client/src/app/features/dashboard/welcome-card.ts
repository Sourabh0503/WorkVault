import { computed, DestroyRef, Signal, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { interval } from 'rxjs';

/** View data for the dashboard welcome card. */
export interface WelcomeCard {
  /** Time-of-day greeting, e.g. "Good morning". */
  greeting: string;
  /** Emoji matching the time of day. */
  emoji: string;
  /** Friendly label for the current user's role. */
  roleLabel: string;
  /** Human-readable date, e.g. "Wednesday, 8 July". */
  date: string;
  /** Current local time, e.g. "2:30 PM". */
  time: string;
  /** Company name for the "Welcome to …" line. */
  companyName: string;
  /** The currently shown onboarding tip. */
  tip: string;
}

/**
 * Presentation model for the dashboard welcome card. A plain class (not an
 * Angular service) because it's view-only state for a single component: it
 * takes its inputs as signals and exposes `card` as a derived signal. The
 * tick timer is tied to the owner's `DestroyRef`, so it stops automatically
 * when the view is destroyed — no manual teardown needed.
 */
export class WelcomeCardModel {
  private readonly tips = [
    'Keep your team directory up to date so everyone stays connected.',
    'Pending invites? A quick nudge helps new folks get onboarded faster.',
    'Organize people into departments to keep your workspace tidy.',
    'Your profile helps teammates know who does what — keep it current.',
    'Explore My Team to see the people you work with every day.'
  ];

  // The only mutable state: the current clock and which tip is shown.
  private readonly now = signal(new Date());
  private readonly tipIndex = signal(0);

  /** Derived view model — recomputes only when the clock or the tip changes. */
  readonly card = computed<WelcomeCard>(() => {
    const now = this.now();
    return {
      greeting: this.timeGreeting(now),
      emoji: this.timeEmoji(now),
      roleLabel: this.roleLabel(),
      date: this.todayLabel(now),
      time: this.currentTime(now),
      companyName: this.companyName(),
      tip: this.tips[this.tipIndex()]
    };
  });

  constructor(
    private readonly role: Signal<string>,
    private readonly companyName: Signal<string>,
    destroyRef: DestroyRef
  ) {
    // A single interval drives everything: tick the clock and advance the tip
    // on the same beat. `card` recomputes because both are signals. The
    // subscription auto-unsubscribes when the owner's DestroyRef fires.
    interval(6000)
      .pipe(takeUntilDestroyed(destroyRef))
      .subscribe(() => {
        this.now.set(new Date());
        this.nextTip();
      });
  }

  /** Advance to the next onboarding tip (wraps around). */
  nextTip(): void {
    this.tipIndex.update((i) => (i + 1) % this.tips.length);
  }

  // ---- Formatting helpers ----

  private currentTime(now: Date): string {
    return now.toLocaleTimeString([], { hour: 'numeric', minute: '2-digit' });
  }

  private timeGreeting(now: Date): string {
    const hour = now.getHours();
    if (hour >= 5 && hour < 12) return 'Good morning';
    if (hour >= 12 && hour < 17) return 'Good afternoon';
    return 'Good evening';
  }

  private timeEmoji(now: Date): string {
    const hour = now.getHours();
    if (hour >= 5 && hour < 12) return '☀️';
    if (hour >= 12 && hour < 17) return '🌤️';
    return '🌙';
  }

  private roleLabel(): string {
    switch (this.role()) {
      case 'CompanyAdmin': return 'Company Admin';
      case 'HR': return 'HR';
      case 'Manager': return 'Manager';
      default: return 'Team member';
    }
  }

  private todayLabel(now: Date): string {
    return now.toLocaleDateString(undefined, {
      weekday: 'long',
      day: 'numeric',
      month: 'long'
    });
  }
}
