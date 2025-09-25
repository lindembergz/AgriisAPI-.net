import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Component } from '@angular/core';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

import { cpfCnpjValidator } from '../shared/utils/document-validators.util';
import { phoneValidator, emailValidator, cepValidator, numericValidator, yearValidator, nameValidator } from '../shared/utils/field-validators.util';
import { ValidationService } from '../shared/services/validation.service';

@Component({
  template: `
    <form [formGroup]="testForm">
      <input formControlName="nome" />
      <input formControlName="email" />
      <input formControlName="cpfCnpj" />
      <input formControlName="telefone" />
      <input formControlName="cep" />
      <input formControlName="area" />
      <input formControlName="anoSafra" />
    </form>
  `,
  standalone: true,
  imports: [ReactiveFormsModule]
})
class TestFormComponent {
  testForm = this.fb.group({
    nome: ['', [Validators.required, nameValidator(2)]],
    email: ['', [Validators.required, emailValidator()]],
    cpfCnpj: ['', [Validators.required, cpfCnpjValidator()]],
    telefone: ['', [phoneValidator()]],
    cep: ['', [cepValidator()]],
    area: ['', [numericValidator(0.1)]],
    anoSafra: ['', [yearValidator()]]
  });

  constructor(private fb: FormBuilder) {}
}

describe('Form Validation Workflow Integration Tests', () => {
  let component: TestFormComponent;
  let fixture: ComponentFixture<TestFormComponent>;
  let validationService: ValidationService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        TestFormComponent,
        ReactiveFormsModule,
        NoopAnimationsModule
      ],
      providers: [
        ValidationService
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(TestFormComponent);
    component = fixture.componentInstance;
    validationService = TestBed.inject(ValidationService);
    fixture.detectChanges();
  });

  describe('Required Field Validations', () => {
    it('should validate required nome field', () => {
      const nomeControl = component.testForm.get('nome');
      
      // Empty value should be invalid
      nomeControl?.setValue('');
      expect(nomeControl?.hasError('required')).toBe(true);
      expect(nomeControl?.valid).toBe(false);

      // Valid value should be valid
      nomeControl?.setValue('João Silva');
      expect(nomeControl?.hasError('required')).toBe(false);
      expect(nomeControl?.valid).toBe(true);
    });

    it('should validate required email field', () => {
      const emailControl = component.testForm.get('email');
      
      // Empty value should be invalid
      emailControl?.setValue('');
      expect(emailControl?.hasError('required')).toBe(true);
      expect(emailControl?.valid).toBe(false);

      // Valid email should be valid
      emailControl?.setValue('test@example.com');
      expect(emailControl?.hasError('required')).toBe(false);
      expect(emailControl?.hasError('email')).toBe(false);
      expect(emailControl?.valid).toBe(true);
    });

    it('should validate required cpfCnpj field', () => {
      const cpfCnpjControl = component.testForm.get('cpfCnpj');
      
      // Empty value should be invalid
      cpfCnpjControl?.setValue('');
      expect(cpfCnpjControl?.hasError('required')).toBe(true);
      expect(cpfCnpjControl?.valid).toBe(false);

      // Valid CPF should be valid
      cpfCnpjControl?.setValue('12345678901');
      expect(cpfCnpjControl?.hasError('required')).toBe(false);
      expect(cpfCnpjControl?.valid).toBe(true);
    });
  });

  describe('Email Format Validation', () => {
    it('should validate email format correctly', () => {
      const emailControl = component.testForm.get('email');

      const testCases = [
        { value: 'invalid-email', valid: false },
        { value: 'invalid@', valid: false },
        { value: '@invalid.com', valid: false },
        { value: 'invalid.com', valid: false },
        { value: 'test@example.com', valid: true },
        { value: 'user.name@domain.co.uk', valid: true },
        { value: 'test+tag@example.org', valid: true }
      ];

      testCases.forEach(testCase => {
        emailControl?.setValue(testCase.value);
        expect(emailControl?.valid).toBe(testCase.valid, 
          `Email "${testCase.value}" should be ${testCase.valid ? 'valid' : 'invalid'}`);
      });
    });
  });

  describe('CPF/CNPJ Validation', () => {
    it('should validate CPF format correctly', () => {
      const cpfCnpjControl = component.testForm.get('cpfCnpj');

      const validCPFs = [
        '12345678901',
        '123.456.789-01',
        '98765432100'
      ];

      const invalidCPFs = [
        '1234567890', // Too short
        '123456789012', // Too long
        '00000000000', // All zeros
        '11111111111', // All same digit
        'abcdefghijk', // Non-numeric
        '123.456.789-00' // Invalid check digits
      ];

      validCPFs.forEach(cpf => {
        cpfCnpjControl?.setValue(cpf);
        expect(cpfCnpjControl?.valid).toBe(true, `CPF "${cpf}" should be valid`);
      });

      invalidCPFs.forEach(cpf => {
        cpfCnpjControl?.setValue(cpf);
        expect(cpfCnpjControl?.valid).toBe(false, `CPF "${cpf}" should be invalid`);
      });
    });

    it('should validate CNPJ format correctly', () => {
      const cpfCnpjControl = component.testForm.get('cpfCnpj');

      const validCNPJs = [
        '12345678000195',
        '12.345.678/0001-95',
        '98765432000123'
      ];

      const invalidCNPJs = [
        '1234567800019', // Too short
        '123456780001955', // Too long
        '00000000000000', // All zeros
        '11111111111111', // All same digit
        'abcdefghijklmn', // Non-numeric
        '12.345.678/0001-00' // Invalid check digits
      ];

      validCNPJs.forEach(cnpj => {
        cpfCnpjControl?.setValue(cnpj);
        expect(cpfCnpjControl?.valid).toBe(true, `CNPJ "${cnpj}" should be valid`);
      });

      invalidCNPJs.forEach(cnpj => {
        cpfCnpjControl?.setValue(cnpj);
        expect(cpfCnpjControl?.valid).toBe(false, `CNPJ "${cnpj}" should be invalid`);
      });
    });
  });

  describe('Telefone Validation', () => {
    it('should validate telefone format correctly', () => {
      const telefoneControl = component.testForm.get('telefone');

      const validTelefones = [
        '(11) 99999-9999',
        '(11) 3333-3333',
        '11999999999',
        '1133333333'
      ];

      const invalidTelefones = [
        '123', // Too short
        '(11) 9999-999', // Missing digit
        '(11) 99999-99999', // Too many digits
        'abcd-efgh', // Non-numeric
        '(00) 99999-9999' // Invalid area code
      ];

      validTelefones.forEach(telefone => {
        telefoneControl?.setValue(telefone);
        expect(telefoneControl?.valid).toBe(true, `Telefone "${telefone}" should be valid`);
      });

      invalidTelefones.forEach(telefone => {
        telefoneControl?.setValue(telefone);
        expect(telefoneControl?.valid).toBe(false, `Telefone "${telefone}" should be invalid`);
      });
    });
  });

  describe('CEP Validation', () => {
    it('should validate CEP format correctly', () => {
      const cepControl = component.testForm.get('cep');

      const validCEPs = [
        '01234-567',
        '01234567',
        '12345-678'
      ];

      const invalidCEPs = [
        '1234-567', // Too short
        '123456789', // Too long
        'abcde-fgh', // Non-numeric
        '00000-000' // All zeros
      ];

      validCEPs.forEach(cep => {
        cepControl?.setValue(cep);
        expect(cepControl?.valid).toBe(true, `CEP "${cep}" should be valid`);
      });

      invalidCEPs.forEach(cep => {
        cepControl?.setValue(cep);
        expect(cepControl?.valid).toBe(false, `CEP "${cep}" should be invalid`);
      });
    });
  });

  describe('Numeric Field Validations', () => {
    it('should validate positive numbers for area field', () => {
      const areaControl = component.testForm.get('area');

      const validAreas = [
        '100',
        '100.5',
        '0.1',
        '1000000'
      ];

      const invalidAreas = [
        '-100', // Negative
        '0', // Zero
        'abc', // Non-numeric
        '', // Empty
        '100.5.5' // Invalid decimal
      ];

      validAreas.forEach(area => {
        areaControl?.setValue(area);
        expect(areaControl?.valid).toBe(true, `Area "${area}" should be valid`);
      });

      invalidAreas.forEach(area => {
        areaControl?.setValue(area);
        expect(areaControl?.valid).toBe(false, `Area "${area}" should be invalid`);
      });
    });

    it('should validate ano safra correctly', () => {
      const anoSafraControl = component.testForm.get('anoSafra');
      const currentYear = new Date().getFullYear();

      const validYears = [
        currentYear.toString(),
        (currentYear + 1).toString(),
        (currentYear - 1).toString(),
        '2020',
        '2025'
      ];

      const invalidYears = [
        '1999', // Too old
        '2050', // Too far in future
        'abc', // Non-numeric
        '20', // Too short
        '20245' // Too long
      ];

      validYears.forEach(year => {
        anoSafraControl?.setValue(year);
        expect(anoSafraControl?.valid).toBe(true, `Year "${year}" should be valid`);
      });

      invalidYears.forEach(year => {
        anoSafraControl?.setValue(year);
        expect(anoSafraControl?.valid).toBe(false, `Year "${year}" should be invalid`);
      });
    });
  });

  describe('Complex Form Validation Scenarios', () => {
    it('should validate entire form correctly', () => {
      // Initially form should be invalid (required fields empty)
      expect(component.testForm.valid).toBe(false);

      // Fill with valid data
      component.testForm.patchValue({
        nome: 'João Silva',
        email: 'joao@example.com',
        cpfCnpj: '12345678901',
        telefone: '(11) 99999-9999',
        cep: '01234-567',
        area: '100.5',
        anoSafra: '2024'
      });

      expect(component.testForm.valid).toBe(true);
    });

    it('should handle partial form validation', () => {
      // Fill only some required fields
      component.testForm.patchValue({
        nome: 'João Silva',
        email: 'joao@example.com'
        // cpfCnpj still empty (required)
      });

      expect(component.testForm.valid).toBe(false);
      expect(component.testForm.get('nome')?.valid).toBe(true);
      expect(component.testForm.get('email')?.valid).toBe(true);
      expect(component.testForm.get('cpfCnpj')?.valid).toBe(false);
    });

    it('should validate form with mixed valid and invalid data', () => {
      component.testForm.patchValue({
        nome: 'João Silva', // Valid
        email: 'invalid-email', // Invalid
        cpfCnpj: '12345678901', // Valid
        telefone: 'invalid-phone', // Invalid
        cep: '01234-567', // Valid
        area: '-100', // Invalid (negative)
        anoSafra: '2024' // Valid
      });

      expect(component.testForm.valid).toBe(false);
      expect(component.testForm.get('nome')?.valid).toBe(true);
      expect(component.testForm.get('email')?.valid).toBe(false);
      expect(component.testForm.get('cpfCnpj')?.valid).toBe(true);
      expect(component.testForm.get('telefone')?.valid).toBe(false);
      expect(component.testForm.get('cep')?.valid).toBe(true);
      expect(component.testForm.get('area')?.valid).toBe(false);
      expect(component.testForm.get('anoSafra')?.valid).toBe(true);
    });
  });

  describe('Validation Service Integration', () => {
    it('should get validation messages correctly', () => {
      const nomeControl = component.testForm.get('nome');
      nomeControl?.setValue('');
      nomeControl?.markAsTouched();

      expect(nomeControl?.hasError('required')).toBe(true);
    });

    it('should validate multiple error types', () => {
      const emailControl = component.testForm.get('email');
      emailControl?.setValue('invalid-email');
      emailControl?.markAsTouched();

      expect(emailControl?.hasError('emailInvalid')).toBe(true);
    });

    it('should handle custom validation messages', () => {
      const cpfCnpjControl = component.testForm.get('cpfCnpj');
      cpfCnpjControl?.setValue('invalid-cpf');
      cpfCnpjControl?.markAsTouched();

      expect(cpfCnpjControl?.hasError('documentInvalid')).toBe(true);
    });
  });

  describe('Real-time Validation Behavior', () => {
    it('should validate on value changes', () => {
      const emailControl = component.testForm.get('email');
      
      // Start with invalid email
      emailControl?.setValue('invalid');
      expect(emailControl?.valid).toBe(false);

      // Update to valid email
      emailControl?.setValue('valid@example.com');
      expect(emailControl?.valid).toBe(true);

      // Update back to invalid
      emailControl?.setValue('invalid-again');
      expect(emailControl?.valid).toBe(false);
    });

    it('should maintain validation state across form interactions', () => {
      // Set initial values
      component.testForm.patchValue({
        nome: 'Valid Name',
        email: 'valid@example.com'
      });

      expect(component.testForm.get('nome')?.valid).toBe(true);
      expect(component.testForm.get('email')?.valid).toBe(true);

      // Change one field to invalid
      component.testForm.get('email')?.setValue('invalid-email');
      
      // Nome should still be valid, email should be invalid
      expect(component.testForm.get('nome')?.valid).toBe(true);
      expect(component.testForm.get('email')?.valid).toBe(false);
      expect(component.testForm.valid).toBe(false);
    });
  });
});