import { Component, OnInit, inject, signal, ChangeDetectionStrategy, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { MultiSelectModule } from 'primeng/multiselect';

import { SelectModule } from 'primeng/select';
import { TextareaModule } from 'primeng/textarea';
import { CheckboxModule } from 'primeng/checkbox';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { DialogModule } from 'primeng/dialog';
import { FieldErrorComponent } from '../../shared/components/field-error.component';
import { HierarchicalSelectorComponent, HierarchicalItem } from '../../shared/components/hierarchical-selector/hierarchical-selector.component';
import { ReferenceCrudBaseComponent } from '../../shared/components/reference-crud-base/reference-crud-base.component';
import { ProdutoService, DropdownOption, ProdutoDisplayDto } from './services/produto.service';
import { ProdutoDto, CriarProdutoDto, AtualizarProdutoDto, TipoProduto, TipoCalculoPeso } from '../../shared/models/produto.model';
import { CategoriaDto } from '../../shared/models/reference.model';
import { ValidationService } from '../../shared/services/validation.service';
import { ApiValidationService } from '../../shared/services/api-validation.service';
import { produtoCodigoUniqueValidator, produtoNomeUniqueValidator, produtoReferencesValidator } from '../../shared/validators/async-validators';
import { TableColumn } from '../../shared/interfaces/component-template.interface';

/**
 * Produtos CRUD component with reference dropdowns
 */
@Component({
  selector: 'app-produtos',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    InputTextModule,
    InputNumberModule,

    SelectModule,
    TextareaModule,
    CheckboxModule,
    ButtonModule,
  MultiSelectModule,
    FieldErrorComponent,
    HierarchicalSelectorComponent,
    TableModule,
    ButtonModule,
    SelectModule,
    ConfirmDialogModule,
    ToastModule,
    ProgressSpinnerModule,
    TagModule,
    TooltipModule,
    DialogModule,
    CheckboxModule
  ],
  templateUrl: './produtos.component.html',
  styleUrls: ['./produtos.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProdutosComponent extends ReferenceCrudBaseComponent<ProdutoDisplayDto, CriarProdutoDto, AtualizarProdutoDto> implements OnInit {
  
  protected service = inject(ProdutoService) as any;
  protected validationService = inject(ValidationService);
  private apiValidationService = inject(ApiValidationService);

  // Reference data signals
  unidadesMedida = signal<DropdownOption[]>([]);
  embalagensDisponiveis = signal<DropdownOption[]>([]);
  categoriasHierarchicas = signal<HierarchicalItem[]>([]);
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

  // Error states
  errorCategorias = signal<string | null>(null);

  // Selected categoria for form control
  selectedCategoria = signal<HierarchicalItem | null>(null);

  // Filter properties
  filtroCategoria: number | null = null;
  filtroUnidadeMedida: number | null = null;
  filtroAtividadeAgropecuaria: number | null = null;
  filtroTexto: string = '';

  // Filter options
  categoriasParaFiltro = signal<DropdownOption[]>([]);
  unidadesParaFiltro = signal<DropdownOption[]>([]);
  atividadesParaFiltro = signal<DropdownOption[]>([]);

  // All items for filtering
  private todosItens: ProdutoDisplayDto[] = [];
  
  // Search debounce timer
  private searchTimer: any;

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

  protected readonly entityDisplayName = () => 'Produto';
  protected readonly entityDescription = () => 'Gerencie o catálogo de produtos agrícolas';
  protected readonly defaultSortField = () => 'nome';
  protected readonly searchFields = () => ['codigo', 'nome', 'categoriaNome', 'unidadeMedidaNome', 'atividadeAgropecuariaNome'];

  protected readonly displayColumns = computed<TableColumn[]>(() => [
    { field: 'codigo', header: 'Código', sortable: true, width: '120px', type: 'text' },
    { field: 'nome', header: 'Nome', sortable: true, type: 'text' },
    { field: 'categoriaNome', header: 'Categoria', sortable: true, hideOnMobile: true, type: 'text' },
    { field: 'unidadeMedidaSimbolo', header: 'Unidade', sortable: true, width: '100px', hideOnMobile: true, type: 'text' },
    //{ field: 'preco', header: 'Preço', sortable: true, width: '120px', type: 'custom', hideOnTablet: true },
    //{ field: 'ativo', header: 'Status', sortable: true, width: '100px', type: 'boolean' }
  ]);

  ngOnInit(): void {
    super.ngOnInit();
    this.carregarDadosReferencia();
  }

  /**
   * Ensure dialog opens when creating a new product
   */
  override novoItem(): void {
    super.novoItem();
    this.showForm.set(true);
  }

  /**
   * Ensure dialog opens when editing a product
   */
  override editarItem(item: ProdutoDisplayDto): void {
    super.editarItem(item);
    this.showForm.set(true);
  }

  // (no-op) `showForm` signal is inherited from ReferenceCrudBaseComponent

  protected createFormGroup(): FormGroup {
    const form = this.fb.group({
      codigo: ['', 
        [Validators.required, Validators.maxLength(20)],
        //[produtoCodigoUniqueValidator(this.apiValidationService, this.isEditMode() ? this.selectedItem()?.id : undefined)]
      ],
      nome: ['', 
        [Validators.required, Validators.maxLength(200)],
        //[produtoNomeUniqueValidator(this.apiValidationService, this.isEditMode() ? this.selectedItem()?.id : undefined)]
      ],
      descricao: ['', [Validators.maxLength(1000)]],
      marca: ['', [Validators.maxLength(100)]],
      tipo: [0, [Validators.required]], // TipoProduto.Semente como padrão
      categoriaId: [null, [Validators.required]],
      unidadeMedidaId: [null, [Validators.required]],
      embalagemId: [null],
      atividadeAgropecuariaId: [null],
      tipoCalculoPeso: [0, [Validators.required]], // TipoCalculoPeso.PesoNominal como padrão
      produtoRestrito: [false],
      observacoesRestricao: [''],
      fornecedorId: [null, [Validators.required]],
      produtoPaiId: [null],
      culturasIds: [[]],
      codigoCda: [''],
      // Dimensões obrigatórias
      dimensoes: this.fb.group({
        altura: [0, [Validators.required, Validators.min(0)]],
        largura: [0, [Validators.required, Validators.min(0)]],
        comprimento: [0, [Validators.required, Validators.min(0)]],
        pesoNominal: [0, [Validators.required, Validators.min(0)]],
        pesoEmbalagem: [0, [Validators.required, Validators.min(0)]],
        pms: [null],
        quantidadeMinima: [1, [Validators.required, Validators.min(1)]],
        embalagem: ['', [Validators.required, Validators.maxLength(100)]],
        faixaDensidadeInicial: [null],
        faixaDensidadeFinal: [null]
      }),
      ativo: [true]
    }, {
      asyncValidators: [produtoReferencesValidator(this.apiValidationService)]
    });

    return form;
  }

  protected mapToCreateDto(formValue: any): CriarProdutoDto {
    return {
      nome: formValue.nome,
      descricao: formValue.descricao || undefined,
      codigo: formValue.codigo,
      marca: formValue.marca || undefined,
      tipo: formValue.tipo,
      unidadeMedidaId: formValue.unidadeMedidaId,
      embalagemId: formValue.embalagemId || undefined,
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

  protected mapToUpdateDto(formValue: any): AtualizarProdutoDto {
    return {
      nome: formValue.nome,
      descricao: formValue.descricao || undefined,
      codigo: formValue.codigo,
      marca: formValue.marca || undefined,
      unidadeMedidaId: formValue.unidadeMedidaId,
      embalagemId: formValue.embalagemId || undefined,
      atividadeAgropecuariaId: formValue.atividadeAgropecuariaId || undefined,
      tipoCalculoPeso: formValue.tipoCalculoPeso,
      produtoRestrito: formValue.produtoRestrito,
      observacoesRestricao: formValue.observacoesRestricao || undefined,
      categoriaId: formValue.categoriaId,
      dimensoes: formValue.dimensoes,
      culturasIds: formValue.culturasIds || []
    };
  }



  protected populateForm(item: ProdutoDisplayDto): void {
    // Use patchValue to set all matching form controls, including nested 'dimensoes'
    this.form.patchValue(item);

    // Set selected categoria for hierarchical selector
    if (item.categoria) {
      this.selectedCategoria.set({
        id: item.categoria.id,
        nome: item.categoria.nome,
        parentId: item.categoria.categoriaPaiId
      });
    }

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
    this.loadingUnidades.set(true);
    
    this.service.obterUnidadesMedida().subscribe({
      next: (unidades) => {
        this.unidadesMedida.set(unidades);
        this.loadingUnidades.set(false);
      },
      error: (error) => {
        console.error('Erro ao carregar unidades de medida:', error);
        this.loadingUnidades.set(false);
      }
    });
  }

  /**
   * Load categorias for hierarchical selector
   */
  private carregarCategorias(): void {
    this.loadingCategorias.set(true);
    this.errorCategorias.set(null);
    
    this.service.obterCategorias().subscribe({
      next: (categorias) => {
        // Convert to hierarchical items
        const hierarchicalItems: HierarchicalItem[] = categorias.map(cat => ({
          id: cat.id,
          nome: cat.nome,
          parentId: cat.categoriaPaiId,
          ativo: cat.ativo
        }));
        
        this.categoriasHierarchicas.set(hierarchicalItems);
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
    this.loadingAtividades.set(true);
    
    this.service.obterAtividadesAgropecuarias().subscribe({
      next: (atividades) => {
        this.atividadesAgropecuarias.set(atividades);
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
    
    this.service.obterEmbalagensPorUnidade(unidadeMedidaId).subscribe({
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
    this.loadingFornecedores.set(true);
    
    this.service.obterFornecedores().subscribe({
      next: (fornecedores) => {
        this.fornecedores.set(fornecedores);
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
    this.loadingCulturas.set(true);
    
    this.service.obterCulturas().subscribe({
      next: (culturas) => {
        this.culturas.set(culturas);
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
    this.loadingProdutosPais.set(true);
    
    this.service.obterProdutosPais().subscribe({
      next: (produtos) => {
        this.produtosPais.set(produtos);
        this.loadingProdutosPais.set(false);
      },
      error: (error) => {
        console.error('Erro ao carregar produtos pais:', error);
        this.loadingProdutosPais.set(false);
      }
    });
  }

  /**
   * Handle categoria selection change
   */
  onCategoriaChange(event: any): void {
    const selectedItems = event.selectedItems;
    
    if (selectedItems && selectedItems.length > 0) {
      const categoria = selectedItems[0];
      this.form.patchValue({ categoriaId: categoria.id });
      this.selectedCategoria.set(categoria);
    } else {
      this.form.patchValue({ categoriaId: null });
      this.selectedCategoria.set(null);
    }
  }

  /**
   * Handle unidade de medida change
   */
  onUnidadeMedidaChange(event: any): void {
    const unidadeMedidaId = event.value;
    
    // Clear embalagem selection
    this.form.patchValue({ embalagemId: null });
    
    // Load embalagens for selected unidade
    if (unidadeMedidaId) {
      this.carregarEmbalagens(unidadeMedidaId);
    } else {
      this.embalagensDisponiveis.set([]);
    }
  }

  /**
   * Get validation error message for form control
   */
  getErrorMessage(controlName: string): string {
    const control = this.form.get(controlName);
    if (!control) return '';
    return this.validationService.getErrorMessage(control, controlName);
  }

  /**
   * Check if form control should show error
   */
  shouldShowError(controlName: string): boolean {
    const control = this.form.get(controlName);
    if (!control) return false;
    return this.validationService.shouldShowError(control);
  }

  /**
   * Override carregarItens to use display DTOs and apply filters
   */
  override carregarItens(): void {
    this.loading.set(true);
    this.tableLoading.set(true);
    
    const filter = this.selectedStatusFilter();
    let request;

    if (filter === 'ativas') {
      request = this.service.obterAtivosParaExibicao();
    } else {
      request = this.service.obterTodosParaExibicao();
    }

    request.subscribe({
      next: (items) => {
        let filteredItems = items;
        
        if (filter === 'inativas') {
          filteredItems = items.filter(item => !item.ativo);
        }
        
        // Store all items in a private property for filtering
        this.todosItens = filteredItems;
        this.aplicarFiltros();
        this.carregarOpcoesParaFiltros(filteredItems);
        this.loading.set(false);
        this.tableLoading.set(false);
      },
      error: (error) => {
        console.error('Erro ao carregar produtos:', error);
        this.loading.set(false);
        this.tableLoading.set(false);
      }
    });
  }

  /**
   * Load filter options from loaded items
   */
  private carregarOpcoesParaFiltros(items: ProdutoDisplayDto[]): void {
    // Extract unique categorias
    const categorias = Array.from(
      new Map(
        items
          .filter(item => item.categoriaId && item.categoriaNome)
          .map(item => [item.categoriaId, { id: item.categoriaId, nome: item.categoriaNome }])
      ).values()
    );
    this.categoriasParaFiltro.set(categorias);

    // Extract unique unidades de medida
    const unidades = Array.from(
      new Map(
        items
          .filter(item => item.unidadeMedidaId && item.unidadeMedidaNome)
          .map(item => [item.unidadeMedidaId, { 
            id: item.unidadeMedidaId, 
            nome: `${item.unidadeMedidaSimbolo} - ${item.unidadeMedidaNome}` 
          }])
      ).values()
    );
    this.unidadesParaFiltro.set(unidades);

    // Extract unique atividades agropecuárias
    const atividades = Array.from(
      new Map(
        items
          .filter(item => item.atividadeAgropecuariaId && item.atividadeAgropecuariaNome)
          .map(item => [item.atividadeAgropecuariaId, { 
            id: item.atividadeAgropecuariaId!, 
            nome: item.atividadeAgropecuariaCodigo 
              ? `${item.atividadeAgropecuariaCodigo} - ${item.atividadeAgropecuariaNome}`
              : item.atividadeAgropecuariaNome!
          }])
      ).values()
    );
    this.atividadesParaFiltro.set(atividades);
  }

  /**
   * Apply all active filters
   */
  aplicarFiltros(): void {
    let itemsFiltrados = [...this.todosItens];

    // Apply categoria filter
    if (this.filtroCategoria) {
      itemsFiltrados = itemsFiltrados.filter(item => item.categoriaId === this.filtroCategoria);
    }

    // Apply unidade de medida filter
    if (this.filtroUnidadeMedida) {
      itemsFiltrados = itemsFiltrados.filter(item => item.unidadeMedidaId === this.filtroUnidadeMedida);
    }

    // Apply atividade agropecuária filter
    if (this.filtroAtividadeAgropecuaria) {
      itemsFiltrados = itemsFiltrados.filter(item => item.atividadeAgropecuariaId === this.filtroAtividadeAgropecuaria);
    }

    // Apply text search filter
    if (this.filtroTexto.trim()) {
      const searchTerm = this.filtroTexto.toLowerCase().trim();
      itemsFiltrados = itemsFiltrados.filter(item =>
        item.codigo.toLowerCase().includes(searchTerm) ||
        item.nome.toLowerCase().includes(searchTerm) ||
        item.categoriaNome.toLowerCase().includes(searchTerm) ||
        item.unidadeMedidaNome.toLowerCase().includes(searchTerm) ||
        (item.atividadeAgropecuariaNome && item.atividadeAgropecuariaNome.toLowerCase().includes(searchTerm))
      );
    }

    // Update the base component's items signal
    this.items.set(itemsFiltrados);
  }

  /**
   * Handle search input change with debounce
   */
  onBuscaChange(event: any): void {
    this.filtroTexto = event.target.value;
    
    // Clear previous timer
    if (this.searchTimer) {
      clearTimeout(this.searchTimer);
    }
    
    // Set new timer
    this.searchTimer = setTimeout(() => {
      this.aplicarFiltros();
    }, 300);
  }

  /**
   * Clear all filters
   */
  limparFiltros(): void {
    this.filtroCategoria = null;
    this.filtroUnidadeMedida = null;
    this.filtroAtividadeAgropecuaria = null;
    this.filtroTexto = '';
    this.aplicarFiltros();
  }

  /**
   * Check if any filters are active
   */
  readonly temFiltrosAtivos = computed(() => !!(this.filtroCategoria || this.filtroUnidadeMedida || this.filtroAtividadeAgropecuaria || this.filtroTexto.trim()));

  /**
   * Format price for display
   */
  formatarPreco(preco?: number): string {
    if (!preco || preco === 0) {
      return 'Não informado';
    }
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    }).format(preco);
  }

  /**
   * Get CSS class for price display
   */
  getPrecoClass(preco?: number): string {
    if (!preco || preco === 0) {
      return 'price-display price-zero';
    }
    return 'price-display';
  }
}
