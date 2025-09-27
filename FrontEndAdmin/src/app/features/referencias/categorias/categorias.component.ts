import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule, AbstractControl, ValidationErrors, AsyncValidatorFn } from '@angular/forms';
import { Observable, of, timer } from 'rxjs';
import { map, switchMap, catchError } from 'rxjs/operators';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TreeTableModule } from 'primeng/treetable';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { SelectModule } from 'primeng/select';
import { CheckboxModule } from 'primeng/checkbox';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService } from 'primeng/api';
import { ConfirmationService } from 'primeng/api';
import { FieldErrorComponent } from '../../../shared/components/field-error.component';
import { 
  CategoriaDto, 
  CriarCategoriaDto, 
  AtualizarCategoriaDto, 
  CategoriaProduto 
} from '../../../shared/models/reference.model';
import { 
  CategoriaService, 
  CategoriaDropdownOption, 
  TipoOption,
  TreeNode 
} from './services/categoria.service';
import { ValidationService } from '../../../shared/services/validation.service';

interface TableColumn {
  field: string;
  header: string;
  sortable?: boolean;
  width?: string;
  type?: 'text' | 'boolean' | 'date' | 'custom';
  hideOnMobile?: boolean;
  hideOnTablet?: boolean;
}

/**
 * Categorias CRUD component with hierarchical tree display
 * Extends ReferenceCrudBaseComponent for consistent behavior
 * Features TreeTable for hierarchy visualization and category management
 */
@Component({
  selector: 'app-categorias',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,

    DialogModule,
    ButtonModule,
    TableModule,
    TreeTableModule,
    InputTextModule,
    TextareaModule,
    SelectModule,
    CheckboxModule,
    ToastModule,
    ConfirmDialogModule,
    ProgressSpinnerModule,
    TagModule,
    TooltipModule,
    FieldErrorComponent
  ],
  template: `
    <div class="categorias-container">
      <!-- Header with title and actions -->
      <div class="header-section">
        <div class="title-section">
          <h2>{{ entityDisplayName() }}</h2>
          <p class="subtitle">{{ entityDescription() }}</p>
        </div>
        
        <div class="actions-section">
          <!-- New Category Button -->
          <p-button
            [label]="'Nova ' + entityDisplayName()"
            icon="pi pi-plus"
            (onClick)="novoItem()"
            class="p-button-primary new-item-btn">
          </p-button>
        </div>
      </div>

      <!-- Filters Section -->
      <div class="filters-section">
        <div class="filters-row">
          <!-- Search Input -->
          <div class="filter-item search-filter">
            <label for="search" class="filter-label">Buscar</label>
            <span class="p-input-icon-left">
              <i class="pi pi-search"></i>
              <input
                id="search"
                type="text"
                pInputText
                [value]="searchText()"
                (input)="onSearchTextChange($event)"
                placeholder="Buscar por nome ou descrição..."
                class="search-input">
            </span>
          </div>

          <!-- Tipo Filter -->
          <div class="filter-item tipo-filter">
            <label for="tipoFilter" class="filter-label">Tipo</label>
            <p-select
              id="tipoFilter"
              [options]="tipoFilterOptions"
              [ngModel]="selectedTipoFilter()"
              (onChange)="onTipoFilterChange($event)"
              placeholder="Filtrar por tipo"
              optionLabel="label"
              optionValue="value"
              [showClear]="true">
            </p-select>
          </div>

          <!-- Status Filter -->
          <div class="filter-item status-filter">
            <label for="statusFilter" class="filter-label">Status</label>
            <p-select
              id="statusFilter"
              [options]="statusFilterOptions"
              [ngModel]="selectedStatusFilter()"
              (onChange)="onStatusFilterChange($event)"
              placeholder="Filtrar por status"
              optionLabel="label"
              optionValue="value">
            </p-select>
          </div>

          <!-- Clear Filters Button -->
          <div class="filter-item clear-filter">
            <p-button
              label="Limpar Filtros"
              icon="pi pi-filter-slash"
              (onClick)="limparFiltros()"
              class="p-button-outlined p-button-secondary"
              [disabled]="!temFiltrosAtivos()">
            </p-button>
          </div>
        </div>

        <!-- Active Filters Summary -->
        <div *ngIf="temFiltrosAtivos()" class="active-filters-summary">
          <span class="summary-label">Filtros ativos:</span>
          <p-tag *ngIf="searchText()" 
                 [value]="'Busca: ' + searchText()" 
                 severity="info" 
                 class="filter-tag">
          </p-tag>
          <p-tag *ngIf="selectedTipoFilter() !== null" 
                 [value]="'Tipo: ' + obterLabelTipo(selectedTipoFilter()!)" 
                 severity="info" 
                 class="filter-tag">
          </p-tag>
          <p-tag *ngIf="selectedStatusFilter() !== 'todas'" 
                 [value]="'Status: ' + (selectedStatusFilter() === 'ativas' ? 'Ativas' : 'Inativas')" 
                 severity="info" 
                 class="filter-tag">
          </p-tag>
        </div>
      </div>

      <!-- Loading Spinner -->
      <div *ngIf="loading()" class="loading-container">
        <p-progressSpinner></p-progressSpinner>
        <p>Carregando {{ entityDisplayName().toLowerCase() }}...</p>
      </div>

      <!-- TreeTable for Hierarchical Categories -->
      <div *ngIf="!loading()" class="tree-container">
        <p-treeTable 
          *ngIf="categoriasTree().length > 0; else emptyState"
          [value]="categoriasTree()" 
          [columns]="getVisibleColumns()"
          [loading]="tableLoading()"
          [scrollable]="true"
          scrollHeight="600px"
          styleClass="p-treetable-sm"
          (onNodeExpand)="onNodeExpand($event)"
          (onNodeCollapse)="onNodeCollapse($event)"
          [resizableColumns]="true">
          
          <!-- Nome Column -->
          <ng-template pTemplate="header" let-columns>
            <tr>
              <th *ngFor="let col of columns" 
                  [style.width]="col.width"
                  [class.hide-on-mobile]="col.hideOnMobile && isMobile()"
                  [class.hide-on-tablet]="col.hideOnTablet && isTablet()">
                {{ col.header }}
              </th>
            </tr>
          </ng-template>

          <ng-template pTemplate="body" let-rowNode let-rowData="rowData" let-columns="columns">
            <tr [class.inactive-row]="!rowData.ativo">
              <td *ngFor="let col of columns; let i = index"
                  [class.hide-on-mobile]="col.hideOnMobile && isMobile()"
                  [class.hide-on-tablet]="col.hideOnTablet && isTablet()">
                
                <!-- Nome Column with Tree Structure -->
                <span *ngIf="col.field === 'nome'">
                  <p-treeTableToggler [rowNode]="rowNode"></p-treeTableToggler>
                  <span class="categoria-nome">{{ rowData.nome }}</span>
                </span>

                <!-- Tipo Column -->
                <span *ngIf="col.field === 'tipo'" class="tipo-badge">
                  {{ obterLabelTipo(rowData.tipo) }}
                </span>

                <!-- Status Column -->
                <span *ngIf="col.field === 'ativo'">
                  <p-tag
                    [value]="getStatusLabel(rowData.ativo)"
                    [severity]="getStatusSeverity(rowData.ativo)">
                  </p-tag>
                </span>

                <!-- Ordem Column -->
                <span *ngIf="col.field === 'ordem'" class="ordem-value">
                  {{ rowData.ordem }}
                </span>

                <!-- Actions Column -->
                <div *ngIf="col.field === 'acoes'" class="action-buttons">
                  <p-button
                    icon="pi pi-pencil"
                    (onClick)="editarItem(rowData)"
                    class="p-button-rounded p-button-text p-button-info p-button-sm"
                    pTooltip="Editar categoria"
                    tooltipPosition="top"
                    [loading]="isActionLoading('edit', rowData.id)"
                    [disabled]="hasAnyActionLoading()">
                  </p-button>
                  
                  <p-button
                    *ngIf="rowData.ativo"
                    icon="pi pi-times"
                    (onClick)="desativarItem(rowData)"
                    class="p-button-rounded p-button-text p-button-warning p-button-sm"
                    pTooltip="Desativar categoria"
                    tooltipPosition="top"
                    [loading]="isActionLoading('deactivate', rowData.id)"
                    [disabled]="hasAnyActionLoading()">
                  </p-button>
                  
                  <p-button
                    *ngIf="!rowData.ativo"
                    icon="pi pi-check"
                    (onClick)="ativarItem(rowData)"
                    class="p-button-rounded p-button-text p-button-success p-button-sm"
                    pTooltip="Ativar categoria"
                    tooltipPosition="top"
                    [loading]="isActionLoading('activate', rowData.id)"
                    [disabled]="hasAnyActionLoading()">
                  </p-button>
                  
                  <p-button
                    icon="pi pi-trash"
                    (onClick)="excluirItem(rowData)"
                    class="p-button-rounded p-button-text p-button-danger p-button-sm"
                    pTooltip="Excluir categoria"
                    tooltipPosition="top"
                    [loading]="isActionLoading('delete', rowData.id)"
                    [disabled]="hasAnyActionLoading()">
                  </p-button>
                </div>
              </td>
            </tr>
          </ng-template>

          <!-- Empty State Template -->
          <ng-template pTemplate="emptymessage">
            <tr>
              <td [attr.colspan]="getVisibleColumns().length" class="text-center">
                <div class="empty-message">
                  <i class="pi pi-info-circle"></i>
                  <span>Nenhuma categoria encontrada</span>
                </div>
              </td>
            </tr>
          </ng-template>
        </p-treeTable>

        <!-- Empty State -->
        <ng-template #emptyState>
          <div class="empty-state">
            <div class="empty-content">
              <i class="pi pi-info-circle empty-icon"></i>
              <h3>
                <span *ngIf="temFiltrosAtivos(); else noFiltersTitle">
                  Nenhuma categoria encontrada
                </span>
                <ng-template #noFiltersTitle>
                  Nenhuma {{ entityDisplayName().toLowerCase() }} encontrada
                </ng-template>
              </h3>
              <p>
                <span *ngIf="temFiltrosAtivos(); else noFiltersMessage">
                  Nenhuma categoria corresponde aos filtros aplicados. Tente ajustar os critérios de busca.
                </span>
                <ng-template #noFiltersMessage>
                  <span *ngIf="selectedStatusFilter() === 'todas'; else statusSpecificMessage">
                    Não há {{ entityDisplayName().toLowerCase() }} cadastradas no sistema.
                  </span>
                  <ng-template #statusSpecificMessage>
                    <span *ngIf="selectedStatusFilter() === 'ativas'; else inactiveMessage">
                      Não há {{ entityDisplayName().toLowerCase() }} ativas no momento.
                    </span>
                    <ng-template #inactiveMessage>
                      Não há {{ entityDisplayName().toLowerCase() }} inativas no momento.
                    </ng-template>
                  </ng-template>
                </ng-template>
              </p>
              <p-button
                *ngIf="temFiltrosAtivos(); else newCategoryButton"
                label="Limpar Filtros"
                icon="pi pi-filter-slash"
                (onClick)="limparFiltros()"
                class="p-button-outlined">
              </p-button>
              <ng-template #newCategoryButton>
                <p-button
                  [label]="'Cadastrar Nova ' + entityDisplayName()"
                  icon="pi pi-plus"
                  (onClick)="novoItem()"
                  class="p-button-primary">
                </p-button>
              </ng-template>
            </div>
          </div>
        </ng-template>
      </div>

      <!-- Form Dialog -->
      <p-dialog
        [header]="dialogTitle()"
        [visible]="showForm()"
        (onHide)="cancelarEdicao()"
        [modal]="true"
        [style]="{ width: '50vw' }"
        [draggable]="false"
        [resizable]="false"
        [closable]="!formLoading()">
        
        <form [formGroup]="form" (ngSubmit)="salvarItem()" class="form-container">
          <!-- Nome -->
          <div class="field">
            <label for="nome" class="required">Nome</label>
            <input
              id="nome"
              type="text"
              pInputText
              formControlName="nome"
              placeholder="Digite o nome da categoria"
              [class.ng-invalid]="shouldShowError('nome')"
              [class.p-invalid]="shouldShowError('nome')"
              maxlength="100"
              autocomplete="off">
            <small class="field-help">
              Nome único da categoria (2-100 caracteres)
            </small>
            <app-field-error 
              *ngIf="shouldShowError('nome')"
              [message]="getErrorMessage('nome')">
            </app-field-error>
          </div>

          <!-- Descrição -->
          <div class="field">
            <label for="descricao">Descrição</label>
            <textarea
              id="descricao"
              pTextarea
              formControlName="descricao"
              placeholder="Digite uma descrição para a categoria (opcional)"
              [class.ng-invalid]="shouldShowError('descricao')"
              [class.p-invalid]="shouldShowError('descricao')"
              rows="3"
              maxlength="500"
              [autoResize]="true">
            </textarea>
            <small class="field-help">
              Descrição detalhada da categoria (máximo 500 caracteres)
            </small>
            <app-field-error 
              *ngIf="shouldShowError('descricao')"
              [message]="getErrorMessage('descricao')">
            </app-field-error>
          </div>

          <!-- Tipo -->
          <div class="field">
            <label for="tipo" class="required">Tipo de Produto</label>
            <p-select
              id="tipo"
              formControlName="tipo"
              [options]="tiposDisponiveis"
              optionLabel="label"
              optionValue="value"
              placeholder="Selecione o tipo da categoria"
              [class.ng-invalid]="shouldShowError('tipo')"
              [class.p-invalid]="shouldShowError('tipo')"
              [showClear]="false">
            </p-select>
            <small class="field-help">
              Tipo de produto que esta categoria irá agrupar
            </small>
            <app-field-error 
              *ngIf="shouldShowError('tipo')"
              [message]="getErrorMessage('tipo')">
            </app-field-error>
          </div>

          <!-- Categoria Pai -->
          <div class="field">
            <label for="categoriaPai">Categoria Pai</label>
            <p-select
              id="categoriaPai"
              formControlName="categoriaPaiId"
              [options]="getCategoriasParaDropdownFiltradas()"
              optionLabel="nomeCompleto"
              optionValue="id"
              placeholder="Selecione a categoria pai (opcional)"
              [class.ng-invalid]="shouldShowError('categoriaPaiId')"
              [class.p-invalid]="shouldShowError('categoriaPaiId')"
              [filter]="true"
              filterBy="nome,nomeCompleto"
              [showClear]="true"
              emptyMessage="Nenhuma categoria disponível"
              emptyFilterMessage="Nenhuma categoria encontrada">
            </p-select>
            <small class="field-help">
              @if (isEditMode()) {
                Categoria pai na hierarquia. Deixe em branco para categoria raiz.
              } @else {
                Categoria pai na hierarquia. Deixe em branco para criar uma categoria raiz.
              }
            </small>
            <app-field-error 
              *ngIf="shouldShowError('categoriaPaiId')"
              [message]="getErrorMessage('categoriaPaiId')">
            </app-field-error>
          </div>

          <!-- Ordem -->
          <div class="field">
            <label for="ordem">Ordem de Exibição</label>
            <input
              id="ordem"
              type="number"
              pInputText
              formControlName="ordem"
              placeholder="0"
              [class.ng-invalid]="shouldShowError('ordem')"
              [class.p-invalid]="shouldShowError('ordem')"
              min="0"
              step="1">
            <small class="field-help">
              Ordem de exibição da categoria (0 = primeira posição)
            </small>
            <app-field-error 
              *ngIf="shouldShowError('ordem')"
              [message]="getErrorMessage('ordem')">
            </app-field-error>
          </div>

          <!-- Ativo (only in edit mode) -->
          <div *ngIf="isEditMode()" class="field">
            <div class="field-checkbox">
              <p-checkbox
                id="ativo"
                formControlName="ativo"
                binary="true"
                label="Categoria ativa">
              </p-checkbox>
            </div>
            <small class="field-help">
              Categorias inativas não aparecem para seleção em novos produtos
            </small>
          </div>
        </form>

        <ng-template pTemplate="footer">
          <div class="dialog-footer">
            <p-button
              label="Cancelar"
              icon="pi pi-times"
              (onClick)="cancelarEdicao()"
              class="p-button-text"
              [disabled]="formLoading()">
            </p-button>
            <p-button
              [label]="isEditMode() ? 'Atualizar' : 'Criar'"
              icon="pi pi-check"
              (onClick)="salvarItem()"
              class="p-button-primary"
              [loading]="formLoading()"
              [disabled]="isSaveDisabled()">
            </p-button>
          </div>
        </ng-template>
      </p-dialog>

      <!-- Toast Messages -->
      <p-toast position="top-right"></p-toast>
      
      <!-- Confirmation Dialog -->
      <p-confirmDialog></p-confirmDialog>
    </div>
  `,
  styleUrls: ['./categorias.component.scss']
})
export class CategoriasComponent implements OnInit {
  
  protected service = inject(CategoriaService) as any;
  protected validationService = inject(ValidationService);
  protected messageService = inject(MessageService);
  protected confirmationService = inject(ConfirmationService);
  protected fb = inject(FormBuilder);

  // Hierarchical data for display
  categoriasHierarchicas = signal<CategoriaDto[]>([]);
  
  // Tree data for TreeTable
  categoriasTree = signal<TreeNode<CategoriaDto>[]>([]);
  
  // Dropdown options
  categoriasParaDropdown = signal<CategoriaDropdownOption[]>([]);
  tiposDisponiveis: TipoOption[] = [];

  // TreeTable columns configuration
  treeColumns = signal<TableColumn[]>([]);

  // Component configuration
  protected entityDisplayName = () => 'Categoria';
  protected entityDescription = () => 'Gerencie as categorias de produtos de forma hierárquica';
  protected defaultSortField = () => 'nome';
  protected searchFields = () => ['nome', 'descricao', 'categoriaPaiNome'];

  // Loading states for individual actions
  actionLoadingStates = signal<Map<string, number>>(new Map());
  tableLoading = signal<boolean>(false);

  // Filter signals
  searchText = signal<string>('');
  selectedTipoFilter = signal<CategoriaProduto | null>(null);
  selectedStatusFilter = signal<string>('todas');
  
  // Filter options
  tipoFilterOptions: Array<{label: string, value: CategoriaProduto | null}> = [];
  statusFilterOptions = [
    { label: 'Todas', value: 'todas' },
    { label: 'Ativas', value: 'ativas' },
    { label: 'Inativas', value: 'inativas' }
  ];
  
  // Original data for filtering
  originalCategorias = signal<CategoriaDto[]>([]);
  
  // Component state signals
  items = signal<CategoriaDto[]>([]);
  selectedItem = signal<CategoriaDto | null>(null);
  currentRowVersion = signal<string | null>(null);
  showForm = signal<boolean>(false);
  loading = signal<boolean>(false);
  formLoading = signal<boolean>(false);
  
  // Form instance
  form!: FormGroup;

  protected displayColumns = (): TableColumn[] => [
    { field: 'nome', header: 'Nome', sortable: true },
    { field: 'tipoDescricao', header: 'Tipo', sortable: true, width: '150px', type: 'custom', hideOnMobile: true },
    { field: 'ordem', header: 'Ordem', sortable: true, width: '100px', hideOnMobile: true },
    { field: 'quantidadeProdutos', header: 'Produtos', sortable: true, width: '120px', hideOnTablet: true },
    { field: 'ativo', header: 'Status', sortable: true, width: '100px', type: 'boolean' }
  ];

  ngOnInit(): void {
    this.form = this.createFormGroup();
    this.inicializarDados();
    this.inicializarColunas();
    this.carregarItens();
  }

  protected createFormGroup(): FormGroup {
    const form = this.fb.group({
      nome: ['', 
        [Validators.required, Validators.minLength(2), Validators.maxLength(100)],
        [this.createNomeUniqueValidator()]
      ],
      descricao: ['', [Validators.maxLength(500)]],
      tipo: [null, [Validators.required]],
      categoriaPaiId: [null, [], [this.createCircularReferenceValidator()]],
      ordem: [0, [Validators.required, Validators.min(0), Validators.pattern(/^\d+$/)]],
      ativo: [true]
    });

    // Add cross-field validation for categoria pai
    form.get('categoriaPaiId')?.addValidators([this.createCategoriaPaiValidator()]);

    return form;
  }

  protected mapToCreateDto(formValue: any): CriarCategoriaDto {
    return {
      nome: formValue.nome,
      descricao: formValue.descricao || undefined,
      tipo: formValue.tipo,
      categoriaPaiId: formValue.categoriaPaiId || undefined,
      ordem: formValue.ordem || 0
    };
  }

  protected mapToUpdateDto(formValue: any): AtualizarCategoriaDto {
    return {
      nome: formValue.nome,
      descricao: formValue.descricao || undefined,
      tipo: formValue.tipo,
      categoriaPaiId: formValue.categoriaPaiId || undefined,
      ordem: formValue.ordem || 0,
      ativo: formValue.ativo
    };
  }



  /**
   * Initialize component data
   */
  private inicializarDados(): void {
    this.carregarTiposDisponiveis();
    this.carregarCategoriasParaDropdown();
    this.inicializarFiltroTipo();
  }

  /**
   * Initialize tipo filter options
   */
  private inicializarFiltroTipo(): void {
    this.tipoFilterOptions = [
      { label: 'Todos os tipos', value: null },
      ...this.service.obterTiposDisponiveis().map(tipo => ({
        label: tipo.label,
        value: tipo.value
      }))
    ];
  }

  /**
   * Initialize TreeTable columns configuration
   */
  private inicializarColunas(): void {
    this.treeColumns.set([
      { 
        field: 'nome', 
        header: 'Nome', 
        sortable: true,
        width: '40%'
      },
      { 
        field: 'tipo', 
        header: 'Tipo', 
        sortable: true, 
        width: '20%',
        type: 'custom',
        hideOnMobile: true 
      },
      { 
        field: 'ativo', 
        header: 'Status', 
        sortable: true, 
        width: '15%',
        type: 'boolean'
      },
      { 
        field: 'ordem', 
        header: 'Ordem', 
        sortable: true, 
        width: '10%',
        hideOnMobile: true 
      },
      { 
        field: 'acoes', 
        header: 'Ações', 
        sortable: false, 
        width: '15%',
        type: 'custom'
      }
    ]);
  }

  /**
   * Load available product types
   */
  private carregarTiposDisponiveis(): void {
    this.tiposDisponiveis = this.service.obterTiposDisponiveis();
  }

  /**
   * Load categories for dropdown selection
   */
  private carregarCategoriasParaDropdown(): void {
    this.service.obterParaDropdown().subscribe({
      next: (categorias) => {
        this.categoriasParaDropdown.set(categorias);
      },
      error: (error) => {
        console.error('Erro ao carregar categorias para dropdown:', error);
      }
    });
  }

  /**
   * Load hierarchical tree data with enhanced error handling
   */
  carregarItens(): void {
    this.loading.set(true);
    this.tableLoading.set(true);
    
    // Always load all data first
    this.service.obterTodos().subscribe({
      next: (items: CategoriaDto[]) => {
        // Store original data for filtering
        this.originalCategorias.set(items);
        
        // Apply all filters
        this.aplicarFiltros();
        
        this.loading.set(false);
        this.tableLoading.set(false);
        
        // Show success message only if this is a manual reload
        if (this.items().length === 0 && items.length > 0) {
          this.showInfoMessage(`${items.length} categorias carregadas com sucesso.`, 'Dados carregados');
        }
      },
      error: (error: any) => {
        this.handleCategoriaError('carregar categorias', error);
        this.loading.set(false);
        this.tableLoading.set(false);
        
        // Set empty state on error
        this.originalCategorias.set([]);
        this.items.set([]);
        this.categoriasHierarchicas.set([]);
        this.categoriasTree.set([]);
      }
    });
  }

  /**
   * Apply all active filters to the data
   */
  private aplicarFiltros(): void {
    let filteredItems = [...this.originalCategorias()];
    
    // Apply status filter
    const statusFilter = this.selectedStatusFilter();
    if (statusFilter === 'ativas') {
      filteredItems = filteredItems.filter(item => item.ativo);
    } else if (statusFilter === 'inativas') {
      filteredItems = filteredItems.filter(item => !item.ativo);
    }
    
    // Apply tipo filter
    const tipoFilter = this.selectedTipoFilter();
    if (tipoFilter !== null) {
      filteredItems = filteredItems.filter(item => item.tipo === tipoFilter);
    }
    
    // Apply search filter
    const searchText = this.searchText().toLowerCase().trim();
    if (searchText) {
      filteredItems = this.aplicarFiltroTexto(filteredItems, searchText);
    }
    
    // Store filtered items for base component
    this.items.set(filteredItems);
    
    // Build hierarchical structure maintaining parent-child relationships
    const hierarchicalData = this.buildHierarchyWithFilters(filteredItems);
    this.categoriasHierarchicas.set(hierarchicalData);
    
    // Transform to TreeTable format
    const treeData = this.service.transformToTreeNodes(hierarchicalData);
    this.categoriasTree.set(treeData);
  }

  /**
   * Apply text search filter to categories
   */
  private aplicarFiltroTexto(categorias: CategoriaDto[], searchText: string): CategoriaDto[] {
    const matchingCategories = new Set<number>();
    const allCategories = new Map<number, CategoriaDto>();
    
    // Create map of all categories
    this.originalCategorias().forEach(cat => {
      allCategories.set(cat.id, cat);
    });
    
    // Find direct matches
    categorias.forEach(categoria => {
      if (categoria.nome.toLowerCase().includes(searchText) ||
          (categoria.descricao && categoria.descricao.toLowerCase().includes(searchText))) {
        matchingCategories.add(categoria.id);
        
        // Add all parents to maintain hierarchy
        this.addParentsToSet(categoria, allCategories, matchingCategories);
        
        // Add all children to maintain hierarchy
        this.addChildrenToSet(categoria, allCategories, matchingCategories);
      }
    });
    
    // Return filtered categories that match or are needed for hierarchy
    return categorias.filter(cat => matchingCategories.has(cat.id));
  }

  /**
   * Add parent categories to the matching set to maintain hierarchy
   */
  private addParentsToSet(categoria: CategoriaDto, allCategories: Map<number, CategoriaDto>, matchingSet: Set<number>): void {
    if (categoria.categoriaPaiId) {
      const parent = allCategories.get(categoria.categoriaPaiId);
      if (parent && !matchingSet.has(parent.id)) {
        matchingSet.add(parent.id);
        this.addParentsToSet(parent, allCategories, matchingSet);
      }
    }
  }

  /**
   * Add child categories to the matching set to maintain hierarchy
   */
  private addChildrenToSet(categoria: CategoriaDto, allCategories: Map<number, CategoriaDto>, matchingSet: Set<number>): void {
    allCategories.forEach(cat => {
      if (cat.categoriaPaiId === categoria.id && !matchingSet.has(cat.id)) {
        matchingSet.add(cat.id);
        this.addChildrenToSet(cat, allCategories, matchingSet);
      }
    });
  }

  /**
   * Build hierarchy maintaining parent-child relationships even with filters
   */
  private buildHierarchyWithFilters(categorias: CategoriaDto[]): CategoriaDto[] {
    const categoryMap = new Map<number, CategoriaDto>();
    const rootCategories: CategoriaDto[] = [];

    // First pass: create map and initialize subCategorias arrays
    categorias.forEach(categoria => {
      categoria.subCategorias = [];
      categoryMap.set(categoria.id, categoria);
    });

    // Second pass: build hierarchy
    categorias.forEach(categoria => {
      if (categoria.categoriaPaiId) {
        const parent = categoryMap.get(categoria.categoriaPaiId);
        if (parent) {
          parent.subCategorias.push(categoria);
        } else {
          // Parent not in filtered set, treat as root
          rootCategories.push(categoria);
        }
      } else {
        rootCategories.push(categoria);
      }
    });

    return rootCategories;
  }

  /**
   * Get tipo label from enum value
   */
  obterLabelTipo(tipo: CategoriaProduto): string {
    return this.service.obterLabelTipo(tipo);
  }

  // ========================================
  // Filter Event Handlers
  // ========================================

  /**
   * Handle search text change
   */
  onSearchTextChange(event: any): void {
    const searchText = event.target?.value || '';
    this.searchText.set(searchText);
    this.aplicarFiltros();
  }

  /**
   * Handle tipo filter change
   */
  onTipoFilterChange(event: any): void {
    this.selectedTipoFilter.set(event.value);
    this.aplicarFiltros();
  }

  /**
   * Handle status filter change
   */
  onStatusFilterChange(event: any): void {
    this.selectedStatusFilter.set(event.value);
    this.aplicarFiltros();
  }

  /**
   * Clear all filters
   */
  limparFiltros(): void {
    this.searchText.set('');
    this.selectedTipoFilter.set(null);
    this.selectedStatusFilter.set('todas');
    this.aplicarFiltros();
  }

  /**
   * Check if any filters are active
   */
  temFiltrosAtivos(): boolean {
    return this.searchText() !== '' || 
           this.selectedTipoFilter() !== null || 
           this.selectedStatusFilter() !== 'todas';
  }



  /**
   * Build hierarchy from flat category list
   */
  private buildHierarchy(categorias: CategoriaDto[]): CategoriaDto[] {
    const categoryMap = new Map<number, CategoriaDto>();
    const rootCategories: CategoriaDto[] = [];

    // First pass: create map and initialize subCategorias arrays
    categorias.forEach(categoria => {
      categoria.subCategorias = [];
      categoryMap.set(categoria.id, categoria);
    });

    // Second pass: build hierarchy
    categorias.forEach(categoria => {
      if (categoria.categoriaPaiId) {
        const parent = categoryMap.get(categoria.categoriaPaiId);
        if (parent) {
          parent.subCategorias.push(categoria);
        }
      } else {
        rootCategories.push(categoria);
      }
    });

    return rootCategories;
  }

  /**
   * Get visible columns based on screen size
   */
  getVisibleColumns(): TableColumn[] {
    const allColumns = this.treeColumns();
    const isMobile = this.isMobile();
    const isTablet = this.isTablet();

    return allColumns.filter(col => {
      if (isMobile && col.hideOnMobile) return false;
      if (isTablet && col.hideOnTablet) return false;
      return true;
    });
  }

  /**
   * Check if device is mobile
   */
  isMobile(): boolean {
    return window.innerWidth < 768;
  }

  /**
   * Check if device is tablet
   */
  isTablet(): boolean {
    return window.innerWidth >= 768 && window.innerWidth <= 1024;
  }

  /**
   * Toggle tree node expansion
   */
  onNodeExpand(event: any): void {
    // TreeTable handles expansion automatically
    // This method can be used for additional logic if needed
  }

  /**
   * Toggle tree node collapse
   */
  onNodeCollapse(event: any): void {
    // TreeTable handles collapse automatically
    // This method can be used for additional logic if needed
  }

  /**
   * Handle TreeTable loading state
   */
  onTreeTableLoad(): void {
    // Additional loading logic if needed
  }

  /**
   * Get column width for responsive design
   */
  getColumnWidth(field: string): string {
    const column = this.treeColumns().find(col => col.field === field);
    return column?.width || 'auto';
  }

  /**
   * Check if column should be hidden on current screen size
   */
  shouldHideColumn(column: TableColumn): boolean {
    if (this.isMobile() && column.hideOnMobile) return true;
    if (this.isTablet() && column.hideOnTablet) return true;
    return false;
  }

  /**
   * Create async validator for nome uniqueness
   */
  private createNomeUniqueValidator(): AsyncValidatorFn {
    return (control: AbstractControl): Observable<ValidationErrors | null> => {
      if (!control.value || control.value.length < 2) {
        return of(null);
      }

      const excludeId = this.isEditMode() ? this.selectedItem()?.id : undefined;

      return timer(300).pipe(
        switchMap(() => this.service.existeComNome(control.value, excludeId)),
        map(exists => exists ? { nomeJaExiste: { value: control.value } } : null),
        catchError(() => of(null))
      );
    };
  }

  /**
   * Create async validator for circular reference prevention
   */
  private createCircularReferenceValidator(): AsyncValidatorFn {
    return (control: AbstractControl): Observable<ValidationErrors | null> => {
      if (!control.value || !this.isEditMode()) {
        return of(null);
      }

      const categoriaId = this.selectedItem()?.id;
      if (!categoriaId) {
        return of(null);
      }

      return timer(300).pipe(
        switchMap(() => this.service.validarReferenciaCircular(categoriaId, control.value)),
        map(isValid => isValid ? null : { referenciaCircular: { categoriaId, categoriaPaiId: control.value } }),
        catchError(() => of(null))
      );
    };
  }

  /**
   * Create validator for categoria pai selection
   */
  private createCategoriaPaiValidator() {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) {
        return null; // Categoria pai é opcional
      }

      // Verificar se a categoria pai existe na lista de opções disponíveis
      const categoriasDisponiveis = this.categoriasParaDropdown();
      const categoriaEncontrada = categoriasDisponiveis.find(cat => cat.id === control.value);

      if (!categoriaEncontrada) {
        return { categoriaPaiInvalida: { value: control.value } };
      }

      // Verificar se a categoria pai está ativa
      if (!categoriaEncontrada.ativo) {
        return { categoriaPaiInativa: { value: control.value } };
      }

      // Em modo de edição, verificar se não está tentando definir a própria categoria como pai
      if (this.isEditMode()) {
        const categoriaAtual = this.selectedItem();
        if (categoriaAtual && categoriaAtual.id === control.value) {
          return { categoriaPaiPropriaCategoria: { value: control.value } };
        }
      }

      return null;
    };
  }

  /**
   * Get error message for form field validation
   */
  getErrorMessage(fieldName: string): string {
    const control = this.form.get(fieldName);
    if (!control || !control.errors) {
      return '';
    }

    const errors = control.errors;

    // Handle custom validation errors
    if (errors['nomeJaExiste']) {
      return `O nome "${errors['nomeJaExiste'].value}" já está sendo usado por outra categoria`;
    }

    if (errors['referenciaCircular']) {
      return 'Esta seleção criaria uma referência circular na hierarquia';
    }

    if (errors['categoriaPaiInvalida']) {
      return 'Categoria pai selecionada não é válida';
    }

    if (errors['categoriaPaiInativa']) {
      return 'A categoria pai selecionada está inativa';
    }

    if (errors['categoriaPaiPropriaCategoria']) {
      return 'Uma categoria não pode ser pai de si mesma';
    }

    // Handle standard validation errors
    switch (fieldName) {
      case 'nome':
        if (errors['required']) return 'Nome é obrigatório';
        if (errors['minlength']) return 'Nome deve ter pelo menos 2 caracteres';
        if (errors['maxlength']) return 'Nome deve ter no máximo 100 caracteres';
        break;

      case 'descricao':
        if (errors['maxlength']) return 'Descrição deve ter no máximo 500 caracteres';
        break;

      case 'tipo':
        if (errors['required']) return 'Tipo é obrigatório';
        break;

      case 'ordem':
        if (errors['required']) return 'Ordem é obrigatória';
        if (errors['min']) return 'Ordem deve ser um número positivo';
        if (errors['pattern']) return 'Ordem deve ser um número inteiro';
        break;
    }

    return 'Campo inválido';
  }

  /**
   * Populate form with item data for editing
   */
  protected populateForm(item: CategoriaDto): void {
    // First populate the form
    this.form.patchValue({
      nome: item.nome,
      descricao: item.descricao,
      tipo: item.tipo,
      categoriaPaiId: item.categoriaPaiId,
      ordem: item.ordem,
      ativo: item.ativo
    });

    // Update async validators to include current item ID for exclusion
    const nomeControl = this.form.get('nome');
    if (nomeControl) {
      nomeControl.clearAsyncValidators();
      nomeControl.setAsyncValidators([this.createNomeUniqueValidator()]);
      nomeControl.updateValueAndValidity();
    }

    const categoriaPaiControl = this.form.get('categoriaPaiId');
    if (categoriaPaiControl) {
      categoriaPaiControl.clearAsyncValidators();
      categoriaPaiControl.setAsyncValidators([this.createCircularReferenceValidator()]);
      categoriaPaiControl.updateValueAndValidity();
    }
  }

  /**
   * Reset form to initial state for create mode
   */
  private resetFormForCreate(): void {
    this.form.reset();
    
    // Set default values
    this.form.patchValue({
      ordem: 0,
      ativo: true
    });

    // Reset async validators for create mode
    const nomeControl = this.form.get('nome');
    if (nomeControl) {
      nomeControl.clearAsyncValidators();
      nomeControl.setAsyncValidators([this.createNomeUniqueValidator()]);
    }

    const categoriaPaiControl = this.form.get('categoriaPaiId');
    if (categoriaPaiControl) {
      categoriaPaiControl.clearAsyncValidators();
      categoriaPaiControl.setAsyncValidators([this.createCircularReferenceValidator()]);
    }
  }

  /**
   * Get filtered categories for dropdown (excluding current category and its descendants in edit mode)
   */
  getCategoriasParaDropdownFiltradas(): CategoriaDropdownOption[] {
    const todasCategorias = this.categoriasParaDropdown();
    
    if (!this.isEditMode() || !this.selectedItem()) {
      return todasCategorias.filter(cat => cat.ativo);
    }

    const categoriaAtual = this.selectedItem()!;
    const categoriasExcluidas = this.getDescendantIds(categoriaAtual.id);
    categoriasExcluidas.add(categoriaAtual.id);

    return todasCategorias.filter(cat => 
      cat.ativo && !categoriasExcluidas.has(cat.id)
    );
  }

  /**
   * Get all descendant IDs of a category (to prevent circular references)
   */
  private getDescendantIds(categoriaId: number): Set<number> {
    const descendants = new Set<number>();
    const allCategories = this.items();
    
    const findDescendants = (parentId: number) => {
      const children = allCategories.filter(cat => cat.categoriaPaiId === parentId);
      children.forEach(child => {
        descendants.add(child.id);
        findDescendants(child.id);
      });
    };

    findDescendants(categoriaId);
    return descendants;
  }



  /**
   * Mark all form controls as touched to show validation errors
   */
  private markFormGroupTouched(): void {
    Object.keys(this.form.controls).forEach(key => {
      const control = this.form.get(key);
      if (control) {
        control.markAsTouched();
        control.updateValueAndValidity();
      }
    });
  }

  /**
   * Check if form field should show error
   */
  shouldShowError(fieldName: string): boolean {
    const control = this.form.get(fieldName);
    return !!(control && control.invalid && (control.dirty || control.touched));
  }

  /**
   * Get form field validation status for styling
   */
  getFieldValidationClass(fieldName: string): string {
    const control = this.form.get(fieldName);
    if (!control) return '';
    
    if (control.pending) return 'p-pending';
    if (control.invalid && (control.dirty || control.touched)) return 'p-invalid';
    if (control.valid && control.dirty) return 'p-valid';
    
    return '';
  }

  /**
   * Check if save button should be disabled
   */
  isSaveDisabled(): boolean {
    return this.form.invalid || this.form.pending || this.formLoading();
  }

  // ========================================
  // CRUD Operations with Hierarchy Validation
  // ========================================

  /**
   * Create novoItem method with modal dialog opening
   * Requirements: 2.3, 2.4
   */
  novoItem(): void {
    // Refresh dropdown data before opening form
    this.carregarCategoriasParaDropdown();
    
    // Reset selected item and form state
    this.selectedItem.set(null);
    this.currentRowVersion.set(null);
    
    // Reset form with default values
    this.resetFormForCreate();
    
    // Open modal dialog
    this.showForm.set(true);
  }

  /**
   * Implement editarItem with form population and validation
   * Requirements: 3.3, 3.4
   */
  editarItem(item: CategoriaDto): void {
    this.setCustomActionLoading('edit', item.id, true);
    
    // Refresh dropdown data before opening form
    this.carregarCategoriasParaDropdown();
    
    // Load fresh data to get current row version and avoid stale data
    this.service.obterPorId(item.id).subscribe({
      next: (freshItem) => {
        this.selectedItem.set(freshItem);
        this.currentRowVersion.set((freshItem as any).rowVersion);
        
        // Populate form with fresh data
        this.populateForm(freshItem);
        
        // Open modal dialog
        this.showForm.set(true);
        this.setCustomActionLoading('edit', item.id, false);
      },
      error: (error) => {
        console.error('Erro ao carregar categoria para edição:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Erro',
          detail: 'Erro ao carregar dados da categoria para edição',
          life: 5000
        });
        this.setCustomActionLoading('edit', item.id, false);
      }
    });
  }

  /**
   * Add salvarItem with create/update logic using POST/PUT endpoints
   * Requirements: 2.3, 2.4, 3.3, 3.4, 5.1, 5.2
   */
  salvarItem(): void {
    // Validate form before submission
    if (this.form.invalid) {
      this.markFormGroupTouched();
      this.messageService.add({
        severity: 'warn',
        summary: 'Formulário Inválido',
        detail: 'Por favor, corrija os erros no formulário antes de salvar',
        life: 5000
      });
      return;
    }

    // Check if form has pending async validations
    if (this.form.pending) {
      // Wait for async validations to complete
      setTimeout(() => this.salvarItem(), 100);
      return;
    }

    this.formLoading.set(true);
    const formValue = this.form.value;
    
    if (this.isEditMode()) {
      // Update existing category using PUT endpoint
      const updateDto = this.mapToUpdateDto(formValue);
      const itemId = this.selectedItem()!.id;
      const rowVersion = this.currentRowVersion();
      
      this.service.atualizar(itemId, updateDto, rowVersion || undefined).subscribe({
        next: (updatedItem) => {
          this.formLoading.set(false);
          this.showForm.set(false);
          this.messageService.add({
            severity: 'success',
            summary: 'Sucesso',
            detail: 'Categoria atualizada com sucesso',
            life: 3000
          });
          
          // Refresh data and dropdown
          this.carregarItens();
          this.carregarCategoriasParaDropdown();
        },
        error: (error) => {
          this.formLoading.set(false);
          
          if (error.originalError?.status === 412) {
            // Concurrency conflict - reload item
            this.messageService.add({
              severity: 'warn',
              summary: 'Dados Desatualizados',
              detail: 'Os dados foram alterados por outro usuário. Recarregando...',
              life: 5000
            });
            this.editarItem(this.selectedItem()!);
          } else if (error.originalError?.status === 400) {
            // Validation error from server
            this.messageService.add({
              severity: 'error',
              summary: 'Erro de Validação',
              detail: error.message || 'Dados inválidos para atualização',
              life: 5000
            });
          } else {
            this.messageService.add({
              severity: 'error',
              summary: 'Erro',
              detail: 'Erro ao atualizar categoria',
              life: 5000
            });
          }
        }
      });
    } else {
      // Create new category using POST endpoint
      const createDto = this.mapToCreateDto(formValue);
      
      this.service.criar(createDto).subscribe({
        next: (newItem) => {
          this.formLoading.set(false);
          this.showForm.set(false);
          this.messageService.add({
            severity: 'success',
            summary: 'Sucesso',
            detail: 'Categoria criada com sucesso',
            life: 3000
          });
          
          // Refresh data and dropdown
          this.carregarItens();
          this.carregarCategoriasParaDropdown();
        },
        error: (error) => {
          this.formLoading.set(false);
          
          if (error.originalError?.status === 400) {
            // Validation error from server
            this.messageService.add({
              severity: 'error',
              summary: 'Erro de Validação',
              detail: error.message || 'Dados inválidos para criação',
              life: 5000
            });
          } else if (error.originalError?.status === 409) {
            // Conflict error (e.g., duplicate name)
            this.messageService.add({
              severity: 'error',
              summary: 'Conflito',
              detail: 'Já existe uma categoria com este nome',
              life: 5000
            });
          } else {
            this.messageService.add({
              severity: 'error',
              summary: 'Erro',
              detail: 'Erro ao criar categoria',
              life: 5000
            });
          }
        }
      });
    }
  }

  /**
   * Implement status toggle using PATCH /ativar and /desativar endpoints
   * Requirements: 4.1, 4.2
   */
  ativarItem(item: CategoriaDto): void {
    this.setCustomActionLoading('activate', item.id, true);
    
    this.service.ativar(item.id).subscribe({
      next: () => {
        this.setCustomActionLoading('activate', item.id, false);
        this.messageService.add({
          severity: 'success',
          summary: 'Sucesso',
          detail: 'Categoria ativada com sucesso',
          life: 3000
        });
        
        // Refresh data to show updated status
        this.carregarItens();
        this.carregarCategoriasParaDropdown();
      },
      error: (error) => {
        this.setCustomActionLoading('activate', item.id, false);
        this.messageService.add({
          severity: 'error',
          summary: 'Erro',
          detail: 'Erro ao ativar categoria',
          life: 5000
        });
      }
    });
  }

  /**
   * Implement status toggle using PATCH /ativar and /desativar endpoints
   * Requirements: 4.1, 4.2
   */
  desativarItem(item: CategoriaDto): void {
    this.confirmationService.confirm({
      message: `Tem certeza que deseja desativar a categoria "${item.nome}"?`,
      header: 'Confirmar Desativação',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Sim, Desativar',
      rejectLabel: 'Cancelar',
      accept: () => {
        this.executarDesativacao(item);
      }
    });
  }

  /**
   * Execute category deactivation
   */
  private executarDesativacao(item: CategoriaDto): void {
    this.setCustomActionLoading('deactivate', item.id, true);
    
    this.service.desativar(item.id).subscribe({
      next: () => {
        this.setCustomActionLoading('deactivate', item.id, false);
        this.messageService.add({
          severity: 'success',
          summary: 'Sucesso',
          detail: 'Categoria desativada com sucesso',
          life: 3000
        });
        
        // Refresh data to show updated status
        this.carregarItens();
        this.carregarCategoriasParaDropdown();
      },
      error: (error) => {
        this.setCustomActionLoading('deactivate', item.id, false);
        this.messageService.add({
          severity: 'error',
          summary: 'Erro',
          detail: 'Erro ao desativar categoria',
          life: 5000
        });
      }
    });
  }

  /**
   * Add confirmarExclusao using podeRemover endpoint for validation before DELETE
   * Requirements: 5.1, 5.2
   */
  excluirItem(item: CategoriaDto): void {
    this.setCustomActionLoading('delete', item.id, true);
    
    // First check if category can be removed using podeRemover endpoint
    this.service.podeRemover(item.id).subscribe({
      next: (canRemove) => {
        this.setCustomActionLoading('delete', item.id, false);
        
        if (!canRemove) {
          // Category cannot be removed due to dependencies
          this.messageService.add({
            severity: 'warn',
            summary: 'Não é possível excluir',
            detail: `A categoria "${item.nome}" não pode ser excluída pois possui produtos associados ou subcategorias`,
            life: 7000
          });
          return;
        }

        // Category can be removed - show confirmation dialog
        this.confirmationService.confirm({
          message: `Tem certeza que deseja excluir permanentemente a categoria "${item.nome}"?`,
          header: 'Confirmar Exclusão',
          icon: 'pi pi-exclamation-triangle',
          acceptLabel: 'Sim, Excluir',
          rejectLabel: 'Cancelar',
          acceptButtonStyleClass: 'p-button-danger',
          accept: () => {
            this.executarExclusao(item);
          }
        });
      },
      error: (error) => {
        this.setCustomActionLoading('delete', item.id, false);
        console.error('Erro ao verificar se categoria pode ser removida:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Erro',
          detail: 'Erro ao verificar se a categoria pode ser excluída',
          life: 5000
        });
      }
    });
  }

  /**
   * Execute category deletion after validation
   */
  private executarExclusao(item: CategoriaDto): void {
    this.setCustomActionLoading('delete', item.id, true);
    
    this.service.remover(item.id).subscribe({
      next: () => {
        this.setCustomActionLoading('delete', item.id, false);
        this.messageService.add({
          severity: 'success',
          summary: 'Sucesso',
          detail: 'Categoria excluída com sucesso',
          life: 3000
        });
        
        // Refresh data and dropdown after deletion
        this.carregarItens();
        this.carregarCategoriasParaDropdown();
      },
      error: (error) => {
        this.setCustomActionLoading('delete', item.id, false);
        
        if (error.originalError?.status === 409) {
          // Conflict - category has dependencies
          this.messageService.add({
            severity: 'error',
            summary: 'Não é possível excluir',
            detail: 'A categoria possui dependências e não pode ser excluída',
            life: 7000
          });
        } else {
          this.messageService.add({
            severity: 'error',
            summary: 'Erro',
            detail: 'Erro ao excluir categoria',
            life: 5000
          });
        }
      }
    });
  }

  // ========================================
  // Custom Action Loading Management
  // ========================================

  /**
   * Set custom action loading state
   */
  private setCustomActionLoading(action: string, id: number, loading: boolean): void {
    const key = `${action}-${id}`;
    const currentStates = new Map(this.actionLoadingStates());
    
    if (loading) {
      currentStates.set(key, id);
    } else {
      currentStates.delete(key);
    }
    
    this.actionLoadingStates.set(currentStates);
  }

  /**
   * Check if specific action is loading for an item
   */
  isActionLoading(action: string, id: number): boolean {
    const key = `${action}-${id}`;
    return this.actionLoadingStates().has(key);
  }

  /**
   * Check if any action is currently loading
   */
  hasAnyActionLoading(): boolean {
    return this.actionLoadingStates().size > 0;
  }

  /**
   * Get status label for display
   */
  getStatusLabel(ativo: boolean): string {
    return ativo ? 'Ativo' : 'Inativo';
  }

  /**
   * Get status severity for PrimeNG tag
   */
  getStatusSeverity(ativo: boolean): string {
    return ativo ? 'success' : 'danger';
  }

  /**
   * Cancel form editing and close modal
   */
  cancelarEdicao(): void {
    this.showForm.set(false);
    this.selectedItem.set(null);
    this.currentRowVersion.set(null);
    this.form.reset();
  }

  /**
   * Get dialog title based on mode
   */
  dialogTitle(): string {
    return this.isEditMode() ? 'Editar Categoria' : 'Nova Categoria';
  }

  /**
   * Check if component is in edit mode
   */
  isEditMode(): boolean {
    return this.selectedItem() !== null;
  }

  // ========================================
  // ERROR HANDLING AND USER FEEDBACK METHODS
  // ========================================

  /**
   * Enhanced error handling for categorias with specific business rules
   */
  private handleCategoriaError(operation: string, error: any): void {
    console.error(`CategoriasComponent: ${operation} failed:`, error);
    
    let errorMessage = 'Ocorreu um erro inesperado';
    let severity: 'error' | 'warn' | 'info' = 'error';
    let summary = 'Erro';
    
    // Handle network errors first
    if (!navigator.onLine) {
      errorMessage = 'Sem conexão com a internet. Verifique sua conexão e tente novamente.';
      severity = 'warn';
      summary = 'Conexão';
    } else if (error.status === 0) {
      errorMessage = 'Não foi possível conectar ao servidor. Tente novamente em alguns instantes.';
      severity = 'warn';
      summary = 'Conexão';
    } else if (error.error && typeof error.error === 'object') {
      // Handle structured API errors
      const apiError = error.error;
      
      if (apiError.errorCode) {
        switch (apiError.errorCode) {
          case 'CATEGORIA_NOT_FOUND':
            errorMessage = 'Categoria não encontrada. Ela pode ter sido removida por outro usuário.';
            severity = 'warn';
            summary = 'Categoria não encontrada';
            break;
          case 'CATEGORIA_NAME_DUPLICATE':
            errorMessage = 'Já existe uma categoria com este nome no mesmo nível hierárquico.';
            severity = 'warn';
            summary = 'Nome duplicado';
            break;
          case 'CIRCULAR_REFERENCE':
            errorMessage = 'Não é possível definir esta categoria como pai pois criaria uma referência circular.';
            severity = 'warn';
            summary = 'Referência circular';
            break;
          case 'CATEGORIA_HAS_PRODUCTS':
            errorMessage = 'Não é possível excluir esta categoria pois ela possui produtos associados. Desative-a ao invés de excluir.';
            severity = 'warn';
            summary = 'Categoria em uso';
            break;
          case 'CATEGORIA_HAS_SUBCATEGORIES':
            errorMessage = 'Não é possível excluir esta categoria pois ela possui subcategorias. Remova as subcategorias primeiro.';
            severity = 'warn';
            summary = 'Possui subcategorias';
            break;
          case 'PARENT_CATEGORIA_INACTIVE':
            errorMessage = 'A categoria pai selecionada está inativa. Selecione uma categoria pai ativa.';
            severity = 'warn';
            summary = 'Categoria pai inativa';
            break;
          case 'PARENT_CATEGORIA_NOT_FOUND':
            errorMessage = 'A categoria pai selecionada não foi encontrada. Ela pode ter sido removida.';
            severity = 'warn';
            summary = 'Categoria pai não encontrada';
            break;
          case 'INVALID_CATEGORIA_TYPE':
            errorMessage = 'Tipo de categoria inválido. Selecione um tipo válido.';
            severity = 'warn';
            summary = 'Tipo inválido';
            break;
          case 'CONCURRENCY_CONFLICT':
            errorMessage = 'Esta categoria foi modificada por outro usuário. A página será recarregada com os dados atuais.';
            severity = 'warn';
            summary = 'Conflito de concorrência';
            // Reload data after showing message
            setTimeout(() => this.carregarItens(), 2000);
            break;
          case 'VALIDATION_ERROR':
            if (apiError.validationErrors) {
              const validationMessages = Object.values(apiError.validationErrors).flat();
              errorMessage = validationMessages.join(', ');
            } else {
              errorMessage = 'Os dados fornecidos são inválidos. Verifique os campos e tente novamente.';
            }
            severity = 'warn';
            summary = 'Dados inválidos';
            break;
          case 'BUSINESS_RULE_VIOLATION':
            errorMessage = apiError.errorDescription || 'Operação violou uma regra de negócio.';
            severity = 'warn';
            summary = 'Regra de negócio';
            break;
          default:
            errorMessage = apiError.errorDescription || errorMessage;
        }
      } else if (apiError.errorDescription) {
        errorMessage = apiError.errorDescription;
      }
    } else if (error.status) {
      // Handle HTTP status codes
      switch (error.status) {
        case 400:
          errorMessage = 'Os dados fornecidos são inválidos. Verifique os campos obrigatórios.';
          severity = 'warn';
          summary = 'Dados inválidos';
          break;
        case 401:
          errorMessage = 'Sua sessão expirou. Você será redirecionado para o login.';
          severity = 'warn';
          summary = 'Sessão expirada';
          // Handle authentication redirect
          setTimeout(() => {
            window.location.href = '/login';
          }, 3000);
          break;
        case 403:
          errorMessage = 'Você não tem permissão para realizar esta operação.';
          severity = 'warn';
          summary = 'Acesso negado';
          break;
        case 404:
          errorMessage = 'Categoria não encontrada. Ela pode ter sido removida.';
          severity = 'warn';
          summary = 'Não encontrada';
          break;
        case 409:
          errorMessage = 'Já existe uma categoria com estes dados. Verifique o nome e tente novamente.';
          severity = 'warn';
          summary = 'Conflito de dados';
          break;
        case 412:
          errorMessage = 'Esta categoria foi modificada por outro usuário. A página será recarregada.';
          severity = 'warn';
          summary = 'Dados desatualizados';
          setTimeout(() => this.carregarItens(), 2000);
          break;
        case 422:
          errorMessage = 'Os dados fornecidos não atendem aos requisitos. Verifique os campos.';
          severity = 'warn';
          summary = 'Dados inválidos';
          break;
        case 500:
          errorMessage = 'Erro interno do servidor. Nossa equipe foi notificada.';
          break;
        case 502:
        case 503:
        case 504:
          errorMessage = 'Serviço temporariamente indisponível. Tente novamente em alguns instantes.';
          severity = 'warn';
          summary = 'Serviço indisponível';
          break;
        default:
          if (error.status >= 500) {
            errorMessage = 'Erro no servidor. Nossa equipe foi notificada.';
          } else {
            errorMessage = `Erro ${error.status}: ${error.statusText || 'Erro desconhecido'}`;
          }
      }
    }

    // Show toast message with appropriate styling
    this.messageService.add({
      severity,
      summary,
      detail: errorMessage,
      life: severity === 'error' ? 8000 : 6000,
      sticky: severity === 'error' && error.status >= 500
    });
  }

  /**
   * Show success notification for completed operations
   */
  private showSuccessMessage(operation: string, categoria?: CategoriaDto): void {
    let message = '';
    let summary = 'Sucesso';
    
    switch (operation) {
      case 'create':
        message = `Categoria "${categoria?.nome}" criada com sucesso.`;
        summary = 'Categoria criada';
        break;
      case 'update':
        message = `Categoria "${categoria?.nome}" atualizada com sucesso.`;
        summary = 'Categoria atualizada';
        break;
      case 'activate':
        message = `Categoria "${categoria?.nome}" ativada com sucesso.`;
        summary = 'Categoria ativada';
        break;
      case 'deactivate':
        message = `Categoria "${categoria?.nome}" desativada com sucesso.`;
        summary = 'Categoria desativada';
        break;
      case 'delete':
        message = `Categoria "${categoria?.nome}" excluída com sucesso.`;
        summary = 'Categoria excluída';
        break;
      case 'load':
        message = 'Categorias carregadas com sucesso.';
        summary = 'Dados carregados';
        break;
      default:
        message = 'Operação realizada com sucesso.';
    }

    this.messageService.add({
      severity: 'success',
      summary,
      detail: message,
      life: 4000
    });
  }

  /**
   * Show informational message for user guidance
   */
  private showInfoMessage(message: string, summary: string = 'Informação'): void {
    this.messageService.add({
      severity: 'info',
      summary,
      detail: message,
      life: 5000
    });
  }

  /**
   * Show warning message for user attention
   */
  private showWarningMessage(message: string, summary: string = 'Atenção'): void {
    this.messageService.add({
      severity: 'warn',
      summary,
      detail: message,
      life: 6000
    });
  }

  /**
   * Validate form before submission with detailed feedback
   */
  private validateFormBeforeSubmit(): boolean {
    if (this.form.valid) {
      return true;
    }

    // Mark all fields as touched to show validation errors
    this.validationService.markFormGroupTouched(this.form);

    // Collect all validation errors
    const validationSummary = this.validationService.getFormValidationSummary(this.form);
    
    if (!validationSummary.isValid) {
      const errorCount = validationSummary.errors.length;
      const errorMessage = errorCount === 1 
        ? 'Há 1 erro no formulário que precisa ser corrigido.'
        : `Há ${errorCount} erros no formulário que precisam ser corrigidos.`;
      
      this.showWarningMessage(errorMessage, 'Formulário inválido');
      
      // Focus on first invalid field
      this.focusFirstInvalidField();
      
      return false;
    }

    return true;
  }

  /**
   * Focus on the first invalid field in the form
   */
  private focusFirstInvalidField(): void {
    const firstInvalidField = document.querySelector('.ng-invalid:not(form)') as HTMLElement;
    if (firstInvalidField) {
      firstInvalidField.focus();
      firstInvalidField.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }
  }

  /**
   * Handle business rule validation errors
   */
  private handleBusinessRuleErrors(errors: string[]): void {
    if (errors.length === 0) return;

    const errorMessage = errors.length === 1
      ? errors[0]
      : `Múltiplas regras de negócio foram violadas:\n• ${errors.join('\n• ')}`;

    this.showWarningMessage(errorMessage, 'Regras de negócio');
  }
}