import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

// PrimeNG imports

import { TabsModule } from 'primeng/tabs';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { InputMaskModule } from 'primeng/inputmask';
import { ToastModule } from 'primeng/toast';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';

// Services and models
import { FornecedorService } from '../services/fornecedor.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ValidationService } from '../../../shared/services/validation.service';
import { Fornecedor, FornecedorForm } from '../../../shared/models/fornecedor.model';
import { FornecedorFormControls, FornecedorDadosGeraisFormControls, EnderecoFormControls, PontoDistribuicaoFormControls, UsuarioFormControls } from '../../../shared/models/forms.model';
import { TipoCliente } from '../../../shared/models/produtor.model';

// Validators
import { conditionalCpfCnpjValidator, getCpfCnpjMask } from '../../../shared/utils/document-validators.util';
import { phoneValidator, emailValidator, nameValidator, passwordValidator, cepValidator } from '../../../shared/utils/field-validators.util';

// Shared components
import { CoordenadasMapComponent } from '../../../shared/components/coordenadas-map.component';
// UsuarioMasterFormComponent removido - usando formulário inline

// Feature components
import { PontoDistribuicaoFormComponent } from './ponto-distribuicao-form.component';

/**
 * Fornecedor Detail Component with tab navigation
 * Handles creation and editing of fornecedor records with complete form validation
 */
@Component({
  selector: 'app-fornecedor-detail',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TabsModule,
    CardModule,
    ButtonModule,
    InputTextModule,
    SelectModule,
    InputMaskModule,
    ToastModule,
    ProgressSpinnerModule,
    ConfirmDialogModule,
    // Shared components
    CoordenadasMapComponent,
    // Feature components
    PontoDistribuicaoFormComponent
  ],
  providers: [ConfirmationService],
  templateUrl: './fornecedor-detail.component.html',
  styleUrls: ['./fornecedor-detail.component.scss']
})
export class FornecedorDetailComponent implements OnInit {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private fornecedorService = inject(FornecedorService);
  private notificationService = inject(NotificationService);
  private validationService = inject(ValidationService);

  // Signals for reactive state management
  loading = signal(false);
  saving = signal(false);
  fornecedorId = signal<number | null>(null);
  isEditMode = computed(() => this.fornecedorId() !== null);
  activeTabIndex = signal(0);

  // Form and validation
  fornecedorForm!: FormGroup<FornecedorFormControls>;
  
  // Dropdown options
  tipoClienteOptions = [
    { label: 'Pessoa Física', value: TipoCliente.PF },
    { label: 'Pessoa Jurídica', value: TipoCliente.PJ }
  ];



  constructor() {
    this.initializeForm();
    this.loadRouteData();
  }

  ngOnInit(): void {
    // Component initialization is handled in constructor
  }

  /**
   * Initialize reactive form with validation
   */
  private initializeForm(): void {
    this.fornecedorForm = this.fb.group<FornecedorFormControls>({
      dadosGerais: this.fb.group<FornecedorDadosGeraisFormControls>({
        nome: this.fb.control('', {
          nonNullable: true,
          validators: [Validators.required, nameValidator(2)]
        }),
        cpfCnpj: this.fb.control('', {
          nonNullable: true,
          validators: [Validators.required, conditionalCpfCnpjValidator('tipoCliente')]
        }),
        tipoCliente: this.fb.control(TipoCliente.PF, {
          nonNullable: true,
          validators: [Validators.required]
        }),
        telefone: this.fb.control('', {
          nonNullable: true,
          validators: [phoneValidator()]
        }),
        email: this.fb.control('', {
          nonNullable: true,
          validators: [emailValidator()]
        })
      }),
      inscricaoEstadual: this.fb.control('', {
        nonNullable: true
      }),
      endereco: this.fb.group<EnderecoFormControls>({
        logradouro: this.fb.control('', { nonNullable: true }),
        numero: this.fb.control('', { nonNullable: true }),
        complemento: this.fb.control('', { nonNullable: true }),
        bairro: this.fb.control('', { nonNullable: true }),
        cidade: this.fb.control('', { nonNullable: true }),
        uf: this.fb.control('', { nonNullable: true }),
        cep: this.fb.control('', { 
          nonNullable: true,
          validators: [cepValidator()]
        }),
        latitude: this.fb.control<number | null>(null),
        longitude: this.fb.control<number | null>(null)
      }),
      pontosDistribuicao: this.fb.array<FormGroup<PontoDistribuicaoFormControls>>([]),
      usuarioMaster: this.fb.group<UsuarioFormControls>({
        nome: this.fb.control('', {   
          validators: [Validators.required]
        }),
        email: this.fb.control('', {
          validators: [Validators.required]
        }),
        senha: this.fb.control('', {
          validators: [Validators.required]
        }),
        telefone: this.fb.control('', {
          nonNullable: true,
          validators: [phoneValidator()]
        })
      })
    });

    // Watch for tipo cliente changes to update CPF/CNPJ validation
    this.fornecedorForm.get('dadosGerais.tipoCliente')?.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe(() => {
        this.updateCpfCnpjValidation();
      });
  }

  /**
   * Load route data and determine if editing existing fornecedor
   */
  private loadRouteData(): void {
    this.route.params
      .pipe(takeUntilDestroyed())
      .subscribe(params => {
        const id = params['id'];
        if (id && id !== 'novo') {
          this.fornecedorId.set(+id);
          this.loadFornecedor(+id);
        }
      });
  }

  /**
   * Load fornecedor data for editing
   */
  private loadFornecedor(id: number): void {
    this.loading.set(true);
    
    this.fornecedorService.getById(id)
      .pipe(takeUntilDestroyed())
      .subscribe({
        next: (fornecedor) => {
          this.populateForm(fornecedor);
          this.loading.set(false);
        },
        error: (error) => {
          this.notificationService.showCustomError('Erro ao carregar fornecedor: ' + error.message);
          this.loading.set(false);
          this.router.navigate(['/fornecedores']);
        }
      });
  }

  /**
   * Populate form with fornecedor data
   */
  private populateForm(fornecedor: Fornecedor): void {
    this.fornecedorForm.patchValue({
      dadosGerais: {
        nome: fornecedor.nome,
        cpfCnpj: fornecedor.cpfCnpj,
        tipoCliente: fornecedor.tipoCliente,
        telefone: fornecedor.telefone || '',
        email: fornecedor.email || ''
      }
    });

    // Populate endereco
    if (fornecedor.endereco) {
      this.enderecoFormGroup.patchValue(fornecedor.endereco);
    }
    
    // Populate inscricao estadual
    if (fornecedor.inscricaoEstadual) {
      this.fornecedorForm.get('inscricaoEstadual')?.patchValue(fornecedor.inscricaoEstadual);
    }

    // Populate pontos de distribuicao
    if (fornecedor.pontosDistribuicao && fornecedor.pontosDistribuicao.length > 0) {
      const pontosArray = this.pontosDistribuicaoFormArray;
      pontosArray.clear();
      fornecedor.pontosDistribuicao.forEach(ponto => {
        const pontoGroup = this.createPontoDistribuicaoFormGroup();
        pontoGroup.patchValue({
          nome: ponto.nome,
          latitude: ponto.latitude,
          longitude: ponto.longitude,
          endereco: ponto.endereco
        });
        pontosArray.push(pontoGroup);
      });
    }

    // Populate usuarioMaster
    if (fornecedor.usuarioMaster) {
      this.fornecedorForm.get('usuarioMaster')?.patchValue({
        nome: fornecedor.usuarioMaster.nome,
        email: fornecedor.usuarioMaster.email,
        senha: fornecedor.usuarioMaster.senha,
        telefone: fornecedor.usuarioMaster.telefone || ''
      });
    }
  }

  /**
   * Update CPF/CNPJ validation based on tipo cliente
   */
  private updateCpfCnpjValidation(): void {
    const cpfCnpjControl = this.fornecedorForm.get('dadosGerais.cpfCnpj');
    if (cpfCnpjControl) {
      cpfCnpjControl.setValidators([Validators.required, conditionalCpfCnpjValidator('tipoCliente')]);
      cpfCnpjControl.updateValueAndValidity();
    }
  }

  /**
   * Get form control for validation display
   */
  getFormControl(path: string) {
    return this.fornecedorForm.get(path);
  }

  /**
   * Check if field has error
   */
  hasFieldError(path: string, errorType?: string): boolean {
    const control = this.getFormControl(path);
    if (!control) return false;
    
    if (errorType) {
      return this.validationService.hasError(control, errorType);
    }
    
    return this.validationService.shouldShowError(control);
  }

  /**
   * Get field error message
   */
  getFieldErrorMessage(path: string): string {
    const control = this.getFormControl(path);
    if (!control) return '';
    
    const fieldName = path.split('.').pop();
    return this.validationService.getErrorMessage(control, fieldName);
  }

  /**
   * Get CPF/CNPJ mask based on tipo cliente
   */
  getCpfCnpjMask(): string {
    const tipoCliente = this.fornecedorForm.get('dadosGerais.tipoCliente')?.value;
    return getCpfCnpjMask(tipoCliente);
  }

  /**
   * Handle tab change
   */
  onTabChange(value: string | number): void {
    this.activeTabIndex.set(typeof value === 'string' ? parseInt(value) : value);
  }

  /**
   * Save fornecedor
   */
  onSave(): void {
    // Mark all form sections as touched to show validation errors
    this.validationService.markFormGroupTouched(this.fornecedorForm);

    // Validate all form sections
    if (!this.validateAllSections()) {
      this.notificationService.showValidationWarning('Por favor, corrija os erros no formulário antes de salvar');
      
      // Navigate to first tab with errors
      this.navigateToFirstErrorTab();
      return;
    }

    this.saving.set(true);
    const formValue = this.fornecedorForm.value;
    
    // Transform form data to API format with proper validation
    const fornecedorData: FornecedorForm = {
      codigo: '', // Will be generated by backend
      nome: formValue.dadosGerais?.nome?.trim() || '',
      cpfCnpj: formValue.dadosGerais?.cpfCnpj?.replace(/\D/g, '') || '', // Remove formatting
      tipoCliente: formValue.dadosGerais?.tipoCliente || TipoCliente.PF,
      telefone: formValue.dadosGerais?.telefone?.replace(/\D/g, '') || undefined, // Remove formatting
      email: formValue.dadosGerais?.email?.trim() || undefined,
      inscricaoEstadual: formValue.inscricaoEstadual?.trim() || undefined,
      endereco: formValue.endereco && formValue.endereco.logradouro ? {
        logradouro: formValue.endereco.logradouro?.trim() || '',
        numero: formValue.endereco.numero?.trim() || '',
        complemento: formValue.endereco.complemento?.trim() || '',
        bairro: formValue.endereco.bairro?.trim() || '',
        cidade: formValue.endereco.cidade?.trim() || '',
        uf: formValue.endereco.uf || '',
        cep: formValue.endereco.cep?.replace(/\D/g, '') || '', // Remove formatting
        latitude: formValue.endereco.latitude || undefined,
        longitude: formValue.endereco.longitude || undefined
      } : undefined,
      pontosDistribuicao: (formValue.pontosDistribuicao || [])
        .filter(ponto => ponto?.nome && ponto?.endereco?.logradouro) // Only include valid pontos
        .map(ponto => ({
          nome: ponto.nome?.trim() || '',
          latitude: ponto.latitude || undefined,
          longitude: ponto.longitude || undefined,
          endereco: {
            logradouro: ponto.endereco?.logradouro?.trim() || '',
            numero: ponto.endereco?.numero?.trim() || '',
            complemento: ponto.endereco?.complemento?.trim() || '',
            bairro: ponto.endereco?.bairro?.trim() || '',
            cidade: ponto.endereco?.cidade?.trim() || '',
            uf: ponto.endereco?.uf || '',
            cep: ponto.endereco?.cep?.replace(/\D/g, '') || '',
            latitude: ponto.endereco?.latitude || undefined,
            longitude: ponto.endereco?.longitude || undefined
          }
        })),
      usuarioMaster: formValue.usuarioMaster && formValue.usuarioMaster.nome && formValue.usuarioMaster.email ? {
        nome: formValue.usuarioMaster.nome.trim(),
        email: formValue.usuarioMaster.email.trim(),
        senha: formValue.usuarioMaster.senha || '',
        telefone: formValue.usuarioMaster.telefone?.replace(/\D/g, '') || ''
      } : undefined
    };

    // Final validation before sending to API
    if (!fornecedorData.nome || !fornecedorData.cpfCnpj || !fornecedorData.usuarioMaster) {
      this.notificationService.showValidationWarning('Dados obrigatórios estão faltando. Verifique todos os campos.');
      this.saving.set(false);
      return;
    }

    const saveOperation = this.isEditMode() 
      ? this.fornecedorService.update(this.fornecedorId()!, fornecedorData)
      : this.fornecedorService.create(fornecedorData);

    saveOperation
      .subscribe({
        next: () => {
          this.notificationService.showCustomSuccess(
            this.isEditMode() ? 'Fornecedor atualizado com sucesso' : 'Fornecedor criado com sucesso'
          );
          this.saving.set(false);
          this.router.navigate(['/fornecedores']);
        },
        error: (error) => {
          this.notificationService.showCustomError('Erro ao salvar fornecedor: ' + error.message);
          this.saving.set(false);
        }
      });
  }

  /**
   * Cancel and return to list
   */
  onCancel(): void {
    this.router.navigate(['/fornecedores']);
  }

  /**
   * Get endereco form group
   */
  get enderecoFormGroup(): FormGroup<EnderecoFormControls> {
    return this.fornecedorForm.get('endereco') as FormGroup<EnderecoFormControls>;
  }

  /**
   * Get pontos distribuicao form array
   */
  get pontosDistribuicaoFormArray(): FormArray<FormGroup<PontoDistribuicaoFormControls>> {
    return this.fornecedorForm.get('pontosDistribuicao') as FormArray<FormGroup<PontoDistribuicaoFormControls>>;
  }

  /**
   * Get usuario master form group
   */
  get usuarioMasterFormGroup(): FormGroup<UsuarioFormControls> {
    return this.fornecedorForm.get('usuarioMaster') as FormGroup<UsuarioFormControls>;
  }

  /**
   * Get current coordinates for map initialization
   */
  get currentCoordinates(): { latitude: number | null; longitude: number | null } {
    return {
      latitude: this.enderecoFormGroup.get('latitude')?.value ?? null,
      longitude: this.enderecoFormGroup.get('longitude')?.value ?? null
    };
  }



  /**
   * Create ponto distribuicao form group
   */
  private createPontoDistribuicaoFormGroup(): FormGroup<PontoDistribuicaoFormControls> {
    return this.fb.group<PontoDistribuicaoFormControls>({
      nome: this.fb.control('', {
        nonNullable: true,
        validators: [Validators.required, nameValidator(2)]
      }),
      latitude: this.fb.control<number | null>(null),
      longitude: this.fb.control<number | null>(null),
      endereco: this.fb.group<EnderecoFormControls>({
        logradouro: this.fb.control('', { nonNullable: true, validators: [Validators.required] }),
        numero: this.fb.control('', { nonNullable: true }),
        complemento: this.fb.control('', { nonNullable: true }),
        bairro: this.fb.control('', { nonNullable: true, validators: [Validators.required] }),
        cidade: this.fb.control('', { nonNullable: true, validators: [Validators.required] }),
        uf: this.fb.control('', { nonNullable: true, validators: [Validators.required] }),
        cep: this.fb.control('', { nonNullable: true, validators: [Validators.required, cepValidator()] }),
        latitude: this.fb.control<number | null>(null),
        longitude: this.fb.control<number | null>(null)
      })
    });
  }



  /**
   * Handle pontos distribuicao changes
   */
  onPontosDistribuicaoChange(pontos: any[]): void {
    // Mark form as dirty to enable save button and trigger validation
    this.pontosDistribuicaoFormArray.markAsDirty();
    this.pontosDistribuicaoFormArray.markAsTouched();
  }

  /**
   * Handle usuario master changes
   */
  onUsuarioMasterChange(usuario: any): void {
    // Mark form as dirty to enable save button and trigger validation
    this.usuarioMasterFormGroup.markAsDirty();
    this.usuarioMasterFormGroup.markAsTouched();

    // Explicitly update validity for each control
    Object.keys(this.usuarioMasterFormGroup.controls).forEach(key => {
      const control = this.usuarioMasterFormGroup.get(key);
      control?.updateValueAndValidity();
    });
  }

  /**
   * Handle coordinates selection for fornecedor address location
   */
  onCoordinatesSelected(coordenadas: { latitude: number | null; longitude: number | null }): void {
    console.log('🗺️ Coordenadas selecionadas no mapa:', coordenadas);
    
    // Atualizar as coordenadas do endereço do fornecedor
    this.enderecoFormGroup.patchValue({
      latitude: coordenadas.latitude,
      longitude: coordenadas.longitude
    });
    
    // Marcar como modificado para habilitar o botão salvar
    this.enderecoFormGroup.markAsDirty();
    this.enderecoFormGroup.markAsTouched();
    
    console.log('📍 Coordenadas atualizadas no formulário de endereço:', {
      latitude: this.enderecoFormGroup.get('latitude')?.value,
      longitude: this.enderecoFormGroup.get('longitude')?.value
    });
    
    // Mostrar notificação de sucesso
    this.notificationService.showCustomSuccess('Coordenadas atualizadas com sucesso!');
  }

  /**
   * Validate all form sections
   */
  private validateAllSections(): boolean {
    let isValid = true;
    const validationErrors: string[] = [];

    // Validate main form (dados gerais)
    if (this.fornecedorForm.get('dadosGerais')?.invalid) {
      isValid = false;
      validationErrors.push('Dados gerais contêm erros');
    }

    // Validate endereco (optional but if present must be valid)
    if (this.enderecoFormGroup.invalid) {
      isValid = false;
      validationErrors.push('Endereço contém erros');
    }

    // Validate pontos distribuicao (optional but if present must be valid)
    this.pontosDistribuicaoFormArray.controls.forEach((pontoControl, pontoIndex) => {
      if (pontoControl.invalid) {
        isValid = false;
        validationErrors.push(`Ponto de distribuição ${pontoIndex + 1} contém erros`);
      }
    });

    // Validate usuario master (required)
    if (this.usuarioMasterFormGroup.invalid) {
      console.log('Usuario Master FormGroup is invalid:', this.usuarioMasterFormGroup);
      Object.keys(this.usuarioMasterFormGroup.controls).forEach(key => {
        const control = this.usuarioMasterFormGroup.get(key);
        if (control && control.invalid) {
          console.log(`Control '${key}' is invalid. Errors:`, control.errors);
        }
      });
      isValid = false;
      validationErrors.push('Dados do usuário master contêm erros');
    }

    // Log validation errors for debugging
    if (!isValid && validationErrors.length > 0) {
      console.warn('Validation errors:', validationErrors);
    }

    return isValid;
  }



  /**
   * Navigate to the first tab that has validation errors
   */
  private navigateToFirstErrorTab(): void {
    // Check dados gerais (tab 0)
    if (this.fornecedorForm.get('dadosGerais')?.invalid) {
      this.activeTabIndex.set(0);
      return;
    }

    // Check endereco (tab 1)
    if (this.enderecoFormGroup.invalid) {
      this.activeTabIndex.set(1);
      return;
    }

    // Check pontos distribuicao (tab 2)
    if (this.pontosDistribuicaoFormArray.invalid) {
      this.activeTabIndex.set(2);
      return;
    }

    // Check usuario master (tab 4)
    if (this.usuarioMasterFormGroup.invalid) {
      this.activeTabIndex.set(4);
      return;
    }

    // Default to first tab if no specific errors found
    this.activeTabIndex.set(0);
  }

  /**
   * Test method to fill usuario master form programmatically
   */
  testFillUsuarioMaster(): void {
    console.log('🧪 Preenchendo formulário de usuário master automaticamente...');
    
    const testData = {
      nome: 'Lindemberg Cortez Gomes',
      email: 'lindemberg.cortez@gmail.com',
      senha: 'senha123456',
      telefone: '84991258889'
    };

    console.log('📋 Dados ANTES do preenchimento:', {
      value: this.usuarioMasterFormGroup.value,
      valid: this.usuarioMasterFormGroup.valid,
      status: this.usuarioMasterFormGroup.status
    });

    // Preenche os dados
    this.usuarioMasterFormGroup.patchValue(testData);
    
    // Marca como touched para mostrar validação
    Object.keys(testData).forEach(key => {
      const control = this.usuarioMasterFormGroup.get(key);
      if (control) {
        control.markAsTouched();
        control.markAsDirty();
        control.updateValueAndValidity();
      }
    });

    // Atualiza o FormGroup
    this.usuarioMasterFormGroup.updateValueAndValidity();

    console.log('✅ Dados DEPOIS do preenchimento:', {
      value: this.usuarioMasterFormGroup.value,
      valid: this.usuarioMasterFormGroup.valid,
      status: this.usuarioMasterFormGroup.status
    });
  }
}