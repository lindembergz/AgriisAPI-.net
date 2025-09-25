import { Directive, Input, ElementRef, Renderer2, OnInit, OnDestroy, inject } from '@angular/core';
import { AbstractControl } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { ValidationService } from '../services/validation.service';

/**
 * Validation Error Directive
 * Automatically adds/removes error styling to form fields based on validation state
 * Can be applied to any form control element
 */
@Directive({
  selector: '[appValidationError]',
  standalone: true
})
export class ValidationErrorDirective implements OnInit, OnDestroy {
  private validationService = inject(ValidationService);
  private el = inject(ElementRef);
  private renderer = inject(Renderer2);
  private destroy$ = new Subject<void>();

  @Input({ required: true }) appValidationError!: AbstractControl;
  @Input() errorClass = 'ng-invalid ng-dirty';
  @Input() validClass = 'ng-valid';

  ngOnInit(): void {
    if (this.appValidationError) {
      // Watch for status changes
      this.appValidationError.statusChanges
        .pipe(takeUntil(this.destroy$))
        .subscribe(() => {
          this.updateErrorState();
        });

      // Watch for value changes
      this.appValidationError.valueChanges
        .pipe(takeUntil(this.destroy$))
        .subscribe(() => {
          this.updateErrorState();
        });

      // Initial state
      this.updateErrorState();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private updateErrorState(): void {
    const hasError = this.validationService.shouldShowError(this.appValidationError);
    
    // Remove existing classes
    this.renderer.removeClass(this.el.nativeElement, this.errorClass);
    this.renderer.removeClass(this.el.nativeElement, this.validClass);
    
    // Add appropriate class
    if (hasError) {
      this.renderer.addClass(this.el.nativeElement, this.errorClass);
    } else if (this.appValidationError.valid && (this.appValidationError.dirty || this.appValidationError.touched)) {
      this.renderer.addClass(this.el.nativeElement, this.validClass);
    }
  }
}