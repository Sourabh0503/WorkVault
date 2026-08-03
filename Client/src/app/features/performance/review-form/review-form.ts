import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  effect,
  inject,
  input,
  output,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Review } from '../../../core/models/review.models';
import { formatInr } from '../../../shared/utils/currency';

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
 * and requires a salary; regular mode requires a rating and leaves salary optional. When a
 * prior salary is known, a linked "hike %" field lets the user enter either the new salary
 * or the % — the other fills in automatically. Purely presentational.
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
  /** Salary the hike% is measured against; when set, the linked % field appears. */
  readonly previousSalary = input<number | null>(null);
  readonly saving = input(false);
  readonly errorMessage = input<string | null>(null);

  readonly save = output<ReviewFormValue>();
  readonly cancel = output<void>();

  readonly todayStr = new Date().toISOString().slice(0, 10);

  // Show the linked salary/% fields only for a regular review with a known prior salary.
  readonly showHike = computed(() => this.mode() === 'regular' && this.previousSalary() != null);
  readonly prevSalaryLabel = computed(() => {
    const prev = this.previousSalary();
    return prev != null ? formatInr(prev) : '';
  });

  // Guards the salary ↔ % two-way sync against feedback loops.
  private syncing = false;

  form = this.fb.nonNullable.group({
    reviewName: ['', [Validators.required, Validators.maxLength(200)]],
    reviewDate: ['', [Validators.required]],
    rating: [null as number | null],
    newSalary: [null as number | null],
    hikePercent: [null as number | null],
    summary: [''],
  });

  get isBaseline(): boolean {
    return this.mode() === 'baseline';
  }

  constructor() {
    const destroyRef = inject(DestroyRef);

    // Reconfigure validators and (re)seed values when mode / review / prior salary change.
    effect(() => {
      const mode = this.mode();
      const review = this.review();
      this.applyValidators(mode);
      this.seed(mode, review);
    });

    // Salary ↔ hike% linkage (each sets the other with emitEvent:false, so no loop).
    this.form.controls.newSalary.valueChanges
      .pipe(takeUntilDestroyed(destroyRef))
      .subscribe((val) => this.onSalaryInput(val));
    this.form.controls.hikePercent.valueChanges
      .pipe(takeUntilDestroyed(destroyRef))
      .subscribe((val) => this.onHikeInput(val));
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
    const prev = this.previousSalary();

    if (review) {
      this.form.reset(
        {
          reviewName: review.reviewName,
          reviewDate: review.reviewDate,
          rating: review.isBaseline ? null : review.rating,
          newSalary: review.newSalary,
          hikePercent: this.toHike(review.newSalary, prev),
          summary: review.summary ?? '',
        },
        { emitEvent: false },
      );
      return;
    }

    // Adding: sensible defaults per mode.
    this.form.reset(
      {
        reviewName: mode === 'baseline' ? 'Starting Salary' : '',
        reviewDate: mode === 'baseline' ? this.defaultDate() || this.todayStr : this.todayStr,
        rating: null,
        newSalary: null,
        hikePercent: null,
        summary: '',
      },
      { emitEvent: false },
    );
  }

  private onSalaryInput(val: number | null): void {
    if (this.syncing) return;
    const prev = this.previousSalary();
    if (prev == null || prev === 0) return;
    this.syncing = true;
    this.form.controls.hikePercent.setValue(this.toHike(val, prev), { emitEvent: false });
    this.syncing = false;
  }

  private onHikeInput(val: number | null): void {
    if (this.syncing) return;
    const prev = this.previousSalary();
    if (prev == null) return;
    this.syncing = true;
    const salary = val == null || (val as unknown) === '' ? null : Math.round(prev * (1 + Number(val) / 100));
    this.form.controls.newSalary.setValue(salary, { emitEvent: false });
    this.syncing = false;
  }

  /** % change of a salary vs the prior salary, rounded to 1 dp — or null. */
  private toHike(salary: number | null, prev: number | null): number | null {
    if (salary == null || (salary as unknown) === '' || prev == null || prev === 0) return null;
    return Math.round(((Number(salary) - prev) / prev) * 1000) / 10;
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
