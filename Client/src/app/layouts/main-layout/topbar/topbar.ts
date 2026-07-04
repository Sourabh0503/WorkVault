import { Component, input } from '@angular/core';

@Component({
  selector: 'app-topbar',
  imports: [],
  templateUrl: './topbar.html',
  styleUrl: './topbar.scss'
})
export class Topbar {
  // Signal input — parent passes the page title
  title = input<string>('Dashboard');
}