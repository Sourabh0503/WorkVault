import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../../../core/services/auth.service';
import { ApiError } from '../../../core/models/auth.models';
import { StepCompany } from './steps/step-company/step-company';
import { StepAuth } from './steps/step-auth/step-auth';

/** One step in the registration wizard. */
interface WizardStep {
  /** Label shown in the progress bar / subtitle. */
  label: string;
  /** Form controls validated before this step can be left. */
  controls: string[];
  /** false = planned but not built yet (shown greyed in the progress bar). */
  available: boolean;
}

@Component({
  selector: 'app-register',
  imports: [RouterLink, StepCompany, StepAuth],
  templateUrl: './register.html',
  styleUrl: './register.scss',
})
/**
 * Registration wizard container. Owns a single shared form and steps through it:
 *   1. Company details
 *   2. Setup authentication (admin identity — triggers the invite email)
 *   3–5. Departments / Designations / Review (planned; shown as upcoming)
 *
 * Each step's fields live in their own component (`step-company`, `step-auth`);
 * this container only handles navigation, per-step validation, submission, and
 * the post-submit "check your email" confirmation.
 */
export class Register {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);

  isSubmitting = signal(false);
  errorMessage = signal<string | null>(null);
  fieldErrors = signal<Record<string, string[]>>({});

  // After a successful submit we swap the wizard for a confirmation panel.
  emailSent = signal(false);
  sentToEmail = signal('');

  // 1-based index of the step currently shown.
  currentStep = signal(1);

  // The wizard blueprint. Only the first two steps are built today; the rest
  // are placeholders so the progress bar already shows the full journey.
  steps: WizardStep[] = [
    { label: 'Company details', controls: ['companyName', 'industry'], available: true },
    { label: 'Setup authentication', controls: ['firstName', 'lastName', 'email'], available: true },
    { label: 'Departments', controls: [], available: false },
    { label: 'Designations', controls: [], available: false },
    { label: 'Review & finish', controls: [], available: false },
  ];

  totalSteps = this.steps.length;

  // One shared form across all steps — the container is the single source of truth.
  form = this.fb.nonNullable.group({
    companyName: ['', [Validators.required, Validators.maxLength(200)]],
    industry: [''],
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
  });

  /** The step config for the step currently shown. */
  activeStep = computed(() => this.steps[this.currentStep() - 1]);

  /** True when the next step isn't available yet — i.e. this is where we submit. */
  isFinalActiveStep = computed(() => {
    const next = this.steps[this.currentStep()];
    return !next || !next.available;
  });

  /** Label for the primary button, driven by step + submission state. */
  primaryLabel = computed(() => {
    if (!this.isFinalActiveStep()) return 'Next →';
    return this.isSubmitting() ? 'Sending confirmation…' : 'Create workspace →';
  });

  /** Validate the current step's controls, then advance or submit. */
  next(): void {
    this.errorMessage.set(null);

    const controls = this.activeStep().controls;
    let valid = true;
    for (const name of controls) {
      const control = this.form.get(name);
      control?.markAsTouched();
      if (control?.invalid) valid = false;
    }
    if (!valid) return;

    if (this.isFinalActiveStep()) {
      this.submit();
      return;
    }

    this.currentStep.update((s) => Math.min(s + 1, this.totalSteps));
  }

  /** Go back one step (clears any banner error). */
  back(): void {
    this.errorMessage.set(null);
    this.currentStep.update((s) => Math.max(1, s - 1));
  }

  private submit(): void {
    this.errorMessage.set(null);
    this.fieldErrors.set({});
    this.isSubmitting.set(true);

    const value = this.form.getRawValue();
    this.authService
      .register({
        companyName: value.companyName,
        email: value.email,
        firstName: value.firstName,
        lastName: value.lastName,
        industry: value.industry || undefined,
      })
      .subscribe({
        next: (response) => {
          this.isSubmitting.set(false);
          this.sentToEmail.set(response.email);
          this.emailSent.set(true);
        },
        error: (err: HttpErrorResponse) => {
          this.isSubmitting.set(false);
          const apiError = err.error as ApiError;

          if (apiError?.errors) {
            // Field-level validation errors — surface them and jump to the
            // step that owns the first offending field.
            this.fieldErrors.set(apiError.errors);
            this.goToStepWithError(Object.keys(apiError.errors));
          } else if (err.status >= 500) {
            this.errorMessage.set('We are facing some problem, please try again later.');
          } else {
            this.errorMessage.set(apiError?.title || 'Registration failed. Try again.');
          }
        },
      });
  }

  /** Move to the earliest step that contains one of the errored fields. */
  private goToStepWithError(erroredFields: string[]): void {
    const lower = erroredFields.map((f) => f.toLowerCase());
    const index = this.steps.findIndex((step) =>
      step.controls.some((c) => lower.includes(c.toLowerCase())),
    );
    if (index >= 0) this.currentStep.set(index + 1);
  }
}
