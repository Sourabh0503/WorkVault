import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../../../core/services/auth.service';
import { ApiError } from '../../../core/models/auth.models';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: './register.scss'
})
/**
 * Registration page — self-serve signup that creates a company + admin user via
 * `AuthService.register`. No password is collected: on success we show a
 * "check your email" confirmation, and the admin sets their password and signs
 * in through the emailed set-password link. Handles field-level API validation errors.
 */
export class Register {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);

  isSubmitting = signal(false);
  errorMessage = signal<string | null>(null);
  fieldErrors = signal<Record<string, string[]>>({});

  // On success we swap the form for a confirmation panel and remember the
  // address we sent the invite to, so we can show "we emailed you at …".
  emailSent = signal(false);
  sentToEmail = signal('');

  form = this.fb.nonNullable.group({
    // Company info
    companyName: ['', [Validators.required, Validators.maxLength(200)]],
    industry: [''],

    // Admin details
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]]
  });

  industries = [
    'IT / Software',
    'Manufacturing',
    'Healthcare',
    'Retail',
    'Education',
    'Finance',
    'Consulting',
    'Other'
  ];

  onSubmit(): void {
    this.errorMessage.set(null);
    this.fieldErrors.set({});

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);

    const value = this.form.getRawValue();
    const payload = {
      companyName: value.companyName,
      email: value.email,
      firstName: value.firstName,
      lastName: value.lastName,
      industry: value.industry || undefined
    };

    this.authService.register(payload).subscribe({
      next: (response) => {
        this.isSubmitting.set(false);
        this.sentToEmail.set(response.email);
        this.emailSent.set(true);
      },
      error: (err: HttpErrorResponse) => {
        this.isSubmitting.set(false);
        const apiError = err.error as ApiError;

        if (apiError?.errors) {
          // Validation errors from backend — map to field-level messages
          this.fieldErrors.set(apiError.errors);
        } else if (err.status >= 500) {
          // Server-side failure — don't leak internals, keep it friendly.
          this.errorMessage.set('We are facing some problem, please try again later.');
        } else {
          // Client errors (e.g. 409 conflict) carry a meaningful message — show it.
          this.errorMessage.set(apiError?.title || 'Registration failed. Try again.');
        }
      }
    });
  }
}