import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CreateReviewRequest,
  CreateReviewResult,
  Review,
  UpdateReviewRequest,
} from '../models/review.models';

/**
 * Client for the Performance/Review endpoints. Tenant scoping and salary access are
 * enforced server-side — the client sends no companyId and receives salary already nulled
 * when it isn't permitted.
 */
@Injectable({ providedIn: 'root' })
export class ReviewService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  /**
   * GET /api/employees/{id}/reviews — an employee's reviews (newest first) for the charts
   * and history. Salary is nulled server-side for callers who can't see it (managers).
   */
  getEmployeeReviews(employeeId: string): Observable<Review[]> {
    return this.http.get<Review[]>(`${this.apiUrl}/employees/${employeeId}/reviews`);
  }

  /** POST /api/employees/{id}/reviews — create a review or the salary baseline. HR/Admin only. */
  createReview(employeeId: string, data: CreateReviewRequest): Observable<CreateReviewResult> {
    return this.http.post<CreateReviewResult>(
      `${this.apiUrl}/employees/${employeeId}/reviews`,
      data,
    );
  }

  /** PUT /api/employees/{id}/reviews/{reviewId} — edit a review (within 30 days). HR/Admin only. */
  updateReview(
    employeeId: string,
    reviewId: string,
    data: UpdateReviewRequest,
  ): Observable<void> {
    return this.http.put<void>(
      `${this.apiUrl}/employees/${employeeId}/reviews/${reviewId}`,
      data,
    );
  }

  /** DELETE /api/employees/{id}/reviews/{reviewId} — soft-delete a review (within 30 days). HR/Admin only. */
  deleteReview(employeeId: string, reviewId: string): Observable<void> {
    return this.http.delete<void>(
      `${this.apiUrl}/employees/${employeeId}/reviews/${reviewId}`,
    );
  }

  /** GET /api/me/performance — the current user's own reviews (ratings + own salary). */
  getMyPerformance(): Observable<Review[]> {
    return this.http.get<Review[]>(`${this.apiUrl}/me/performance`);
  }
}
