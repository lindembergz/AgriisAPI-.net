import { Component, Input, Output, EventEmitter, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';

// PrimeNG imports
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { DividerModule } from 'primeng/divider';

// Models
import { UsuarioForm } from '../models/user.model';
import { UsuarioFormControls } from '../models/forms.model';

/**
 * Reusable Usuario Master Form Component
 * Can be used by both Produtores and Fornecedores modules
 */
@Component({
  selector: 'app-usuario-master-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    CardModule,
    ButtonModule,
    InputTextModule,
    PasswordModule,
    DividerModule
  ],
  templateUrl: './usuario-master-form.component.html',
  styleUrls: ['./usuario-master-form.component.scss']
})
export class UsuarioMasterFormComponent implements OnInit {
  private fb = inject(FormBuilder);

  @Input() usuarioFormGroup!: FormGroup<UsuarioFormControls>;
  @Input() readonly = false;
  @Input() showCard = true;
  @Output() usuarioChange = new EventEmitter<UsuarioForm>();

  // Signals for reactive state
  loading = signal(false);
  showPassword = signal(false);

  // Validation messages
  validationMessages = {
    nome: {
      required: 'Nome é obrigatório',
      minlength: 'Nome deve ter pelo menos 2 caracteres',
      maxlength: 'Nome deve ter no máximo 100 caracteres'
    },
    email: {
      required: 'E-mail é obrigatório',
      email: 'E-mail inválido',
      maxlength: 'E-mail deve ter no máximo 100 caracteres'
    },
    senha: {
      required: 'Senha é obrigatória',
      minlength: 'Senha deve ter pelo menos 6 caracteres',
      maxlength: 'Senha deve ter no máximo 50 caracteres'
    },
    telefone: {
      pattern: 'Telefone inválido',
      maxlength: 'Telefone deve ter no máximo 20 caracteres'
    }
  };

  ngOnInit(): void {
    // The form group should always be provided by the parent component
    if (!this.usuarioFormGroup) {
      throw new Error('usuarioFormGroup is required for UsuarioMasterFormComponent');
    }

    // Subscribe to form changes
    this.usuarioFormGroup.valueChanges.subscribe(() => {
      this.emitChange();
    });

    // Subscribe to status changes to ensure proper validation
    this.usuarioFormGroup.statusChanges.subscribe(() => {
      this.emitChange();
    });
  }

  /**
   * Create new usuario form group
   */
  private createUsuarioFormGroup(): FormGroup<UsuarioFormControls> {
    return this.fb.group<UsuarioFormControls>({
      nome: this.fb.control('', {
        nonNullable: true,
        validators: [
          Validators.required,
          Validators.minLength(2),
          Validators.maxLength(100)
        ]
      }),
      email: this.fb.control('', {
        nonNullable: true,
        validators: [
          Validators.required,
          Validators.email,
          Validators.maxLength(100)
        ]
      }),
      senha: this.fb.control('', {
        nonNullable: true,
        validators: [
          Validators.required,
          Validators.minLength(6),
          Validators.maxLength(50)
        ]
      }),
      telefone: this.fb.control('', {
        nonNullable: true,
        validators: [
          this.telefoneValidator,
          Validators.maxLength(20)
        ]
      })
    });
  }

  /**
   * Telefone validator (Brazilian phone format)
   */
  private telefoneValidator = (control: any) => {
    if (!control.value) return null;
    
    const value = control.value.replace(/\D/g, '');
    // Brazilian phone: 10 digits (landline) or 11 digits (mobile)
    if (value.length !== 10 && value.length !== 11) {
      return { pattern: true };
    }
    
    return null;
  };

  /**
   * Get form control for validation display
   */
  getFormControl(controlName: string) {
    return this.usuarioFormGroup.get(controlName);
  }

  /**
   * Check if field has error
   */
  hasFieldError(controlName: string, errorType?: string): boolean {
    const control = this.getFormControl(controlName);
    if (!control) return false;
    
    if (errorType) {
      return control.hasError(errorType) && (control.dirty || control.touched);
    }
    
    return control.invalid && (control.dirty || control.touched);
  }

  /**
   * Get field error message
   */
  getFieldErrorMessage(controlName: string): string {
    const control = this.getFormControl(controlName);
    if (!control || !control.errors) return '';
    
    const fieldMessages = this.validationMessages[controlName as keyof typeof this.validationMessages];
    if (!fieldMessages) return 'Campo inválido';
    
    const errorType = Object.keys(control.errors)[0] as keyof typeof fieldMessages;
    return fieldMessages[errorType] || 'Campo inválido';
  }

  /**
   * Toggle password visibility
   */
  togglePasswordVisibility(): void {
    this.showPassword.set(!this.showPassword());
  }

  /**
   * Generate random password
   */
  generatePassword(): void {
    if (this.readonly) return;

    const length = 12;
    const charset = 'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*';
    let password = '';
    
    for (let i = 0; i < length; i++) {
      password += charset.charAt(Math.floor(Math.random() * charset.length));
    }
    
    this.usuarioFormGroup.patchValue({ senha: password });
    this.showPassword.set(true);
  }

  /**
   * Clear form
   */
  clearForm(): void {
    if (this.readonly) return;
    
    this.usuarioFormGroup.reset();
    this.showPassword.set(false);
  }

  /**
   * Force validation update
   */
  forceValidationUpdate(): void {
    this.usuarioFormGroup.markAllAsTouched();
    this.usuarioFormGroup.updateValueAndValidity();
    Object.keys(this.usuarioFormGroup.controls).forEach(key => {
      const control = this.usuarioFormGroup.get(key);
      control?.markAsTouched();
      control?.updateValueAndValidity();
    });
  }

  /**
   * Validate form
   */
  validateForm(): boolean {
    this.usuarioFormGroup.markAllAsTouched();
    return this.usuarioFormGroup.valid;
  }

  /**
   * Get form data
   */
  getFormData(): UsuarioForm {
    return this.usuarioFormGroup.value as UsuarioForm;
  }

  /**
   * Set form data
   */
  setFormData(usuario: Partial<UsuarioForm>): void {
    this.usuarioFormGroup.patchValue(usuario);
  }

  /**
   * Emit changes to parent component
   */
  private emitChange(): void {
    const usuario = this.usuarioFormGroup.value as UsuarioForm;
    this.usuarioChange.emit(usuario);
  }

  /**
   * Check if form is valid
   */
  get isFormValid(): boolean {
    return this.usuarioFormGroup.valid;
  }

  /**
   * Check if form is dirty
   */
  get isFormDirty(): boolean {
    return this.usuarioFormGroup.dirty;
  }

  /**
   * Check if form is touched
   */
  get isFormTouched(): boolean {
    return this.usuarioFormGroup.touched;
  }
}