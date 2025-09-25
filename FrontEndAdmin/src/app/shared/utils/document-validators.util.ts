import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/**
 * Brazilian document validation utilities
 * Provides comprehensive CPF and CNPJ validation following official algorithms
 */

/**
 * CPF (Cadastro de Pessoas Físicas) validator
 * Validates Brazilian individual taxpayer registry
 */
export function cpfValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) {
      return null; // Don't validate empty values, use required validator for that
    }

    const cpf = control.value.replace(/\D/g, ''); // Remove non-digits

    // Check basic format
    if (cpf.length !== 11) {
      return { cpfInvalid: { message: 'CPF deve conter 11 dígitos' } };
    }

    // Check for known invalid patterns (all same digits)
    if (/^(\d)\1{10}$/.test(cpf)) {
      return { cpfInvalid: { message: 'CPF inválido' } };
    }

    // Validate check digits using official algorithm
    if (!isValidCpf(cpf)) {
      return { cpfInvalid: { message: 'CPF inválido' } };
    }

    return null;
  };
}

/**
 * CNPJ (Cadastro Nacional da Pessoa Jurídica) validator
 * Validates Brazilian company taxpayer registry
 */
export function cnpjValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) {
      return null; // Don't validate empty values, use required validator for that
    }

    const cnpj = control.value.replace(/\D/g, ''); // Remove non-digits

    // Check basic format
    if (cnpj.length !== 14) {
      return { cnpjInvalid: { message: 'CNPJ deve conter 14 dígitos' } };
    }

    // Check for known invalid patterns (all same digits)
    if (/^(\d)\1{13}$/.test(cnpj)) {
      return { cnpjInvalid: { message: 'CNPJ inválido' } };
    }

    // Validate check digits using official algorithm
    if (!isValidCnpj(cnpj)) {
      return { cnpjInvalid: { message: 'CNPJ inválido' } };
    }

    return null;
  };
}

/**
 * Dynamic CPF/CNPJ validator based on document type
 * Automatically determines validation based on document length
 */
export function cpfCnpjValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) {
      return null; // Don't validate empty values, use required validator for that
    }

    const document = control.value.replace(/\D/g, ''); // Remove non-digits

    // Determine document type by length
    if (document.length === 11) {
      // CPF validation
      return cpfValidator()(control);
    } else if (document.length === 14) {
      // CNPJ validation
      return cnpjValidator()(control);
    } else if (document.length > 0) {
      // Invalid length
      return { 
        documentInvalid: { 
          message: 'Documento deve conter 11 dígitos (CPF) ou 14 dígitos (CNPJ)' 
        } 
      };
    }

    return null;
  };
}

/**
 * Conditional CPF/CNPJ validator based on tipo cliente field
 * Validates CPF for PF (Pessoa Física) and CNPJ for PJ (Pessoa Jurídica)
 */
export function conditionalCpfCnpjValidator(tipoClienteControlName: string): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) {
      return null; // Don't validate empty values, use required validator for that
    }

    const parent = control.parent;
    if (!parent) {
      return null; // Can't validate without parent form
    }

    const tipoClienteControl = parent.get(tipoClienteControlName);
    if (!tipoClienteControl) {
      return null; // Can't validate without tipo cliente control
    }

    const tipoCliente = tipoClienteControl.value;
    const document = control.value.replace(/\D/g, ''); // Remove non-digits

    if (tipoCliente === 'PF') {
      // Validate as CPF
      if (document.length !== 11) {
        return { cpfInvalid: { message: 'CPF deve conter 11 dígitos' } };
      }
      
      if (/^(\d)\1{10}$/.test(document)) {
        return { cpfInvalid: { message: 'CPF inválido' } };
      }
      
      if (!isValidCpf(document)) {
        return { cpfInvalid: { message: 'CPF inválido' } };
      }
    } else if (tipoCliente === 'PJ') {
      // Validate as CNPJ
      if (document.length !== 14) {
        return { cnpjInvalid: { message: 'CNPJ deve conter 14 dígitos' } };
      }
      
      if (/^(\d)\1{13}$/.test(document)) {
        return { cnpjInvalid: { message: 'CNPJ inválido' } };
      }
      
      if (!isValidCnpj(document)) {
        return { cnpjInvalid: { message: 'CNPJ inválido' } };
      }
    }

    return null;
  };
}

/**
 * CPF validation algorithm implementation
 * Uses the official Brazilian CPF validation algorithm
 */
function isValidCpf(cpf: string): boolean {
  // Calculate first check digit
  let sum = 0;
  for (let i = 0; i < 9; i++) {
    sum += parseInt(cpf.charAt(i)) * (10 - i);
  }
  
  let remainder = sum % 11;
  let firstCheckDigit = remainder < 2 ? 0 : 11 - remainder;
  
  if (parseInt(cpf.charAt(9)) !== firstCheckDigit) {
    return false;
  }

  // Calculate second check digit
  sum = 0;
  for (let i = 0; i < 10; i++) {
    sum += parseInt(cpf.charAt(i)) * (11 - i);
  }
  
  remainder = sum % 11;
  let secondCheckDigit = remainder < 2 ? 0 : 11 - remainder;
  
  return parseInt(cpf.charAt(10)) === secondCheckDigit;
}

/**
 * CNPJ validation algorithm implementation
 * Uses the official Brazilian CNPJ validation algorithm
 */
function isValidCnpj(cnpj: string): boolean {
  // Calculate first check digit
  const weights1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
  let sum = 0;
  
  for (let i = 0; i < 12; i++) {
    sum += parseInt(cnpj.charAt(i)) * weights1[i];
  }
  
  let remainder = sum % 11;
  let firstCheckDigit = remainder < 2 ? 0 : 11 - remainder;
  
  if (parseInt(cnpj.charAt(12)) !== firstCheckDigit) {
    return false;
  }

  // Calculate second check digit
  const weights2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
  sum = 0;
  
  for (let i = 0; i < 13; i++) {
    sum += parseInt(cnpj.charAt(i)) * weights2[i];
  }
  
  remainder = sum % 11;
  let secondCheckDigit = remainder < 2 ? 0 : 11 - remainder;
  
  return parseInt(cnpj.charAt(13)) === secondCheckDigit;
}

/**
 * Utility function to format CPF
 */
export function formatCpf(cpf: string): string {
  const cleaned = cpf.replace(/\D/g, '');
  if (cleaned.length !== 11) return cpf;
  
  return cleaned.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
}

/**
 * Utility function to format CNPJ
 */
export function formatCnpj(cnpj: string): string {
  const cleaned = cnpj.replace(/\D/g, '');
  if (cleaned.length !== 14) return cnpj;
  
  return cleaned.replace(/(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})/, '$1.$2.$3/$4-$5');
}

/**
 * Utility function to format CPF or CNPJ automatically
 */
export function formatCpfCnpj(document: string): string {
  const cleaned = document.replace(/\D/g, '');
  
  if (cleaned.length === 11) {
    return formatCpf(document);
  } else if (cleaned.length === 14) {
    return formatCnpj(document);
  }
  
  return document;
}

/**
 * Utility function to get appropriate mask for CPF/CNPJ input
 */
export function getCpfCnpjMask(tipoCliente: string | null | undefined): string {
  return tipoCliente === 'PF' ? '999.999.999-99' : '99.999.999/9999-99';
}

/**
 * Utility function to get dynamic mask based on input length
 */
export function getDynamicCpfCnpjMask(value: string): string {
  const cleaned = value.replace(/\D/g, '');
  
  if (cleaned.length <= 11) {
    return '999.999.999-99'; // CPF mask
  } else {
    return '99.999.999/9999-99'; // CNPJ mask
  }
}