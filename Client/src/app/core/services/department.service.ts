import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CreateDepartmentRequest,
  CreateDepartmentResult,
  Department,
  UpdateDepartmentRequest,
} from '../models/department.models';

@Injectable({ providedIn: 'root' })
export class DepartmentService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  /** GET /api/departments — all departments in the current company. */
  getDepartments(): Observable<Department[]> {
    return this.http.get<Department[]>(`${this.apiUrl}/departments`);
  }

  /** POST /api/departments — create a new department. */
  createDepartment(data: CreateDepartmentRequest): Observable<CreateDepartmentResult> {
    return this.http.post<CreateDepartmentResult>(`${this.apiUrl}/departments`, data);
  }

  /** PUT /api/departments/{id} — update a department. */
  updateDepartment(
    id: string,
    data: UpdateDepartmentRequest,
  ): Observable<{ id: string; name: string }> {
    return this.http.put<{ id: string; name: string }>(`${this.apiUrl}/departments/${id}`, data);
  }

  /** DELETE /api/departments/{id} — soft-delete a department. */
  deleteDepartment(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/departments/${id}`);
  }
}
