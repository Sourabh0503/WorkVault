import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../../core/services/auth.service';
import { DashboardService } from '../../core/services/dashboard.service';
import { DashboardStats } from '../../core/models/dashboard.models';

@Component({
  selector: 'app-dashboard',
  imports: [RouterLink],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
})
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