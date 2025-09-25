import { Injectable } from '@angular/core';
import { AbstractControl, FormGroup, FormArray } from '@angular/forms';

/**
 * Validation Service
 * Provides centralized validation message handling and form validation utilities
 */
@Injectable({
  providedIn: 'root'
})
export class ValidationService {

  /**
   * Default validation messages for common validators
   */
  private readonly defaultMessages: { [key: string]: string } = {
    required: 'Campo obrigatório',
    email: 'E-mail inválido',
    minlength: 'Muito curto',
    maxlength: 'Muito longo',
    min: 'Valor muito baixo',
    max: 'Valor muito alto',
    pattern: 'Formato inválido',
    
    // Custom validators
    cpfInvalid: 'CPF inválido',
    cnpjInvalid: 'CNPJ inválido',
    documentInvalid: 'Documento inválido',
    phoneInvalid: 'Telefone inválido',
    emailInvalid: 'E-mail inválido',
    cepInvalid: 'CEP inválido',
    nameInvalid: 'Nome inválido',
    passwordInvalid: 'Senha inválida',
    strongPasswordInvalid: 'Senha não atende aos critérios de segurança',
    numericInvalid: 'Valor numérico inválido',
    areaInvalid: 'Área inválida',
    yearInvalid: 'Ano inválido'
  };

  /**
   * Field-specific validation messages
   */
  private readonly fieldMessages: { [fieldName: string]: { [errorType: string]: string } } = {
    nome: {
      required: 'Nome é obrigatório',
      minlength: 'Nome deve ter pelo menos 2 caracteres',
      nameInvalid: 'Nome contém caracteres inválidos'
    },
    cpfCnpj: {
      required: 'CPF/CNPJ é obrigatório',
      cpfInvalid: 'CPF inválido',
      cnpjInvalid: 'CNPJ inválido',
      documentInvalid: 'CPF/CNPJ inválido'
    },
    tipoCliente: {
      required: 'Tipo de cliente é obrigatório'
    },
    email: {
      required: 'E-mail é obrigatório',
      email: 'E-mail inválido',
      emailInvalid: 'E-mail inválido'
    },
    telefone: {
      phoneInvalid: 'Telefone inválido',
      pattern: 'Telefone inválido'
    },
    senha: {
      required: 'Senha é obrigatória',
      minlength: 'Senha deve ter pelo menos 6 caracteres',
      passwordInvalid: 'Senha inválida',
      strongPasswordInvalid: 'Senha deve conter letras maiúsculas, minúsculas, números e símbolos'
    },
    logradouro: {
      required: 'Logradouro é obrigatório',
      minlength: 'Logradouro deve ter pelo menos 3 caracteres'
    },
    numero: {
      required: 'Número é obrigatório'
    },
    bairro: {
      required: 'Bairro é obrigatório',
      minlength: 'Bairro deve ter pelo menos 2 caracteres'
    },
    cidade: {
      required: 'Cidade é obrigatória',
      minlength: 'Cidade deve ter pelo menos 2 caracteres'
    },
    uf: {
      required: 'UF é obrigatória'
    },
    cep: {
      required: 'CEP é obrigatório',
      cepInvalid: 'CEP inválido',
      pattern: 'CEP inválido'
    },
    area: {
      required: 'Área é obrigatória',
      min: 'Área deve ser maior que zero',
      areaInvalid: 'Área inválida'
    },
    anoSafra: {
      required: 'Ano da safra é obrigatório',
      min: 'Ano muito antigo',
      max: 'Ano muito futuro',
      yearInvalid: 'Ano inválido'
    },
    areaCultivada: {
      required: 'Área cultivada é obrigatória',
      min: 'Área cultivada deve ser maior que zero',
      areaInvalid: 'Área cultivada inválida'
    },
    tipo: {
      required: 'Tipo é obrigatório'
    }
  };

  /**
   * Get validation error message for a form control
   */
  getErrorMessage(control: AbstractControl, fieldName?: string): string {
    if (!control.errors) {
      return '';
    }

    const errorKeys = Object.keys(control.errors);
    const firstErrorKey = errorKeys[0];
    const firstError = control.errors[firstErrorKey];

    // Check if error has custom message
    if (firstError && typeof firstError === 'object' && firstError.message) {
      return firstError.message;
    }

    // Check field-specific messages
    if (fieldName && this.fieldMessages[fieldName] && this.fieldMessages[fieldName][firstErrorKey]) {
      return this.fieldMessages[fieldName][firstErrorKey];
    }

    // Check for detailed error messages with parameters
    if (firstErrorKey === 'minlength' && firstError.requiredLength) {
      return `Mínimo de ${firstError.requiredLength} caracteres`;
    }

    if (firstErrorKey === 'maxlength' && firstError.requiredLength) {
      return `Máximo de ${firstError.requiredLength} caracteres`;
    }

    if (firstErrorKey === 'min' && firstError.min !== undefined) {
      return `Valor mínimo: ${firstError.min}`;
    }

    if (firstErrorKey === 'max' && firstError.max !== undefined) {
      return `Valor máximo: ${firstError.max}`;
    }

    // Handle complex password validation errors
    if (firstErrorKey === 'passwordInvalid' && typeof firstError === 'object') {
      const passwordErrors = Object.values(firstError);
      return passwordErrors[0] as string || 'Senha inválida';
    }

    if (firstErrorKey === 'strongPasswordInvalid' && typeof firstError === 'object') {
      const passwordErrors = Object.values(firstError);
      return passwordErrors[0] as string || 'Senha não atende aos critérios de segurança';
    }

    // Fallback to default messages
    return this.defaultMessages[firstErrorKey] || 'Campo inválido';
  }

  /**
   * Check if a form control has a specific error and is touched/dirty
   */
  hasError(control: AbstractControl, errorType: string): boolean {
    return !!(control.errors && control.errors[errorType] && (control.dirty || control.touched));
  }

  /**
   * Check if a form control is invalid and should show error
   */
  shouldShowError(control: AbstractControl): boolean {
    return !!(control.invalid && (control.dirty || control.touched));
  }

  /**
   * Get all error messages for a form control
   */
  getAllErrorMessages(control: AbstractControl, fieldName?: string): string[] {
    if (!control.errors) {
      return [];
    }

    return Object.keys(control.errors).map(errorKey => {
      const error = control.errors![errorKey];
      
      // Check if error has custom message
      if (error && typeof error === 'object' && error.message) {
        return error.message;
      }

      // Check field-specific messages
      if (fieldName && this.fieldMessages[fieldName] && this.fieldMessages[fieldName][errorKey]) {
        return this.fieldMessages[fieldName][errorKey];
      }

      // Fallback to default messages
      return this.defaultMessages[errorKey] || 'Campo inválido';
    });
  }

  /**
   * Mark all controls in a form group as touched to trigger validation display
   */
  markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      } else if (control instanceof FormArray) {
        this.markFormArrayTouched(control);
      } else {
        control?.markAsTouched();
      }
    });
  }

  /**
   * Mark all controls in a form array as touched
   */
  markFormArrayTouched(formArray: FormArray): void {
    formArray.controls.forEach(control => {
      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      } else if (control instanceof FormArray) {
        this.markFormArrayTouched(control);
      } else {
        control.markAsTouched();
      }
    });
    formArray.markAsTouched();
  }

  /**
   * Get validation summary for a form group
   */
  getFormValidationSummary(formGroup: FormGroup): { isValid: boolean; errors: string[] } {
    const errors: string[] = [];
    
    const collectErrors = (group: FormGroup, prefix: string = '') => {
      Object.keys(group.controls).forEach(key => {
        const control = group.get(key);
        const fieldPath = prefix ? `${prefix}.${key}` : key;
        
        if (control instanceof FormGroup) {
          collectErrors(control, fieldPath);
        } else if (control instanceof FormArray) {
          control.controls.forEach((arrayControl, index) => {
            if (arrayControl instanceof FormGroup) {
              collectErrors(arrayControl, `${fieldPath}[${index}]`);
            } else if (arrayControl.invalid) {
              const errorMessage = this.getErrorMessage(arrayControl, key);
              errors.push(`${fieldPath}[${index}]: ${errorMessage}`);
            }
          });
        } else if (control && control.invalid) {
          const errorMessage = this.getErrorMessage(control, key);
          errors.push(`${fieldPath}: ${errorMessage}`);
        }
      });
    };

    collectErrors(formGroup);

    return {
      isValid: formGroup.valid,
      errors
    };
  }

  /**
   * Validate required fields in a form group
   */
  validateRequiredFields(formGroup: FormGroup, requiredFields: string[]): { isValid: boolean; missingFields: string[] } {
    const missingFields: string[] = [];

    requiredFields.forEach(fieldPath => {
      const control = formGroup.get(fieldPath);
      if (!control || !control.value || (typeof control.value === 'string' && !control.value.trim())) {
        missingFields.push(fieldPath);
      }
    });

    return {
      isValid: missingFields.length === 0,
      missingFields
    };
  }

  /**
   * Clear all validation errors from a form group
   */
  clearValidationErrors(formGroup: FormGroup): void {
    const clearErrors = (group: FormGroup) => {
      Object.keys(group.controls).forEach(key => {
        const control = group.get(key);
        if (control instanceof FormGroup) {
          clearErrors(control);
        } else if (control instanceof FormArray) {
          control.controls.forEach(arrayControl => {
            if (arrayControl instanceof FormGroup) {
              clearErrors(arrayControl);
            } else {
              arrayControl.setErrors(null);
            }
          });
        } else if (control) {
          control.setErrors(null);
        }
      });
    };

    clearErrors(formGroup);
  }

  /**
   * Add custom validation message for a field
   */
  addFieldMessage(fieldName: string, errorType: string, message: string): void {
    if (!this.fieldMessages[fieldName]) {
      this.fieldMessages[fieldName] = {};
    }
    this.fieldMessages[fieldName][errorType] = message;
  }

  /**
   * Add custom default validation message
   */
  addDefaultMessage(errorType: string, message: string): void {
    this.defaultMessages[errorType] = message;
  }
}