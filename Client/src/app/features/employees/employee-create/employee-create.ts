import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { EmployeeService } from '../../../core/services/employee.service';
import { DepartmentService } from '../../../core/services/department.service';
import { DesignationService } from '../../../core/services/designation.service';
import { CreateEmployeeResult, DepartmentMember } from '../../../core/models/employee.models';
import { Department } from '../../../core/models/department.models';
import { Designation } from '../../../core/models/designation.models';
import { AuthService } from '../../../core/services/auth.service';
import { ASSIGNABLE_ROLES, COMPANY_ADMIN_ROLE_ID, EMPLOYEE_ROLE_ID } from '../../../core/models/role.models';
import { ApiError } from '../../../core/models/auth.models';

@Component({
  selector: 'app-employee-create',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './employee-create.html',
  styleUrl: './employee-create.scss'
})
/**
 * Add-employee page. Loads departments + designations for the dropdowns, submits the
 * create form via `EmployeeService.createEmployee`, maps field-level API errors, and on
 * success shows a confirmation panel with the shareable invite link.
 *
 * Designation and manager are department-scoped: both are disabled until a department is
 * chosen; designations are filtered to that department client-side, and managers are
 * fetched per department (any Active employee in that department).
 */
export class EmployeeCreate implements OnInit {
  private fb = inject(FormBuilder);
  private employeeService = inject(EmployeeService);
  private departmentService = inject(DepartmentService);
  private designationService = inject(DesignationService);
  private authService = inject(AuthService);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  // ---- Form + state ----
  isSubmitting = signal(false);
  errorMessage = signal<string | null>(null);
  fieldErrors = signal<Record<string, string[]>>({});

  // ---- Dropdown data ----
  departments = signal<Department[]>([]);
  private allDesignations = signal<Designation[]>([]);
  deptMembers = signal<DepartmentMember[]>([]);

  // Only a company admin can create another company admin.
  roles = computed(() =>
    this.authService.role() === 'CompanyAdmin'
      ? ASSIGNABLE_ROLES
      : ASSIGNABLE_ROLES.filter((r) => r.id !== COMPANY_ADMIN_ROLE_ID),
  );

  // Department currently selected — drives the dependent dropdowns.
  private selectedDepartmentId = signal<string>('');
  departmentSelected = computed(() => this.selectedDepartmentId() !== '');

  // Only the selected department's designations.
  filteredDesignations = computed(() => {
    const deptId = this.selectedDepartmentId();
    return deptId ? this.allDesignations().filter((d) => d.departmentId === deptId) : [];
  });

  // ---- Success state ----
  createdResult = signal<CreateEmployeeResult | null>(null);
  linkCopied = signal(false);

  form = this.fb.nonNullable.group({
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
    phone: [''],
    joinDate: ['', [Validators.required]],
    departmentId: [''],
    // Disabled until a department is chosen (they're department-scoped).
    designationId: [{ value: '', disabled: true }],
    managerId: [{ value: '', disabled: true }],
    roleId: [EMPLOYEE_ROLE_ID, [Validators.required]]
  });

  ngOnInit(): void {
    // Load dropdown data — the form still works if any of these fail (all optional).
    this.departmentService.getDepartments().subscribe({
      next: (depts) => this.departments.set(depts),
      error: () => {}
    });

    this.designationService.getDesignations().subscribe({
      next: (items) => this.allDesignations.set(items),
      error: () => {}
    });

    // When the department changes, reset the dependent fields and reload managers.
    this.form.controls.departmentId.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((deptId) => this.onDepartmentChange(deptId));
  }

  private onDepartmentChange(departmentId: string): void {
    this.selectedDepartmentId.set(departmentId);
    // Clear selections that no longer apply to the new department.
    this.form.controls.designationId.setValue('');
    this.form.controls.managerId.setValue('');
    this.deptMembers.set([]);

    if (!departmentId) {
      this.form.controls.designationId.disable();
      this.form.controls.managerId.disable();
      return;
    }

    this.form.controls.designationId.enable();
    this.form.controls.managerId.enable();
    this.employeeService.getDepartmentMembers(departmentId).subscribe({
      next: (items) => this.deptMembers.set(items),
      error: () => {}
    });
  }

  onSubmit(): void {
    this.errorMessage.set(null);
    this.fieldErrors.set({});

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    const value = this.form.getRawValue();

    this.employeeService.createEmployee({
      firstName: value.firstName,
      lastName: value.lastName,
      email: value.email,
      phone: value.phone || undefined,
      joinDate: value.joinDate,
      departmentId: value.departmentId || undefined,
      designationId: value.designationId || undefined,
      managerId: value.managerId || undefined,
      roleId: value.roleId
    }).subscribe({
      next: (result) => {
        this.isSubmitting.set(false);
        this.createdResult.set(result);   // switch to success view
      },
      error: (err: HttpErrorResponse) => {
        this.isSubmitting.set(false);
        const apiError = err.error as ApiError;
        if (apiError?.errors) {
          this.fieldErrors.set(apiError.errors);
        } else {
          this.errorMessage.set(apiError?.title || 'Could not create employee.');
        }
      }
    });
  }

  copyLink(): void {
    const link = this.createdResult()?.inviteLink;
    if (!link) return;
    navigator.clipboard.writeText(link).then(() => {
      this.linkCopied.set(true);
      setTimeout(() => this.linkCopied.set(false), 2000);
    });
  }

  addAnother(): void {
    this.createdResult.set(null);
    this.form.reset({ roleId: EMPLOYEE_ROLE_ID });
    this.selectedDepartmentId.set('');
    this.deptMembers.set([]);
  }

  goToList(): void {
    this.router.navigate(['/employees']);
  }
}
