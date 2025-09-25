# Comprehensive Form Validation Guide

This guide explains how to use the comprehensive validation system implemented in the FrontEndAdmin application.

## Overview

The validation system provides:
- Brazilian document validation (CPF/CNPJ)
- Field-specific validators (phone, email, CEP, etc.)
- Centralized validation service
- Reusable validation components
- Consistent error messaging

## Core Components

### 1. Document Validators (`document-validators.util.ts`)

#### CPF Validation
```typescript
import { cpfValidator } from '../utils/document-validators.util';

// In form control
cpf: this.fb.control('', [Validators.required, cpfValidator()])
```

#### CNPJ Validation
```typescript
import { cnpjValidator } from '../utils/document-validators.util';

// In form control
cnpj: this.fb.control('', [Validators.required, cnpjValidator()])
```

#### Dynamic CPF/CNPJ Validation
```typescript
import { cpfCnpjValidator } from '../utils/document-validators.util';

// Automatically detects CPF (11 digits) or CNPJ (14 digits)
document: this.fb.control('', [Validators.required, cpfCnpjValidator()])
```

#### Conditional CPF/CNPJ Validation
```typescript
import { conditionalCpfCnpjValidator } from '../utils/document-validators.util';

// Validates based on another field (tipoCliente)
cpfCnpj: this.fb.control('', [
  Validators.required, 
  conditionalCpfCnpjValidator('tipoCliente')
])
```

### 2. Field Validators (`field-validators.util.ts`)

#### Phone Validation
```typescript
import { phoneValidator } from '../utils/field-validators.util';

telefone: this.fb.control('', [phoneValidator()])
```

#### Email Validation
```typescript
import { emailValidator } from '../utils/field-validators.util';

email: this.fb.control('', [Validators.required, emailValidator()])
```

#### Name Validation
```typescript
import { nameValidator } from '../utils/field-validators.util';

nome: this.fb.control('', [Validators.required, nameValidator(2)])
```

#### Password Validation
```typescript
import { passwordValidator, strongPasswordValidator } from '../utils/field-validators.util';

// Basic password (requires letter and number)
senha: this.fb.control('', [Validators.required, passwordValidator(6)])

// Strong password (requires uppercase, lowercase, number, special char)
senhaForte: this.fb.control('', [Validators.required, strongPasswordValidator()])
```

#### CEP Validation
```typescript
import { cepValidator } from '../utils/field-validators.util';

cep: this.fb.control('', [Validators.required, cepValidator()])
```

#### Area Validation
```typescript
import { areaValidator } from '../utils/field-validators.util';

area: this.fb.control(0, [Validators.required, areaValidator()])
```

#### Year Validation
```typescript
import { yearValidator } from '../utils/field-validators.util';

anoSafra: this.fb.control(2024, [
  Validators.required, 
  yearValidator(2000, 2030)
])
```

### 3. Validation Service (`validation.service.ts`)

#### Inject the Service
```typescript
import { ValidationService } from '../services/validation.service';

export class MyComponent {
  private validationService = inject(ValidationService);
}
```

#### Get Error Messages
```typescript
getFieldError(fieldName: string): string {
  const control = this.form.get(fieldName);
  return this.validationService.getErrorMessage(control, fieldName);
}
```

#### Check Error State
```typescript
hasFieldError(fieldName: string): boolean {
  const control = this.form.get(fieldName);
  return this.validationService.shouldShowError(control);
}
```

#### Mark Form as Touched
```typescript
onSubmit(): void {
  if (this.form.invalid) {
    this.validationService.markFormGroupTouched(this.form);
    return;
  }
  // Process valid form
}
```

#### Get Validation Summary
```typescript
showValidationSummary(): void {
  const summary = this.validationService.getFormValidationSummary(this.form);
  console.log('Form is valid:', summary.isValid);
  console.log('Errors:', summary.errors);
}
```

### 4. Field Error Component (`field-error.component.ts`)

#### Usage in Templates
```html
<input 
  formControlName="nome" 
  pInputText 
  class="w-full"
/>
<app-field-error 
  [control]="form.get('nome')" 
  fieldName="nome"
/>
```

### 5. Validation Error Directive (`validation-error.directive.ts`)

#### Usage in Templates
```html
<input 
  formControlName="email" 
  pInputText 
  [appValidationError]="form.get('email')"
  class="w-full"
/>
```

## Complete Form Example

```typescript
import { Component, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ValidationService } from '../services/validation.service';
import { 
  conditionalCpfCnpjValidator, 
  getCpfCnpjMask 
} from '../utils/document-validators.util';
import { 
  phoneValidator, 
  emailValidator, 
  nameValidator 
} from '../utils/field-validators.util';

@Component({
  selector: 'app-example-form',
  template: `
    <form [formGroup]="exampleForm" (ngSubmit)="onSubmit()">
      
      <!-- Tipo Cliente -->
      <div class="field">
        <label for="tipoCliente">Tipo de Cliente *</label>
        <p-select
          id="tipoCliente"
          formControlName="tipoCliente"
          [options]="tipoClienteOptions"
          [appValidationError]="getControl('tipoCliente')"
        />
        <app-field-error 
          [control]="getControl('tipoCliente')" 
          fieldName="tipoCliente"
        />
      </div>

      <!-- Nome -->
      <div class="field">
        <label for="nome">Nome *</label>
        <input
          id="nome"
          formControlName="nome"
          pInputText
          [appValidationError]="getControl('nome')"
        />
        <app-field-error 
          [control]="getControl('nome')" 
          fieldName="nome"
        />
      </div>

      <!-- CPF/CNPJ -->
      <div class="field">
        <label for="cpfCnpj">{{ getCpfCnpjLabel() }} *</label>
        <p-inputMask
          id="cpfCnpj"
          formControlName="cpfCnpj"
          [mask]="getCpfCnpjMask()"
          [appValidationError]="getControl('cpfCnpj')"
        />
        <app-field-error 
          [control]="getControl('cpfCnpj')" 
          fieldName="cpfCnpj"
        />
      </div>

      <!-- Email -->
      <div class="field">
        <label for="email">E-mail *</label>
        <input
          id="email"
          formControlName="email"
          pInputText
          [appValidationError]="getControl('email')"
        />
        <app-field-error 
          [control]="getControl('email')" 
          fieldName="email"
        />
      </div>

      <!-- Telefone -->
      <div class="field">
        <label for="telefone">Telefone</label>
        <p-inputMask
          id="telefone"
          formControlName="telefone"
          mask="(99) 99999-9999"
          [appValidationError]="getControl('telefone')"
        />
        <app-field-error 
          [control]="getControl('telefone')" 
          fieldName="telefone"
        />
      </div>

      <p-button 
        type="submit" 
        label="Salvar" 
        [disabled]="exampleForm.invalid"
      />
    </form>
  `
})
export class ExampleFormComponent {
  private fb = inject(FormBuilder);
  private validationService = inject(ValidationService);

  tipoClienteOptions = [
    { label: 'Pessoa Física', value: 'PF' },
    { label: 'Pessoa Jurídica', value: 'PJ' }
  ];

  exampleForm = this.fb.group({
    tipoCliente: ['PF', [Validators.required]],
    nome: ['', [Validators.required, nameValidator(2)]],
    cpfCnpj: ['', [Validators.required, conditionalCpfCnpjValidator('tipoCliente')]],
    email: ['', [Validators.required, emailValidator()]],
    telefone: ['', [phoneValidator()]]
  });

  constructor() {
    // Update CPF/CNPJ validation when tipo cliente changes
    this.exampleForm.get('tipoCliente')?.valueChanges.subscribe(() => {
      this.updateCpfCnpjValidation();
    });
  }

  getControl(controlName: string) {
    return this.exampleForm.get(controlName)!;
  }

  getCpfCnpjLabel(): string {
    const tipoCliente = this.exampleForm.get('tipoCliente')?.value;
    return tipoCliente === 'PF' ? 'CPF' : 'CNPJ';
  }

  getCpfCnpjMask(): string {
    const tipoCliente = this.exampleForm.get('tipoCliente')?.value;
    return getCpfCnpjMask(tipoCliente);
  }

  private updateCpfCnpjValidation(): void {
    const cpfCnpjControl = this.exampleForm.get('cpfCnpj');
    if (cpfCnpjControl) {
      cpfCnpjControl.setValidators([
        Validators.required, 
        conditionalCpfCnpjValidator('tipoCliente')
      ]);
      cpfCnpjControl.updateValueAndValidity();
    }
  }

  onSubmit(): void {
    if (this.exampleForm.valid) {
      console.log('Form submitted:', this.exampleForm.value);
    } else {
      this.validationService.markFormGroupTouched(this.exampleForm);
    }
  }
}
```

## Custom Validation Messages

### Adding Field-Specific Messages
```typescript
// In component constructor or ngOnInit
this.validationService.addFieldMessage('customField', 'customError', 'Custom error message');
```

### Adding Global Default Messages
```typescript
this.validationService.addDefaultMessage('customValidator', 'Default custom message');
```

## Testing Validation

### Unit Testing Validators
```typescript
import { FormControl } from '@angular/forms';
import { cpfValidator } from '../utils/document-validators.util';

describe('CPF Validator', () => {
  it('should validate valid CPF', () => {
    const validator = cpfValidator();
    const control = new FormControl('11144477735');
    
    expect(validator(control)).toBeNull();
  });

  it('should reject invalid CPF', () => {
    const validator = cpfValidator();
    const control = new FormControl('12345678901');
    
    expect(validator(control)).toEqual({
      cpfInvalid: { message: 'CPF inválido' }
    });
  });
});
```

### Testing Validation Service
```typescript
import { TestBed } from '@angular/core/testing';
import { FormBuilder, Validators } from '@angular/forms';
import { ValidationService } from './validation.service';

describe('ValidationService', () => {
  let service: ValidationService;
  let fb: FormBuilder;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ValidationService);
    fb = TestBed.inject(FormBuilder);
  });

  it('should return error message', () => {
    const control = fb.control('', Validators.required);
    control.markAsTouched();
    
    const message = service.getErrorMessage(control, 'nome');
    expect(message).toBe('Nome é obrigatório');
  });
});
```

## Best Practices

1. **Always use the ValidationService** for consistent error handling
2. **Combine validators** for comprehensive validation
3. **Use field-specific error messages** for better UX
4. **Test your validators** with unit tests
5. **Mark forms as touched** before showing validation errors
6. **Use the FieldErrorComponent** for consistent error display
7. **Apply the ValidationErrorDirective** for visual feedback

## Error Message Customization

The validation system supports multiple levels of error message customization:

1. **Validator-level messages**: Built into each validator
2. **Field-specific messages**: Defined in ValidationService
3. **Custom messages**: Added at runtime
4. **Default fallbacks**: Generic messages for unknown errors

This hierarchical approach ensures that users always see meaningful error messages while allowing for easy customization when needed.