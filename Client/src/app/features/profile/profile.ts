import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { AuthService } from '../../core/services/auth.service';
import { CurrentUserProfile } from '../../core/models/profile.models';
import { EMPLOYEE_STATUS_LABELS, EmployeeStatus } from '../../core/models/employee.models';

@Component({
  selector: 'app-profile',
  imports: [DatePipe],
  templateUrl: './profile.html',
  styleUrl: './profile.scss',
})
/**
 * My Profile page. Read-only view of the signed-in user's own account (name, email,
 * role, tenant) and employee details (code, department, designation, join date, status),
 * sourced from `AuthService.getMe` (GET /api/auth/me).
 */
export class Profile implements OnInit {
  private authService = inject(AuthService);

  loading = signal(true);
  error = signal<string | null>(null);
  profile = signal<CurrentUserProfile | null>(null);

  initials = computed(() => {
    const p = this.profile();
    if (!p) return '';
    return `${p.firstName.charAt(0)}${p.lastName.charAt(0)}`.toUpperCase();
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.authService.getMe().subscribe({
      next: (profile) => {
        this.profile.set(profile);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load your profile.');
        this.loading.set(false);
      },
    });
  }

  statusLabel(status: EmployeeStatus): string {
    return EMPLOYEE_STATUS_LABELS[status] ?? 'Unknown';
  }

  roleLabel(name: string): string {
    return name === 'CompanyAdmin' ? 'Company Admin' : name;
  }
}
