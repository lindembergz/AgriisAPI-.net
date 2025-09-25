import { FormControl } from '@angular/forms';
import { 
  cpfValidator, 
  cnpjValidator, 
  cpfCnpjValidator, 
  conditionalCpfCnpjValidator,
  formatCpf,
  formatCnpj,
  formatCpfCnpj,
  getCpfCnpjMask,
  getDynamicCpfCnpjMask
} from './document-validators.util';

describe('DocumentValidators', () => {

  describe('cpfValidator', () => {
    const validator = cpfValidator();

    it('should return null for empty values', () => {
      expect(validator(new FormControl(''))).toBeNull();
      expect(validator(new FormControl(null))).toBeNull();
      expect(validator(new FormControl(undefined))).toBeNull();
    });

    it('should validate valid CPF numbers', () => {
      // Valid CPF numbers (using real algorithm)
      expect(validator(new FormControl('11144477735'))).toBeNull();
      expect(validator(new FormControl('111.444.777-35'))).toBeNull();
    });

    it('should reject invalid CPF numbers', () => {
      // Invalid length
      expect(validator(new FormControl('123456789'))).toEqual({
        cpfInvalid: { message: 'CPF deve conter 11 dígitos' }
      });

      // All same digits
      expect(validator(new FormControl('11111111111'))).toEqual({
        cpfInvalid: { message: 'CPF inválido' }
      });

      // Invalid check digits
      expect(validator(new FormControl('12345678901'))).toEqual({
        cpfInvalid: { message: 'CPF inválido' }
      });
    });

    it('should handle formatted CPF numbers', () => {
      expect(validator(new FormControl('111.444.777-35'))).toBeNull();
      expect(validator(new FormControl('123.456.789-01'))).toEqual({
        cpfInvalid: { message: 'CPF inválido' }
      });
    });
  });

  describe('cnpjValidator', () => {
    const validator = cnpjValidator();

    it('should return null for empty values', () => {
      expect(validator(new FormControl(''))).toBeNull();
      expect(validator(new FormControl(null))).toBeNull();
      expect(validator(new FormControl(undefined))).toBeNull();
    });

    it('should validate valid CNPJ numbers', () => {
      // Valid CNPJ numbers (using real algorithm)
      expect(validator(new FormControl('11222333000181'))).toBeNull();
      expect(validator(new FormControl('11.222.333/0001-81'))).toBeNull();
    });

    it('should reject invalid CNPJ numbers', () => {
      // Invalid length
      expect(validator(new FormControl('123456789012'))).toEqual({
        cnpjInvalid: { message: 'CNPJ deve conter 14 dígitos' }
      });

      // All same digits
      expect(validator(new FormControl('11111111111111'))).toEqual({
        cnpjInvalid: { message: 'CNPJ inválido' }
      });

      // Invalid check digits
      expect(validator(new FormControl('12345678901234'))).toEqual({
        cnpjInvalid: { message: 'CNPJ inválido' }
      });
    });

    it('should handle formatted CNPJ numbers', () => {
      expect(validator(new FormControl('11.222.333/0001-81'))).toBeNull();
      expect(validator(new FormControl('12.345.678/9012-34'))).toEqual({
        cnpjInvalid: { message: 'CNPJ inválido' }
      });
    });
  });

  describe('cpfCnpjValidator', () => {
    const validator = cpfCnpjValidator();

    it('should return null for empty values', () => {
      expect(validator(new FormControl(''))).toBeNull();
      expect(validator(new FormControl(null))).toBeNull();
      expect(validator(new FormControl(undefined))).toBeNull();
    });

    it('should validate CPF when 11 digits provided', () => {
      expect(validator(new FormControl('11144477735'))).toBeNull();
      expect(validator(new FormControl('12345678901'))).toEqual({
        cpfInvalid: { message: 'CPF inválido' }
      });
    });

    it('should validate CNPJ when 14 digits provided', () => {
      expect(validator(new FormControl('11222333000181'))).toBeNull();
      expect(validator(new FormControl('12345678901234'))).toEqual({
        cnpjInvalid: { message: 'CNPJ inválido' }
      });
    });

    it('should reject invalid lengths', () => {
      expect(validator(new FormControl('123456789'))).toEqual({
        documentInvalid: { message: 'Documento deve conter 11 dígitos (CPF) ou 14 dígitos (CNPJ)' }
      });

      expect(validator(new FormControl('123456789012345'))).toEqual({
        documentInvalid: { message: 'Documento deve conter 11 dígitos (CPF) ou 14 dígitos (CNPJ)' }
      });
    });
  });

  describe('conditionalCpfCnpjValidator', () => {
    it('should validate CPF when tipo cliente is PF', () => {
      const formGroup = {
        get: (controlName: string) => {
          if (controlName === 'tipoCliente') {
            return { value: 'PF' };
          }
          return null;
        }
      };

      const control = new FormControl('11144477735');
      Object.defineProperty(control, 'parent', { value: formGroup, writable: true });

      const validator = conditionalCpfCnpjValidator('tipoCliente');
      expect(validator(control)).toBeNull();
    });

    it('should validate CNPJ when tipo cliente is PJ', () => {
      const formGroup = {
        get: (controlName: string) => {
          if (controlName === 'tipoCliente') {
            return { value: 'PJ' };
          }
          return null;
        }
      };

      const control = new FormControl('11222333000181');
      Object.defineProperty(control, 'parent', { value: formGroup, writable: true });

      const validator = conditionalCpfCnpjValidator('tipoCliente');
      expect(validator(control)).toBeNull();
    });

    it('should reject invalid CPF when tipo cliente is PF', () => {
      const formGroup = {
        get: (controlName: string) => {
          if (controlName === 'tipoCliente') {
            return { value: 'PF' };
          }
          return null;
        }
      };

      const control = new FormControl('12345678901');
      Object.defineProperty(control, 'parent', { value: formGroup, writable: true });

      const validator = conditionalCpfCnpjValidator('tipoCliente');
      expect(validator(control)).toEqual({
        cpfInvalid: { message: 'CPF inválido' }
      });
    });

    it('should reject invalid CNPJ when tipo cliente is PJ', () => {
      const formGroup = {
        get: (controlName: string) => {
          if (controlName === 'tipoCliente') {
            return { value: 'PJ' };
          }
          return null;
        }
      };

      const control = new FormControl('12345678901234');
      Object.defineProperty(control, 'parent', { value: formGroup, writable: true });

      const validator = conditionalCpfCnpjValidator('tipoCliente');
      expect(validator(control)).toEqual({
        cnpjInvalid: { message: 'CNPJ inválido' }
      });
    });
  });

  describe('formatCpf', () => {
    it('should format valid CPF', () => {
      expect(formatCpf('11144477735')).toBe('111.444.777-35');
    });

    it('should return original value for invalid CPF', () => {
      expect(formatCpf('123456789')).toBe('123456789');
      expect(formatCpf('abc')).toBe('abc');
    });

    it('should handle already formatted CPF', () => {
      expect(formatCpf('111.444.777-35')).toBe('111.444.777-35');
    });
  });

  describe('formatCnpj', () => {
    it('should format valid CNPJ', () => {
      expect(formatCnpj('11222333000181')).toBe('11.222.333/0001-81');
    });

    it('should return original value for invalid CNPJ', () => {
      expect(formatCnpj('123456789012')).toBe('123456789012');
      expect(formatCnpj('abc')).toBe('abc');
    });

    it('should handle already formatted CNPJ', () => {
      expect(formatCnpj('11.222.333/0001-81')).toBe('11.222.333/0001-81');
    });
  });

  describe('formatCpfCnpj', () => {
    it('should format CPF when 11 digits', () => {
      expect(formatCpfCnpj('11144477735')).toBe('111.444.777-35');
    });

    it('should format CNPJ when 14 digits', () => {
      expect(formatCpfCnpj('11222333000181')).toBe('11.222.333/0001-81');
    });

    it('should return original value for invalid lengths', () => {
      expect(formatCpfCnpj('123456789')).toBe('123456789');
      expect(formatCpfCnpj('123456789012345')).toBe('123456789012345');
    });
  });

  describe('getCpfCnpjMask', () => {
    it('should return CPF mask for PF', () => {
      expect(getCpfCnpjMask('PF')).toBe('999.999.999-99');
    });

    it('should return CNPJ mask for PJ', () => {
      expect(getCpfCnpjMask('PJ')).toBe('99.999.999/9999-99');
    });
  });

  describe('getDynamicCpfCnpjMask', () => {
    it('should return CPF mask for values up to 11 digits', () => {
      expect(getDynamicCpfCnpjMask('123')).toBe('999.999.999-99');
      expect(getDynamicCpfCnpjMask('12345678901')).toBe('999.999.999-99');
    });

    it('should return CNPJ mask for values over 11 digits', () => {
      expect(getDynamicCpfCnpjMask('123456789012')).toBe('99.999.999/9999-99');
      expect(getDynamicCpfCnpjMask('12345678901234')).toBe('99.999.999/9999-99');
    });

    it('should handle formatted values', () => {
      expect(getDynamicCpfCnpjMask('111.444.777-35')).toBe('999.999.999-99');
      expect(getDynamicCpfCnpjMask('11.222.333/0001-81')).toBe('99.999.999/9999-99');
    });
  });
});