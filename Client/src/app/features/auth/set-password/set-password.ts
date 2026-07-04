import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, effect, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ApiError, InviteInfo } from '../../../core/models/auth.models';

@Component({
  selector: 'app-set-password',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './set-password.html',
  styleUrl: './set-password.scss'
})
export class SetPassword {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  // ---- UI state ----
  isLoading = signal(true);              // waiting for invite validation
  isSubmitting = signal(false);
  isInvalidLink = signal(false);         // token invalid/expired/used
  invite = signal<InviteInfo | null>(null);
  errorMessage = signal<string | null>(null);
  showPassword = signal(false);

  togglePassword(): void {
    this.showPassword.update(v => !v);
  }

  // Password field is a signal so we can drive live checklist off it
  passwordValue = signal('');

  // Live password requirement checks (matches design's live checklist)
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
    // Sync form password → signal, so checklist updates live
    this.form.controls.password.valueChanges.subscribe(value => {
      this.passwordValue.set(value ?? '');
    });

    // On construction, read the token from the URL and validate it
    this.token = this.route.snapshot.queryParamMap.get('token');

    if (!this.token) {
      this.isLoading.set(false);
      this.isInvalidLink.set(true);
      return;
    }

    this.authService.validateInvite(this.token).subscribe({
      next: info => {
        this.invite.set(info);
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

    this.authService.setPassword({
      token: this.token,
      password: this.form.controls.password.value
    }).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.router.navigate(['/dashboard']);
      },
      error: (err: HttpErrorResponse) => {
        this.isSubmitting.set(false);
        const apiError = err.error as ApiError;
        this.errorMessage.set(apiError?.title || 'Could not set password. Try again.');
      }
    });
  }
}