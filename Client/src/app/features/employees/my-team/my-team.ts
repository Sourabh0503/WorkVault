import { Component, OnInit, inject, signal } from '@angular/core';
import { EmployeeService } from '../../../core/services/employee.service';
import { TeamMember } from '../../../core/models/employee.models';

@Component({
  selector: 'app-my-team',
  imports: [],
  templateUrl: './my-team.html',
  styleUrl: './my-team.scss'
})
export class MyTeam implements OnInit {
  private employeeService = inject(EmployeeService);

  loading = signal(true);
  error = signal<string | null>(null);
  departmentName = signal<string | null>(null);
  members = signal<TeamMember[]>([]);

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

  // First-letter avatar fallback when no photo
  initials(fullName: string): string {
    const parts = fullName.trim().split(/\s+/);
    if (parts.length === 0 || parts[0] === '') return '?';
    const first = parts[0].charAt(0);
    const last = parts.length > 1 ? parts[parts.length - 1].charAt(0) : '';
    return (first + last).toUpperCase();
  }
}