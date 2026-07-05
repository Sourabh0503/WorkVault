import { Component, computed, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

interface NavItem {
  label: string;
  route: string;
  icon: string;
  roles: string[];   // which roles can see this item; empty = everyone
}

@Component({
  selector: 'app-sidebar',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.scss'
})
export class Sidebar {
  private authService = inject(AuthService);

  user = this.authService.user;
  private role = this.authService.role;

  // Full nav config. `roles: []` means visible to everyone.
  private allNavItems: NavItem[] = [
    { label: 'Dashboard',   route: '/dashboard',   icon: '⌂', roles: [] },
    { label: 'My Team',     route: '/my-team',     icon: '◎', roles: [] },
    { label: 'Employees',   route: '/employees',   icon: '☰', roles: ['HR', 'CompanyAdmin'] },
    { label: 'Departments', route: '/departments', icon: '⊞', roles: ['HR', 'CompanyAdmin'] },
    { label: 'Assets',      route: '/assets',      icon: '◇', roles: ['HR', 'CompanyAdmin'] },
    { label: 'Settings',    route: '/settings',    icon: '⚙', roles: ['CompanyAdmin'] }
  ];

  // Derived: only items the current role is allowed to see
  navItems = computed(() => {
    const currentRole = this.role();
    return this.allNavItems.filter(item =>
      item.roles.length === 0 || item.roles.includes(currentRole)
    );
  },{});

  onLogout(): void {
    this.authService.logout();
  }
}