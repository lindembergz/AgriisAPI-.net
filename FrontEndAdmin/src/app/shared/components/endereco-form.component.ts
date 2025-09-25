import { Component, Input, Output, EventEmitter, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';

// PrimeNG imports
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { InputMaskModule } from 'primeng/inputmask';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';

// Models
import { EnderecoForm } from '../models/endereco.model';
import { EnderecoFormControls } from '../models/forms.model';

/**
 * Reusable Endereco Form Component
 * Can be used by both Produtores and Fornecedores modules
 */
@Component({
  selector: 'app-endereco-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    CardModule,
    ButtonModule,
    InputTextModule,
    SelectModule,
    InputMaskModule,
    ConfirmDialogModule
  ],
  providers: [ConfirmationService],
  templateUrl: './endereco-form.component.html',
  styleUrls: ['./endereco-form.component.scss']
})
export class EnderecoFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private confirmationService = inject(ConfirmationService);

  @Input() enderecosFormArray!: FormArray<FormGroup<EnderecoFormControls>>;
  @Input() readonly = false;
  @Output() enderecosChange = new EventEmitter<EnderecoForm[]>();

  // Signals for reactive state
  loading = signal(false);

  // Dropdown options for UF (Brazilian states)
  ufOptions = [
    { label: 'Acre', value: 'AC' },
    { label: 'Alagoas', value: 'AL' },
    { label: 'Amapá', value: 'AP' },
    { label: 'Amazonas', value: 'AM' },
    { label: 'Bahia', value: 'BA' },
    { label: 'Ceará', value: 'CE' },
    { label: 'Distrito Federal', value: 'DF' },
    { label: 'Espírito Santo', value: 'ES' },
    { label: 'Goiás', value: 'GO' },
    { label: 'Maranhão', value: 'MA' },
    { label: 'Mato Grosso', value: 'MT' },
    { label: 'Mato Grosso do Sul', value: 'MS' },
    { label: 'Minas Gerais', value: 'MG' },
    { label: 'Pará', value: 'PA' },
    { label: 'Paraíba', value: 'PB' },
    { label: 'Paraná', value: 'PR' },
    { label: 'Pernambuco', value: 'PE' },
    { label: 'Piauí', value: 'PI' },
    { label: 'Rio de Janeiro', value: 'RJ' },
    { label: 'Rio Grande do Norte', value: 'RN' },
    { label: 'Rio Grande do Sul', value: 'RS' },
    { label: 'Rondônia', value: 'RO' },
    { label: 'Roraima', value: 'RR' },
    { label: 'Santa Catarina', value: 'SC' },
    { label: 'São Paulo', value: 'SP' },
    { label: 'Sergipe', value: 'SE' },
    { label: 'Tocantins', value: 'TO' }
  ];

  // Validation messages
  validationMessages = {
    logradouro: {
      required: 'Logradouro é obrigatório',
      minlength: 'Logradouro deve ter pelo menos 3 caracteres'
    },
    numero: {
      required: 'Número é obrigatório'
    },
    bairro: {
      required: 'Bairro é obrigatório',
      minlength: 'Bairro deve ter pelo menos 2 caracteres'
    },
    cidade: {
      required: 'Cidade é obrigatória',
      minlength: 'Cidade deve ter pelo menos 2 caracteres'
    },
    uf: {
      required: 'UF é obrigatória'
    },
    cep: {
      required: 'CEP é obrigatório',
      pattern: 'CEP inválido'
    }
  };

  ngOnInit(): void {
    // Initialize with at least one endereco if empty
    if (this.enderecosFormArray.length === 0) {
      this.addEndereco();
    }
  }

  /**
   * Create new endereco form group
   */
  private createEnderecoFormGroup(): FormGroup<EnderecoFormControls> {
    return this.fb.group<EnderecoFormControls>({
      logradouro: this.fb.control('', {
        nonNullable: true,
        validators: [Validators.required, Validators.minLength(3)]
      }),
      numero: this.fb.control('', {
        nonNullable: true,
        validators: [Validators.required]
      }),
      complemento: this.fb.control('', {
        nonNullable: true
      }),
      bairro: this.fb.control('', {
        nonNullable: true,
        validators: [Validators.required, Validators.minLength(2)]
      }),
      cidade: this.fb.control('', {
        nonNullable: true,
        validators: [Validators.required, Validators.minLength(2)]
      }),
      uf: this.fb.control('', {
        nonNullable: true,
        validators: [Validators.required]
      }),
      cep: this.fb.control('', {
        nonNullable: true,
        validators: [Validators.required, this.cepValidator]
      }),
      latitude: this.fb.control<number | null>(null),
      longitude: this.fb.control<number | null>(null)
    });
  }

  /**
   * CEP validator
   */
  private cepValidator = (control: any) => {
    if (!control.value) return null;
    
    const value = control.value.replace(/\D/g, '');
    if (value.length !== 8) {
      return { pattern: true };
    }
    
    return null;
  };

  /**
   * Add new endereco to the form array
   */
  addEndereco(): void {
    const enderecoGroup = this.createEnderecoFormGroup();
    this.enderecosFormArray.push(enderecoGroup);
    this.emitChange();
  }

  /**
   * Remove endereco from the form array
   */
  removeEndereco(index: number): void {
    if (this.enderecosFormArray.length <= 1) {
      return; // Keep at least one endereco
    }

    this.confirmationService.confirm({
      message: 'Tem certeza que deseja remover este endereço?',
      header: 'Confirmar Remoção',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Sim',
      rejectLabel: 'Não',
      accept: () => {
        this.enderecosFormArray.removeAt(index);
        this.emitChange();
      }
    });
  }

  /**
   * Get endereco form group at index
   */
  getEnderecoFormGroup(index: number): FormGroup<EnderecoFormControls> {
    return this.enderecosFormArray.at(index) as FormGroup<EnderecoFormControls>;
  }

  /**
   * Get form control for validation display
   */
  getFormControl(index: number, controlName: string) {
    return this.getEnderecoFormGroup(index).get(controlName);
  }

  /**
   * Check if field has error
   */
  hasFieldError(index: number, controlName: string, errorType?: string): boolean {
    const control = this.getFormControl(index, controlName);
    if (!control) return false;
    
    if (errorType) {
      return control.hasError(errorType) && (control.dirty || control.touched);
    }
    
    return control.invalid && (control.dirty || control.touched);
  }

  /**
   * Get field error message
   */
  getFieldErrorMessage(index: number, controlName: string): string {
    const control = this.getFormControl(index, controlName);
    if (!control || !control.errors) return '';
    
    const fieldMessages = this.validationMessages[controlName as keyof typeof this.validationMessages];
    if (!fieldMessages) return 'Campo inválido';
    
    const errorType = Object.keys(control.errors)[0] as keyof typeof fieldMessages;
    return fieldMessages[errorType] || 'Campo inválido';
  }

  /**
   * Handle CEP blur to fetch address data (ViaCEP integration)
   */
  onCepBlur(index: number): void {
    const cepControl = this.getFormControl(index, 'cep');
    if (!cepControl || cepControl.invalid) return;
    
    const cep = cepControl.value.replace(/\D/g, '');
    if (cep.length === 8) {
      this.fetchAddressByCep(index, cep);
    }
  }

  /**
   * Fetch address data from ViaCEP API
   */
  private fetchAddressByCep(index: number, cep: string): void {
    this.loading.set(true);
    
    // Using ViaCEP API to fetch address data
    fetch(`https://viacep.com.br/ws/${cep}/json/`)
      .then(response => response.json())
      .then(data => {
        if (!data.erro) {
          const enderecoGroup = this.getEnderecoFormGroup(index);
          enderecoGroup.patchValue({
            logradouro: data.logradouro || '',
            bairro: data.bairro || '',
            cidade: data.localidade || '',
            uf: data.uf || ''
          });
          this.emitChange();
        }
        this.loading.set(false);
      })
      .catch(error => {
        console.error('Erro ao buscar CEP:', error);
        this.loading.set(false);
      });
  }

  /**
   * Emit changes to parent component
   */
  private emitChange(): void {
    const enderecos = this.enderecosFormArray.value as EnderecoForm[];
    this.enderecosChange.emit(enderecos);
    // Mark the form array as dirty to trigger change detection
    this.enderecosFormArray.markAsDirty();
  }

  /**
   * Get endereco title for display
   */
  getEnderecoTitle(index: number): string {
    const endereco = this.getEnderecoFormGroup(index).value;
    if (endereco.logradouro && endereco.cidade) {
      return `${endereco.logradouro}, ${endereco.cidade}`;
    }
    return `Endereço ${index + 1}`;
  }

  /**
   * Check if can remove endereco (must have at least one)
   */
  canRemoveEndereco(): boolean {
    return this.enderecosFormArray.length > 1 && !this.readonly;
  }
}