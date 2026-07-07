import { Component, inject } from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { filter, map, startWith } from 'rxjs';
import { Sidebar } from './sidebar/sidebar';
import { Topbar } from './topbar/topbar';

@Component({
  selector: 'app-main-layout',
  imports: [RouterOutlet, Sidebar, Topbar],
  templateUrl: './main-layout.html',
  styleUrl: './main-layout.scss'
})
/**
 * Shell for authenticated pages: renders the sidebar + topbar around a `<router-outlet>`.
 * Derives the topbar title from the deepest active route's `data.title` on every navigation.
 */
export class MainLayout {
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  // Derive the page title from the active route's data
  pageTitle = toSignal(
    this.router.events.pipe(
      filter(e => e instanceof NavigationEnd),
      map(() => this.resolveTitle()),
      startWith(this.resolveTitle())
    ),
    { initialValue: 'Dashboard' }
  );

  private resolveTitle(): string {
    // Walk to the deepest activated child route
    let child = this.route.firstChild;
    while (child?.firstChild) {
      child = child.firstChild;
    }
    return child?.snapshot?.data['title'] ?? 'Dashboard';
  }
}