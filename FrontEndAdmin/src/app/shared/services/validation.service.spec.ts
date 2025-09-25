import { TestBed } from '@angular/core/testing';
import { FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { ValidationService } from './validation.service';

describe('ValidationService', () => {
  let service: ValidationService;
  let fb: FormBuilder;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ValidationService);
    fb = TestBed.inject(FormBuilder);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getErrorMessage', () => {
    it('should return custom error message when available', () => {
      const control = fb.control('');
      control.setErrors({ customError: { message: 'Custom error message' } });

      const message = service.getErrorMessage(control);
      expect(message).toBe('Custom error message');
    });

    it('should return field-specific message when available', () => {
      const control = fb.control('');
      control.setErrors({ required: true });

      const message = service.getErrorMessage(control, 'nome');
      expect(message).toBe('Nome é obrigatório');
    });

    it('should return detailed message for minlength error', () => {
      const control = fb.control('');
      control.setErrors({ minlength: { requiredLength: 5, actualLength: 2 } });

      const message = service.getErrorMessage(control);
      expect(message).toBe('Mínimo de 5 caracteres');
    });

    it('should return detailed message for min error', () => {
      const control = fb.control('');
      control.setErrors({ min: { min: 10, actual: 5 } });

      const message = service.getErrorMessage(control);
      expect(message).toBe('Valor mínimo: 10');
    });

    it('should return default message for unknown error', () => {
      const control = fb.control('');
      control.setErrors({ unknownError: true });

      const message = service.getErrorMessage(control);
      expect(message).toBe('Campo inválido');
    });

    it('should return empty string when no errors', () => {
      const control = fb.control('valid');

      const message = service.getErrorMessage(control);
      expect(message).toBe('');
    });
  });

  describe('hasError', () => {
    it('should return true when control has specific error and is touched', () => {
      const control = fb.control('');
      control.setErrors({ required: true });
      control.markAsTouched();

      expect(service.hasError(control, 'required')).toBe(true);
    });

    it('should return false when control has no error', () => {
      const control = fb.control('valid');
      control.markAsTouched();

      expect(service.hasError(control, 'required')).toBe(false);
    });

    it('should return false when control is not touched', () => {
      const control = fb.control('');
      control.setErrors({ required: true });

      expect(service.hasError(control, 'required')).toBe(false);
    });
  });

  describe('shouldShowError', () => {
    it('should return true when control is invalid and touched', () => {
      const control = fb.control('', Validators.required);
      control.markAsTouched();

      expect(service.shouldShowError(control)).toBe(true);
    });

    it('should return true when control is invalid and dirty', () => {
      const control = fb.control('', Validators.required);
      control.markAsDirty();

      expect(service.shouldShowError(control)).toBe(true);
    });

    it('should return false when control is valid', () => {
      const control = fb.control('valid', Validators.required);
      control.markAsTouched();

      expect(service.shouldShowError(control)).toBe(false);
    });

    it('should return false when control is invalid but not touched or dirty', () => {
      const control = fb.control('', Validators.required);

      expect(service.shouldShowError(control)).toBe(false);
    });
  });

  describe('getAllErrorMessages', () => {
    it('should return all error messages for a control', () => {
      const control = fb.control('');
      control.setErrors({ 
        required: true, 
        minlength: { requiredLength: 5 },
        customError: { message: 'Custom message' }
      });

      const messages = service.getAllErrorMessages(control, 'nome');
      expect(messages).toContain('Nome é obrigatório');
      expect(messages).toContain('Mínimo de 5 caracteres');
      expect(messages).toContain('Custom message');
    });

    it('should return empty array when no errors', () => {
      const control = fb.control('valid');

      const messages = service.getAllErrorMessages(control);
      expect(messages).toEqual([]);
    });
  });

  describe('markFormGroupTouched', () => {
    it('should mark all controls in form group as touched', () => {
      const form = fb.group({
        name: [''],
        email: [''],
        nested: fb.group({
          field1: [''],
          field2: ['']
        }),
        array: fb.array([
          fb.control(''),
          fb.group({ item: [''] })
        ])
      });

      service.markFormGroupTouched(form);

      expect(form.get('name')?.touched).toBe(true);
      expect(form.get('email')?.touched).toBe(true);
      expect(form.get('nested.field1')?.touched).toBe(true);
      expect(form.get('nested.field2')?.touched).toBe(true);
      
      const array = form.get('array') as FormArray;
      expect(array.at(0).touched).toBe(true);
      expect(array.at(1).get('item')?.touched).toBe(true);
    });
  });

  describe('getFormValidationSummary', () => {
    it('should return validation summary for form', () => {
      const form = fb.group({
        name: ['', Validators.required],
        email: ['invalid-email', Validators.email],
        nested: fb.group({
          field: ['', Validators.required]
        })
      });

      const summary = service.getFormValidationSummary(form);

      expect(summary.isValid).toBe(false);
      expect(summary.errors.length).toBeGreaterThan(0);
      expect(summary.errors.some(error => error.includes('name'))).toBe(true);
      expect(summary.errors.some(error => error.includes('email'))).toBe(true);
      expect(summary.errors.some(error => error.includes('nested.field'))).toBe(true);
    });

    it('should return valid summary for valid form', () => {
      const form = fb.group({
        name: ['John Doe', Validators.required],
        email: ['john@example.com', Validators.email]
      });

      const summary = service.getFormValidationSummary(form);

      expect(summary.isValid).toBe(true);
      expect(summary.errors).toEqual([]);
    });
  });

  describe('validateRequiredFields', () => {
    it('should validate required fields', () => {
      const form = fb.group({
        name: ['John'],
        email: [''],
        phone: ['123456789']
      });

      const result = service.validateRequiredFields(form, ['name', 'email', 'phone']);

      expect(result.isValid).toBe(false);
      expect(result.missingFields).toContain('email');
      expect(result.missingFields).not.toContain('name');
      expect(result.missingFields).not.toContain('phone');
    });

    it('should return valid when all required fields are present', () => {
      const form = fb.group({
        name: ['John'],
        email: ['john@example.com']
      });

      const result = service.validateRequiredFields(form, ['name', 'email']);

      expect(result.isValid).toBe(true);
      expect(result.missingFields).toEqual([]);
    });
  });

  describe('clearValidationErrors', () => {
    it('should clear all validation errors from form', () => {
      const form = fb.group({
        name: ['', Validators.required],
        nested: fb.group({
          field: ['', Validators.required]
        }),
        array: fb.array([
          fb.control('', Validators.required)
        ])
      });

      // Trigger validation
      form.markAllAsTouched();
      form.updateValueAndValidity();

      // Verify errors exist
      expect(form.get('name')?.errors).toBeTruthy();
      expect(form.get('nested.field')?.errors).toBeTruthy();

      // Clear errors
      service.clearValidationErrors(form);

      // Verify errors are cleared
      expect(form.get('name')?.errors).toBeNull();
      expect(form.get('nested.field')?.errors).toBeNull();
      expect((form.get('array') as FormArray).at(0).errors).toBeNull();
    });
  });

  describe('custom message management', () => {
    it('should add custom field message', () => {
      service.addFieldMessage('customField', 'customError', 'Custom field error');

      const control = fb.control('');
      control.setErrors({ customError: true });

      const message = service.getErrorMessage(control, 'customField');
      expect(message).toBe('Custom field error');
    });

    it('should add custom default message', () => {
      service.addDefaultMessage('customGlobalError', 'Custom global error');

      const control = fb.control('');
      control.setErrors({ customGlobalError: true });

      const message = service.getErrorMessage(control);
      expect(message).toBe('Custom global error');
    });
  });
});