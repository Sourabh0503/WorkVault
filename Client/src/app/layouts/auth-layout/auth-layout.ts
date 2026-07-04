import { Component, computed, inject } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterOutlet } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { filter, map, startWith } from 'rxjs';

interface PanelCopy {
  kicker: string;
  heading: string;
  sub: string;
}

@Component({
  selector: 'app-auth-layout',
  imports: [RouterOutlet, RouterLink],
  templateUrl: './auth-layout.html',
  styleUrl: './auth-layout.scss',
})
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
