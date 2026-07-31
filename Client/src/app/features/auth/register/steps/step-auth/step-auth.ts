import { Component, Input } from '@angular/core';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';

/**
 * Registration wizard — Step 2: setup authentication.
 * Collects the admin's identity (name + login email). No password is collected:
 * on submit the container triggers the invite email so the admin sets a password
 * via the confirmation link.
 */
@Component({
  selector: 'app-step-auth',
  imports: [ReactiveFormsModule],
  templateUrl: './step-auth.html',
  styleUrl: './step-auth.scss',
})
export class StepAuth {
  /** The wizard's shared form group (owned by the container). */
  @Input({ required: true }) form!: FormGroup;

  /** Server-side validation errors keyed by PascalCase field name. */
  @Input() fieldErrors: Record<string, string[]> = {};
}
