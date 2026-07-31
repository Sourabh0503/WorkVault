import { Component, Input } from '@angular/core';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';

/**
 * Registration wizard — Step 1: company details.
 * Presentational: renders controls of the shared parent form; the container
 * owns validation, navigation, and submission.
 */
@Component({
  selector: 'app-step-company',
  imports: [ReactiveFormsModule],
  templateUrl: './step-company.html',
  styleUrl: './step-company.scss',
})
export class StepCompany {
  /** The wizard's shared form group (owned by the container). */
  @Input({ required: true }) form!: FormGroup;

  /** Server-side validation errors keyed by PascalCase field name. */
  @Input() fieldErrors: Record<string, string[]> = {};

  industries = [
    'IT / Software',
    'Manufacturing',
    'Healthcare',
    'Retail',
    'Education',
    'Finance',
    'Consulting',
    'Other',
  ];
}
