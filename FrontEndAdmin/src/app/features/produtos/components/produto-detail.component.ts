import { Component, OnInit, OnDestroy, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subject, takeUntil } from 'rxjs';

// PrimeNG imports
import { TabsModule } from 'primeng/tabs';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectModule } from 'primeng/select';
import { MultiSelectModule } from 'primeng/multiselect';
import { TextareaModule } from 'primeng/textarea';
import { CheckboxModule } from 'primeng/checkbox';
import { ToastModule } from 'primeng/toast';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { BreadcrumbModule } from 'primeng/breadcrumb';
import { ConfirmationService } from 'primeng/api';

// Services and models
import { ProdutoService, DropdownOption, ProdutoDisplayDto } from '../services/produto.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ValidationService } from '../../../shared/services/validation.service';

import { CriarProdutoDto, AtualizarProdutoDto, TipoProduto, TipoCalculoPeso } from '../../../shared/models/produto.model';

// Shared components
import { FieldErrorComponent } from '../../../shared/components/field-error.component';

/**
 * Produto Detail Component with tab navigation
 * Handles creation and editing of produto records with complete form validation
 */
@Component({
  selector: 'app-produto-detail',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TabsModule,
    CardModule,
    ButtonModule,
    InputTextModule,
    InputNumberModule,
    SelectModule,
    MultiSelectModule,
    TextareaModule,
    CheckboxModule,
    ToastModule,
    ProgressSpinnerModule,
    ConfirmDialogModule,
    BreadcrumbModule,
    // Shared components
    FieldErrorComponent
  ],
  providers: [ConfirmationService],
  templateUrl: './produto-detail.component.html',
  styleUrls: ['./produto-detail.component.scss']
})
export class ProdutoDetailComponent implements OnInit, OnDestroy {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private produtoService = inject(ProdutoService);
  private notificationService = inject(NotificationService);
  private validationService = inject(ValidationService);


  // Subject for managing subscriptions
  private destroy$ = new Subject<void>();

  // Signals for reactive state management
  loading = signal(false);
  saving = signal(false);
  produtoId = signal<number | null>(null);
  isEditMode = computed(() => this.produtoId() !== null);
  activeTabIndex = signal(0);

  // Form and validation
  produtoForm!: FormGroup;

  // Reference data signals
  unidadesMedida = signal<DropdownOption[]>([]);
  embalagensDisponiveis = signal<DropdownOption[]>([]);
  categorias = signal<DropdownOption[]>([]);
  atividadesAgropecuarias = signal<DropdownOption[]>([]);
  fornecedores = signal<DropdownOption[]>([]);
  culturas = signal<DropdownOption[]>([]);
  produtosPais = signal<DropdownOption[]>([]);

  // Loading states
  loadingUnidades = signal<boolean>(false);
  loadingEmbalagens = signal<boolean>(false);
  loadingCategorias = signal<boolean>(false);
  loadingAtividades = signal<boolean>(false);
  loadingFornecedores = signal<boolean>(false);
  loadingCulturas = signal<boolean>(false);
  loadingProdutosPais = signal<boolean>(false);

  // Cache flags to avoid repeated API calls
  private unidadesCarregadas = false;
  private categoriasCarregadas = false;
  private atividadesCarregadas = false;
  private fornecedoresCarregados = false;
  private culturasCarregadas = false;
  private produtosPaisCarregados = false;

  // Error states
  errorCategorias = signal<string | null>(null);

  // Computed signal for p-select format
  categoriasParaSelect = computed(() => 
    this.categorias().map(cat => ({
      label: cat.nome,
      value: cat.id
    }))
  );

  // Enum options for dropdowns
  tiposProduto = [
    { label: 'Semente', value: TipoProduto.Semente },
    { label: 'Defensivo', value: TipoProduto.Defensivo },
    { label: 'Fertilizante', value: TipoProduto.Fertilizante },
    { label: 'Inoculante', value: TipoProduto.Inoculante },
    { label: 'Adjuvante', value: TipoProduto.Adjuvante },
    { label: 'Equipamento', value: TipoProduto.Equipamento },
    { label: 'Serviço', value: TipoProduto.Servico },
    { label: 'Outro', value: TipoProduto.Outro }
  ];

  tiposCalculoPeso = [
    { label: 'Peso Nominal', value: TipoCalculoPeso.PesoNominal },
    { label: 'Peso Cubado', value: TipoCalculoPeso.PesoCubado },
    { label: 'Maior Peso', value: TipoCalculoPeso.MaiorPeso }
  ];

  constructor() {
    this.initializeForm();
    this.loadRouteData();
  }

  ngOnInit(): void {
    this.carregarDadosReferencia();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Initialize reactive form with validation
   */
  private initializeForm(): void {
    this.produtoForm = this.fb.group({
      codigo: ['', [Validators.required, Validators.maxLength(20)]],
      nome: ['', [Validators.required, Validators.maxLength(200)]],
      descricao: ['', [Validators.maxLength(1000)]],
      marca: ['', [Validators.maxLength(100)]],
      tipo: [0, [Validators.required]], // TipoProduto.Semente como padrão
      categoriaId: [null, []],
      unidadeMedidaId: [null, [Validators.required]],
      embalagemId: [null],
      quantidadeEmbalagem: [1.0, [Validators.min(0.01)]],
      atividadeAgropecuariaId: [null],
      tipoCalculoPeso: [0, []], // TipoCalculoPeso.PesoNominal como padrão
      produtoRestrito: [false],
      observacoesRestricao: [''],
      fornecedorId: [null, []],
      produtoPaiId: [null],
      culturasIds: [[]],
      codigoCda: [''],
      // Dimensões obrigatórias
      dimensoes: this.fb.group({
        altura: [0, [Validators.required, Validators.min(0)]],
        largura: [0, [Validators.required, Validators.min(0)]],
        comprimento: [0, [Validators.required, Validators.min(0)]],
        pesoNominal: [0, [Validators.required, Validators.min(0)]],
        pesoEmbalagem: [0, [Validators.min(0)]],
        pms: [null],
        quantidadeMinima: [1, [Validators.min(1)]],
        embalagem: ['', [Validators.maxLength(100)]],
        faixaDensidadeInicial: [null],
        faixaDensidadeFinal: [null]
      }),
      ativo: [true]
    });
  }

  /**
   * Load route data and determine if editing existing produto
   */
  private loadRouteData(): void {
    this.route.params
      .pipe(takeUntilDestroyed())
      .subscribe(params => {
        const id = params['id'];
        if (id && id !== 'novo') {
          this.produtoId.set(+id);
          this.loadProduto(+id);
        }
      });
  }

  /**
   * Load produto data for editing
   */
  private loadProduto(id: number): void {
    this.loading.set(true);

    this.produtoService.obterPorId(id)
      .pipe(takeUntilDestroyed())
      .subscribe({
        next: (produto) => {
          this.populateForm(produto);
          this.loading.set(false);
        },
        error: (error) => {
          this.notificationService.showCustomError('Erro ao carregar produto: ' + error.message);
          this.loading.set(false);
          this.router.navigate(['/produtos']);
        }
      });
  }

  /**
   * Populate form with produto data
   */
  private populateForm(item: any): void {
    // Use patchValue to set all matching form controls, including nested 'dimensoes'
    this.produtoForm.patchValue(item);

    // Load embalagens for selected unidade de medida
    if (item.unidadeMedidaId) {
      this.carregarEmbalagens(item.unidadeMedidaId);
    }
  }

  /**
   * Load all reference data needed for the form
   */
  private carregarDadosReferencia(): void {
    this.carregarUnidadesMedida();
    this.carregarCategorias();
    this.carregarAtividadesAgropecuarias();
    this.carregarFornecedores();
    this.carregarCulturas();
    this.carregarProdutosPais();
  }

  /**
   * Load unidades de medida
   */
  private carregarUnidadesMedida(): void {
    if (this.unidadesCarregadas) return;

    this.loadingUnidades.set(true);

    this.produtoService.obterUnidadesMedida().subscribe({
      next: (unidades) => {
        this.unidadesMedida.set(unidades);
        this.unidadesCarregadas = true;
        this.loadingUnidades.set(false);
      },
      error: (error) => {
        console.error('Erro ao carregar unidades de medida:', error);
        this.loadingUnidades.set(false);
      }
    });
  }

  /**
   * Load categorias for p-select
   */
  private carregarCategorias(): void {
    if (this.categoriasCarregadas) return;

    this.loadingCategorias.set(true);
    this.errorCategorias.set(null);

    this.produtoService.obterCategorias().subscribe({
      next: (categorias) => {
        // Convert to dropdown options
        const dropdownOptions: DropdownOption[] = categorias.map(cat => ({
          id: cat.id,
          nome: cat.nome,
          ativo: cat.ativo
        }));

        this.categorias.set(dropdownOptions);
        this.categoriasCarregadas = true;
        this.loadingCategorias.set(false);
      },
      error: (error) => {
        console.error('Erro ao carregar categorias:', error);
        this.errorCategorias.set('Erro ao carregar categorias');
        this.loadingCategorias.set(false);
      }
    });
  }

  /**
   * Load atividades agropecuarias
   */
  private carregarAtividadesAgropecuarias(): void {
    if (this.atividadesCarregadas) return;

    this.loadingAtividades.set(true);

    this.produtoService.obterAtividadesAgropecuarias().subscribe({
      next: (atividades) => {
        this.atividadesAgropecuarias.set(atividades);
        this.atividadesCarregadas = true;
        this.loadingAtividades.set(false);
      },
      error: (error) => {
        console.error('Erro ao carregar atividades agropecuárias:', error);
        this.loadingAtividades.set(false);
      }
    });
  }

  /**
   * Load embalagens filtered by unidade de medida
   */
  private carregarEmbalagens(unidadeMedidaId?: number): void {
    this.loadingEmbalagens.set(true);

    this.produtoService.obterEmbalagensPorUnidade(unidadeMedidaId).subscribe({
      next: (embalagens) => {
        this.embalagensDisponiveis.set(embalagens);
        this.loadingEmbalagens.set(false);
      },
      error: (error) => {
        console.error('Erro ao carregar embalagens:', error);
        this.loadingEmbalagens.set(false);
      }
    });
  }

  /**
   * Load fornecedores
   */
  private carregarFornecedores(): void {
    if (this.fornecedoresCarregados) return;

    this.loadingFornecedores.set(true);

    this.produtoService.obterFornecedores().subscribe({
      next: (fornecedores) => {
        this.fornecedores.set(fornecedores);
        this.fornecedoresCarregados = true;
        this.loadingFornecedores.set(false);
      },
      error: (error) => {
        console.error('Erro ao carregar fornecedores:', error);
        this.loadingFornecedores.set(false);
      }
    });
  }

  /**
   * Load culturas
   */
  private carregarCulturas(): void {
    if (this.culturasCarregadas) return;

    this.loadingCulturas.set(true);

    this.produtoService.obterCulturas().subscribe({
      next: (culturas) => {
        this.culturas.set(culturas);
        this.culturasCarregadas = true;
        this.loadingCulturas.set(false);
      },
      error: (error) => {
        console.error('Erro ao carregar culturas:', error);
        this.loadingCulturas.set(false);
      }
    });
  }

  /**
   * Load produtos pais
   */
  private carregarProdutosPais(): void {
    if (this.produtosPaisCarregados) return;

    this.loadingProdutosPais.set(true);

    this.produtoService.obterProdutosPais().subscribe({
      next: (produtos) => {
        this.produtosPais.set(produtos);
        this.produtosPaisCarregados = true;
        this.loadingProdutosPais.set(false);
      },
      error: (error) => {
        console.error('Erro ao carregar produtos pais:', error);
        this.loadingProdutosPais.set(false);
      }
    });
  }



  /**
   * Handle unidade de medida change
   */
  onUnidadeMedidaChange(event: any): void {
    const unidadeMedidaId = event.value;

    // Clear embalagem selection
    this.produtoForm.patchValue({ embalagemId: null });

    // Load embalagens for selected unidade
    if (unidadeMedidaId) {
      this.carregarEmbalagens(unidadeMedidaId);
    } else {
      this.embalagensDisponiveis.set([]);
    }
  }

  /**
   * Handle tab change
   */
  onTabChange(value: string | number): void {
    this.activeTabIndex.set(typeof value === 'string' ? parseInt(value) : value);
  }

  /**
   * Get validation error message for form control
   */
  getErrorMessage(controlName: string): string {
    const control = this.produtoForm.get(controlName);
    if (!control) return '';
    return this.validationService.getErrorMessage(control, controlName);
  }

  /**
   * Check if form control should show error
   */
  shouldShowError(controlName: string): boolean {
    const control = this.produtoForm.get(controlName);
    if (!control) return false;
    return this.validationService.shouldShowError(control);
  }

  /**
   * Save produto
   */
  onSave(): void {
    // Mark all form sections as touched to show validation errors
    this.validationService.markFormGroupTouched(this.produtoForm);

    if (!this.produtoForm.valid) {
      this.notificationService.showValidationWarning('Por favor, corrija os erros no formulário antes de salvar');
      this.navigateToFirstErrorTab();
      return;
    }

    this.saving.set(true);
    const formValue = this.produtoForm.value;

    const produtoData = this.isEditMode()
      ? this.mapToUpdateDto(formValue)
      : this.mapToCreateDto(formValue);

    const saveOperation = this.isEditMode()
      ? this.produtoService.atualizar(this.produtoId()!, produtoData as AtualizarProdutoDto)
      : this.produtoService.criar(produtoData as CriarProdutoDto);

    saveOperation
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.notificationService.showCustomSuccess(
            this.isEditMode() ? 'Produto atualizado com sucesso' : 'Produto criado com sucesso'
          );
          this.saving.set(false);
          this.router.navigate(['/produtos']);
        },
        error: (error) => {
          this.notificationService.showCustomError('Erro ao salvar produto: ' + error.message);
          this.saving.set(false);
        }
      });
  }

  /**
   * Map form value to create DTO
   */
  private mapToCreateDto(formValue: any): CriarProdutoDto {
    return {
      nome: formValue.nome,
      descricao: formValue.descricao || undefined,
      codigo: formValue.codigo,
      marca: formValue.marca || undefined,
      tipo: formValue.tipo,
      unidadeMedidaId: formValue.unidadeMedidaId,
      embalagemId: formValue.embalagemId || undefined,
      quantidadeEmbalagem: formValue.quantidadeEmbalagem || 1.0,
      atividadeAgropecuariaId: formValue.atividadeAgropecuariaId || undefined,
      tipoCalculoPeso: formValue.tipoCalculoPeso,
      produtoRestrito: formValue.produtoRestrito,
      observacoesRestricao: formValue.observacoesRestricao || undefined,
      categoriaId: formValue.categoriaId,
      fornecedorId: formValue.fornecedorId,
      produtoPaiId: formValue.produtoPaiId || undefined,
      dimensoes: formValue.dimensoes,
      culturasIds: formValue.culturasIds || []
    };
  }

  /**
   * Map form value to update DTO
   */
  private mapToUpdateDto(formValue: any): AtualizarProdutoDto {
    return {
      nome: formValue.nome,
      descricao: formValue.descricao || undefined,
      codigo: formValue.codigo,
      marca: formValue.marca || undefined,
      unidadeMedidaId: formValue.unidadeMedidaId,
      embalagemId: formValue.embalagemId || undefined,
      quantidadeEmbalagem: formValue.quantidadeEmbalagem || 1.0,
      atividadeAgropecuariaId: formValue.atividadeAgropecuariaId || undefined,
      tipoCalculoPeso: formValue.tipoCalculoPeso,
      produtoRestrito: formValue.produtoRestrito,
      observacoesRestricao: formValue.observacoesRestricao || undefined,
      categoriaId: formValue.categoriaId,
      dimensoes: formValue.dimensoes,
      culturasIds: formValue.culturasIds || []
    };
  }

  /**
   * Cancel and return to list
   */
  onCancel(): void {
    this.router.navigate(['/produtos']);
  }

  /**
   * Navigate to the first tab that has validation errors
   */
  private navigateToFirstErrorTab(): void {
    // Check basic info (tab 0)
    const basicFields = ['codigo', 'nome', 'tipo', 'fornecedorId', 'categoriaId'];
    if (basicFields.some(field => this.produtoForm.get(field)?.invalid)) {
      this.activeTabIndex.set(0);
      return;
    }

    // Check packaging (tab 1)
    const packagingFields = ['unidadeMedidaId', 'quantidadeEmbalagem'];
    if (packagingFields.some(field => this.produtoForm.get(field)?.invalid)) {
      this.activeTabIndex.set(1);
      return;
    }

    // Check dimensions (tab 2)
    const dimensoesGroup = this.produtoForm.get('dimensoes');
    if (dimensoesGroup?.invalid) {
      this.activeTabIndex.set(2);
      return;
    }

    // Check configurations (tab 3)
    const configFields = ['tipoCalculoPeso'];
    if (configFields.some(field => this.produtoForm.get(field)?.invalid)) {
      this.activeTabIndex.set(3);
      return;
    }

    // Default to first tab
    this.activeTabIndex.set(0);
  }

  /**
   * Breadcrumb items for navigation
   */
  readonly breadcrumbItems = computed(() => [
    { label: 'Produtos', command: () => this.onCancel() },
    { label: this.isEditMode() ? 'Editar' : 'Novo' }
  ]);

  readonly breadcrumbHome = computed(() => ({
    icon: 'pi pi-home',
    command: () => this.onCancel()
  }));
}