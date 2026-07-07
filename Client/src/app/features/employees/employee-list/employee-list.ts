import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';
import { EmployeeService } from '../../../core/services/employee.service';
import {
  EMPLOYEE_STATUS_LABELS,
  EmployeeListItem,
  EmployeeStatus,
} from '../../../core/models/employee.models';

@Component({
  selector: 'app-employee-list',
  imports: [DatePipe, RouterLink],
  templateUrl: './employee-list.html',
  styleUrl: './employee-list.scss',
})
/**
 * Employees list page. Renders a paginated, searchable, status-filterable table via
 * `EmployeeService.getEmployees` (search input is debounced), links each row to the
 * detail page, and provides the "Add Employee" entry point.
 */
export class EmployeeList implements OnInit, OnDestroy {
  private employeeService = inject(EmployeeService);

  // ---- Data state ----
  employees = signal<EmployeeListItem[]>([]);
  loading = signal<boolean>(false);
  error = signal<string | null>(null);
  totalCount = signal<number>(0);

  // ---- Query state ----
  pageNumber = signal<number>(1);
  pageSize = signal<number>(20);
  search = signal<string>('');
  status = signal<EmployeeStatus | null>(null);
  totalPages = computed(() => Math.max(1, Math.ceil(this.totalCount() / this.pageSize())));

  // Debounce source for the search box. The template pushes raw input here;
  // we only actually fetch after the user pauses typing.
  private searchInput$ = new Subject<string>();
  private destroy$ = new Subject<void>();

  ngOnInit(): void {
    this.searchInput$
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe((term) => {
        this.search.set(term);
        this.pageNumber.set(1); // new search shrinks results — reset to page 1
        this.load();
      });

    this.load();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /** Fetches the current page with current filters. */
  load(): void {
    this.loading.set(true);
    this.error.set(null);

    this.employeeService
      .getEmployees({
        pageNumber: this.pageNumber(),
        pageSize: this.pageSize(),
        search: this.search() || undefined,
        status: this.status() ?? undefined,
      })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          this.employees.set(result.items);
          this.totalCount.set(result.totalCount);
          this.loading.set(false);
        },
        error: () => {
          this.error.set('Could not load employees. Please try again.');
          this.loading.set(false);
        },
      });
  }

  // ---- Template event handlers ----

  /** Called on every keystroke in the search box; debounced via searchInput$. */
  onSearchInput(value: string): void {
    this.searchInput$.next(value);
  }

  /** Status filter changed — parse the raw select value, reset to page 1, reload. */
  onStatusChange(value: string): void {
    this.status.set(value === '' ? null : (Number(value) as EmployeeStatus));
    this.pageNumber.set(1);
    this.load();
  }

  goToPage(page: number): void {
    if (page < 1) return;
    this.pageNumber.set(page);
    this.load();
  }

  statusLabel(status: EmployeeStatus): string {
    return EMPLOYEE_STATUS_LABELS[status] ?? 'Unknown';
  }
}