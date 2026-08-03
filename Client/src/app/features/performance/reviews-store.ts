import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { ApiError } from '../../core/models/auth.models';
import { Review, hasVisibleSalary } from '../../core/models/review.models';
import { ReviewService } from '../../core/services/review.service';
import { averageRating, currentSalaryLabel, currentSalaryValue, salaryBefore } from './review-stats';
import { ReviewFormMode, ReviewFormValue } from './review-form/review-form';

/**
 * Owns all performance-review state and operations for one employee: loading, the
 * derived stats (current salary, average rating, salary delta), and the add/edit/delete
 * flows. Provided at the employee-detail page and shared by both the hero (which shows
 * current salary + Add review) and the performance section — so neither reaches into the
 * other, and neither re-fetches.
 *
 * Component-scoped (not root): a fresh instance per employee-detail page. Access and salary
 * visibility are enforced server-side; this only reflects the returned data.
 */
@Injectable()
export class ReviewsStore {
  private reviewService = inject(ReviewService);

  // ---- Context (set by the page via setContext) ----
  private readonly employeeId = signal<string | null>(null);
  readonly canManage = signal(false);
  readonly addLocked = signal(false);
  readonly defaultDate = signal('');
  private loadedFor: string | null = null;

  // ---- Data ----
  readonly reviews = signal<Review[]>([]);
  readonly loading = signal(true);
  readonly loadError = signal<string | null>(null);
  readonly forbidden = signal(false); // 403 — caller may not see these reviews

  // ---- Add / edit form ----
  readonly formOpen = signal(false);
  readonly formMode = signal<ReviewFormMode>('regular');
  readonly editingReview = signal<Review | null>(null);
  readonly saving = signal(false);
  readonly saveError = signal<string | null>(null);
  // Salary the form's hike% is measured against (prior salary); null hides the % field.
  readonly formPreviousSalary = signal<number | null>(null);

  // ---- Delete ----
  readonly pendingDelete = signal<Review | null>(null);
  readonly deleting = signal(false);

  // ---- Derived ----
  readonly mostRecent = computed<Review | null>(() => this.reviews()[0] ?? null);
  readonly isEmpty = computed(() => this.reviews().length === 0);
  readonly showSalaryChart = computed(() => hasVisibleSalary(this.reviews()));

  readonly avgRating = computed(() => averageRating(this.reviews()));
  readonly currentSalary = computed(() => currentSalaryLabel(this.reviews()));

  /** Update per-employee context. Resets view state when the employee changes. */
  setContext(employeeId: string, canManage: boolean, addLocked: boolean, defaultDate: string): void {
    this.canManage.set(canManage);
    this.addLocked.set(addLocked);
    this.defaultDate.set(defaultDate);

    if (this.employeeId() !== employeeId) {
      this.employeeId.set(employeeId);
      this.loadedFor = null;
      this.reviews.set([]);
      this.closeForm();
      this.pendingDelete.set(null);
    }
  }

  /** Load once for the current employee (lazy — call when the section becomes visible). */
  ensureLoaded(): void {
    const id = this.employeeId();
    if (id && this.loadedFor !== id) this.load(id);
  }

  private load(employeeId: string): void {
    this.loadedFor = employeeId;
    this.loading.set(true);
    this.loadError.set(null);
    this.forbidden.set(false);

    this.reviewService.getEmployeeReviews(employeeId).subscribe({
      next: (reviews) => {
        this.reviews.set(reviews);
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        if (err.status === 403) this.forbidden.set(true);
        else this.loadError.set('Could not load performance data.');
        this.loading.set(false);
      },
    });
  }

  private reload(): void {
    const id = this.employeeId();
    if (id) this.load(id);
  }

  // ---- Add / edit ----
  openAdd(): void {
    if (this.addLocked()) return; // suspended/offboarded — no new reviews
    this.editingReview.set(null);
    // No reviews yet → the first entry must be the baseline; otherwise a regular review.
    const baseline = this.isEmpty();
    this.formMode.set(baseline ? 'baseline' : 'regular');
    // A new regular review's hike is measured against the current salary.
    this.formPreviousSalary.set(baseline ? null : currentSalaryValue(this.reviews()));
    this.saveError.set(null);
    this.formOpen.set(true);
  }

  openEdit(review: Review): void {
    this.editingReview.set(review);
    this.formMode.set(review.isBaseline ? 'baseline' : 'regular');
    // When editing, the hike is measured against the salary in effect just before it.
    this.formPreviousSalary.set(review.isBaseline ? null : salaryBefore(this.reviews(), review.id));
    this.saveError.set(null);
    this.formOpen.set(true);
  }

  closeForm(): void {
    this.formOpen.set(false);
    this.editingReview.set(null);
  }

  save(value: ReviewFormValue): void {
    const employeeId = this.employeeId();
    if (!employeeId) return;
    const editing = this.editingReview();
    this.saving.set(true);
    this.saveError.set(null);

    const done = {
      next: () => {
        this.saving.set(false);
        this.closeForm();
        this.reload();
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.saveError.set((err.error as ApiError)?.title ?? 'Could not save.');
      },
    };

    if (editing) {
      this.reviewService
        .updateReview(employeeId, editing.id, {
          reviewName: value.reviewName,
          reviewDate: value.reviewDate,
          rating: value.rating,
          newSalary: value.newSalary,
          summary: value.summary,
        })
        .subscribe(done);
    } else {
      this.reviewService
        .createReview(employeeId, {
          reviewName: value.reviewName,
          reviewDate: value.reviewDate,
          rating: value.rating,
          newSalary: value.newSalary,
          summary: value.summary,
          isBaseline: this.formMode() === 'baseline',
        })
        .subscribe(done);
    }
  }

  // ---- Delete ----
  askDelete(review: Review): void {
    this.saveError.set(null);
    this.pendingDelete.set(review);
  }

  cancelDelete(): void {
    this.pendingDelete.set(null);
  }

  confirmDelete(): void {
    const review = this.pendingDelete();
    const employeeId = this.employeeId();
    if (!review || !employeeId) return;
    this.deleting.set(true);

    this.reviewService.deleteReview(employeeId, review.id).subscribe({
      next: () => {
        this.deleting.set(false);
        this.pendingDelete.set(null);
        this.reload();
      },
      error: (err: HttpErrorResponse) => {
        this.deleting.set(false);
        this.pendingDelete.set(null);
        this.loadError.set((err.error as ApiError)?.title ?? 'Could not delete review.');
      },
    });
  }
}
