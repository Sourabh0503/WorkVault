import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { DatePipe } from '@angular/common';
import { EmployeeService } from '../../../core/services/employee.service';
import { DepartmentService } from '../../../core/services/department.service';
import {
  EmployeeDetail,
  EmployeeStatus,
  EMPLOYEE_STATUS_LABELS
} from '../../../core/models/employee.models';
import { Department } from '../../../core/models/department.models';
import { ApiError } from '../../../core/models/auth.models';

@Component({
  selector: 'app-employee-detail-page',
  imports: [ReactiveFormsModule, RouterLink, DatePipe],
  templateUrl: './employee-detail-page.html',
  styleUrl: './employee-detail-page.scss'
})
/**
 * Employee detail/edit page. Loads an employee by the route `:id`, renders their
 * profile, and toggles into an edit form (status, department, etc.) submitted via
 * `EmployeeService.updateEmployee`. For pending employees it can also resend the invite.
 */
export class EmployeeDetailPage implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private fb = inject(FormBuilder);
  private employeeService = inject(EmployeeService);
  private departmentService = inject(DepartmentService);

  // ---- State ----
  loading = signal(true);
  error = signal<string | null>(null);
  employee = signal<EmployeeDetail | null>(null);

  editing = signal(false);
  isSaving = signal(false);
  saveError = signal<string | null>(null);

  departments = signal<Department[]>([]);

  // Resend invite state
  isResending = signal(false);
  resendResult = signal<string | null>(null);
  inviteCopied = signal(false);

  // Expose status enum + labels to template
  statusOptions = [
    EmployeeStatus.Active,
    EmployeeStatus.OnNotice,
    EmployeeStatus.Suspended,
    EmployeeStatus.Offboarded
  ];
  statusLabels = EMPLOYEE_STATUS_LABELS;

  // Is this employee still pending (invite not accepted)?
  isPending = computed(() => this.employee()?.status === EmployeeStatus.Pending);

  private employeeId!: string;

  form = this.fb.nonNullable.group({
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.maxLength(100)]],
    phone: [''],
    joinDate: ['', [Validators.required]],
    departmentId: [''],
    status: [EmployeeStatus.Active, [Validators.required]]
  });

  ngOnInit(): void {
    this.employeeId = this.route.snapshot.paramMap.get('id') ?? '';
    if (!this.employeeId) {
      this.error.set('No employee specified.');
      this.loading.set(false);
      return;
    }

    this.loadEmployee();
    this.loadDepartments();
  }

  private loadEmployee(): void {
    this.loading.set(true);
    this.error.set(null);

    this.employeeService.getEmployeeById(this.employeeId).subscribe({
      next: (emp) => {
        this.employee.set(emp);
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.error.set(err.status === 404 ? 'Employee not found.' : 'Could not load employee.');
        this.loading.set(false);
      }
    });
  }

  private loadDepartments(): void {
    this.departmentService.getDepartments().subscribe({
      next: (depts) => this.departments.set(depts),
      error: () => {}
    });
  }

  // ---- Edit mode ----
  startEdit(): void {
    const emp = this.employee();
    if (!emp) return;

    // Pre-fill form with current values
    this.form.setValue({
      firstName: emp.firstName,
      lastName: emp.lastName,
      phone: emp.phone ?? '',
      joinDate: emp.joinDate,
      departmentId: emp.department?.id ?? '',
      status: emp.status
    });

    this.saveError.set(null);
    this.editing.set(true);
  }

  cancelEdit(): void {
    this.editing.set(false);
    this.saveError.set(null);
  }

  save(): void {
    this.saveError.set(null);

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const emp = this.employee();
    if (!emp) return;

    this.isSaving.set(true);
    const value = this.form.getRawValue();

    this.employeeService.updateEmployee(this.employeeId, {
      id: this.employeeId,
      firstName: value.firstName,
      lastName: value.lastName,
      phone: value.phone || undefined,
      joinDate: value.joinDate,
      departmentId: value.departmentId || undefined,
      status: Number(value.status),
      // Preserve fields we don't edit in this form
      designationId: emp.designation?.id ?? undefined,
      managerId: emp.manager?.id ?? undefined,
      dateOfBirth: emp.dateOfBirth ?? undefined,
      resignationDate: emp.resignationDate ?? undefined,
      lastWorkingDay: emp.lastWorkingDay ?? undefined
    }).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.editing.set(false);
        this.loadEmployee();  // refresh with saved data
      },
      error: (err: HttpErrorResponse) => {
        this.isSaving.set(false);
        const apiError = err.error as ApiError;
        this.saveError.set(apiError?.title || 'Could not save changes.');
      }
    });
  }

  // ---- Resend invite ----
  resendInvite(): void {
    this.isResending.set(true);
    this.resendResult.set(null);
    this.inviteCopied.set(false);

    this.employeeService.resendInvite(this.employeeId).subscribe({
      next: (result) => {
        this.isResending.set(false);
        this.resendResult.set(result.inviteLink);
      },
      error: () => {
        this.isResending.set(false);
        this.saveError.set('Could not resend invite.');
      }
    });
  }

  copyInviteLink(): void {
    const link = this.resendResult();
    if (!link) return;

    navigator.clipboard.writeText(link).then(() => {
      this.inviteCopied.set(true);
      setTimeout(() => this.inviteCopied.set(false), 2000);
    });
  }

  statusLabel(status: EmployeeStatus): string {
    return this.statusLabels[status] ?? 'Unknown';
  }
}