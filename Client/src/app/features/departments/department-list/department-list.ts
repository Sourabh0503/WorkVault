import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { DepartmentService } from '../../../core/services/department.service';
import { Department } from '../../../core/models/department.models';
import { ApiError } from '../../../core/models/auth.models';

@Component({
  selector: 'app-department-list',
  imports: [ReactiveFormsModule],
  templateUrl: './department-list.html',
  styleUrl: './department-list.scss'
})
export class DepartmentList implements OnInit {
  private departmentService = inject(DepartmentService);
  private fb = inject(FormBuilder);

  // ---- List state ----
  departments = signal<Department[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);

  // ---- Create form state ----
  showForm = signal(false);
  isSubmitting = signal(false);
  formError = signal<string | null>(null);

  form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    description: ['', [Validators.maxLength(500)]]
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);

    this.departmentService.getDepartments().subscribe({
      next: (depts) => {
        this.departments.set(depts);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load departments.');
        this.loading.set(false);
      }
    });
  }

  toggleForm(): void {
    this.showForm.update(v => !v);
    this.formError.set(null);
    this.form.reset();
  }

  onSubmit(): void {
    this.formError.set(null);

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    const value = this.form.getRawValue();

    this.departmentService.createDepartment({
      name: value.name,
      description: value.description || undefined
    }).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.showForm.set(false);
        this.form.reset();
        this.load();  // refresh the list
      },
      error: (err: HttpErrorResponse) => {
        this.isSubmitting.set(false);
        const apiError = err.error as ApiError;
        this.formError.set(apiError?.title || 'Could not create department.');
      }
    });
  }
}