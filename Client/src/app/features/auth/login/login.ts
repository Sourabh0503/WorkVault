import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { ApiError } from '../../../core/models/auth.models';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
/**
 * Login page. Collects email + password, submits via `AuthService.login`, surfaces
 * API errors, and toggles password visibility. On success the service stores the
 * session and the guard routes onward.
 */
export class Login {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  // ---- UI state ----
  isSubmitting = signal(false);
  errorMessage = signal<string | null>(null);
  showPassword = signal(false);

  // Set when login fails because the account (a self-registered admin) isn't
  // activated yet — we then offer to resend the confirmation link.
  notActivated = signal(false);
  // 'idle' | 'sending' | 'sent' — drives the resend button state.
  resendState = signal<'idle' | 'sending' | 'sent'>('idle');

  togglePassword(): void {
    this.showPassword.update(v => !v);
  }

  // ---- Form definition ----
  form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]]
  });

  onSubmit(): void {
    // Clear any previous error / resend state
    this.errorMessage.set(null);
    this.notActivated.set(false);
    this.resendState.set('idle');

    // Trigger validation display
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);

    this.authService.login(this.form.getRawValue()).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.router.navigate(['/dashboard']);
      },
      error: (err: HttpErrorResponse) => {
        this.isSubmitting.set(false);
        const apiError = err.error as ApiError;

        // Unactivated admin — offer to resend the confirmation link instead of
        // pretending it's a wrong password.
        if (err.status === 403 && apiError?.type === 'AccountNotActivated') {
          this.notActivated.set(true);
          this.errorMessage.set(apiError.title);
          return;
        }

        // Everything else: generic message, don't reveal account state.
        this.errorMessage.set(
          apiError?.title || 'Login failed. Check your email and password.'
        );
      }
    });
  }

  /** Resend the account-confirmation email to the address in the form. */
  resendConfirmation(): void {
    const email = this.form.controls.email.value;
    if (!email || this.resendState() === 'sending') return;

    this.resendState.set('sending');
    this.authService.resendConfirmation(email).subscribe({
      // Always resolves (server returns 204 regardless) — show a sent state either way.
      next: () => this.resendState.set('sent'),
      error: () => this.resendState.set('sent')
    });
  }
}