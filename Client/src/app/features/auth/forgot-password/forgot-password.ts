import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-forgot-password',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './forgot-password.html',
  styleUrl: './forgot-password.scss'
})
/**
 * Forgot-password page. Submits an email to `AuthService.forgotPassword`, then switches
 * to a "check your email" confirmation. The API always succeeds regardless of whether
 * the email exists, so the UI never reveals account existence.
 */
export class ForgotPassword {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);

  isSubmitting = signal(false);
  isSubmitted = signal(false);   // switches UI to "check your email" state

  form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]]
  });

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    const email = this.form.controls.email.value;

    this.authService.forgotPassword(email).subscribe({
      // Both success AND error should show the same "check your email" state.
      // Reason: silent-success pattern — the frontend must not reveal whether
      // the email was found. This mirrors the backend's non-enumerable design.
      next: () => this.showConfirmation(),
      error: () => this.showConfirmation()
    });
  }

  private showConfirmation(): void {
    this.isSubmitting.set(false);
    this.isSubmitted.set(true);
  }

  // Let the user retry with a different email
  onTryAgain(): void {
    this.isSubmitted.set(false);
    this.form.reset();
  }
}