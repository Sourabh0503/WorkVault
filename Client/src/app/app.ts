import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
/** Root component — the app shell that hosts the top-level `<router-outlet>`. */
export class App {
  protected readonly title = signal('Client');
}
