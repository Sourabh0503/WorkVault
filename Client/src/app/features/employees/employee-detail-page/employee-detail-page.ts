import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { DatePipe } from '@angular/common';
import { EmployeeService } from '../../../core/services/employee.service';
import { DepartmentService } from '../../../core/services/department.service';
import { DesignationService } from '../../../core/services/designation.service';
import { AuthService } from '../../../core/services/auth.service';
import {
  EmployeeDetail,
  EmployeeStatus,
  EMPLOYEE_STATUS_LABELS,
  ManagerOption
} from '../../../core/models/employee.models';
import { Department } from '../../../core/models/department.models';
import { Designation } from '../../../core/models/designation.models';
import { ASSIGNABLE_ROLES, EMPLOYEE_ROLE_ID } from '../../../core/models/role.models';
import { ApiError } from '../../../core/models/auth.models';

@Component({
  selector: 'app-employee-detail-page',
  imports: [ReactiveFormsModule, RouterLink, DatePipe],
  templateUrl: './employee-detail-page.html',
  styleUrl: './employee-detail-page.scss'
})
/**
 * Employee detail/edit page. Loads an employee by the route `:id`, renders their
 * profile, and toggles into an edit form (status, department, designation, manager,
 * role) submitted via `EmployeeService.updateEmployee`. For pending employees it can
 * also resend the invite or delete the record.
 *
 * Designation and manager are department-scoped: disabled until a department is chosen,
 * designations filtered to that department, managers fetched per department.
 */
export class EmployeeDetailPage implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private fb = inject(FormBuilder);
  private employeeService = inject(EmployeeService);
  private departmentService = inject(DepartmentService);
  private designationService = inject(DesignationService);
  private authService = inject(AuthService);
  private destroyRef = inject(DestroyRef);

  // Only HR / CompanyAdmin may edit or delete; everyone else sees a read-only profile.
  canManage = computed(() => ['HR', 'CompanyAdmin'].includes(this.authService.role()));

  // ---- State ----
  loading = signal(true);
  error = signal<string | null>(null);
  employee = signal<EmployeeDetail | null>(null);

  editing = signal(false);
  isSaving = signal(false);
  saveError = signal<string | null>(null);

  departments = signal<Department[]>([]);
  private allDesignations = signal<Designation[]>([]);
  managers = signal<ManagerOption[]>([]);
  readonly roles = ASSIGNABLE_ROLES;

  // Department selected in the edit form — drives the dependent dropdowns.
  private selectedDepartmentId = signal<string>('');
  departmentSelected = computed(() => this.selectedDepartmentId() !== '');
  filteredDesignations = computed(() => {
    const deptId = this.selectedDepartmentId();
    return deptId ? this.allDesignations().filter((d) => d.departmentId === deptId) : [];
  });

  // Resend invite state
  isResending = signal(false);
  resendResult = signal<string | null>(null);
  inviteCopied = signal(false);

  // Delete state
  confirmingDelete = signal(false);
  isDeleting = signal(false);

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
    // Disabled until a department is chosen (department-scoped).
    designationId: [{ value: '', disabled: true }],
    managerId: [{ value: '', disabled: true }],
    roleId: [EMPLOYEE_ROLE_ID, [Validators.required]],
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

    this.designationService.getDesignations().subscribe({
      next: (items) => this.allDesignations.set(items),
      error: () => {}
    });

    // React to user-driven department changes (prefill uses emitEvent:false, so it's skipped).
    this.form.controls.departmentId.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((deptId) => this.onDepartmentChange(deptId));
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

  private loadManagers(departmentId: string): void {
    this.employeeService.getDepartmentManagers(departmentId).subscribe({
      next: (items) => this.managers.set(items),
      error: () => {}
    });
  }

  /** User changed the department in the edit form: reset & reload the dependent fields. */
  private onDepartmentChange(departmentId: string): void {
    this.selectedDepartmentId.set(departmentId);
    this.form.controls.designationId.setValue('');
    this.form.controls.managerId.setValue('');
    this.managers.set([]);

    if (!departmentId) {
      this.form.controls.designationId.disable();
      this.form.controls.managerId.disable();
      return;
    }

    this.form.controls.designationId.enable();
    this.form.controls.managerId.enable();
    this.loadManagers(departmentId);
  }

  // ---- Edit mode ----
  startEdit(): void {
    const emp = this.employee();
    if (!emp) return;

    const deptId = emp.department?.id ?? '';
    this.selectedDepartmentId.set(deptId);

    // Prefill without emitting — so the department-change reset doesn't wipe designation/manager.
    this.form.setValue({
      firstName: emp.firstName,
      lastName: emp.lastName,
      phone: emp.phone ?? '',
      joinDate: emp.joinDate,
      departmentId: deptId,
      designationId: emp.designation?.id ?? '',
      managerId: emp.manager?.id ?? '',
      roleId: emp.role?.id || EMPLOYEE_ROLE_ID,
      status: emp.status
    }, { emitEvent: false });

    // Enable/disable the department-scoped fields to match the prefilled department.
    if (deptId) {
      this.form.controls.designationId.enable({ emitEvent: false });
      this.form.controls.managerId.enable({ emitEvent: false });
      this.loadManagers(deptId);
    } else {
      this.form.controls.designationId.disable({ emitEvent: false });
      this.form.controls.managerId.disable({ emitEvent: false });
      this.managers.set([]);
    }

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
      designationId: value.designationId || undefined,
      managerId: value.managerId || undefined,
      roleId: value.roleId,
      status: Number(value.status),
      // Preserve fields not exposed in this form
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

  // ---- Delete ----
  askDelete(): void {
    this.saveError.set(null);
    this.confirmingDelete.set(true);
  }

  cancelDelete(): void {
    this.confirmingDelete.set(false);
  }

  confirmDelete(): void {
    this.isDeleting.set(true);
    this.saveError.set(null);

    this.employeeService.deleteEmployee(this.employeeId).subscribe({
      next: () => {
        this.isDeleting.set(false);
        this.confirmingDelete.set(false);
        this.router.navigate(['/employees']);
      },
      error: (err: HttpErrorResponse) => {
        this.isDeleting.set(false);
        this.confirmingDelete.set(false);
        const apiError = err.error as ApiError;
        this.saveError.set(apiError?.title || 'Could not delete employee.');
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
