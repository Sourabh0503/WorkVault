import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ApiError, ResetTokenInfo } from '../../../core/models/auth.models';

@Component({
  selector: 'app-reset-password',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './reset-password.html',
  styleUrl: './reset-password.scss'
})
export class ResetPassword {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  // ---- UI state ----
  isLoading = signal(true);
  isInvalidLink = signal(false);
  isSubmitting = signal(false);
  isSuccess = signal(false);
  errorMessage = signal<string | null>(null);
  showPassword = signal(false);

  // User info from token validation
  userInfo = signal<ResetTokenInfo | null>(null);

  // Password field mirror for live checklist
  passwordValue = signal('');

  passwordChecks = computed(() => {
    const p = this.passwordValue();
    return {
      length: p.length >= 8,
      upperAndDigit: /[A-Z]/.test(p) && /[0-9]/.test(p),
      special: /[^a-zA-Z0-9]/.test(p)
    };
  });

  form = this.fb.nonNullable.group({
    password: ['', [
      Validators.required,
      Validators.minLength(8),
      Validators.pattern(/[A-Z]/),
      Validators.pattern(/[a-z]/),
      Validators.pattern(/[0-9]/),
      Validators.pattern(/[^a-zA-Z0-9]/)
    ]]
  });

  private token: string | null = null;

  constructor() {
    // Keep passwordValue signal in sync with the form control
    this.form.controls.password.valueChanges.subscribe(value => {
      this.passwordValue.set(value ?? '');
    });

    // Read token from URL and pre-validate
    this.token = this.route.snapshot.queryParamMap.get('token');

    if (!this.token) {
      this.isLoading.set(false);
      this.isInvalidLink.set(true);
      return;
    }

    this.authService.validateResetToken(this.token).subscribe({
      next: (info) => {
        this.userInfo.set(info);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
        this.isInvalidLink.set(true);
      }
    });
  }

  onSubmit(): void {
    if (!this.token) return;
    this.errorMessage.set(null);

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);

    this.authService.resetPassword({
      token: this.token,
      password: this.form.controls.password.value
    }).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.isSuccess.set(true);
      },
      error: (err: HttpErrorResponse) => {
        this.isSubmitting.set(false);
        const apiError = err.error as ApiError;

        // 404 means token became invalid between validation and submit
        // (expired mid-form, or used in another tab)
        if (err.status === 404) {
          this.isInvalidLink.set(true);
        } else {
          this.errorMessage.set(apiError?.title || 'Could not reset password. Try again.');
        }
      }
    });
  }

  togglePassword(): void {
    this.showPassword.update(v => !v);
  }
}