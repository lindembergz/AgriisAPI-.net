import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/**
 * Comprehensive field validation utilities
 * Provides common validation functions for forms
 */

/**
 * Brazilian phone number validator
 * Validates Brazilian mobile and landline phone numbers
 */
export function phoneValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) {
      return null; // Don't validate empty values, use required validator for that
    }

    const phone = control.value.replace(/\D/g, ''); // Remove non-digits

    // Brazilian phone numbers: 10 digits (landline) or 11 digits (mobile)
    if (phone.length < 10 || phone.length > 11) {
      return { 
        phoneInvalid: { 
          message: 'Telefone deve conter 10 ou 11 dígitos' 
        } 
      };
    }

    // Validate area code (first 2 digits should be between 11-99)
    const areaCode = parseInt(phone.substring(0, 2));
    if (areaCode < 11 || areaCode > 99) {
      return { 
        phoneInvalid: { 
          message: 'Código de área inválido' 
        } 
      };
    }

    // For mobile numbers (11 digits), the third digit should be 9
    if (phone.length === 11) {
      const thirdDigit = parseInt(phone.charAt(2));
      if (thirdDigit !== 9) {
        return { 
          phoneInvalid: { 
            message: 'Número de celular deve começar com 9 após o DDD' 
          } 
        };
      }
    }

    return null;
  };
}

/**
 * Enhanced email validator with Brazilian domain support
 * More comprehensive than Angular's built-in email validator
 */
export function emailValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) {
      return null; // Don't validate empty values, use required validator for that
    }

    const email = control.value.trim().toLowerCase();
    
    // Basic email regex pattern
    const emailPattern = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    
    if (!emailPattern.test(email)) {
      return { 
        emailInvalid: { 
          message: 'E-mail inválido' 
        } 
      };
    }

    // Additional validations
    const [localPart, domain] = email.split('@');
    
    // Local part validations
    if (localPart.length > 64) {
      return { 
        emailInvalid: { 
          message: 'Parte local do e-mail muito longa' 
        } 
      };
    }
    
    if (localPart.startsWith('.') || localPart.endsWith('.')) {
      return { 
        emailInvalid: { 
          message: 'E-mail não pode começar ou terminar com ponto' 
        } 
      };
    }
    
    if (localPart.includes('..')) {
      return { 
        emailInvalid: { 
          message: 'E-mail não pode conter pontos consecutivos' 
        } 
      };
    }

    // Domain validations
    if (domain.length > 253) {
      return { 
        emailInvalid: { 
          message: 'Domínio do e-mail muito longo' 
        } 
      };
    }

    return null;
  };
}

/**
 * Brazilian CEP (postal code) validator
 */
export function cepValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) {
      return null; // Don't validate empty values, use required validator for that
    }

    const cep = control.value.replace(/\D/g, ''); // Remove non-digits

    if (cep.length !== 8) {
      return { 
        cepInvalid: { 
          message: 'CEP deve conter 8 dígitos' 
        } 
      };
    }

    // Check for invalid patterns (all same digits)
    if (/^(\d)\1{7}$/.test(cep)) {
      return { 
        cepInvalid: { 
          message: 'CEP inválido' 
        } 
      };
    }

    return null;
  };
}

/**
 * Name validator with Brazilian naming conventions
 */
export function nameValidator(minLength: number = 2): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) {
      return null; // Don't validate empty values, use required validator for that
    }

    const name = control.value.trim();

    if (name.length < minLength) {
      return { 
        nameInvalid: { 
          message: `Nome deve ter pelo menos ${minLength} caracteres` 
        } 
      };
    }

    // Check for valid characters (letters, spaces, hyphens, apostrophes)
    const namePattern = /^[a-zA-ZÀ-ÿ\s'-]+$/;
    if (!namePattern.test(name)) {
      return { 
        nameInvalid: { 
          message: 'Nome contém caracteres inválidos' 
        } 
      };
    }

    // Check for consecutive spaces
    if (name.includes('  ')) {
      return { 
        nameInvalid: { 
          message: 'Nome não pode conter espaços consecutivos' 
        } 
      };
    }

    // Check if starts or ends with space
    if (name !== name.trim()) {
      return { 
        nameInvalid: { 
          message: 'Nome não pode começar ou terminar com espaço' 
        } 
      };
    }

    return null;
  };
}

/**
 * Password strength validator
 */
export function passwordValidator(minLength: number = 6): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) {
      return null; // Don't validate empty values, use required validator for that
    }

    const password = control.value;
    const errors: any = {};

    if (password.length < minLength) {
      errors.minLength = `Senha deve ter pelo menos ${minLength} caracteres`;
    }

    // Check for at least one letter
    if (!/[a-zA-Z]/.test(password)) {
      errors.noLetter = 'Senha deve conter pelo menos uma letra';
    }

    // Check for at least one number
    if (!/\d/.test(password)) {
      errors.noNumber = 'Senha deve conter pelo menos um número';
    }

    // Return errors if any, otherwise null
    return Object.keys(errors).length > 0 ? { passwordInvalid: errors } : null;
  };
}

/**
 * Strong password validator (more strict)
 */
export function strongPasswordValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) {
      return null; // Don't validate empty values, use required validator for that
    }

    const password = control.value;
    const errors: any = {};

    if (password.length < 8) {
      errors.minLength = 'Senha deve ter pelo menos 8 caracteres';
    }

    if (!/[a-z]/.test(password)) {
      errors.noLowercase = 'Senha deve conter pelo menos uma letra minúscula';
    }

    if (!/[A-Z]/.test(password)) {
      errors.noUppercase = 'Senha deve conter pelo menos uma letra maiúscula';
    }

    if (!/\d/.test(password)) {
      errors.noNumber = 'Senha deve conter pelo menos um número';
    }

    if (!/[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]/.test(password)) {
      errors.noSpecialChar = 'Senha deve conter pelo menos um caractere especial';
    }

    // Return errors if any, otherwise null
    return Object.keys(errors).length > 0 ? { strongPasswordInvalid: errors } : null;
  };
}

/**
 * Numeric validator with min/max constraints
 */
export function numericValidator(min?: number, max?: number): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value && control.value !== 0) {
      return null; // Don't validate empty values, use required validator for that
    }

    const value = parseFloat(control.value);

    if (isNaN(value)) {
      return { 
        numericInvalid: { 
          message: 'Valor deve ser numérico' 
        } 
      };
    }

    if (min !== undefined && value < min) {
      return { 
        numericInvalid: { 
          message: `Valor deve ser maior ou igual a ${min}` 
        } 
      };
    }

    if (max !== undefined && value > max) {
      return { 
        numericInvalid: { 
          message: `Valor deve ser menor ou igual a ${max}` 
        } 
      };
    }

    return null;
  };
}

/**
 * Area validator for agricultural properties (in hectares)
 */
export function areaValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value && control.value !== 0) {
      return null; // Don't validate empty values, use required validator for that
    }

    const area = parseFloat(control.value);

    if (isNaN(area)) {
      return { 
        areaInvalid: { 
          message: 'Área deve ser numérica' 
        } 
      };
    }

    if (area <= 0) {
      return { 
        areaInvalid: { 
          message: 'Área deve ser maior que zero' 
        } 
      };
    }

    // Maximum reasonable area for a single property (100,000 hectares)
    if (area > 100000) {
      return { 
        areaInvalid: { 
          message: 'Área muito grande (máximo 100.000 hectares)' 
        } 
      };
    }

    return null;
  };
}

/**
 * Year validator for agricultural seasons
 */
export function yearValidator(minYear?: number, maxYear?: number): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) {
      return null; // Don't validate empty values, use required validator for that
    }

    const year = parseInt(control.value);
    const currentYear = new Date().getFullYear();
    
    const min = minYear || 2000;
    const max = maxYear || (currentYear + 5);

    if (isNaN(year)) {
      return { 
        yearInvalid: { 
          message: 'Ano deve ser numérico' 
        } 
      };
    }

    if (year < min) {
      return { 
        yearInvalid: { 
          message: `Ano deve ser maior ou igual a ${min}` 
        } 
      };
    }

    if (year > max) {
      return { 
        yearInvalid: { 
          message: `Ano deve ser menor ou igual a ${max}` 
        } 
      };
    }

    return null;
  };
}

/**
 * Utility function to get validation error message from control
 */
export function getValidationErrorMessage(control: AbstractControl): string {
  if (!control.errors) return '';

  // Check for custom error messages first
  const errorKeys = Object.keys(control.errors);
  const firstError = control.errors[errorKeys[0]];

  if (firstError && typeof firstError === 'object' && firstError.message) {
    return firstError.message;
  }

  // Fallback to default messages
  const errorMessages: { [key: string]: string } = {
    required: 'Campo obrigatório',
    email: 'E-mail inválido',
    minlength: `Mínimo de ${control.errors['minlength']?.requiredLength} caracteres`,
    maxlength: `Máximo de ${control.errors['maxlength']?.requiredLength} caracteres`,
    min: `Valor mínimo: ${control.errors['min']?.min}`,
    max: `Valor máximo: ${control.errors['max']?.max}`,
    pattern: 'Formato inválido'
  };

  return errorMessages[errorKeys[0]] || 'Campo inválido';
}

/**
 * Utility function to check if field has specific error
 */
export function hasFieldError(control: AbstractControl, errorType: string): boolean {
  return !!(control.errors && control.errors[errorType] && (control.dirty || control.touched));
}

/**
 * Utility function to check if field is invalid and touched
 */
export function isFieldInvalid(control: AbstractControl): boolean {
  return !!(control.invalid && (control.dirty || control.touched));
}