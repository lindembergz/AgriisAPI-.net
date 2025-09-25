import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';

// PrimeNG imports
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { InputMaskModule } from 'primeng/inputmask';
import { SelectModule } from 'primeng/select';
import { ButtonModule } from 'primeng/button';
import { InputNumberModule } from 'primeng/inputnumber';

// Validation imports
import { ValidationService } from '../services/validation.service';
import { 
  cpfValidator, 
  cnpjValidator, 
  cpfCnpjValidator, 
  conditionalCpfCnpjValidator,
  getCpfCnpjMask 
} from '../utils/document-validators.util';
import { 
  phoneValidator, 
  emailValidator, 
  nameValidator, 
  passwordValidator, 
  cepValidator,
  areaValidator,
  yearValidator
} from '../utils/field-validators.util';

// Shared components
import { FieldErrorComponent } from './field-error.component';
import { ValidationErrorDirective } from '../directives/validation-error.directive';

/**
 * Validation Example Component
 * Demonstrates comprehensive form validation features
 * Can be used as a reference for implementing validation in other forms
 */
@Component({
  selector: 'app-validation-example',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    CardModule,
    InputTextModule,
    InputMaskModule,
    SelectModule,
    ButtonModule,
    InputNumberModule,
    FieldErrorComponent,
    ValidationErrorDirective
  ],
  template: `
    <div class="validation-example">
      <p-card header="Exemplo de Validação Abrangente">
        <form [formGroup]="exampleForm" (ngSubmit)="onSubmit()">
          
          <!-- Tipo Cliente -->
          <div class="field">
            <label for="tipoCliente">Tipo de Cliente *</label>
            <p-select
              id="tipoCliente"
              formControlName="tipoCliente"
              [options]="tipoClienteOptions"
              placeholder="Selecione o tipo"
              [appValidationError]="getControl('tipoCliente')"
              class="w-full"
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
              type="text"
              pInputText
              formControlName="nome"
              placeholder="Digite o nome completo"
              [appValidationError]="getControl('nome')"
              class="w-full"
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
              placeholder="{{ getCpfCnpjPlaceholder() }}"
              [appValidationError]="getControl('cpfCnpj')"
              class="w-full"
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
              type="email"
              pInputText
              formControlName="email"
              placeholder="exemplo@email.com"
              [appValidationError]="getControl('email')"
              class="w-full"
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
              placeholder="(11) 99999-9999"
              [appValidationError]="getControl('telefone')"
              class="w-full"
            />
            <app-field-error 
              [control]="getControl('telefone')" 
              fieldName="telefone"
            />
          </div>

          <!-- CEP -->
          <div class="field">
            <label for="cep">CEP *</label>
            <p-inputMask
              id="cep"
              formControlName="cep"
              mask="99999-999"
              placeholder="12345-678"
              [appValidationError]="getControl('cep')"
              class="w-full"
            />
            <app-field-error 
              [control]="getControl('cep')" 
              fieldName="cep"
            />
          </div>

          <!-- Área -->
          <div class="field">
            <label for="area">Área (hectares) *</label>
            <p-inputNumber
              id="area"
              formControlName="area"
              mode="decimal"
              [minFractionDigits]="2"
              [maxFractionDigits]="2"
              suffix=" ha"
              placeholder="0,00 ha"
              [appValidationError]="getControl('area')"
              class="w-full"
            />
            <app-field-error 
              [control]="getControl('area')" 
              fieldName="area"
            />
          </div>

          <!-- Ano Safra -->
          <div class="field">
            <label for="anoSafra">Ano da Safra *</label>
            <p-inputNumber
              id="anoSafra"
              formControlName="anoSafra"
              [useGrouping]="false"
              placeholder="2024"
              [appValidationError]="getControl('anoSafra')"
              class="w-full"
            />
            <app-field-error 
              [control]="getControl('anoSafra')" 
              fieldName="anoSafra"
            />
          </div>

          <!-- Senha -->
          <div class="field">
            <label for="senha">Senha *</label>
            <input
              id="senha"
              type="password"
              pInputText
              formControlName="senha"
              placeholder="Digite uma senha segura"
              [appValidationError]="getControl('senha')"
              class="w-full"
            />
            <app-field-error 
              [control]="getControl('senha')" 
              fieldName="senha"
            />
          </div>

          <!-- Buttons -->
          <div class="flex gap-2 mt-4">
            <p-button
              type="submit"
              label="Validar Formulário"
              icon="pi pi-check"
              [disabled]="exampleForm.invalid"
            />
            <p-button
              type="button"
              label="Limpar"
              icon="pi pi-times"
              severity="secondary"
              (onClick)="onClear()"
            />
            <p-button
              type="button"
              label="Mostrar Erros"
              icon="pi pi-exclamation-triangle"
              severity="warn"
              (onClick)="showValidationSummary()"
            />
          </div>

          <!-- Validation Summary -->
          <div *ngIf="validationSummary" class="mt-4">
            <p-card header="Resumo de Validação">
              <div class="validation-summary">
                <p><strong>Status:</strong> {{ validationSummary.isValid ? 'Válido' : 'Inválido' }}</p>
                <div *ngIf="!validationSummary.isValid && validationSummary.errors.length > 0">
                  <p><strong>Erros encontrados:</strong></p>
                  <ul>
                    <li *ngFor="let error of validationSummary.errors" class="text-red-500">
                      {{ error }}
                    </li>
                  </ul>
                </div>
              </div>
            </p-card>
          </div>

        </form>
      </p-card>
    </div>
  `,
  styles: [`
    .validation-example {
      max-width: 600px;
      margin: 0 auto;
      padding: 1rem;
    }

    .field {
      margin-bottom: 1rem;
    }

    .field label {
      display: block;
      margin-bottom: 0.25rem;
      font-weight: 500;
      color: var(--text-color);
    }

    .validation-summary ul {
      margin: 0.5rem 0;
      padding-left: 1.5rem;
    }

    .validation-summary li {
      margin-bottom: 0.25rem;
    }
  `]
})
export class ValidationExampleComponent {
  private fb = inject(FormBuilder);
  private validationService = inject(ValidationService);

  validationSummary: { isValid: boolean; errors: string[] } | null = null;

  tipoClienteOptions = [
    { label: 'Pessoa Física', value: 'PF' },
    { label: 'Pessoa Jurídica', value: 'PJ' }
  ];

  exampleForm = this.fb.group({
    tipoCliente: ['PF', [Validators.required]],
    nome: ['', [Validators.required, nameValidator(2)]],
    cpfCnpj: ['', [Validators.required, conditionalCpfCnpjValidator('tipoCliente')]],
    email: ['', [Validators.required, emailValidator()]],
    telefone: ['', [phoneValidator()]],
    cep: ['', [Validators.required, cepValidator()]],
    area: [0, [Validators.required, areaValidator()]],
    anoSafra: [new Date().getFullYear(), [Validators.required, yearValidator()]],
    senha: ['', [Validators.required, passwordValidator(6)]]
  });

  constructor() {
    // Watch for tipo cliente changes to update CPF/CNPJ validation
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
    return getCpfCnpjMask(tipoCliente || 'PF');
  }

  getCpfCnpjPlaceholder(): string {
    const tipoCliente = this.exampleForm.get('tipoCliente')?.value;
    return tipoCliente === 'PF' ? '123.456.789-01' : '12.345.678/0001-90';
  }

  private updateCpfCnpjValidation(): void {
    const cpfCnpjControl = this.exampleForm.get('cpfCnpj');
    if (cpfCnpjControl) {
      cpfCnpjControl.setValidators([Validators.required, conditionalCpfCnpjValidator('tipoCliente')]);
      cpfCnpjControl.updateValueAndValidity();
    }
  }

  onSubmit(): void {
    if (this.exampleForm.valid) {
      console.log('Form is valid!', this.exampleForm.value);
      this.validationSummary = { isValid: true, errors: [] };
    } else {
      this.validationService.markFormGroupTouched(this.exampleForm);
      this.showValidationSummary();
    }
  }

  onClear(): void {
    this.exampleForm.reset({
      tipoCliente: 'PF',
      anoSafra: new Date().getFullYear(),
      area: 0
    });
    this.validationSummary = null;
  }

  showValidationSummary(): void {
    this.validationSummary = this.validationService.getFormValidationSummary(this.exampleForm);
  }
}