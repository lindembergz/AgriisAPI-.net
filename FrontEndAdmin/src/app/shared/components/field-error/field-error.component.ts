
import { Component, Input, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AbstractControl, FormGroup } from '@angular/forms';
import { ValidationService } from '../../services/validation.service';
import { Subscription } from 'rxjs';
import { startWith, debounceTime } from 'rxjs/operators';

export type ErrorDisplayMode = 'text' | 'tooltip' | 'inline' | 'badge';
export type ErrorSeverity = 'error' | 'warning' | 'info';

/**
 * Field Error Component
 * Displays form field validation errors with various display modes and styles
 */
@Component({
  selector: 'app-field-error',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './field-error.component.html',
  styleUrls: ['./field-error.component.scss']
})
export class FieldErrorComponent implements OnInit, OnDestroy {
  // Input Properties
  @Input() control: AbstractControl | null = null;
  @Input() fieldName = '';
  @Input() displayMode: ErrorDisplayMode = 'text';
  @Input() severity: ErrorSeverity = 'error';
  @Input() showIcon = true;
  @Input() showMultiple = false;
  @Input() maxErrors = 3;
  @Input() customMessages: Record<string, string> = {};
  @Input() hideWhenValid = true;
  @Input() showOnTouch = true;
  @Input() showOnDirty = true;
  @Input() showOnSubmit = false;
  @Input() animateChanges = true;
  @Input() position: 'top' | 'bottom' | 'left' | 'right' = 'bottom';
  @Input() styleClass = '';
  @Input() errorIcon = 'pi pi-exclamation-triangle';
  @Input() warningIcon = 'pi pi-exclamation-circle';
  @Input() infoIcon = 'pi pi-info-circle';
  @Input() prefix = '';
  @Input() suffix = '';
  @Input() template?: string;
  @Input() showFieldName = false;
  @Input() fieldDisplayName = '';
  @Input() groupErrors = false;
  @Input() sortErrors = true;
  @Input() debounceTime = 300;
  @Input() minLength = 0;
  @Input() maxLength = 200;
  @Input() truncateErrors = true;
  @Input() showErrorCode = false;
  @Input() linkToField = false;
  @Input() focusOnError = false;
  @Input() ariaLive: 'polite' | 'assertive' | 'off' = 'polite';

  // Internal State
  protected isVisible = signal(false);
  protected errorMessages = signal<string[]>([]);
  private formSubmitSubscription?: Subscription;

  constructor(private validationService: ValidationService) {}

  ngOnInit(): void {
    if (this.control) {
      this.control.statusChanges.pipe(
        startWith(this.control.status),
        debounceTime(this.debounceTime)
      ).subscribe(() => this.updateErrorState());

      if (this.showOnSubmit) {
        const form = this.control.root;
        if (form instanceof FormGroup) {
          // this.formSubmitSubscription = this.validationService.getFormSubmitObservable(form).subscribe(() => {
          //   this.updateErrorState(true);
          // });
        }
      }
    }
  }

  ngOnDestroy(): void {
    this.formSubmitSubscription?.unsubscribe();
  }

  private updateErrorState(submitted = false): void {
    if (!this.control) {
      this.isVisible.set(false);
      return;
    }

    const { dirty, touched, errors } = this.control;
    const isTouched = !this.showOnTouch || touched;
    const isDirty = !this.showOnDirty || dirty;
    const isSubmitted = !this.showOnSubmit || submitted;

    if ((isTouched && isDirty) || isSubmitted) {
      if (errors) {
        const messages = Object.keys(errors).map(key =>
          this.customMessages[key] || this.validationService.getErrorMessage(this.control, this.fieldName)
        );
        this.errorMessages.set(this.showMultiple ? messages.slice(0, this.maxErrors) : [messages[0]]);
        this.isVisible.set(true);
      } else {
        this.isVisible.set(this.hideWhenValid ? false : true);
        this.errorMessages.set([]);
      }
    } else {
      this.isVisible.set(false);
    }
  }

  shouldShow(): boolean {
    return this.isVisible();
  }

  displayText(): string {
    return this.errorMessages().join(', ');
  }

  containerClasses(): string {
    return `field-error-container ${this.displayMode} ${this.styleClass}`;
  }

  iconClass(): string {
    return this.getIconClass();
  }

  getFieldDisplayName(): string {
    return this.fieldDisplayName || this.fieldName;
  }

  hasMultipleErrors(): boolean {
    return this.errorMessages().length > 1;
  }

  errorCount(): number {
    return this.errorMessages().length;
  }

  getAllErrors(): string[] {
    return this.errorMessages();
  }

  trackByError(index: number, error: string): string {
    return error;
  }

  onErrorClick(): void {
    if (this.focusOnError && this.control) {
      (this.control as any).nativeElement?.focus();
    }
  }

  getFirstError(): string {
    return this.errorMessages()[0] || '';
  }

  getErrorCountText(): string {
    return `${this.errorCount()} erros`;
  }

  getAriaLabel(): string {
    return `Erro no campo ${this.getFieldDisplayName()}: ${this.displayText()}`;
  }

  protected getIconClass(): string {
    switch (this.severity) {
      case 'warning':
        return this.warningIcon;
      case 'info':
        return this.infoIcon;
      default:
        return this.errorIcon;
    }
  }
}
