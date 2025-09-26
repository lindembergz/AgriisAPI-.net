import { FormControl, FormGroup } from '@angular/forms';
import { 
  phoneValidator, 
  emailValidator, 
  cepValidator, 
  nameValidator, 
  passwordValidator, 
  strongPasswordValidator,
  numericValidator,
  areaValidator,
  yearValidator,
  getValidationErrorMessage,
  hasFieldError,
  isFieldInvalid,
  requiredValidator,
  maxLengthValidator,
  dateRangeValidator,
  safraDateValidator,
  culturaNomeValidator,
  safraPlantioNomeValidator,
  safraDescricaoValidator,
  getFormValidationErrorMessage,
  VALIDATION_MESSAGES
} from './field-validators.util';

describe('FieldValidators', () => {

  describe('phoneValidator', () => {
    const validator = phoneValidator();

    it('should return null for empty values', () => {
      expect(validator(new FormControl(''))).toBeNull();
      expect(validator(new FormControl(null))).toBeNull();
      expect(validator(new FormControl(undefined))).toBeNull();
    });

    it('should validate valid Brazilian phone numbers', () => {
      // Landline (10 digits)
      expect(validator(new FormControl('1133334444'))).toBeNull();
      expect(validator(new FormControl('(11) 3333-4444'))).toBeNull();
      
      // Mobile (11 digits)
      expect(validator(new FormControl('11999887766'))).toBeNull();
      expect(validator(new FormControl('(11) 99988-7766'))).toBeNull();
    });

    it('should reject invalid phone numbers', () => {
      // Too short
      expect(validator(new FormControl('123456789'))).toEqual({
        phoneInvalid: { message: 'Telefone deve conter 10 ou 11 dígitos' }
      });

      // Too long
      expect(validator(new FormControl('123456789012'))).toEqual({
        phoneInvalid: { message: 'Telefone deve conter 10 ou 11 dígitos' }
      });

      // Invalid area code
      expect(validator(new FormControl('0933334444'))).toEqual({
        phoneInvalid: { message: 'Código de área inválido' }
      });

      // Mobile without 9
      expect(validator(new FormControl('11833334444'))).toEqual({
        phoneInvalid: { message: 'Número de celular deve começar com 9 após o DDD' }
      });
    });
  });

  describe('emailValidator', () => {
    const validator = emailValidator();

    it('should return null for empty values', () => {
      expect(validator(new FormControl(''))).toBeNull();
      expect(validator(new FormControl(null))).toBeNull();
      expect(validator(new FormControl(undefined))).toBeNull();
    });

    it('should validate valid email addresses', () => {
      expect(validator(new FormControl('test@example.com'))).toBeNull();
      expect(validator(new FormControl('user.name@domain.com.br'))).toBeNull();
      expect(validator(new FormControl('test+tag@example.org'))).toBeNull();
    });

    it('should reject invalid email addresses', () => {
      // Missing @
      expect(validator(new FormControl('testexample.com'))).toEqual({
        emailInvalid: { message: 'E-mail inválido' }
      });

      // Missing domain
      expect(validator(new FormControl('test@'))).toEqual({
        emailInvalid: { message: 'E-mail inválido' }
      });

      // Invalid format
      expect(validator(new FormControl('test@.com'))).toEqual({
        emailInvalid: { message: 'E-mail inválido' }
      });

      // Starts with dot
      expect(validator(new FormControl('.test@example.com'))).toEqual({
        emailInvalid: { message: 'E-mail não pode começar ou terminar com ponto' }
      });

      // Consecutive dots
      expect(validator(new FormControl('te..st@example.com'))).toEqual({
        emailInvalid: { message: 'E-mail não pode conter pontos consecutivos' }
      });
    });
  });

  describe('cepValidator', () => {
    const validator = cepValidator();

    it('should return null for empty values', () => {
      expect(validator(new FormControl(''))).toBeNull();
      expect(validator(new FormControl(null))).toBeNull();
      expect(validator(new FormControl(undefined))).toBeNull();
    });

    it('should validate valid CEP', () => {
      expect(validator(new FormControl('12345678'))).toBeNull();
      expect(validator(new FormControl('12345-678'))).toBeNull();
    });

    it('should reject invalid CEP', () => {
      // Wrong length
      expect(validator(new FormControl('1234567'))).toEqual({
        cepInvalid: { message: 'CEP deve conter 8 dígitos' }
      });

      // All same digits
      expect(validator(new FormControl('11111111'))).toEqual({
        cepInvalid: { message: 'CEP inválido' }
      });
    });
  });

  describe('nameValidator', () => {
    it('should return null for empty values', () => {
      const validator = nameValidator();
      expect(validator(new FormControl(''))).toBeNull();
      expect(validator(new FormControl(null))).toBeNull();
      expect(validator(new FormControl(undefined))).toBeNull();
    });

    it('should validate valid names', () => {
      const validator = nameValidator(2);
      expect(validator(new FormControl('João Silva'))).toBeNull();
      expect(validator(new FormControl('Maria José'))).toBeNull();
      expect(validator(new FormControl("O'Connor"))).toBeNull();
      expect(validator(new FormControl('José-Maria'))).toBeNull();
    });

    it('should reject invalid names', () => {
      const validator = nameValidator(2);
      
      // Too short
      expect(validator(new FormControl('A'))).toEqual({
        nameInvalid: { message: 'Nome deve ter pelo menos 2 caracteres' }
      });

      // Invalid characters
      expect(validator(new FormControl('João123'))).toEqual({
        nameInvalid: { message: 'Nome contém caracteres inválidos' }
      });

      // Consecutive spaces
      expect(validator(new FormControl('João  Silva'))).toEqual({
        nameInvalid: { message: 'Nome não pode conter espaços consecutivos' }
      });
    });
  });

  describe('passwordValidator', () => {
    it('should return null for empty values', () => {
      const validator = passwordValidator();
      expect(validator(new FormControl(''))).toBeNull();
      expect(validator(new FormControl(null))).toBeNull();
      expect(validator(new FormControl(undefined))).toBeNull();
    });

    it('should validate valid passwords', () => {
      const validator = passwordValidator(6);
      expect(validator(new FormControl('abc123'))).toBeNull();
      expect(validator(new FormControl('password1'))).toBeNull();
    });

    it('should reject invalid passwords', () => {
      const validator = passwordValidator(6);
      
      // Too short
      expect(validator(new FormControl('12345'))).toEqual({
        passwordInvalid: { minLength: 'Senha deve ter pelo menos 6 caracteres' }
      });

      // No letter
      expect(validator(new FormControl('123456'))).toEqual({
        passwordInvalid: { noLetter: 'Senha deve conter pelo menos uma letra' }
      });

      // No number
      expect(validator(new FormControl('abcdef'))).toEqual({
        passwordInvalid: { noNumber: 'Senha deve conter pelo menos um número' }
      });
    });
  });

  describe('strongPasswordValidator', () => {
    const validator = strongPasswordValidator();

    it('should return null for empty values', () => {
      expect(validator(new FormControl(''))).toBeNull();
      expect(validator(new FormControl(null))).toBeNull();
      expect(validator(new FormControl(undefined))).toBeNull();
    });

    it('should validate strong passwords', () => {
      expect(validator(new FormControl('Abc123!@'))).toBeNull();
      expect(validator(new FormControl('MyP@ssw0rd'))).toBeNull();
    });

    it('should reject weak passwords', () => {
      // Too short
      expect(validator(new FormControl('Abc1!'))).toEqual({
        strongPasswordInvalid: { minLength: 'Senha deve ter pelo menos 8 caracteres' }
      });

      // No lowercase
      expect(validator(new FormControl('ABC123!@'))).toEqual({
        strongPasswordInvalid: { noLowercase: 'Senha deve conter pelo menos uma letra minúscula' }
      });

      // No uppercase
      expect(validator(new FormControl('abc123!@'))).toEqual({
        strongPasswordInvalid: { noUppercase: 'Senha deve conter pelo menos uma letra maiúscula' }
      });

      // No special character
      expect(validator(new FormControl('Abc12345'))).toEqual({
        strongPasswordInvalid: { noSpecialChar: 'Senha deve conter pelo menos um caractere especial' }
      });
    });
  });

  describe('numericValidator', () => {
    it('should return null for empty values', () => {
      const validator = numericValidator();
      expect(validator(new FormControl(''))).toBeNull();
      expect(validator(new FormControl(null))).toBeNull();
      expect(validator(new FormControl(undefined))).toBeNull();
    });

    it('should validate numeric values', () => {
      const validator = numericValidator(0, 100);
      expect(validator(new FormControl(50))).toBeNull();
      expect(validator(new FormControl('50'))).toBeNull();
      expect(validator(new FormControl(0))).toBeNull();
      expect(validator(new FormControl(100))).toBeNull();
    });

    it('should reject invalid values', () => {
      const validator = numericValidator(0, 100);
      
      // Not numeric
      expect(validator(new FormControl('abc'))).toEqual({
        numericInvalid: { message: 'Valor deve ser numérico' }
      });

      // Below minimum
      expect(validator(new FormControl(-1))).toEqual({
        numericInvalid: { message: 'Valor deve ser maior ou igual a 0' }
      });

      // Above maximum
      expect(validator(new FormControl(101))).toEqual({
        numericInvalid: { message: 'Valor deve ser menor ou igual a 100' }
      });
    });
  });

  describe('areaValidator', () => {
    const validator = areaValidator();

    it('should return null for empty values', () => {
      expect(validator(new FormControl(''))).toBeNull();
      expect(validator(new FormControl(null))).toBeNull();
      expect(validator(new FormControl(undefined))).toBeNull();
    });

    it('should validate valid areas', () => {
      expect(validator(new FormControl(10.5))).toBeNull();
      expect(validator(new FormControl('100'))).toBeNull();
      expect(validator(new FormControl(99999))).toBeNull();
    });

    it('should reject invalid areas', () => {
      // Not numeric
      expect(validator(new FormControl('abc'))).toEqual({
        areaInvalid: { message: 'Área deve ser numérica' }
      });

      // Zero or negative
      expect(validator(new FormControl(0))).toEqual({
        areaInvalid: { message: 'Área deve ser maior que zero' }
      });

      expect(validator(new FormControl(-1))).toEqual({
        areaInvalid: { message: 'Área deve ser maior que zero' }
      });

      // Too large
      expect(validator(new FormControl(100001))).toEqual({
        areaInvalid: { message: 'Área muito grande (máximo 100.000 hectares)' }
      });
    });
  });

  describe('yearValidator', () => {
    it('should return null for empty values', () => {
      const validator = yearValidator();
      expect(validator(new FormControl(''))).toBeNull();
      expect(validator(new FormControl(null))).toBeNull();
      expect(validator(new FormControl(undefined))).toBeNull();
    });

    it('should validate valid years', () => {
      const currentYear = new Date().getFullYear();
      const validator = yearValidator(2000, currentYear + 5);
      
      expect(validator(new FormControl(2020))).toBeNull();
      expect(validator(new FormControl(currentYear))).toBeNull();
      expect(validator(new FormControl('2021'))).toBeNull();
    });

    it('should reject invalid years', () => {
      const currentYear = new Date().getFullYear();
      const validator = yearValidator(2000, currentYear + 5);
      
      // Not numeric
      expect(validator(new FormControl('abc'))).toEqual({
        yearInvalid: { message: 'Ano deve ser numérico' }
      });

      // Too old
      expect(validator(new FormControl(1999))).toEqual({
        yearInvalid: { message: 'Ano deve ser maior ou igual a 2000' }
      });

      // Too future
      expect(validator(new FormControl(currentYear + 10))).toEqual({
        yearInvalid: { message: `Ano deve ser menor ou igual a ${currentYear + 5}` }
      });
    });
  });

  describe('utility functions', () => {
    describe('getValidationErrorMessage', () => {
      it('should return custom error messages', () => {
        const control = new FormControl('');
        control.setErrors({ phoneInvalid: { message: 'Custom phone error' } });
        
        expect(getValidationErrorMessage(control)).toBe('Custom phone error');
      });

      it('should return default error messages', () => {
        const control = new FormControl('');
        control.setErrors({ required: true });
        
        expect(getValidationErrorMessage(control)).toBe('Campo obrigatório');
      });

      it('should return empty string for no errors', () => {
        const control = new FormControl('valid');
        
        expect(getValidationErrorMessage(control)).toBe('');
      });
    });

    describe('hasFieldError', () => {
      it('should return true when field has specific error and is touched', () => {
        const control = new FormControl('');
        control.setErrors({ required: true });
        control.markAsTouched();
        
        expect(hasFieldError(control, 'required')).toBe(true);
      });

      it('should return false when field has no error', () => {
        const control = new FormControl('valid');
        control.markAsTouched();
        
        expect(hasFieldError(control, 'required')).toBe(false);
      });

      it('should return false when field is not touched', () => {
        const control = new FormControl('');
        control.setErrors({ required: true });
        
        expect(hasFieldError(control, 'required')).toBe(false);
      });
    });

    describe('isFieldInvalid', () => {
      it('should return true when field is invalid and touched', () => {
        const control = new FormControl('');
        control.setErrors({ required: true });
        control.markAsTouched();
        
        expect(isFieldInvalid(control)).toBe(true);
      });

      it('should return false when field is valid', () => {
        const control = new FormControl('valid');
        control.markAsTouched();
        
        expect(isFieldInvalid(control)).toBe(false);
      });

      it('should return false when field is not touched', () => {
        const control = new FormControl('');
        control.setErrors({ required: true });
        
        expect(isFieldInvalid(control)).toBe(false);
      });
    });
  });

  describe('requiredValidator', () => {
    it('should return null for valid values', () => {
      const validator = requiredValidator('Nome');
      expect(validator(new FormControl('Valid value'))).toBeNull();
      expect(validator(new FormControl('  Valid  '))).toBeNull();
    });

    it('should reject empty values', () => {
      const validator = requiredValidator('Nome');
      
      expect(validator(new FormControl(''))).toEqual({
        required: { message: 'Nome é obrigatório' }
      });

      expect(validator(new FormControl('   '))).toEqual({
        required: { message: 'Nome é obrigatório' }
      });

      expect(validator(new FormControl(null))).toEqual({
        required: { message: 'Nome é obrigatório' }
      });
    });
  });

  describe('maxLengthValidator', () => {
    it('should return null for valid lengths', () => {
      const validator = maxLengthValidator(10, 'Nome');
      expect(validator(new FormControl('Short'))).toBeNull();
      expect(validator(new FormControl('1234567890'))).toBeNull();
      expect(validator(new FormControl(''))).toBeNull();
    });

    it('should reject values that are too long', () => {
      const validator = maxLengthValidator(5, 'Nome');
      
      expect(validator(new FormControl('TooLong'))).toEqual({
        maxlength: { 
          message: 'Nome não pode ter mais de 5 caracteres',
          requiredLength: 5,
          actualLength: 7
        }
      });
    });
  });

  describe('dateRangeValidator', () => {
    it('should return null for valid date ranges', () => {
      const form = new FormGroup({
        startDate: new FormControl('2024-01-01'),
        endDate: new FormControl('2024-12-31')
      });
      
      const validator = dateRangeValidator('startDate', 'endDate');
      expect(validator(form.get('endDate')!)).toBeNull();
    });

    it('should reject invalid date ranges', () => {
      const form = new FormGroup({
        startDate: new FormControl('2024-12-31'),
        endDate: new FormControl('2024-01-01')
      });
      
      const validator = dateRangeValidator('startDate', 'endDate');
      expect(validator(form.get('endDate')!)).toEqual({
        dateRange: { message: 'Data final deve ser posterior à data inicial' }
      });
    });

    it('should return null when dates are missing', () => {
      const form = new FormGroup({
        startDate: new FormControl(''),
        endDate: new FormControl('')
      });
      
      const validator = dateRangeValidator('startDate', 'endDate');
      expect(validator(form.get('endDate')!)).toBeNull();
    });
  });

  describe('safraDateValidator', () => {
    const validator = safraDateValidator();

    it('should return null for valid dates', () => {
      expect(validator(new FormControl('2024-06-15'))).toBeNull();
      expect(validator(new FormControl(new Date('2023-01-01')))).toBeNull();
      expect(validator(new FormControl(''))).toBeNull();
    });

    it('should reject dates before 1900', () => {
      expect(validator(new FormControl('1899-12-31'))).toEqual({
        safraDate: { message: 'Data não pode ser anterior a 1900' }
      });
    });

    it('should reject dates too far in the future', () => {
      const futureYear = new Date().getFullYear() + 15;
      expect(validator(new FormControl(`${futureYear}-01-01`))).toEqual({
        safraDate: { message: `Data não pode ser superior a ${new Date().getFullYear() + 10}` }
      });
    });

    it('should reject invalid dates', () => {
      expect(validator(new FormControl('invalid-date'))).toEqual({
        safraDate: { message: 'Data inválida' }
      });
    });
  });

  describe('culturaNomeValidator', () => {
    const validator = culturaNomeValidator();

    it('should return null for valid cultura names', () => {
      expect(validator(new FormControl('Soja'))).toBeNull();
      expect(validator(new FormControl('Milho Safrinha'))).toBeNull();
      expect(validator(new FormControl('Algodão-BT'))).toBeNull();
    });

    it('should reject empty values', () => {
      expect(validator(new FormControl(''))).toEqual({
        required: { message: 'Nome da cultura é obrigatório' }
      });

      expect(validator(new FormControl('   '))).toEqual({
        required: { message: 'Nome da cultura é obrigatório' }
      });

      expect(validator(new FormControl(null))).toEqual({
        required: { message: 'Nome da cultura é obrigatório' }
      });
    });

    it('should reject names that are too long', () => {
      const longName = 'A'.repeat(257);
      expect(validator(new FormControl(longName))).toEqual({
        maxlength: { 
          message: 'Nome da cultura não pode ter mais de 256 caracteres',
          requiredLength: 256,
          actualLength: 257
        }
      });
    });

    it('should reject names with invalid characters', () => {
      expect(validator(new FormControl('Soja@#$'))).toEqual({
        pattern: { message: 'Nome da cultura contém caracteres inválidos' }
      });
    });
  });

  describe('safraPlantioNomeValidator', () => {
    const validator = safraPlantioNomeValidator();

    it('should return null for valid plantio names', () => {
      expect(validator(new FormControl('Plantio Principal'))).toBeNull();
      expect(validator(new FormControl('Safrinha 2024'))).toBeNull();
    });

    it('should reject empty values', () => {
      expect(validator(new FormControl(''))).toEqual({
        required: { message: 'Nome do plantio é obrigatório' }
      });

      expect(validator(new FormControl(null))).toEqual({
        required: { message: 'Nome do plantio é obrigatório' }
      });
    });

    it('should reject names that are too long', () => {
      const longName = 'A'.repeat(257);
      expect(validator(new FormControl(longName))).toEqual({
        maxlength: { 
          message: 'Nome do plantio não pode ter mais de 256 caracteres',
          requiredLength: 256,
          actualLength: 257
        }
      });
    });
  });

  describe('safraDescricaoValidator', () => {
    const validator = safraDescricaoValidator();

    it('should return null for valid descriptions', () => {
      expect(validator(new FormControl('Safra principal de soja'))).toBeNull();
      expect(validator(new FormControl('Plantio de milho safrinha'))).toBeNull();
    });

    it('should reject empty values', () => {
      expect(validator(new FormControl(''))).toEqual({
        required: { message: 'Descrição é obrigatória' }
      });

      expect(validator(new FormControl(null))).toEqual({
        required: { message: 'Descrição é obrigatória' }
      });
    });

    it('should reject descriptions that are too long', () => {
      const longDescription = 'A'.repeat(65);
      expect(validator(new FormControl(longDescription))).toEqual({
        maxlength: { 
          message: 'Descrição não pode ter mais de 64 caracteres',
          requiredLength: 64,
          actualLength: 65
        }
      });
    });
  });

  describe('getFormValidationErrorMessage', () => {
    it('should return custom error messages', () => {
      const control = new FormControl('');
      control.setErrors({ required: { message: 'Custom required message' } });
      
      expect(getFormValidationErrorMessage(control, 'cultura', 'nome')).toBe('Custom required message');
    });

    it('should return specific cultura validation messages', () => {
      const control = new FormControl('');
      control.setErrors({ required: true });
      
      expect(getFormValidationErrorMessage(control, 'cultura', 'nome')).toBe('Nome da cultura é obrigatório');
    });

    it('should return specific safra validation messages', () => {
      const control = new FormControl('');
      control.setErrors({ required: true });
      
      expect(getFormValidationErrorMessage(control, 'safra', 'plantioNome')).toBe('Nome do plantio é obrigatório');
    });

    it('should fallback to generic messages', () => {
      const control = new FormControl('');
      control.setErrors({ unknownError: true });
      
      expect(getFormValidationErrorMessage(control, 'cultura', 'nome')).toBe('Campo inválido');
    });

    it('should return empty string for no errors', () => {
      const control = new FormControl('valid');
      
      expect(getFormValidationErrorMessage(control, 'cultura', 'nome')).toBe('');
    });
  });

  describe('VALIDATION_MESSAGES', () => {
    it('should contain cultura validation messages', () => {
      expect(VALIDATION_MESSAGES.cultura.nome.required).toBe('Nome da cultura é obrigatório');
      expect(VALIDATION_MESSAGES.cultura.nome.maxlength).toBe('Nome da cultura não pode ter mais de 256 caracteres');
    });

    it('should contain safra validation messages', () => {
      expect(VALIDATION_MESSAGES.safra.plantioInicial.required).toBe('Data inicial do plantio é obrigatória');
      expect(VALIDATION_MESSAGES.safra.plantioFinal.dateRange).toBe('Data final deve ser posterior à data inicial');
      expect(VALIDATION_MESSAGES.safra.descricao.maxlength).toBe('Descrição não pode ter mais de 64 caracteres');
    });
  });
});