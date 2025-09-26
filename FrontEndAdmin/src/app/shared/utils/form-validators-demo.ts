/**
 * Demo file to test the custom validators
 * This file demonstrates how to use the custom validators for Culturas and Safras
 */

import { FormControl, FormGroup } from '@angular/forms';
import { 
  culturaNomeValidator, 
  safraDateValidator, 
  safraPlantioNomeValidator, 
  safraDescricaoValidator,
  dateRangeValidator,
  maxLengthValidator,
  getFormValidationErrorMessage,
  VALIDATION_MESSAGES
} from './field-validators.util';

/**
 * Demo function to test cultura validators
 */
export function testCulturaValidators() {
  console.log('=== Testing Cultura Validators ===');
  
  // Test cultura nome validator
  const nomeControl = new FormControl('');
  const nomeValidator = culturaNomeValidator();
  
  // Test empty name
  console.log('Empty name:', nomeValidator(nomeControl));
  
  // Test valid name
  nomeControl.setValue('Soja');
  console.log('Valid name (Soja):', nomeValidator(nomeControl));
  
  // Test name too long
  nomeControl.setValue('A'.repeat(257));
  console.log('Name too long:', nomeValidator(nomeControl));
  
  // Test invalid characters
  nomeControl.setValue('Soja@#$');
  console.log('Invalid characters:', nomeValidator(nomeControl));
  
  // Test max length validator for description
  const descricaoControl = new FormControl('A'.repeat(501));
  const maxLengthVal = maxLengthValidator(500, 'Descrição');
  console.log('Description too long:', maxLengthVal(descricaoControl));
}

/**
 * Demo function to test safra validators
 */
export function testSafraValidators() {
  console.log('=== Testing Safra Validators ===');
  
  // Test safra date validator
  const dateValidator = safraDateValidator();
  const dateControl = new FormControl('');
  
  // Test empty date
  console.log('Empty date:', dateValidator(dateControl));
  
  // Test valid date
  dateControl.setValue('2024-06-15');
  console.log('Valid date:', dateValidator(dateControl));
  
  // Test date too old
  dateControl.setValue('1899-12-31');
  console.log('Date too old:', dateValidator(dateControl));
  
  // Test date too far in future
  const futureYear = new Date().getFullYear() + 15;
  dateControl.setValue(`${futureYear}-01-01`);
  console.log('Date too far in future:', dateValidator(dateControl));
  
  // Test plantio nome validator
  const plantioNomeValidator = safraPlantioNomeValidator();
  const plantioNomeControl = new FormControl('');
  
  console.log('Empty plantio nome:', plantioNomeValidator(plantioNomeControl));
  
  plantioNomeControl.setValue('Plantio Principal');
  console.log('Valid plantio nome:', plantioNomeValidator(plantioNomeControl));
  
  plantioNomeControl.setValue('A'.repeat(257));
  console.log('Plantio nome too long:', plantioNomeValidator(plantioNomeControl));
  
  // Test description validator
  const descricaoValidator = safraDescricaoValidator();
  const descricaoControl = new FormControl('');
  
  console.log('Empty description:', descricaoValidator(descricaoControl));
  
  descricaoControl.setValue('Safra principal de soja');
  console.log('Valid description:', descricaoValidator(descricaoControl));
  
  descricaoControl.setValue('A'.repeat(65));
  console.log('Description too long:', descricaoValidator(descricaoControl));
}

/**
 * Demo function to test date range validator
 */
export function testDateRangeValidator() {
  console.log('=== Testing Date Range Validator ===');
  
  const form = new FormGroup({
    startDate: new FormControl('2024-01-01'),
    endDate: new FormControl('2024-12-31')
  });
  
  const validator = dateRangeValidator('startDate', 'endDate');
  
  // Test valid range
  console.log('Valid date range:', validator(form.get('endDate')!));
  
  // Test invalid range
  form.patchValue({
    startDate: '2024-12-31',
    endDate: '2024-01-01'
  });
  console.log('Invalid date range:', validator(form.get('endDate')!));
}

/**
 * Demo function to test error message generation
 */
export function testErrorMessages() {
  console.log('=== Testing Error Messages ===');
  
  // Test cultura error messages
  const culturaControl = new FormControl('');
  culturaControl.setErrors({ required: { message: 'Nome da cultura é obrigatório' } });
  
  console.log('Cultura error message:', getFormValidationErrorMessage(culturaControl, 'cultura', 'nome'));
  
  // Test safra error messages
  const safraControl = new FormControl('');
  safraControl.setErrors({ required: true });
  
  console.log('Safra error message:', getFormValidationErrorMessage(safraControl, 'safra', 'plantioNome'));
  
  // Test validation messages constant
  console.log('Validation messages:', VALIDATION_MESSAGES);
}

/**
 * Run all validator tests
 */
export function runAllValidatorTests() {
  testCulturaValidators();
  testSafraValidators();
  testDateRangeValidator();
  testErrorMessages();
}