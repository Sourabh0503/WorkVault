import { Component, computed, inject, signal } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../../../core/services/auth.service';
import { ApiError, InviteInfo } from '../../../core/models/auth.models';
import { StepCompany } from './steps/step-company/step-company';
import { StepAuth } from './steps/step-auth/step-auth';
import { StepPassword } from './steps/step-password/step-password';

/** One step in the registration wizard. */
interface WizardStep {
  /** Label shown in the progress bar / subtitle. */
  label: string;
  /** Form controls validated before this step can be left. */
  controls: string[];
  /** false = planned but not built yet (shown greyed in the progress bar). */
  available: boolean;
}

/** Group validator: flags a mismatch between `password` and `confirmPassword`. */
function passwordsMatch(group: AbstractControl): ValidationErrors | null {
  const password = group.get('password')?.value;
  const confirm = group.get('confirmPassword')?.value;
  return password && confirm && password !== confirm ? { passwordMismatch: true } : null;
}

@Component({
  selector: 'app-register',
  imports: [RouterLink, StepCompany, StepAuth, StepPassword],
  templateUrl: './register.html',
  styleUrl: './register.scss',
})
/**
 * Registration wizard container. Owns a single shared form and steps through it:
 *   1. Company details
 *   2. Setup authentication (admin identity — triggers the confirmation email)
 *   3. Setup password (reached via the emailed `/register/:token` link — activates + logs in)
 *   4–5. Departments / Designations (planned; shown as upcoming)
 *
 * Two entry modes:
 *  - `/register`         → fresh signup, starts at step 1.
 *  - `/register/:token`  → confirmation link, jumps to step 3 after validating the token.
 *
 * Each step's fields live in their own component; this container handles
 * navigation, per-step validation, and the two submissions (register / set-password).
 */
export class Register {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  isSubmitting = signal(false);
  errorMessage = signal<string | null>(null);
  fieldErrors = signal<Record<string, string[]>>({});

  // After a successful register we swap the wizard for a confirmation panel.
  emailSent = signal(false);
  sentToEmail = signal('');

  // ---- Confirmation-link (token) mode ----
  private token: string | null = null;
  tokenMode = signal(false);
  validatingToken = signal(false);
  invalidToken = signal(false);
  invite = signal<InviteInfo | null>(null);

  // 1-based index of the step currently shown.
  currentStep = signal(1);

  // The wizard blueprint. Steps 4–5 are placeholders so the progress bar
  // already shows the full journey.
  steps: WizardStep[] = [
    { label: 'Company details', controls: ['companyName', 'industry'], available: true },
    { label: 'Setup authentication', controls: ['firstName', 'lastName', 'email'], available: true },
    { label: 'Setup password', controls: ['password', 'confirmPassword'], available: true },
    { label: 'Departments', controls: [], available: false },
    { label: 'Designations', controls: [], available: false },
  ];

  totalSteps = this.steps.length;

  // One shared form across all steps — the container is the single source of truth.
  form = this.fb.nonNullable.group(
    {
      companyName: ['', [Validators.required, Validators.maxLength(200)]],
      industry: [''],
      firstName: ['', [Validators.required, Validators.maxLength(100)]],
      lastName: ['', [Validators.required, Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
      password: ['', [
        Validators.required,
        Validators.minLength(8),
        Validators.pattern(/[A-Z]/),
        Validators.pattern(/[a-z]/),
        Validators.pattern(/[0-9]/),
        Validators.pattern(/[^a-zA-Z0-9]/),
      ]],
      confirmPassword: ['', [Validators.required]],
    },
    { validators: passwordsMatch },
  );

  constructor() {
    // If we arrived via the confirmation link, jump to the password step and
    // validate the token before showing the form.
    this.token = this.route.snapshot.paramMap.get('token');
    if (this.token) {
      this.tokenMode.set(true);
      this.currentStep.set(3);
      this.validateToken(this.token);
    }
  }

  /** The step config for the step currently shown. */
  activeStep = computed(() => this.steps[this.currentStep() - 1]);

  /** Label for the primary button, driven by step + submission state. */
  primaryLabel = computed(() => {
    switch (this.currentStep()) {
      case 2:
        return this.isSubmitting() ? 'Sending confirmation…' : 'Create workspace →';
      case 3:
        return this.isSubmitting() ? 'Setting up…' : 'Set password & sign in';
      default:
        return 'Next →';
    }
  });

  /** Back is only available when moving within the pre-email steps (1 → 2). */
  canGoBack = computed(() => !this.tokenMode() && this.currentStep() > 1);

  private validateToken(token: string): void {
    this.validatingToken.set(true);
    this.authService.validateInvite(token).subscribe({
      next: (info) => {
        this.invite.set(info);
        this.validatingToken.set(false);
      },
      error: () => {
        this.validatingToken.set(false);
        this.invalidToken.set(true);
      },
    });
  }

  /** Validate the current step's controls, then advance or submit. */
  next(): void {
    this.errorMessage.set(null);
    if (!this.isStepValid()) return;

    switch (this.currentStep()) {
      case 1:
        this.currentStep.set(2);
        break;
      case 2:
        this.submitRegister();
        break;
      case 3:
        this.submitPassword();
        break;
    }
  }

  /** Go back one step (clears any banner error). Disabled in token mode. */
  back(): void {
    if (!this.canGoBack()) return;
    this.errorMessage.set(null);
    this.currentStep.update((s) => Math.max(1, s - 1));
  }

  /** Touch + validate just the controls owned by the current step. */
  private isStepValid(): boolean {
    let valid = true;
    for (const name of this.activeStep().controls) {
      const control = this.form.get(name);
      control?.markAsTouched();
      if (control?.invalid) valid = false;
    }
    // The password step also needs the two passwords to match (group-level error).
    if (this.currentStep() === 3 && this.form.errors?.['passwordMismatch']) {
      this.form.get('confirmPassword')?.markAsTouched();
      valid = false;
    }
    return valid;
  }

  private submitRegister(): void {
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

  private submitPassword(): void {
    if (!this.token) return;
    this.errorMessage.set(null);
    this.isSubmitting.set(true);

    this.authService
      .setPassword({ token: this.token, password: this.form.get('password')!.value })
      .subscribe({
        next: () => {
          this.isSubmitting.set(false);
          this.router.navigate(['/dashboard']);
        },
        error: (err: HttpErrorResponse) => {
          this.isSubmitting.set(false);
          const apiError = err.error as ApiError;
          if (err.status === 404) {
            // Token became invalid/expired between validation and submit.
            this.invalidToken.set(true);
          } else if (err.status >= 500) {
            this.errorMessage.set('We are facing some problem, please try again later.');
          } else {
            this.errorMessage.set(apiError?.title || 'Could not set password. Try again.');
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
