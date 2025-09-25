import { Component, OnInit, OnDestroy, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subject, takeUntil } from 'rxjs';

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
import { ProdutorService } from '../services/produtor.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ValidationService } from '../../../shared/services/validation.service';
import { Produtor, ProdutorForm, TipoCliente } from '../../../shared/models/produtor.model';
import { ProdutorFormControls, ProdutorDadosGeraisFormControls, EnderecoFormControls, PropriedadeFormControls, UsuarioFormControls, CulturaFormControls } from '../../../shared/models/forms.model';
import { TipoCultura } from '../../../shared/models/cultura.model';

// Validators
import { conditionalCpfCnpjValidator, getCpfCnpjMask } from '../../../shared/utils/document-validators.util';
import { phoneValidator, emailValidator, nameValidator, areaValidator, yearValidator, cepValidator } from '../../../shared/utils/field-validators.util';

// Shared components
import { EnderecoFormComponent } from '../../../shared/components/endereco-form.component';
import { CoordenadasMapComponent } from '../../../shared/components/coordenadas-map.component';

// Feature components
import { PropriedadeFormComponent } from './propriedade-form.component';
import { CulturaFormComponent } from './cultura-form.component';

/**
 * Produtor Detail Component with tab navigation
 * Handles creation and editing of produtor records with complete form validation
 */
@Component({
  selector: 'app-produtor-detail',
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
    EnderecoFormComponent,
    CoordenadasMapComponent,
    // Feature components
    PropriedadeFormComponent,
    CulturaFormComponent
  ],
  providers: [ConfirmationService],
  templateUrl: './produtor-detail.component.html',
  styleUrls: ['./produtor-detail.component.scss']
})
export class ProdutorDetailComponent implements OnInit, OnDestroy {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private produtorService = inject(ProdutorService);
  private notificationService = inject(NotificationService);
  private validationService = inject(ValidationService);
  
  // Subject for managing subscriptions
  private destroy$ = new Subject<void>();

  // Signals for reactive state management
  loading = signal(false);
  saving = signal(false);
  produtorId = signal<number | null>(null);
  isEditMode = computed(() => this.produtorId() !== null);
  activeTabIndex = signal(0);

  // Form and validation
  produtorForm!: FormGroup<ProdutorFormControls>;
  
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

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Initialize reactive form with validation
   */
  private initializeForm(): void {
    this.produtorForm = this.fb.group<ProdutorFormControls>({
      dadosGerais: this.fb.group<ProdutorDadosGeraisFormControls>({
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
        telefone1: this.fb.control('', {
          nonNullable: true,
          validators: [phoneValidator()]
        }),
        telefone2: this.fb.control('', {
          nonNullable: true,
          validators: [phoneValidator()]
        }),
        telefone3: this.fb.control('', {
          nonNullable: true,
          validators: [phoneValidator()]
        }),
        email: this.fb.control('', {
          nonNullable: true,
          validators: [emailValidator()]
        })
      }),
      enderecos: this.fb.array<FormGroup<EnderecoFormControls>>([]),
      propriedades: this.fb.array<FormGroup<PropriedadeFormControls>>([]),
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
    this.produtorForm.get('dadosGerais.tipoCliente')?.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe(() => {
        this.updateCpfCnpjValidation();
      });
  }

  /**
   * Load route data and determine if editing existing produtor
   */
  private loadRouteData(): void {
    this.route.params
      .pipe(takeUntilDestroyed())
      .subscribe(params => {
        const id = params['id'];
        if (id && id !== 'novo') {
          this.produtorId.set(+id);
          this.loadProdutor(+id);
        }
      });
  }

  /**
   * Load produtor data for editing
   */
  private loadProdutor(id: number): void {
    this.loading.set(true);
    
    this.produtorService.getById(id)
      .pipe(takeUntilDestroyed())
      .subscribe({
        next: (produtor) => {
          this.populateForm(produtor);
          this.loading.set(false);
        },
        error: (error) => {
          this.notificationService.showCustomError('Erro ao carregar produtor: ' + error.message);
          this.loading.set(false);
          this.router.navigate(['/produtores']);
        }
      });
  }

  /**
   * Populate form with produtor data
   */
  private populateForm(produtor: Produtor): void {
    this.produtorForm.patchValue({
      dadosGerais: {
        nome: produtor.nome,
        cpfCnpj: produtor.cpfCnpj,
        tipoCliente: produtor.tipoCliente,
        telefone1: produtor.telefone1 || '',
        telefone2: produtor.telefone2 || '',
        telefone3: produtor.telefone3 || '',
        email: produtor.email || ''
      }
    });

    // Populate enderecos
    if (produtor.enderecos && produtor.enderecos.length > 0) {
      const enderecosArray = this.enderecosFormArray;
      enderecosArray.clear();
      produtor.enderecos.forEach(endereco => {
        const enderecoGroup = this.createEnderecoFormGroup();
        enderecoGroup.patchValue(endereco);
        enderecosArray.push(enderecoGroup);
      });
    }

    // Populate propriedades
    if (produtor.propriedades && produtor.propriedades.length > 0) {
      const propriedadesArray = this.propriedadesFormArray;
      propriedadesArray.clear();
      produtor.propriedades.forEach(propriedade => {
        const propriedadeGroup = this.createPropriedadeFormGroup();
        
        // Set basic propriedade data
        propriedadeGroup.patchValue({
          nome: propriedade.nome,
          area: propriedade.area,
          latitude: propriedade.latitude,
          longitude: propriedade.longitude
        });

        // Populate culturas for this propriedade
        if (propriedade.culturas && propriedade.culturas.length > 0) {
          const culturasArray = propriedadeGroup.get('culturas') as FormArray;
          culturasArray.clear();
          propriedade.culturas.forEach(cultura => {
            const culturaGroup = this.createCulturaFormGroup();
            culturaGroup.patchValue(cultura);
            culturasArray.push(culturaGroup);
          });
        }

        propriedadesArray.push(propriedadeGroup);
      });
    }

    // Populate usuarioMaster
    if (produtor.usuarioMaster) {
      this.produtorForm.get('usuarioMaster')?.patchValue({
        nome: produtor.usuarioMaster.nome,
        email: produtor.usuarioMaster.email,
        senha: produtor.usuarioMaster.senha,
        telefone: produtor.usuarioMaster.telefone || ''
      });
    }
  }

  /**
   * Update CPF/CNPJ validation based on tipo cliente
   */
  private updateCpfCnpjValidation(): void {
    const cpfCnpjControl = this.produtorForm.get('dadosGerais.cpfCnpj');
    if (cpfCnpjControl) {
      cpfCnpjControl.setValidators([Validators.required, conditionalCpfCnpjValidator('tipoCliente')]);
      cpfCnpjControl.updateValueAndValidity();
    }
  }

  /**
   * Get form control for validation display
   */
  getFormControl(path: string) {
    return this.produtorForm.get(path);
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
    const tipoCliente = this.produtorForm.get('dadosGerais.tipoCliente')?.value;
    return getCpfCnpjMask(tipoCliente);
  }

  /**
   * Handle tab change
   */
  onTabChange(value: string | number): void {
    this.activeTabIndex.set(typeof value === 'string' ? parseInt(value) : value);
  }



  /**
   * Save produtor
   */
  onSave(): void {
    // Mark all form sections as touched to show validation errors
    this.validationService.markFormGroupTouched(this.produtorForm);

    // Validate all form sections
    if (!this.validateAllSections()) {
      this.notificationService.showValidationWarning('Por favor, corrija os erros no formulário antes de salvar');
      
      // Navigate to first tab with errors
      this.navigateToFirstErrorTab();
      return;
    }

    this.saving.set(true);
    const formValue = this.produtorForm.value;
    
    // Transform form data to API format with proper validation
    const produtorData: ProdutorForm = {
      codigo: '', // Will be generated by backend
      nome: formValue.dadosGerais?.nome?.trim() || '',
      cpfCnpj: formValue.dadosGerais?.cpfCnpj?.replace(/\D/g, '') || '', // Remove formatting
      tipoCliente: formValue.dadosGerais?.tipoCliente || TipoCliente.PF,
      telefone1: formValue.dadosGerais?.telefone1?.replace(/\D/g, '') || undefined, // Remove formatting
      telefone2: formValue.dadosGerais?.telefone2?.replace(/\D/g, '') || undefined, // Remove formatting
      telefone3: formValue.dadosGerais?.telefone3?.replace(/\D/g, '') || undefined, // Remove formatting
      email: formValue.dadosGerais?.email?.trim() || undefined,
      enderecos: (formValue.enderecos || [])
        .filter(endereco => endereco?.logradouro && endereco?.cidade) // Only include valid enderecos
        .map(endereco => ({
          logradouro: endereco.logradouro?.trim() || '',
          numero: endereco.numero?.trim() || '',
          complemento: endereco.complemento?.trim() || '',
          bairro: endereco.bairro?.trim() || '',
          cidade: endereco.cidade?.trim() || '',
          uf: endereco.uf || '',
          cep: endereco.cep?.replace(/\D/g, '') || '', // Remove formatting
          latitude: endereco.latitude || undefined,
          longitude: endereco.longitude || undefined
        })),
      propriedades: (formValue.propriedades || [])
        .filter(propriedade => propriedade?.nome && propriedade?.area && propriedade.area > 0) // Only include valid propriedades
        .map(propriedade => ({
          nome: propriedade.nome?.trim() || '',
          area: propriedade.area || 0,
          latitude: propriedade.latitude || undefined,
          longitude: propriedade.longitude || undefined,
          culturas: (propriedade.culturas || [])
            .filter(cultura => cultura?.tipo && cultura?.anoSafra && cultura?.areaCultivada && cultura.areaCultivada > 0) // Only include valid culturas
            .map(cultura => ({
              tipo: cultura.tipo || TipoCultura.SOJA,
              anoSafra: cultura.anoSafra || new Date().getFullYear(),
              areaCultivada: cultura.areaCultivada || 0
            }))
        })),
      usuarioMaster: formValue.usuarioMaster && formValue.usuarioMaster.nome && formValue.usuarioMaster.email ? {
        nome: formValue.usuarioMaster.nome.trim(),
        email: formValue.usuarioMaster.email.trim(),
        senha: formValue.usuarioMaster.senha || '',
        telefone: formValue.usuarioMaster.telefone?.replace(/\D/g, '') || ''
      } : undefined
    };

    // Final validation before sending to API
    if (!produtorData.nome || !produtorData.cpfCnpj || produtorData.enderecos.length === 0 || 
        produtorData.propriedades.length === 0 || !produtorData.usuarioMaster) {
      this.notificationService.showValidationWarning('Dados obrigatórios estão faltando. Verifique todos os campos.');
      this.saving.set(false);
      return;
    }

    const saveOperation = this.isEditMode() 
      ? this.produtorService.update(this.produtorId()!, produtorData)
      : this.produtorService.createComplete(produtorData);

    saveOperation
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.notificationService.showCustomSuccess(
            this.isEditMode() ? 'Produtor atualizado com sucesso' : 'Produtor criado com sucesso'
          );
          this.saving.set(false);
          this.router.navigate(['/produtores']);
        },
        error: (error) => {
          this.notificationService.showCustomError('Erro ao salvar produtor: ' + error.message);
          this.saving.set(false);
        }
      });
  }

  /**
   * Cancel and return to list
   */
  onCancel(): void {
    this.router.navigate(['/produtores']);
  }

  /**
   * Get enderecos form array
   */
  get enderecosFormArray(): FormArray<FormGroup<EnderecoFormControls>> {
    return this.produtorForm.get('enderecos') as FormArray<FormGroup<EnderecoFormControls>>;
  }

  /**
   * Get propriedades form array
   */
  get propriedadesFormArray(): FormArray<FormGroup<PropriedadeFormControls>> {
    return this.produtorForm.get('propriedades') as FormArray<FormGroup<PropriedadeFormControls>>;
  }

  /**
   * Get usuario master form group
   */
  get usuarioMasterFormGroup(): FormGroup<UsuarioFormControls> {
    return this.produtorForm.get('usuarioMaster') as FormGroup<UsuarioFormControls>;
  }

  /**
   * Create endereco form group
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
      complemento: this.fb.control('', { nonNullable: true }),
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
        validators: [Validators.required, cepValidator()]
      }),
      latitude: this.fb.control<number | null>(null),
      longitude: this.fb.control<number | null>(null)
    });
  }

  /**
   * Create propriedade form group
   */
  private createPropriedadeFormGroup(): FormGroup<PropriedadeFormControls> {
    return this.fb.group<PropriedadeFormControls>({
      nome: this.fb.control('', {
        nonNullable: true,
        validators: [Validators.required, nameValidator(2)]
      }),
      area: this.fb.control(0, {
        nonNullable: true,
        validators: [Validators.required, areaValidator()]
      }),
      latitude: this.fb.control<number | null>(null),
      longitude: this.fb.control<number | null>(null),
      culturas: this.fb.array<FormGroup<CulturaFormControls>>([])
    });
  }

  /**
   * Create cultura form group
   */
  private createCulturaFormGroup(): FormGroup<CulturaFormControls> {
    return this.fb.group<CulturaFormControls>({
      tipo: this.fb.control(TipoCultura.SOJA, {
        nonNullable: true,
        validators: [Validators.required]
      }),
      anoSafra: this.fb.control(new Date().getFullYear(), {
        nonNullable: true,
        validators: [Validators.required, yearValidator(2000, new Date().getFullYear() + 5)]
      }),
      areaCultivada: this.fb.control(0, {
        nonNullable: true,
        validators: [Validators.required, areaValidator()]
      })
    });
  }

  /**
   * Handle endereco changes
   */
  onEnderecosChange(): void {
    // Mark form as dirty to enable save button and trigger validation
    this.enderecosFormArray.markAsDirty();
    this.enderecosFormArray.markAsTouched();
  }

  /**
   * Handle propriedade changes
   */
  onPropriedadesChange(): void {
    // Mark form as dirty to enable save button and trigger validation
    this.propriedadesFormArray.markAsDirty();
    this.propriedadesFormArray.markAsTouched();
  }

  /**
   * Handle usuario master changes
   */
  onUsuarioMasterChange(): void {
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
   * Handle coordinates selection for main produtor location
   */
  onCoordinatesSelected(coordenadas: { latitude: number | null; longitude: number | null }): void {
    // Store main coordinates - could be used for general produtor location
    // For now, we'll store it in a separate property or use it for the first property
    if (this.propriedadesFormArray.length > 0) {
      const firstProperty = this.propriedadesFormArray.at(0);
      firstProperty.patchValue({
        latitude: coordenadas.latitude,
        longitude: coordenadas.longitude
      });
      this.propriedadesFormArray.markAsDirty();
    }
  }

  /**
   * Handle culturas changes
   */
  onCulturasChange(): void {
    // Mark propriedades as dirty since culturas are nested within propriedades
    this.propriedadesFormArray.markAsDirty();
    this.propriedadesFormArray.markAsTouched();
  }

  /**
   * Validate all form sections
   */
  private validateAllSections(): boolean {
    let isValid = true;
    const validationErrors: string[] = [];

    // Validate main form (dados gerais)
    if (this.produtorForm.get('dadosGerais')?.invalid) {
      isValid = false;
      validationErrors.push('Dados gerais contêm erros');
    }

    // Validate enderecos (at least one is required)
    if (this.enderecosFormArray.length === 0) {
      isValid = false;
      validationErrors.push('Pelo menos um endereço é obrigatório');
    } else {
      let hasValidEndereco = false;
      this.enderecosFormArray.controls.forEach((control, index) => {
        if (control.valid) {
          hasValidEndereco = true;
        } else if (control.invalid) {
          isValid = false;
          validationErrors.push(`Endereço ${index + 1} contém erros`);
        }
      });
      
      if (!hasValidEndereco) {
        isValid = false;
        validationErrors.push('Pelo menos um endereço válido é obrigatório');
      }
    }

    // Validate propriedades (at least one is required)
    if (this.propriedadesFormArray.length === 0) {
      isValid = false;
      validationErrors.push('Pelo menos uma propriedade é obrigatória');
    } else {
      let hasValidPropriedade = false;
      this.propriedadesFormArray.controls.forEach((propriedadeControl, propIndex) => {
        if (propriedadeControl.get('nome')?.valid && propriedadeControl.get('area')?.valid) {
          hasValidPropriedade = true;
        }
        
        if (propriedadeControl.invalid) {
          isValid = false;
          validationErrors.push(`Propriedade ${propIndex + 1} contém erros`);
        }
        
        // Validate culturas within each propriedade (optional but if present must be valid)
        const culturasArray = propriedadeControl.get('culturas') as FormArray;
        if (culturasArray && culturasArray.length > 0) {
          culturasArray.controls.forEach((culturaControl, cultIndex) => {
            if (culturaControl.invalid) {
              isValid = false;
              validationErrors.push(`Cultura ${cultIndex + 1} da propriedade ${propIndex + 1} contém erros`);
            }
          });
        }
      });
      
      if (!hasValidPropriedade) {
        isValid = false;
        validationErrors.push('Pelo menos uma propriedade válida é obrigatória');
      }
    }

    // Validate usuario master (required)
    if (this.usuarioMasterFormGroup.invalid) {
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
    if (this.produtorForm.get('dadosGerais')?.invalid) {
      this.activeTabIndex.set(0);
      return;
    }

    // Check enderecos (tab 1)
    if (this.enderecosFormArray.length === 0 || this.enderecosFormArray.invalid) {
      this.activeTabIndex.set(1);
      return;
    }

    // Check propriedades (tab 2)
    if (this.propriedadesFormArray.length === 0 || this.propriedadesFormArray.invalid) {
      this.activeTabIndex.set(2);
      return;
    }

    // Check culturas (tab 3) - validate culturas within propriedades
    let hasCulturaErrors = false;
    this.propriedadesFormArray.controls.forEach(propriedadeControl => {
      const culturasArray = propriedadeControl.get('culturas') as FormArray;
      if (culturasArray && culturasArray.invalid) {
        hasCulturaErrors = true;
      }
    });
    if (hasCulturaErrors) {
      this.activeTabIndex.set(3);
      return;
    }

    // Check usuario master (tab 5)
    if (this.usuarioMasterFormGroup.invalid) {
      this.activeTabIndex.set(5);
      return;
    }

    // Default to first tab if no specific errors found
    this.activeTabIndex.set(0);
  }
}