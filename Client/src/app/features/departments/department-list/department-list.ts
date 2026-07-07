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

  departments = signal<Department[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);

  showForm = signal(false);
  isSubmitting = signal(false);
  formError = signal<string | null>(null);

  // Which department is being edited (null = creating a new one)
  editingId = signal<string | null>(null);

  // Delete confirmation state
  deletingId = signal<string | null>(null);

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

  // ---- Create ----
  openCreate(): void {
    this.editingId.set(null);
    this.formError.set(null);
    this.form.reset();
    this.showForm.set(true);
  }

  // ---- Edit ----
  openEdit(dept: Department): void {
    this.editingId.set(dept.id);
    this.formError.set(null);
    this.form.setValue({
      name: dept.name,
      description: dept.description ?? ''
    });
    this.showForm.set(true);
  }

  closeForm(): void {
    this.showForm.set(false);
    this.editingId.set(null);
    this.form.reset();
    this.formError.set(null);
  }

  onSubmit(): void {
    this.formError.set(null);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    const value = this.form.getRawValue();
    const id = this.editingId();

    if (id) {
      // Update
      this.departmentService.updateDepartment(id, {
        id,
        name: value.name,
        description: value.description || undefined
      }).subscribe({
        next: () => this.onSaveSuccess(),
        error: (err) => this.onSaveError(err)
      });
    } else {
      // Create
      this.departmentService.createDepartment({
        name: value.name,
        description: value.description || undefined
      }).subscribe({
        next: () => this.onSaveSuccess(),
        error: (err) => this.onSaveError(err)
      });
    }
  }

  private onSaveSuccess(): void {
    this.isSubmitting.set(false);
    this.closeForm();
    this.load();
  }

  private onSaveError(err: HttpErrorResponse): void {
    this.isSubmitting.set(false);
    const apiError = err.error as ApiError;
    this.formError.set(apiError?.title || 'Could not save department.');
  }

  // ---- Delete ----
  confirmDelete(id: string): void {
    this.deletingId.set(id);
  }

  cancelDelete(): void {
    this.deletingId.set(null);
  }

  doDelete(id: string): void {
    this.departmentService.deleteDepartment(id).subscribe({
      next: () => {
        this.deletingId.set(null);
        this.load();
      },
      error: (err: HttpErrorResponse) => {
        const apiError = err.error as ApiError;
        this.error.set(apiError?.title || 'Could not delete department.');
        this.deletingId.set(null);
      }
    });
  }
}