import { Component, input } from '@angular/core';

@Component({
  selector: 'app-topbar',
  imports: [],
  templateUrl: './topbar.html',
  styleUrl: './topbar.scss'
})
/** Top bar for the main layout. Displays the current page title supplied by the parent. */
export class Topbar {
  /** Page title, passed in by {@link MainLayout} from the active route's `data.title`. */
  title = input<string>('Dashboard');
}