import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { DashboardStats, HeadcountPoint } from '../models/dashboard.models';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  /** GET /api/dashboard/stats — company aggregates (HR/Admin only). */
  getStats(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>(`${this.apiUrl}/dashboard/stats`);
  }

  /** GET /api/dashboard/headcount — monthly joiner counts, YTD (HR/Admin only). */
  getHeadcountTrend(): Observable<HeadcountPoint[]> {
    return this.http.get<HeadcountPoint[]>(`${this.apiUrl}/dashboard/headcount`);
  }
}