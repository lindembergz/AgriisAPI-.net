# Custom Form Validators Guide

This guide explains how to use the custom validators implemented for the Culturas and Safras modules.

## Available Validators

### 1. Required Field Validator
```typescript
import { requiredValidator } from './field-validators.util';

// Usage in form control
nome: this.fb.control('', { validators: [requiredValidator('Nome')] })
```

### 2. Max Length Validator
```typescript
import { maxLengthValidator } from './field-validators.util';

// Usage in form control
descricao: this.fb.control('', { validators: [maxLengthValidator(500, 'Descrição')] })
```

### 3. Cultura Nome Validator
Validates cultura names with specific business rules:
- Required field
- Maximum 256 characters
- Only allows letters, numbers, spaces, and hyphens

```typescript
import { culturaNomeValidator } from './field-validators.util';

// Usage in form control
nome: this.fb.control('', { validators: [culturaNomeValidator()] })
```

### 4. Safra Date Validator
Validates dates according to agricultural season requirements:
- Not before year 1900
- Not more than 10 years in the future

```typescript
import { safraDateValidator } from './field-validators.util';

// Usage in form control
plantioInicial: this.fb.control(new Date(), { validators: [safraDateValidator()] })
```

### 5. Date Range Validator
Validates that end date is after start date:

```typescript
import { dateRangeValidator } from './field-validators.util';

// Usage in form control (cross-field validation)
plantioFinal: this.fb.control(new Date(), { 
  validators: [dateRangeValidator('plantioInicial', 'plantioFinal')] 
})
```

### 6. Safra Plantio Nome Validator
Validates plantio names:
- Required field
- Maximum 256 characters

```typescript
import { safraPlantioNomeValidator } from './field-validators.util';

// Usage in form control
plantioNome: this.fb.control('', { validators: [safraPlantioNomeValidator()] })
```

### 7. Safra Descrição Validator
Validates safra descriptions:
- Required field
- Maximum 64 characters

```typescript
import { safraDescricaoValidator } from './field-validators.util';

// Usage in form control
descricao: this.fb.control('', { validators: [safraDescricaoValidator()] })
```

## Error Message Handling

### Get Error Messages
```typescript
import { getFormValidationErrorMessage } from './field-validators.util';

// In component method
getFieldErrorMessage(fieldName: string): string {
  const control = this.form.get(fieldName);
  if (!control) return '';
  return getFormValidationErrorMessage(control, 'cultura', fieldName); // or 'safra'
}
```

### Predefined Messages
The validators come with predefined Portuguese error messages:

```typescript
import { VALIDATION_MESSAGES } from './field-validators.util';

// Access messages
console.log(VALIDATION_MESSAGES.cultura.nome.required); // "Nome da cultura é obrigatório"
console.log(VALIDATION_MESSAGES.safra.plantioInicial.required); // "Data inicial do plantio é obrigatória"
```

## Complete Form Example

### Cultura Form
```typescript
import { 
  culturaNomeValidator, 
  maxLengthValidator, 
  getFormValidationErrorMessage 
} from '../../../../shared/utils/field-validators.util';

// Form initialization
private initializeForm(): void {
  this.culturaForm = this.fb.group({
    nome: this.fb.control('', { 
      nonNullable: true, 
      validators: [culturaNomeValidator()] 
    }),
    descricao: this.fb.control<string | null>(null, {
      validators: [maxLengthValidator(500, 'Descrição')]
    }),
    ativo: this.fb.control(true, { nonNullable: true })
  });
}

// Error message method
getFieldErrorMessage(fieldName: string): string {
  const control = this.culturaForm.get(fieldName);
  if (!control) return '';
  return getFormValidationErrorMessage(control, 'cultura', fieldName);
}
```

### Safra Form
```typescript
import { 
  safraDateValidator, 
  dateRangeValidator, 
  safraPlantioNomeValidator, 
  safraDescricaoValidator,
  getFormValidationErrorMessage 
} from '../../../../shared/utils/field-validators.util';

// Form initialization
private initializeForm(): void {
  this.safraForm = this.fb.group({
    plantioInicial: this.fb.control<Date>(new Date(), { 
      nonNullable: true, 
      validators: [safraDateValidator()] 
    }),
    plantioFinal: this.fb.control<Date>(new Date(), { 
      nonNullable: true, 
      validators: [safraDateValidator(), dateRangeValidator('plantioInicial', 'plantioFinal')] 
    }),
    plantioNome: this.fb.control('', { 
      nonNullable: true, 
      validators: [safraPlantioNomeValidator()] 
    }),
    descricao: this.fb.control('', { 
      nonNullable: true, 
      validators: [safraDescricaoValidator()] 
    })
  });
}

// Error message method
getFieldErrorMessage(fieldName: string): string {
  const control = this.safraForm.get(fieldName);
  if (!control) return '';
  return getFormValidationErrorMessage(control, 'safra', fieldName);
}
```

## Template Usage

### Display Error Messages
```html
<!-- Field with validation -->
<input
  id="nome"
  type="text"
  pInputText
  formControlName="nome"
  [class.ng-invalid]="hasFieldError('nome')"
/>
<small 
  *ngIf="hasFieldError('nome')" 
  class="error-message"
>
  {{ getFieldErrorMessage('nome') }}
</small>
```

### Check Field Errors
```typescript
// In component
hasFieldError(fieldName: string): boolean {
  const field = this.form.get(fieldName);
  return !!(field && field.invalid && field.touched);
}
```

## Requirements Covered

This implementation covers the following requirements:

- **2.4, 2.5**: Cultura name validation (required, character limits)
- **4.4, 4.5, 4.6, 4.7**: Safra date validation (year limits, date range)
- **4.12, 4.13**: Safra field validation (plantio nome, descrição limits)

All validators provide Portuguese error messages and follow Angular reactive forms best practices.