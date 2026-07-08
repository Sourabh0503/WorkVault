import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../../core/services/auth.service';
import { DashboardService } from '../../core/services/dashboard.service';
import { WelcomeCardModel } from './welcome-card';
import { DashboardStats } from '../../core/models/dashboard.models';

@Component({
  selector: 'app-dashboard',
  imports: [RouterLink],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
})
/**
 * Dashboard landing page. For HR/CompanyAdmin it loads company aggregates
 * (`DashboardService.getStats`); other roles see a role-appropriate view without stats.
 */
export class Dashboard implements OnInit {
  private authService = inject(AuthService);
  private dashboardService = inject(DashboardService);

  private role = this.authService.role;

  // Can this user see company stats? (HR/Admin only)
  canSeeStats = computed(() =>
    this.role() === 'HR' || this.role() === 'CompanyAdmin'
  );

  loading = signal(true);
  stats = signal<DashboardStats | null>(null);

  // ---- Welcome card ----
  // Plain presentation model (not a service). Its timer is bound to this
  // component's DestroyRef, so it runs only while the page is alive.
  private welcome = new WelcomeCardModel(
    this.authService.role,
    this.authService.companyName,
    inject(DestroyRef)
  );
  card = this.welcome.card;

  nextTip(): void {
    this.welcome.nextTip();
  }

  ngOnInit(): void {
    if (this.canSeeStats()) {
      this.dashboardService.getStats().subscribe({
        next: (s) => {
          this.stats.set(s);
          this.loading.set(false);
        },
        error: (_err: HttpErrorResponse) => {
          this.loading.set(false);
        }
      });
    } else {
      this.loading.set(false);
    }
  }
}