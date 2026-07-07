import { Component, computed, inject } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterOutlet } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { filter, map, startWith } from 'rxjs';
import { Logo } from '../../shared/components/logo/logo';

interface PanelCopy {
  kicker: string;
  heading: string;
  sub: string;
}

@Component({
  selector: 'app-auth-layout',
  imports: [RouterOutlet, RouterLink, Logo],
  templateUrl: './auth-layout.html',
  styleUrl: './auth-layout.scss',
})
/**
 * Shell for the auth pages: a green brand panel beside the form column. The panel's copy
 * (kicker/heading/sub) is chosen reactively from the current route so each auth screen
 * gets tailored messaging.
 */
export class AuthLayout {
  private router = inject(Router);

  // Track the current URL as a signal — updates on every navigation
  private currentUrl = toSignal(
    this.router.events.pipe(
      filter((event) => event instanceof NavigationEnd),
      map((event) => (event as NavigationEnd).urlAfterRedirects),
      startWith(this.router.url),
    ),
    { initialValue: this.router.url },
  );

  // Derived signal — the panel copy for the current route
  copy = computed<PanelCopy>(() => {
    const url = this.currentUrl();

    if (url.startsWith('/register')) {
      return {
        kicker: 'Zero to live in minutes',
        heading: 'Pick your industry. Watch the system build itself.',
        sub: 'Departments, designations, asset types and leave policies pre-populated by AI.',
      };
    }

    if (url.startsWith('/set-password')) {
      return {
        kicker: "You've been invited",
        heading: 'Your seat is ready. Set a password to begin.',
        sub: "You're joining as an HR Manager. Bring your team, assets, and processes into one system.",
      };
    }

    if (url.startsWith('/forgot-password')) {
      return {
        kicker: 'Password recovery',
        heading: 'One email away from getting back in.',
        sub: "Enter your work email and we'll send a link to set a new password. It's valid for 2 hours.",
      };
    }

    if (url.startsWith('/reset-password')) {
      return {
        kicker: 'Secure reset',
        heading: 'A new password, a fresh start.',
        sub: 'For your safety, this link is valid for 2 hours and can only be used once. All active sessions will be signed out.',
      };
    }

    // Default = login
    return {
      kicker: 'Workspace access',
      heading: 'One system for your entire workplace.',
      sub: 'Employees, assets, attendance, bookings and compliance — scoped to your company, isolated by design.',
    };
  });
}
