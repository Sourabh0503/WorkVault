import {
  ChangeDetectionStrategy,
  Component,
  effect,
  inject,
  input,
  output,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Review } from '../../../core/models/review.models';

/** Which kind of entry this form edits. Baseline = salary-only, no rating. */
export type ReviewFormMode = 'baseline' | 'regular';

/** Raw values emitted on submit; the container adds isBaseline and calls the API. */
export interface ReviewFormValue {
  reviewName: string;
  reviewDate: string;
  rating: number;
  newSalary: number | null;
  summary: string | null;
}

/**
 * Add/edit form for a review or the salary baseline. Baseline mode drops the rating field
 * and requires a salary; regular mode requires a rating and leaves salary optional. Purely
 * presentational — the container owns saving state, errors, and the API calls.
 */
@Component({
  selector: 'app-review-form',
  imports: [ReactiveFormsModule],
  templateUrl: './review-form.html',
  styleUrl: './review-form.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ReviewForm {
  private fb = inject(FormBuilder);

  readonly mode = input.required<ReviewFormMode>();
  /** Existing review when editing; null when adding. */
  readonly review = input<Review | null>(null);
  /** Default review date for a new baseline (the employee's join date). */
  readonly defaultDate = input<string>('');
  readonly saving = input(false);
  readonly errorMessage = input<string | null>(null);

  readonly save = output<ReviewFormValue>();
  readonly cancel = output<void>();

  readonly todayStr = new Date().toISOString().slice(0, 10);

  form = this.fb.nonNullable.group({
    reviewName: ['', [Validators.required, Validators.maxLength(200)]],
    reviewDate: ['', [Validators.required]],
    rating: [null as number | null],
    newSalary: [null as number | null],
    summary: [''],
  });

  get isBaseline(): boolean {
    return this.mode() === 'baseline';
  }

  constructor() {
    // Reconfigure validators and (re)seed values whenever the mode or edited review changes.
    effect(() => {
      const mode = this.mode();
      const review = this.review();
      this.applyValidators(mode);
      this.seed(mode, review);
    });
  }

  private applyValidators(mode: ReviewFormMode): void {
    const rating = this.form.controls.rating;
    const salary = this.form.controls.newSalary;

    if (mode === 'baseline') {
      rating.clearValidators();
      salary.setValidators([Validators.required, Validators.min(0)]);
    } else {
      rating.setValidators([Validators.required, Validators.min(0), Validators.max(5)]);
      salary.setValidators([Validators.min(0)]);
    }
    rating.updateValueAndValidity({ emitEvent: false });
    salary.updateValueAndValidity({ emitEvent: false });
  }

  private seed(mode: ReviewFormMode, review: Review | null): void {
    if (review) {
      this.form.reset({
        reviewName: review.reviewName,
        reviewDate: review.reviewDate,
        rating: review.isBaseline ? null : review.rating,
        newSalary: review.newSalary,
        summary: review.summary ?? '',
      });
      return;
    }

    // Adding: sensible defaults per mode.
    this.form.reset({
      reviewName: mode === 'baseline' ? 'Starting Salary' : '',
      reviewDate: mode === 'baseline' ? this.defaultDate() || this.todayStr : this.todayStr,
      rating: null,
      newSalary: null,
      summary: '',
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    this.save.emit({
      reviewName: v.reviewName.trim(),
      reviewDate: v.reviewDate,
      // Baseline carries no rating; server ignores it, but send a clean 0.
      rating: this.isBaseline ? 0 : Number(v.rating ?? 0),
      newSalary: v.newSalary == null ? null : Number(v.newSalary),
      summary: v.summary?.trim() ? v.summary.trim() : null,
    });
  }
}
