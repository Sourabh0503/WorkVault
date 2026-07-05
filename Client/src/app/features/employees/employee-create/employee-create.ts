import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { EmployeeService } from '../../../core/services/employee.service';
import { DepartmentService } from '../../../core/services/department.service';
import { CreateEmployeeResult } from '../../../core/models/employee.models';
import { Department } from '../../../core/models/department.models';
import { ApiError } from '../../../core/models/auth.models';

@Component({
  selector: 'app-employee-create',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './employee-create.html',
  styleUrl: './employee-create.scss'
})
export class EmployeeCreate implements OnInit {
  private fb = inject(FormBuilder);
  private employeeService = inject(EmployeeService);
  private departmentService = inject(DepartmentService);
  private router = inject(Router);

  // ---- Form + state ----
  isSubmitting = signal(false);
  errorMessage = signal<string | null>(null);
  fieldErrors = signal<Record<string, string[]>>({});

  // ---- Dropdown data ----
  departments = signal<Department[]>([]);

  // ---- Success state ----
  createdResult = signal<CreateEmployeeResult | null>(null);
  linkCopied = signal(false);

  form = this.fb.nonNullable.group({
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
    phone: [''],
    joinDate: ['', [Validators.required]],
    departmentId: ['']
  });

  ngOnInit(): void {
    // Load departments for the dropdown
    this.departmentService.getDepartments().subscribe({
      next: (depts) => this.departments.set(depts),
      error: () => {} // form still works without departments
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
      departmentId: value.departmentId || undefined
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
    this.form.reset();
  }

  goToList(): void {
    this.router.navigate(['/employees']);
  }
}