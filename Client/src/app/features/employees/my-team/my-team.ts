import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { EmployeeService } from '../../../core/services/employee.service';
import { AuthService } from '../../../core/services/auth.service';
import { TeamMember } from '../../../core/models/employee.models';

@Component({
  selector: 'app-my-team',
  imports: [],
  templateUrl: './my-team.html',
  styleUrl: './my-team.scss'
})
/**
 * My Team page. Loads the signed-in user's department roster via
 * `EmployeeService.getMyTeam` and renders members (highlighting "you"). Empty when the
 * user has no department assigned.
 */
export class MyTeam implements OnInit {
  private employeeService = inject(EmployeeService);
  private authService = inject(AuthService);
  private router = inject(Router);

  loading = signal(true);
  error = signal<string | null>(null);
  departmentName = signal<string | null>(null);
  members = signal<TeamMember[]>([]);

  // Viewer's access role, shown in the team banner.
  myRole = this.authService.role;

  // First letter of the department, for the banner tile.
  deptInitial = computed(() => (this.departmentName()?.trim().charAt(0) || '·').toUpperCase());

  ngOnInit(): void {
    this.employeeService.getMyTeam().subscribe({
      next: (result) => {
        this.departmentName.set(result.departmentName);
        this.members.set(result.members);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load your team. Please try again.');
        this.loading.set(false);
      }
    });
  }

  // Open a member's profile (read-only unless the viewer is HR/CompanyAdmin).
  openMember(id: string): void {
    this.router.navigate(['/employees', id]);
  }

  // First-letter avatar fallback when no photo
  initials(fullName: string): string {
    const parts = fullName.trim().split(/\s+/);
    if (parts.length === 0 || parts[0] === '') return '?';
    const first = parts[0].charAt(0);
    const last = parts.length > 1 ? parts[parts.length - 1].charAt(0) : '';
    return (first + last).toUpperCase();
  }
}