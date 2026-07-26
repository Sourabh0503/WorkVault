import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { DesignationService } from '../../../core/services/designation.service';
import { DepartmentService } from '../../../core/services/department.service';
import { Designation } from '../../../core/models/designation.models';
import { Department } from '../../../core/models/department.models';
import { ApiError } from '../../../core/models/auth.models';

@Component({
  selector: 'app-designation-list',
  imports: [ReactiveFormsModule],
  templateUrl: './designation-list.html',
  styleUrl: './designation-list.scss',
})
/**
 * Designations page. Lists job titles as a card grid and manages them inline via a
 * single form that both creates and edits (`editingId` distinguishes the mode). Each
 * designation belongs to a department, so the form loads departments for its dropdown.
 */
export class DesignationList implements OnInit {
  private designationService = inject(DesignationService);
  private departmentService = inject(DepartmentService);
  private fb = inject(FormBuilder);

  designations = signal<Designation[]>([]);
  departments = signal<Department[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);

  showForm = signal(false);
  isSubmitting = signal(false);
  formError = signal<string | null>(null);

  // Which designation is being edited (null = creating a new one)
  editingId = signal<string | null>(null);

  // Delete confirmation state
  deletingId = signal<string | null>(null);

  form = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(100)]],
    level: [1, [Validators.required, Validators.min(1), Validators.max(10)]],
    departmentId: ['', [Validators.required]],
  });

  ngOnInit(): void {
    this.load();
    this.loadDepartments();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.designationService.getDesignations().subscribe({
      next: (items) => {
        this.designations.set(items);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load designations.');
        this.loading.set(false);
      },
    });
  }

  private loadDepartments(): void {
    this.departmentService.getDepartments().subscribe({
      next: (depts) => this.departments.set(depts),
      error: () => {}, // form still renders; department select is just empty
    });
  }

  // ---- Create ----
  openCreate(): void {
    this.editingId.set(null);
    this.formError.set(null);
    this.form.reset({ title: '', level: 1, departmentId: '' });
    this.showForm.set(true);
  }

  // ---- Edit ----
  openEdit(designation: Designation): void {
    this.editingId.set(designation.id);
    this.formError.set(null);
    this.form.setValue({
      title: designation.title,
      level: designation.level,
      departmentId: designation.departmentId,
    });
    this.showForm.set(true);
  }

  closeForm(): void {
    this.showForm.set(false);
    this.editingId.set(null);
    this.form.reset({ title: '', level: 1, departmentId: '' });
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
      this.designationService
        .updateDesignation(id, {
          id,
          title: value.title,
          level: Number(value.level),
          departmentId: value.departmentId,
        })
        .subscribe({
          next: () => this.onSaveSuccess(),
          error: (err) => this.onSaveError(err),
        });
    } else {
      this.designationService
        .createDesignation({
          title: value.title,
          level: Number(value.level),
          departmentId: value.departmentId,
        })
        .subscribe({
          next: () => this.onSaveSuccess(),
          error: (err) => this.onSaveError(err),
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
    this.formError.set(apiError?.title || 'Could not save designation.');
  }

  // ---- Delete ----
  confirmDelete(id: string): void {
    this.deletingId.set(id);
  }

  cancelDelete(): void {
    this.deletingId.set(null);
  }

  doDelete(id: string): void {
    this.designationService.deleteDesignation(id).subscribe({
      next: () => {
        this.deletingId.set(null);
        this.load();
      },
      error: (err: HttpErrorResponse) => {
        const apiError = err.error as ApiError;
        this.error.set(apiError?.title || 'Could not delete designation.');
        this.deletingId.set(null);
      },
    });
  }
}
