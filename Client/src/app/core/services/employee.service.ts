import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { EmployeeListItem, EmployeeListQuery, MyTeamResult } from '../models/employee.models';
import { PagedResult } from '../../shared/models/PagedResult.model';

@Injectable({ providedIn: 'root' })
export class EmployeeService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  /**
   * GET /api/employees
   * Returns a paged list of employees for the current company.
   * Tenant scoping is enforced server-side via the CompanyId global filter —
   * no companyId is sent from the client.
   *
   * @param query pageNumber/pageSize are required; search + status are optional.
   *              search matches email, full name, or employee code (single string).
   */
  getEmployees(query: EmployeeListQuery): Observable<PagedResult<EmployeeListItem>> {
    let params = new HttpParams()
      .set('pageNumber', query.pageNumber)
      .set('pageSize', query.pageSize);

    // Only append optional params when they carry a value, so we don't send
    // ?search=&status= empty strings that the API might treat as a filter.
    const search = query.search?.trim();
    if (search) {
      params = params.set('search', search);
    }
    if (query.status !== undefined && query.status !== null) {
      params = params.set('status', query.status);
    }

    return this.http.get<PagedResult<EmployeeListItem>>(`${this.apiUrl}/employees`, {
      params,
    });
  }

  /**
   * GET /api/employees/my-team
   * Returns members of the current user's own department.
   * Scoped server-side — no params needed.
   */
  getMyTeam(): Observable<MyTeamResult> {
    return this.http.get<MyTeamResult>(`${this.apiUrl}/employees/my-team`);
  }
}
