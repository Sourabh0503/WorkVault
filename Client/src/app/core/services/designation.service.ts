import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CreateDesignationRequest,
  CreateDesignationResult,
  Designation,
  UpdateDesignationRequest,
} from '../models/designation.models';

@Injectable({ providedIn: 'root' })
export class DesignationService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  /** GET /api/designations — designations in the current company, optionally by department. */
  getDesignations(departmentId?: string): Observable<Designation[]> {
    let params = new HttpParams();
    if (departmentId) {
      params = params.set('departmentId', departmentId);
    }
    return this.http.get<Designation[]>(`${this.apiUrl}/designations`, { params });
  }

  /** POST /api/designations — create a new designation. */
  createDesignation(data: CreateDesignationRequest): Observable<CreateDesignationResult> {
    return this.http.post<CreateDesignationResult>(`${this.apiUrl}/designations`, data);
  }

  /** PUT /api/designations/{id} — update a designation. */
  updateDesignation(
    id: string,
    data: UpdateDesignationRequest,
  ): Observable<{ id: string; title: string }> {
    return this.http.put<{ id: string; title: string }>(`${this.apiUrl}/designations/${id}`, data);
  }

  /** DELETE /api/designations/{id} — soft-delete a designation. */
  deleteDesignation(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/designations/${id}`);
  }
}
