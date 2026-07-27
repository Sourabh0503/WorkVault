import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../../core/services/auth.service';
import { DashboardService } from '../../core/services/dashboard.service';
import { EmployeeService } from '../../core/services/employee.service';
import { DepartmentService } from '../../core/services/department.service';
import { WelcomeCardModel } from './welcome-card';
import { DashboardStats, HeadcountPoint } from '../../core/models/dashboard.models';
import { EmployeeListItem, EmployeeStatus } from '../../core/models/employee.models';
import { Department } from '../../core/models/department.models';
import { CurrentUserProfile } from '../../core/models/profile.models';

@Component({
  selector: 'app-dashboard',
  imports: [RouterLink],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
/**
 * Dashboard landing page. HR/CompanyAdmin see company aggregates: KPI band, headcount
 * trend, workforce pulse, a departments overview, and pending invites (with resend).
 * Other roles get a role-appropriate welcome view without company stats.
 */
export class Dashboard implements OnInit {
  private authService = inject(AuthService);
  private dashboardService = inject(DashboardService);
  private employeeService = inject(EmployeeService);
  private departmentService = inject(DepartmentService);

  private role = this.authService.role;
  firstName = this.authService.firstName;

  // Can this user see company stats? (HR/Admin only)
  canSeeStats = computed(() => this.role() === 'HR' || this.role() === 'CompanyAdmin');

  // ---- Celebrations (own birthday / work anniversary today) ----
  private myProfile = signal<CurrentUserProfile | null>(null);

  celebration = computed(() => {
    const emp = this.myProfile()?.employee;
    const now = new Date();
    const month = now.getMonth() + 1;
    const day = now.getDate();

    let birthday = false;
    let anniversaryYears: number | null = null;

    if (emp?.dateOfBirth) {
      const [, bm, bd] = emp.dateOfBirth.split('-').map(Number);
      if (bm === month && bd === day) birthday = true;
    }
    if (emp?.joinDate) {
      const [jy, jm, jd] = emp.joinDate.split('-').map(Number);
      if (jm === month && jd === day) {
        const years = now.getFullYear() - jy;
        if (years >= 1) anniversaryYears = years;
      }
    }
    return { birthday, anniversaryYears };
  });

  hasCelebration = computed(() => {
    const c = this.celebration();
    return c.birthday || c.anniversaryYears !== null;
  });

  loading = signal(true);
  stats = signal<DashboardStats | null>(null);
  headcount = signal<HeadcountPoint[]>([]);
  pendingInvites = signal<EmployeeListItem[]>([]);
  departments = signal<Department[]>([]);

  // Resend-invite per-row state
  private resendingId = signal<string | null>(null);
  private sentIds = signal<Set<string>>(new Set());

  // Tallest bar → scales the headcount chart heights.
  private maxJoin = computed(() => Math.max(1, ...this.headcount().map((h) => h.count)));

  chart = computed(() =>
    this.headcount().map((h) => ({
      month: h.month,
      count: h.count,
      heightPct: Math.round((h.count / this.maxJoin()) * 100),
    })),
  );

  // Workforce composition — real ratios that sum to the whole company.
  pulse = computed(() => {
    const s = this.stats();
    if (!s || s.totalEmployees === 0) return [];
    const other = Math.max(0, s.totalEmployees - s.activeEmployees - s.pendingInvites);
    const pct = (n: number) => Math.round((n / s.totalEmployees) * 100);
    return [
      { label: 'Active workforce', value: s.activeEmployees, pct: pct(s.activeEmployees) },
      { label: 'Pending onboarding', value: s.pendingInvites, pct: pct(s.pendingInvites) },
      { label: 'On notice / other', value: other, pct: pct(other) },
    ];
  });

  // Top departments by headcount for the overview panel.
  topDepartments = computed(() =>
    [...this.departments()].sort((a, b) => b.memberCount - a.memberCount).slice(0, 5),
  );

  // ---- Welcome card (non-HR view) ----
  private welcome = new WelcomeCardModel(
    this.authService.role,
    this.authService.companyName,
    inject(DestroyRef),
  );
  card = this.welcome.card;

  nextTip(): void {
    this.welcome.nextTip();
  }

  ngOnInit(): void {
    // Load own profile to check for birthday / work anniversary (all roles).
    this.authService.getMe().subscribe({
      next: (p) => this.myProfile.set(p),
      error: () => {},
    });

    if (!this.canSeeStats()) {
      this.loading.set(false);
      return;
    }

    this.dashboardService.getStats().subscribe({
      next: (s) => {
        this.stats.set(s);
        this.loading.set(false);
      },
      error: (_err: HttpErrorResponse) => this.loading.set(false),
    });

    this.dashboardService.getHeadcountTrend().subscribe({
      next: (t) => this.headcount.set(t),
      error: () => {},
    });

    this.employeeService
      .getEmployees({ pageNumber: 1, pageSize: 5, status: EmployeeStatus.Pending })
      .subscribe({
        next: (res) => this.pendingInvites.set(res.items),
        error: () => {},
      });

    this.departmentService.getDepartments().subscribe({
      next: (d) => this.departments.set(d),
      error: () => {},
    });
  }

  initials(fullName: string): string {
    const parts = fullName.trim().split(/\s+/);
    const first = parts[0]?.charAt(0) ?? '';
    const last = parts.length > 1 ? parts[parts.length - 1].charAt(0) : '';
    return (first + last).toUpperCase() || '?';
  }

  isResending(id: string): boolean {
    return this.resendingId() === id;
  }

  isSent(id: string): boolean {
    return this.sentIds().has(id);
  }

  resendInvite(id: string): void {
    if (this.isResending(id) || this.isSent(id)) return;
    this.resendingId.set(id);
    this.employeeService.resendInvite(id).subscribe({
      next: () => {
        this.resendingId.set(null);
        this.sentIds.update((s) => new Set(s).add(id));
      },
      error: () => this.resendingId.set(null),
    });
  }
}
