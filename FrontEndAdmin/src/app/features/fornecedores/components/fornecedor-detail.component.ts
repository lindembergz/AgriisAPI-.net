import { Component, OnInit, ViewChild, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule, FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

// PrimeNG imports

import { TabsModule } from 'primeng/tabs';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputMaskModule } from 'primeng/inputmask';
import { CheckboxModule } from 'primeng/checkbox';
import { RadioButtonModule } from 'primeng/radiobutton';
import { ToastModule } from 'primeng/toast';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';

// Services and models
import { FornecedorService } from '../services/fornecedor.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ValidationService } from '../../../shared/services/validation.service';
import { Fornecedor, FornecedorForm, RAMOS_ATIVIDADE_DISPONIVEIS, ENDERECO_CORRESPONDENCIA_OPTIONS } from '../../../shared/models/fornecedor.model';
import { FornecedorFormControls, FornecedorDadosGeraisFormControls, EnderecoFormControls, PontoDistribuicaoFormControls, UsuarioFormControls } from '../../../shared/models/forms.model';
import { TipoCliente } from '../../../shared/models/produtor.model';
import { GeographicData, UfDto, MunicipioDto } from '../../../shared/models/reference.model';
import { UfService } from '../../../features/referencias/ufs/services/uf.service';
import { MunicipioService } from '../../../features/referencias/municipios/services/municipio.service';

// Validators
import { conditionalCpfCnpjValidator, getCpfCnpjMask } from '../../../shared/utils/document-validators.util';
import { phoneValidator, emailValidator, nameValidator, passwordValidator, cepValidator } from '../../../shared/utils/field-validators.util';

// Shared components
import { CoordenadasMapComponent } from '../../../shared/components/coordenadas-map.component';
// GeographicSelectorComponent removido - usando selects nativos
// UsuarioMasterFormComponent removido - usando formulário inline

// Feature components
import { PontoDistribuicaoFormComponent } from './ponto-distribuicao-form.component';
import { InputNumber } from 'primeng/inputnumber';

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
    FormsModule,
    TabsModule,
    CardModule,
    ButtonModule,
    InputTextModule,
    InputNumberModule,
    SelectModule,
    InputMaskModule,
    CheckboxModule,
    RadioButtonModule,
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
  private ufService = inject(UfService);
  private municipioService = inject(MunicipioService);

  // Signals for reactive state management
  loading = signal(false);
  saving = signal(false);
  fornecedorId = signal<number | null>(null);
  isEditMode = computed(() => this.fornecedorId() !== null);
  activeTabIndex = signal(0);

  // Geographic data signals
  selectedGeographicData = signal<GeographicData | null>(null);
  geographicDataLoading = signal(false);

  // Geographic data for selects
  availableUfs = signal<UfDto[]>([]);
  availableMunicipios = signal<MunicipioDto[]>([]);
  ufsLoading = signal<boolean>(false);
  municipiosLoading = signal<boolean>(false);

  // Form and validation
  fornecedorForm!: FormGroup<FornecedorFormControls>;

  // Dropdown options
  tipoClienteOptions = [
    { label: 'Pessoa Física', value: TipoCliente.PF },
    { label: 'Pessoa Jurídica', value: TipoCliente.PJ }
  ];

  // NOVOS CAMPOS - Opções para os novos campos
  ramosDisponiveis = RAMOS_ATIVIDADE_DISPONIVEIS;
  enderecoCorrespondenciaOptions = ENDERECO_CORRESPONDENCIA_OPTIONS;



  constructor() {
    this.initializeForm();
    this.loadRouteData();
    this.loadUfsForSelect();
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
        nomeFantasia: this.fb.control('', { // NOVO CAMPO
          nonNullable: true,
          validators: [Validators.maxLength(200)]
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
        }),
        ramosAtividade: this.fb.control<string[]>([], { // NOVO CAMPO
          nonNullable: true
        }),
        enderecoCorrespondencia: this.fb.control('MesmoFaturamento', { // NOVO CAMPO
          nonNullable: true,
          validators: [Validators.required]
        }),
        inscricaoEstadual: this.fb.control('', { // MOVIDO PARA DENTRO DE DADOS GERAIS
          nonNullable: true
        })
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
        longitude: this.fb.control<number | null>(null),
        // New geographic reference fields
        ufId: this.fb.control<number | null>(null, { validators: [Validators.required] }),
        municipioId: this.fb.control<number | null>(null, { validators: [Validators.required] })
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
          console.log(fornecedor);
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
    console.log('📝 Populando formulário com dados do fornecedor:', fornecedor);



    this.fornecedorForm.patchValue({
      dadosGerais: {
        nome: fornecedor.nome,
        nomeFantasia: fornecedor.nomeFantasia || '', // NOVO CAMPO
        cpfCnpj: fornecedor.cnpj || fornecedor.cpfCnpj || '', // Usar cnpj como prioridade
        tipoCliente: fornecedor.tipoCliente as any,
        telefone: fornecedor.telefone || '',
        email: fornecedor.email || '',
        ramosAtividade: fornecedor.ramosAtividade || [], // NOVO CAMPO
        enderecoCorrespondencia: fornecedor.enderecoCorrespondencia || 'MesmoFaturamento', // NOVO CAMPO
        inscricaoEstadual: fornecedor.inscricaoEstadual || '' // MOVIDO PARA DENTRO DE DADOS GERAIS
      }
    });

    console.log('📝 Valores após patchValue:', {
      cpfCnpj: this.fornecedorForm.get('dadosGerais.cpfCnpj')?.value,
      inscricaoEstadual: this.fornecedorForm.get('dadosGerais.inscricaoEstadual')?.value,
      ramosAtividade: this.fornecedorForm.get('dadosGerais.ramosAtividade')?.value
    });

    // Debug específico para inscrição estadual
    console.log('🔍 DEBUG Inscrição Estadual:', {
      valorOriginal: fornecedor.inscricaoEstadual,
      valorFormulario: this.fornecedorForm.get('dadosGerais.inscricaoEstadual')?.value,
      controleExiste: !!this.fornecedorForm.get('dadosGerais.inscricaoEstadual'),
      formularioCompleto: this.fornecedorForm.value
    });

    // Populate endereco
    console.log('🏠 Populando endereço do fornecedor:', {
      logradouro: fornecedor.logradouro,
      bairro: fornecedor.bairro,
      ufId: fornecedor.ufId,
      municipioId: fornecedor.municipioId,
      ufNome: fornecedor.ufNome,
      municipioNome: fornecedor.municipioNome
    });

    this.enderecoFormGroup.patchValue({
      logradouro: fornecedor.logradouro || '',
      bairro: fornecedor.bairro || '',
      cep: fornecedor.cep || '',
      complemento: fornecedor.complemento || '',
      latitude: fornecedor.latitude,
      longitude: fornecedor.longitude,
      ufId: fornecedor.ufId,
      municipioId: fornecedor.municipioId,
      uf: fornecedor.ufCodigo || '',
      cidade: fornecedor.municipioNome || ''
    });

    // Load geographic data if UF and Municipio IDs are available
    const ufId = fornecedor.ufId;
    const municipioId = fornecedor.municipioId;

    if (ufId) {
      console.log('🌍 Carregando municípios para UF:', ufId);
      this.loadMunicipiosForSelect(ufId);
    }

    // Inscrição estadual já foi mapeada no patchValue acima

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

    // Populate usuarioMaster - buscar o usuário Master da lista de usuários
    console.log('👤 Populando usuário Master:', fornecedor.usuarios);

    // Encontrar o usuário Master (role 3 = RoleFornecedorWebAdmin)
    const usuarioMaster = fornecedor.usuarios?.find(u => u.role === 3 || u.roleNome === 'Administrador');

    if (usuarioMaster) {
      console.log('👤 Usuário Master encontrado:', usuarioMaster);
      this.fornecedorForm.get('usuarioMaster')?.patchValue({
        nome: usuarioMaster.usuarioNome || '',
        email: usuarioMaster.usuarioEmail || '',
        senha: '', // Não retornamos a senha por segurança
        telefone: '' // Telefone não está na estrutura atual do UsuarioFornecedorDto
      });
    } else {
      console.log('⚠️ Usuário Master não encontrado na lista de usuários');
    }

    // Fallback: tentar usar fornecedor.usuarioMaster se existir (para compatibilidade)
    if (!usuarioMaster && fornecedor.usuarioMaster) {
      console.log('👤 Usando fallback usuarioMaster:', fornecedor.usuarioMaster);
      this.fornecedorForm.get('usuarioMaster')?.patchValue({
        nome: fornecedor.usuarioMaster.nome,
        email: fornecedor.usuarioMaster.email,
        senha: fornecedor.usuarioMaster.senha || '',
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
   * Check if ramo de atividade is selected
   */
  isRamoAtividadeSelecionado(ramo: string): boolean {
    const ramosAtividade = this.fornecedorForm.get('dadosGerais.ramosAtividade')?.value || [];
    return ramosAtividade.includes(ramo);
  }

  /**
   * Handle ramo de atividade change
   */
  onRamoAtividadeChange(ramo: string, event: any): void {
    const ramosControl = this.fornecedorForm.get('dadosGerais.ramosAtividade');
    if (!ramosControl) return;

    const currentRamos = ramosControl.value || [];

    if (event.target.checked) {
      // Add ramo if not already present
      if (!currentRamos.includes(ramo)) {
        ramosControl.setValue([...currentRamos, ramo]);
      }
    } else {
      // Remove ramo
      ramosControl.setValue(currentRamos.filter((r: string) => r !== ramo));
    }

    // Mark as dirty and touched
    ramosControl.markAsDirty();
    ramosControl.markAsTouched();
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

    // Transform form data to API format matching CriarFornecedorCompletoRequest
    const fornecedorData: any = {
      codigo: '', // Will be generated by backend
      nome: formValue.dadosGerais?.nome?.trim() || '',
      nomeFantasia: formValue.dadosGerais?.nomeFantasia?.trim() || undefined, // NOVO CAMPO
      cpfCnpj: formValue.dadosGerais?.cpfCnpj?.replace(/\D/g, '') || '', // Remove formatting
      tipoCliente: formValue.dadosGerais?.tipoCliente || TipoCliente.PF,
      telefone: formValue.dadosGerais?.telefone?.replace(/\D/g, '') || undefined, // Remove formatting
      email: formValue.dadosGerais?.email?.trim() || undefined,
      inscricaoEstadual: formValue.dadosGerais?.inscricaoEstadual?.trim() || undefined,
      ramosAtividade: formValue.dadosGerais?.ramosAtividade || [], // NOVO CAMPO
      enderecoCorrespondencia: formValue.dadosGerais?.enderecoCorrespondencia || 'MesmoFaturamento', // NOVO CAMPO
      endereco: formValue.endereco && formValue.endereco.logradouro ? {
        logradouro: formValue.endereco.logradouro?.trim() || '',
        numero: formValue.endereco.numero?.trim() || '',
        complemento: formValue.endereco.complemento?.trim() || '',
        bairro: formValue.endereco.bairro?.trim() || '',
        cidade: formValue.endereco.cidade?.trim() || '',
        uf: formValue.endereco.uf || '',
        cep: formValue.endereco.cep?.replace(/\D/g, '') || '', // Remove formatting
        latitude: formValue.endereco.latitude || undefined,
        longitude: formValue.endereco.longitude || undefined,
        // Include geographic references
        ufId: formValue.endereco.ufId || undefined,
        municipioId: formValue.endereco.municipioId || undefined
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
        longitude: this.fb.control<number | null>(null),
        ufId: this.fb.control<number | null>(null),
        municipioId: this.fb.control<number | null>(null)
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
   * Handle geographic selection change
   */
  onGeographicSelectionChange(geographicData: GeographicData): void {
    console.log('🌍 Geographic data changed:', geographicData);

    this.selectedGeographicData.set(geographicData);

    // Update form with geographic references
    if (geographicData.uf) {
      this.enderecoFormGroup.patchValue({
        ufId: geographicData.uf.id,
        uf: geographicData.uf.uf // Keep the old field for backward compatibility
      });
    } else {
      this.enderecoFormGroup.patchValue({
        ufId: null,
        uf: ''
      });
    }

    if (geographicData.municipio) {
      this.enderecoFormGroup.patchValue({
        municipioId: geographicData.municipio.id,
        cidade: geographicData.municipio.nome // Update cidade field with municipio name
      });
    } else {
      this.enderecoFormGroup.patchValue({
        municipioId: null,
        cidade: ''
      });
    }

    // Mark form as dirty to enable save button
    this.enderecoFormGroup.markAsDirty();
    this.enderecoFormGroup.markAsTouched();

    console.log('📍 Form updated with geographic data:', {
      ufId: this.enderecoFormGroup.get('ufId')?.value,
      municipioId: this.enderecoFormGroup.get('municipioId')?.value,
      uf: this.enderecoFormGroup.get('uf')?.value,
      cidade: this.enderecoFormGroup.get('cidade')?.value
    });
  }

  /**
   * Validate geographic selection
   */
  validateGeographicSelection(): boolean {
    const ufId = this.enderecoFormGroup.get('ufId')?.value;
    const municipioId = this.enderecoFormGroup.get('municipioId')?.value;

    if (!ufId || !municipioId) {
      this.notificationService.showValidationWarning('Por favor, selecione UF e Município');
      return false;
    }

    // Validate that municipio belongs to selected UF
    const selectedGeographic = this.selectedGeographicData();
    if (selectedGeographic?.municipio && selectedGeographic?.uf) {
      if (selectedGeographic.municipio.ufId !== selectedGeographic.uf.id) {
        this.notificationService.showValidationWarning('O município selecionado não pertence à UF selecionada');
        return false;
      }
    }

    return true;
  }



  /**
   * Load UFs for select dropdown
   */
  private loadUfsForSelect(): void {
    this.ufsLoading.set(true);

    this.ufService.obterAtivos().subscribe({
      next: (ufs) => {
        console.log('🏛️ UFs carregadas para select:', ufs.length);
        this.availableUfs.set(ufs);
        this.ufsLoading.set(false);
      },
      error: (error) => {
        console.error('❌ Erro ao carregar UFs para select:', error);
        this.ufsLoading.set(false);
      }
    });
  }

  /**
   * Load Municípios for select dropdown based on selected UF
   */
  private loadMunicipiosForSelect(ufId: number): void {
    this.municipiosLoading.set(true);

    this.municipioService.obterAtivosPorUf(ufId).subscribe({
      next: (municipios) => {
        console.log('🏘️ Municípios carregados para select UF', ufId, ':', municipios.length);
        this.availableMunicipios.set(municipios);
        this.municipiosLoading.set(false);
      },
      error: (error) => {
        console.error('❌ Erro ao carregar municípios para select:', error);
        this.municipiosLoading.set(false);
      }
    });
  }

  /**
   * Handle UF selection change
   */
  onUfSelectionChange(event: any): void {
    const ufId = event.value;
    console.log('🏛️ UF selecionada:', ufId);

    // Reset município selection
    this.enderecoFormGroup.patchValue({ municipioId: null, cidade: '' });
    this.availableMunicipios.set([]);

    if (ufId) {
      // Update UF code for backward compatibility
      const uf = this.availableUfs().find(u => u.id === ufId);
      if (uf) {
        this.enderecoFormGroup.patchValue({ uf: uf.uf });
      }

      this.loadMunicipiosForSelect(ufId);
    }

    // Mark form as dirty
    this.enderecoFormGroup.markAsDirty();
    this.enderecoFormGroup.markAsTouched();
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
      console.log('❌ Erros em dados gerais:', this.fornecedorForm.get('dadosGerais')?.errors);
    }

    // Validate endereco (optional but if present must be valid)
    if (this.enderecoFormGroup.invalid) {
      isValid = false;
      validationErrors.push('Endereço contém erros');
    }

    // Validate geographic selection if endereco is provided
    const hasEndereco = this.enderecoFormGroup.get('logradouro')?.value;
    if (hasEndereco && !this.validateGeographicSelection()) {
      isValid = false;
      validationErrors.push('Seleção geográfica inválida');
    }

    // Validate pontos distribuicao
    const pontosArray = this.pontosDistribuicaoFormArray;
    for (let i = 0; i < pontosArray.length; i++) {
      const pontoGroup = pontosArray.at(i);
      if (pontoGroup.invalid) {
        isValid = false;
        validationErrors.push(`Ponto de distribuição ${i + 1} contém erros`);
      }
    }

    // Validate usuario master
    if (this.usuarioMasterFormGroup.invalid) {
      isValid = false;
      validationErrors.push('Usuário master contém erros');
    }

    if (!isValid) {
      console.log('❌ Erros de validação encontrados:', validationErrors);
    }

    return isValid;
  }

  /**
   * Navigate to first tab with validation errors
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

    // Check usuario master (tab 3)
    if (this.usuarioMasterFormGroup.invalid) {
      this.activeTabIndex.set(3);
      return;
    }
  }

  /**
   * Handle Município selection change
   */
  onMunicipioSelectionChange(event: any): void {
    const municipioId = event.value;
    console.log('🏘️ Município selecionado:', municipioId);

    // Update cidade field with municipio name for backward compatibility
    if (municipioId) {
      const municipio = this.availableMunicipios().find(m => m.id === municipioId);
      if (municipio) {
        this.enderecoFormGroup.patchValue({ cidade: municipio.nome });
      }
    } else {
      this.enderecoFormGroup.patchValue({ cidade: '' });
    }

    // Mark form as dirty
    this.enderecoFormGroup.markAsDirty();
    this.enderecoFormGroup.markAsTouched();
  }

  /**
   * Check if form can be saved
   */
  canSave(): boolean {
    return this.fornecedorForm.valid && !this.saving();
  }

  /**
   * Check if form has been modified
   */
  isFormDirty(): boolean {
    return this.fornecedorForm.dirty;
  }

  /**
   * Reset form to initial state
   */
  resetForm(): void {
    this.fornecedorForm.reset();
    this.selectedGeographicData.set(null);
    this.availableMunicipios.set([]);
    this.activeTabIndex.set(0);
  }



  /**
   * Get page title based on mode
   */
  getPageTitle(): string {
    return this.isEditMode() ? 'Editar Fornecedor' : 'Novo Fornecedor';
  }

  /**
   * Get save button text based on mode
   */
  getSaveButtonText(): string {
    if (this.saving()) {
      return this.isEditMode() ? 'Atualizando...' : 'Criando...';
    }
    return this.isEditMode() ? 'Atualizar' : 'Criar';
  }
}