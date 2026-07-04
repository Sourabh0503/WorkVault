import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

interface NavItem {
  label: string;
  route: string;
  icon: string;
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

  navItems: NavItem[] = [
    { label: 'Dashboard', route: '/dashboard', icon: '⌂' },
    { label: 'Employees', route: '/employees', icon: '☰' },
    { label: 'Departments', route: '/departments', icon: '⊞' },
    { label: 'Assets', route: '/assets', icon: '◇' },
    { label: 'Settings', route: '/settings', icon: '⚙' }
  ];

  onLogout(): void {
    this.authService.logout();
  }
}