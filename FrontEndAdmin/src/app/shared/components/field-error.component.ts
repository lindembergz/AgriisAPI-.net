import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AbstractControl } from '@angular/forms';
import { ValidationService } from '../services/validation.service';

/**
 * Field Error Component
 * Displays validation error messages for form controls
 * Provides consistent error display across the application
 */
@Component({
  selector: 'app-field-error',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div 
      *ngIf="shouldShowError" 
      class="field-error"
      [attr.data-testid]="'error-' + fieldName"
    >
      <small class="p-error">{{ errorMessage }}</small>
    </div>
  `,
  styles: [`
    .field-error {
      margin-top: 0.25rem;
      display: block;
    }
    
    .p-error {
      color: var(--red-500);
      font-size: 0.875rem;
      line-height: 1.25rem;
    }
  `]
})
export class FieldErrorComponent {
  private validationService = inject(ValidationService);

  @Input({ required: true }) control!: AbstractControl;
  @Input() fieldName?: string;

  get shouldShowError(): boolean {
    return this.validationService.shouldShowError(this.control);
  }

  get errorMessage(): string {
    return this.validationService.getErrorMessage(this.control, this.fieldName);
  }
}