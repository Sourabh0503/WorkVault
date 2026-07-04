import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../../../core/services/auth.service';
import { ApiError } from '../../../core/models/auth.models';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: './register.scss'
})
export class Register {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  isSubmitting = signal(false);
  errorMessage = signal<string | null>(null);
  fieldErrors = signal<Record<string, string[]>>({});

  form = this.fb.nonNullable.group({
    // Company info
    companyName: ['', [Validators.required, Validators.maxLength(200)]],
    industry: [''],

    // Admin details
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
    password: ['', [
      Validators.required,
      Validators.minLength(8),
      Validators.pattern(/[A-Z]/),        // one uppercase
      Validators.pattern(/[a-z]/),        // one lowercase
      Validators.pattern(/[0-9]/),        // one digit
      Validators.pattern(/[^a-zA-Z0-9]/)  // one special char
    ]]
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
      password: value.password,
      firstName: value.firstName,
      lastName: value.lastName,
      industry: value.industry || undefined
    };

    this.authService.register(payload).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.router.navigate(['/dashboard']);
      },
      error: (err: HttpErrorResponse) => {
        this.isSubmitting.set(false);
        const apiError = err.error as ApiError;

        if (apiError?.errors) {
          // Validation errors from backend — map to field-level messages
          this.fieldErrors.set(apiError.errors);
        } else {
          this.errorMessage.set(apiError?.title || 'Registration failed. Try again.');
        }
      }
    });
  }
}