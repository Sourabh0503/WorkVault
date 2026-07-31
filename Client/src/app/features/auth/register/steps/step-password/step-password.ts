import { Component, Input, signal } from '@angular/core';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { InviteInfo } from '../../../../../core/models/auth.models';

/**
 * Registration wizard — Step 3: setup password.
 * Reached via the emailed confirmation link (`/register/:token`). Sets the
 * admin's first password with a live strength checklist and confirm-match.
 * The container owns token validation and the set-password submission.
 */
@Component({
  selector: 'app-step-password',
  imports: [ReactiveFormsModule],
  templateUrl: './step-password.html',
  styleUrl: './step-password.scss',
})
export class StepPassword {
  /** The wizard's shared form group (owned by the container). */
  @Input({ required: true }) form!: FormGroup;

  /** Invite details resolved from the token — used to greet + show the locked email. */
  @Input() invite: InviteInfo | null = null;

  showPassword = signal(false);
  showConfirmPassword = signal(false);

  togglePassword(): void {
    this.showPassword.update((v) => !v);
  }

  toggleConfirmPassword(): void {
    this.showConfirmPassword.update((v) => !v);
  }

  /**
   * Live password requirement checks. A plain method (not a computed signal) so
   * default change detection re-evaluates it as the user types — the form value
   * isn't a signal we can depend on.
   */
  passwordChecks(): { length: boolean; upperAndDigit: boolean; special: boolean } {
    const p: string = this.form?.get('password')?.value ?? '';
    return {
      length: p.length >= 8,
      upperAndDigit: /[A-Z]/.test(p) && /[0-9]/.test(p),
      special: /[^a-zA-Z0-9]/.test(p),
    };
  }
}
