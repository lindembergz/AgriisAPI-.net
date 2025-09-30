
import { Component, OnInit, inject, signal, input, output, OnDestroy, effect, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { TableModule } from 'primeng/table';
import { TreeTableModule } from 'primeng/treetable';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { CheckboxModule } from 'primeng/checkbox';
import { ProgressBarModule } from 'primeng/progressbar';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ReferenceCrudService } from '../../services/reference-crud.service';
import { BaseEntity } from '../../models/base.model';
import { ValidationService } from '../../services/validation.service';
import { LoadingStateService, LoadingStateType } from '../../services/loading-state.service';
import { ErrorHandlingService } from '../../services/error-handling.service';
import { FeedbackService } from '../../services/feedback.service';
import { Subject, takeUntil } from 'rxjs';

import { 
  ComponentTemplate, 
  CustomFilter, 
  CustomAction, 
  EmptyStateConfig, 
  LoadingStateConfig, 
  ResponsiveConfig, 
  TableColumn, 
  ActiveFilter,
  DisplayMode
} from '../../interfaces/component-template.interface';
import { ComponentStateService } from '../../services/component-state.service';
import { UnifiedErrorHandlingService } from '../../services/unified-error-handling.service';
import { EmptyStateComponent } from '../empty-state/empty-state.component';
import { FieldErrorComponent } from '../field-error/field-error.component';
import { LoadingSpinnerComponent } from '../loading-spinner/loading-spinner.component';
import { FilterSummaryComponent } from '../filter-summary/filter-summary.component';

interface StatusFilter {
  label: string;
  value: string;
}

/**
 * Base component for reference entity CRUD operations
 * Provides common functionality for list, create, edit, and delete operations
 */
@Component({
  selector: 'app-reference-crud-base',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    TableModule,
    TreeTableModule,
    ButtonModule,
    SelectModule,
    ConfirmDialogModule,
    ToastModule,
    ProgressSpinnerModule,
    ProgressBarModule,
    TagModule,
    TooltipModule,
    DialogModule,
    InputTextModule,
    CheckboxModule,
    EmptyStateComponent,
    FieldErrorComponent,
    LoadingSpinnerComponent,
    FilterSummaryComponent
  ],
  providers: [ConfirmationService, MessageService],
  templateUrl: './reference-crud-base.component.html',
  styleUrls: ['./reference-crud-base.component.scss', '../../styles/reference-components.scss']
})
export abstract class ReferenceCrudBaseComponent<
  TDto extends BaseEntity,
  TCreateDto,
  TUpdateDto
> implements OnInit, OnDestroy {
  
  // Injected services
  protected fb = inject(FormBuilder);
  protected router = inject(Router);
  protected validationService = inject(ValidationService);
  protected loadingStateService = inject(LoadingStateService);
  protected errorHandlingService = inject(ErrorHandlingService);
  protected feedbackService = inject(FeedbackService);
  protected componentStateService = inject(ComponentStateService);
  protected unifiedErrorHandlingService = inject(UnifiedErrorHandlingService);
  protected confirmationService = inject(ConfirmationService);
  protected messageService = inject(MessageService);

  // Destroy subject for cleanup
  private destroy$ = new Subject<void>();

  constructor() {
    // Setup loading message effect in injection context
    try {
      effect(() => {
        const messages = this.loadingStateService.loadingMessages();
        if (messages.length > 0) {
          this.currentLoadingMessage.set(messages[0]);
        } else {
          this.currentLoadingMessage.set(`Carregando ${this.entityDisplayName().toLowerCase()}...`);
        }
      });
    } catch (error) {
      console.warn('Effect setup failed, using fallback loading message:', error);
      this.currentLoadingMessage.set('Carregando dados...');
    }
  }

  // Abstract properties that must be implemented by child components
  protected abstract service: ReferenceCrudService<TDto, TCreateDto, TUpdateDto>;
  protected abstract entityDisplayName: () => string;
  protected abstract entityDescription: () => string;
  protected abstract displayColumns: () => TableColumn[];
  protected abstract searchFields: () => string[];
  protected abstract defaultSortField: () => string;

  // Abstract methods that must be implemented by child components
  protected abstract createFormGroup(): FormGroup;
  protected abstract mapToCreateDto(formValue: any): TCreateDto;
  protected abstract mapToUpdateDto(formValue: any): TUpdateDto;
  protected abstract populateForm(item: TDto): void;

  // Enhanced abstract methods for unified system
  protected getDisplayMode(): DisplayMode { return 'table'; }
  protected getCustomActions(): CustomAction[] { return []; }
  protected getResponsiveBreakpoints(): ResponsiveConfig { return this.componentStateService.getResponsiveConfig(); }
  protected canPerformAction(action: string, item: TDto): boolean { return true; }
  protected hasAtivoField(): boolean { return true; }
  protected getAtivoFieldHelp(): string { return 'Indica se o registro está ativo no sistema'; }
  protected canSaveForm(): boolean { return true; }


  // Optional methods for custom filters (can be overridden by child components)
  protected getCustomFilters(): CustomFilter[] {
    return [];
  }

  protected applyCustomFilter(items: TDto[], filterKey: string, filterValue: any): TDto[] {
    return items; // Default implementation does nothing
  }

  // Enhanced configuration methods
  protected getEmptyStateConfig(): EmptyStateConfig {
    return {
      icon: 'pi pi-info-circle',
      title: `Nenhum ${this.entityDisplayName().toLowerCase()} encontrado`,
      description: `Não há ${this.entityDisplayName().toLowerCase()} cadastrados no sistema.`,
      primaryAction: {
        label: `Cadastrar Novo ${this.entityDisplayName()}`,
        icon: 'pi pi-plus',
        action: () => this.novoItem()
      }
    };
  }

  protected getLoadingStateConfig(): LoadingStateConfig {
    return {
      message: 'Carregando ' + this.entityDisplayName().toLowerCase() + '...',
      showProgress: false
    };
  }

  protected getComponentName(): string {
    return this.entityDisplayName().toLowerCase().replace(/\s+/g, '-');
  }

  protected executeCustomAction(actionKey: string): void {
    const action = this.getCustomActions().find(a => a.key === actionKey);
    if (action) {
      console.log(`Executing custom action: ${actionKey}`);
      // Child components can override this method to handle custom actions
    }
  }

  protected executeEmptyStatePrimaryAction(): void {
    this.novoItem();
  }

  protected executeEmptyStateSecondaryAction(action: any): void {
    if (action.key === 'refresh') {
      this.carregarItens();
    }
  }

  // Signals for reactive state management
  items = signal<TDto[]>([]);
  filteredItems = signal<TDto[]>([]);
  loading = signal<boolean>(false);
  tableLoading = signal<boolean>(false);
  formLoading = signal<boolean>(false);
  showForm = signal<boolean>(false);
  selectedItem = signal<TDto | null>(null);
  selectedStatusFilter = signal<string>('todas');
  searchTerm = signal<string>('');
  customFilters = signal<Map<string, any>>(new Map());
  pageSize = signal<number>(10);
  multiSortMeta = signal<any[]>([]);
  actionLoadingStates = signal<Map<string, number>>(new Map());
  highlightedRowIndex = signal<number>(-1);
  currentRowVersion = signal<string | null>(null);
  
  // Enhanced loading states
  currentLoadingMessage = signal<string>('Carregando dados...');
  loadingProgress = signal<number>(0);
  hasError = signal<boolean>(false);
  errorMessage = signal<string>('');

  // Form
  form!: FormGroup;

  // Filter options
  statusFilterOptions: StatusFilter[] = [
    { label: 'Todos os Status', value: 'todas' },
    { label: 'Apenas Ativos', value: 'ativas' },
    { label: 'Apenas Inativos', value: 'inativas' }
  ];

  ngOnInit(): void {
    try {
      this.form = this.createFormGroup();
      if (!this.form) {
        throw new Error('Form group creation failed');
      }
    } catch (error) {
      console.error('Error creating form group:', error);
      // Create a minimal form as fallback
      this.form = this.fb.group({
        ativo: [true]
      });
    }
    
    this.setupLoadingStateSubscriptions();
    this.resetFiltersOnInit();
    this.carregarItens();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.loadingStateService.clearType('table-data');
    this.loadingStateService.clearType('form-submit');
    this.loadingStateService.clearType('item-action');
    
    // Optionally clear saved filter state on navigation
    // Uncomment the line below if you want filters to reset when navigating between components
    // this.clearSavedFilterState();
  }

  /**
   * Reset filters when component initializes
   */
  private resetFiltersOnInit(): void {
    // Try to restore saved filter state first
    const restored = this.restoreFilterState();
    
    // If no saved state, initialize with defaults
    if (!restored) {
      this.selectedStatusFilter.set('todas');
      this.searchTerm.set('');
      this.customFilters.set(new Map());
    }
    
    // Setup filter state maintenance
    this.maintainFilterState();
  }

  /**
   * Setup loading state subscriptions
   */
  private setupLoadingStateSubscriptions(): void {
    // Subscribe to table loading state
    this.loadingStateService.getTypeLoading$('table-data')
      .pipe(takeUntil(this.destroy$))
      .subscribe(isLoading => {
        this.loading.set(isLoading);
        this.tableLoading.set(isLoading);
      });

    // Subscribe to form loading state
    this.loadingStateService.getTypeLoading$('form-submit')
      .pipe(takeUntil(this.destroy$))
      .subscribe(isLoading => {
        this.formLoading.set(isLoading);
      });


  }

  /**
   * Load items from API
   */
  carregarItens(): void {
    const loadingMessage = `Carregando ${this.entityDisplayName().toLowerCase()}...`;
    
    this.hasError.set(false);
    this.errorMessage.set('');
    this.loadingProgress.set(0);
    
    const request = this.loadingStateService.wrapAsync(
      'table-data',
      'load-all',
      () => this.service.obterTodos(),
      loadingMessage
    );

    // Simulate progress for better UX
    this.simulateProgress();

    request.subscribe({
      next: (items) => {
        console.log(items)
        this.items.set(items);
        this.applyFilters(); // Apply current filters to the loaded items
        this.loadingProgress.set(100);
        
        // Show info message if no items found
        if (items.length === 0) {
          this.errorHandlingService.showInfo(
            `Nenhum ${this.entityDisplayName().toLowerCase()} encontrado`,
            'Informação'
          );
        }
      },
      error: (error) => {
        console.error(`Erro ao carregar ${this.entityDisplayName().toLowerCase()}:`, error);
        this.hasError.set(true);
        this.errorMessage.set(error.message || 'Erro ao carregar dados');
        this.loadingProgress.set(0);
        this.filteredItems.set([]); // Clear filtered items on error
      }
    });
  }

  /**
   * Handle status filter change
   */
  onStatusFilterChange(event: any): void {
    this.selectedStatusFilter.set(event.value);
    this.applyFilters();
  }

  /**
   * Handle search input change
   */
  onSearchChange(event: any): void {
    this.searchTerm.set(event.target.value);
    this.applyFilters();
  }

  /**
   * Clear search input
   */
  clearSearch(): void {
    this.searchTerm.set('');
    this.applyFilters();
  }

  /**
   * Apply all filters (search + status + custom)
   */
  private applyFilters(): void {
    let filtered = [...this.items()];
    
    // Apply status filter
    const statusFilter = this.selectedStatusFilter();
    if (statusFilter === 'ativas') {
      filtered = filtered.filter(item => item.ativo);
    } else if (statusFilter === 'inativas') {
      filtered = filtered.filter(item => !item.ativo);
    }
    
    // Apply search filter
    const searchTerm = this.searchTerm().toLowerCase().trim();
    if (searchTerm) {
      const searchFields = this.searchFields();
      filtered = filtered.filter(item => {
        return searchFields.some(field => {
          const value = this.getFieldValue(item, field).toLowerCase();
          return value.includes(searchTerm);
        });
      });
    }
    
    // Apply custom filters
    const customFiltersMap = this.customFilters();
    for (const [filterKey, filterValue] of customFiltersMap.entries()) {
      if (filterValue !== null && filterValue !== undefined && filterValue !== '') {
        filtered = this.applyCustomFilter(filtered, filterKey, filterValue);
      }
    }
    
    this.filteredItems.set(filtered);
  }

  /**
   * Handle table sorting
   */
  onSort(event: any): void {
    this.multiSortMeta.set(event.multiSortMeta || []);
  }

  /**
   * Handle page change
   */
  onPageChange(event: any): void {
    this.pageSize.set(event.rows);
  }

  /**
   * Handle dialog hide event
   */
  async onDialogHide(): Promise<void> {
    if (this.hasUnsavedChanges()) {
      const confirmed = await this.feedbackService.showUnsavedChangesConfirmation();
      if (!confirmed) {
        // Prevent dialog from closing
        this.showForm.set(true);
        return;
      }
    }
    
    if (!this.formLoading()) {
      this.cancelarEdicao();
    }
  }

  /**
   * Handle custom filter change
   */
  onCustomFilterChange(filterKey: string, event: any): void {
    const currentFilters = new Map(this.customFilters());
    currentFilters.set(filterKey, event.value);
    this.customFilters.set(currentFilters);
    this.applyFilters();
  }

  /**
   * Get custom filter value
   */
  getCustomFilterValue(filterKey: string): any {
    return this.customFilters().get(filterKey) || null;
  }

  /**
   * Clear custom filter
   */
  clearCustomFilter(filterKey: string): void {
    const currentFilters = new Map(this.customFilters());
    currentFilters.delete(filterKey);
    this.customFilters.set(currentFilters);
    this.applyFilters();
    this.saveFilterState();
  }

  /**
   * Clear all filters
   */
  clearAllFilters(): void {
    this.selectedStatusFilter.set('todas');
    this.searchTerm.set('');
    this.customFilters.set(new Map());
    this.clearSavedFilterState();
    this.applyFilters();
    
    this.feedbackService.showInfo(
      'Filtros removidos. Mostrando todos os registros.',
      'Filtros Limpos'
    );
  }

  /**
   * Reset filters to default state
   */
  resetFilters(): void {
    this.selectedStatusFilter.set('ativas'); // Default to active items
    this.searchTerm.set('');
    this.customFilters.set(new Map());
    this.clearSavedFilterState();
    this.applyFilters();
    
    this.feedbackService.showInfo(
      'Filtros resetados para o padrão (apenas ativos).',
      'Filtros Resetados'
    );
  }

  /**
   * Remove specific filter from active filters
   */
  removeFilter(filterKey: string): void {
    if (filterKey === 'search') {
      this.clearSearch();
    } else if (filterKey === 'status') {
      this.selectedStatusFilter.set('todas');
      this.applyFilters();
      this.saveFilterState();
    } else {
      this.clearCustomFilter(filterKey);
    }
  }

  /**
   * Save current filter state
   */
  private saveFilterState(): void {
    const filterState = {
      statusFilter: this.selectedStatusFilter(),
      searchTerm: this.searchTerm(),
      customFilters: Object.fromEntries(this.customFilters())
    };
    
    const key = `filters_${this.constructor.name}`;
    sessionStorage.setItem(key, JSON.stringify(filterState));
  }

  /**
   * Restore filter state from session storage
   */
  private restoreFilterState(): boolean {
    const key = `filters_${this.constructor.name}`;
    const savedState = sessionStorage.getItem(key);
    
    if (savedState) {
      try {
        const filterState = JSON.parse(savedState);
        
        this.selectedStatusFilter.set(filterState.statusFilter || 'todas');
        this.searchTerm.set(filterState.searchTerm || '');
        
        if (filterState.customFilters) {
          const customFiltersMap = new Map(Object.entries(filterState.customFilters));
          this.customFilters.set(customFiltersMap);
        }
        
        return true;
      } catch (error) {
        console.warn('Error restoring filter state:', error);
        sessionStorage.removeItem(key);
      }
    }
    
    return false;
  }

  /**
   * Clear saved filter state
   */
  private clearSavedFilterState(): void {
    const key = `filters_${this.constructor.name}`;
    sessionStorage.removeItem(key);
  }

  /**
   * Open form for creating new item
   */
  novoItem(): void {
    this.selectedItem.set(null);
    this.currentRowVersion.set(null);
    this.form.reset();
    
    // Set default values for new items
    if (this.hasAtivoField()) {
      this.form.patchValue({ ativo: true });
    }
    
    this.showForm.set(true);
  }

  /**
   * Open form for editing item
   */
  editarItem(item: TDto): void {
    const loadingKey = `edit-${item.id}`;
    
    this.loadingStateService.wrapAsync(
      'item-action',
      loadingKey,
      () => this.service.obterPorId(item.id),
      `Carregando ${this.entityDisplayName().toLowerCase()} para edição...`
    ).subscribe({
      next: (freshItem) => {
        this.selectedItem.set(freshItem);
        this.currentRowVersion.set((freshItem as any).rowVersion);
        this.populateForm(freshItem);
        this.showForm.set(true);
      },
      error: (error) => {
        console.error('Erro ao carregar item para edição:', error);
        // Error is already handled by the error handling service
      }
    });
  }

  /**
   * Save item (create or update)
   */
  salvarItem(): void {
    if (this.form.invalid) {
      this.validationService.markFormGroupTouched(this.form);
      
      // Collect validation errors
      const errors = this.collectFormErrors();
      this.feedbackService.showValidationFeedback(errors, {
        showSummaryErrors: true,
        scrollToFirstError: true,
        focusFirstError: true
      });
      return;
    }

    const formValue = this.form.value;
    
    if (this.isEditMode()) {
      // Update existing item
      const updateDto = this.mapToUpdateDto(formValue);
      const itemId = this.selectedItem()!.id;
      const rowVersion = this.currentRowVersion();
      
      this.loadingStateService.wrapAsync(
        'form-submit',
        'update',
        () => this.service.atualizar(itemId, updateDto, rowVersion || undefined),
        `Atualizando ${this.entityDisplayName().toLowerCase()}...`
      ).subscribe({
        next: (updatedItem) => {
          this.showForm.set(false);
          this.feedbackService.showSuccess({
            message: this.entityDisplayName() + ' atualizado com sucesso',
            summary: 'Atualização Realizada'
          });
          this.carregarItens();
          this.saveFilterState();
        },
        error: (error) => {
          if (error.originalError?.status === 412) {
            // Concurrency conflict - reload item
            this.errorHandlingService.showWarning(
              'Este registro foi modificado por outro usuário. Recarregando dados atuais...', 
              'Conflito de Concorrência'
            );
            this.editarItem(this.selectedItem()!);
          }
          // Other errors are already handled by the error handling service
        }
      });
    } else {
      // Create new item
      const createDto = this.mapToCreateDto(formValue);
      
      this.loadingStateService.wrapAsync(
        'form-submit',
        'create',
        () => this.service.criar(createDto),
        `Criando ${this.entityDisplayName().toLowerCase()}...`
      ).subscribe({
        next: (newItem) => {
          this.showForm.set(false);
          this.feedbackService.showSuccess({
            message: this.entityDisplayName() + ' criado com sucesso',
            summary: 'Criação Realizada',
            includeUndo: false // Could be enabled for specific entities
          });
          this.carregarItens();
          this.saveFilterState();
        },
        error: (error) => {
          // Error is already handled by the service
        }
      });
    }
  }

  /**
   * Cancel form editing
   */
  cancelarEdicao(): void {
    this.showForm.set(false);
    this.selectedItem.set(null);
    this.currentRowVersion.set(null);
    this.form.reset();
  }

  /**
   * Activate item with confirmation
   */
  async ativarItem(item: TDto): Promise<void> {
    try {
      // Get custom warning message if any
      const warningMessage = this.getStatusChangeWarning(item, true);
      let confirmationMessage = `Tem certeza que deseja ativar este ${this.entityDisplayName().toLowerCase()}?`;
      
      if (warningMessage) {
        confirmationMessage += `\n\n⚠️ ${warningMessage}`;
      }

      const confirmed = await this.feedbackService.showConfirmation({
        message: confirmationMessage,
        header: 'Confirmar Ativação',
        icon: 'pi pi-question-circle',
        acceptLabel: 'Ativar',
        rejectLabel: 'Cancelar',
        acceptButtonStyleClass: 'p-button-success',
        rejectButtonStyleClass: 'p-button-text'
      });
      
      if (confirmed) {
        this.confirmarAtivacao(item);
      }
    } catch (error) {
      console.error('Erro ao processar ativação:', error);
      this.feedbackService.showError(
        'Erro ao processar ativação. Tente novamente.',
        'Erro de Operação'
      );
    }
  }

  /**
   * Execute item activation
   */
  private confirmarAtivacao(item: TDto): void {
    const loadingKey = `activate-${item.id}`;
    
    // Optimistically update the UI
    this.updateItemStatusOptimistically(item, true);
    
    this.loadingStateService.wrapAsync(
      'item-action',
      loadingKey,
      () => this.service.ativar(item.id),
      `Ativando ${this.entityDisplayName().toLowerCase()}...`
    ).subscribe({
      next: () => {
        this.feedbackService.showSuccess({
          message: this.entityDisplayName() + ' ativado com sucesso',
          summary: 'Ativação Realizada',
          includeUndo: true,
          undoAction: () => this.confirmarDesativacao(item),
          undoLabel: 'Desativar'
        });
        
        // Highlight the row temporarily
        this.highlightRowTemporarily(item);
        
        // Refresh data to ensure consistency
        this.refreshItemData(item);
      },
      error: (error) => {
        // Revert optimistic update on error
        this.updateItemStatusOptimistically(item, false);
        // Error is already handled by the service
      }
    });
  }

  /**
   * Deactivate item with enhanced confirmation and dependency checking
   */
  async desativarItem(item: TDto): Promise<void> {
    // Check for dependencies before showing confirmation
    try {
      const dependencyInfo = await this.checkDeactivationDependencies(item);
      if (dependencyInfo && !dependencyInfo.canDeactivate) {
        // Show dependency warning
        this.feedbackService.showWarning(
          dependencyInfo.message || `Este ${this.entityDisplayName().toLowerCase()} está sendo usado por outros registros e não pode ser desativado`,
          'Desativação Não Permitida'
        );
        return;
      }

      // Get custom warning message if any
      const warningMessage = this.getStatusChangeWarning(item, false);
      let confirmationMessage = `Tem certeza que deseja desativar este ${this.entityDisplayName().toLowerCase()}?`;
      
      if (warningMessage) {
        confirmationMessage += `\n\n⚠️ ${warningMessage}`;
      }

      if (dependencyInfo && dependencyInfo.warningMessage) {
        confirmationMessage += `\n\n⚠️ ${dependencyInfo.warningMessage}`;
      }

      const confirmed = await this.feedbackService.showConfirmation({
        message: confirmationMessage,
        header: 'Confirmar Desativação',
        icon: 'pi pi-exclamation-triangle',
        acceptLabel: 'Desativar',
        rejectLabel: 'Cancelar',
        acceptButtonStyleClass: 'p-button-warning',
        rejectButtonStyleClass: 'p-button-text'
      });
      
      if (confirmed) {
        this.confirmarDesativacao(item);
      }
    } catch (error) {
      console.error('Erro ao verificar dependências para desativação:', error);
      this.feedbackService.showError(
        'Erro ao verificar dependências. Tente novamente.',
        'Erro de Validação'
      );
    }
  }

  /**
   * Check if item can be deactivated (override in child components if needed)
   */
  protected async checkDeactivationDependencies(item: TDto): Promise<{
    canDeactivate: boolean;
    message?: string;
    warningMessage?: string;
  } | null> {
    // Default implementation - no dependency checks
    // Override in child components that need to check dependencies
    // Return null to indicate no dependency checks needed
    return null;
  }

  /**
   * Execute item deactivation
   */
  private confirmarDesativacao(item: TDto): void {
    const loadingKey = `deactivate-${item.id}`;
    
    // Optimistically update the UI
    this.updateItemStatusOptimistically(item, false);
    
    this.loadingStateService.wrapAsync(
      'item-action',
      loadingKey,
      () => this.service.desativar(item.id),
      `Desativando ${this.entityDisplayName().toLowerCase()}...`
    ).subscribe({
      next: () => {
        this.feedbackService.showSuccess({
          message: this.entityDisplayName() + ' desativado com sucesso',
          summary: 'Desativação Realizada',
          includeUndo: true,
          undoAction: () => this.confirmarAtivacao(item),
          undoLabel: 'Reativar'
        });
        
        // Highlight the row temporarily
        this.highlightRowTemporarily(item);
        
        // Refresh data to ensure consistency
        this.refreshItemData(item);
      },
      error: (error) => {
        // Revert optimistic update on error
        this.updateItemStatusOptimistically(item, true);
        // Error is already handled by the service
      }
    });
  }

  /**
   * Confirm and delete item with enhanced feedback
   */
  async excluirItem(item: TDto): Promise<void> {
    try {
      // First check if item can be removed
      const canRemove = await this.service.podeRemover(item.id).toPromise();
      
      if (!canRemove) {
        this.feedbackService.showWarning(
          `Este ${this.entityDisplayName().toLowerCase()} está sendo usado por outros registros e não pode ser excluído`,
          'Exclusão Não Permitida'
        );
        return;
      }

      // Show enhanced delete confirmation
      const confirmed = await this.feedbackService.showDeleteConfirmation(
        `este ${this.entityDisplayName().toLowerCase()}`
      );
      
      if (confirmed) {
        this.confirmarExclusao(item);
      }
    } catch (error) {
      console.error('Erro ao verificar se pode remover:', error);
      this.feedbackService.showError(
        'Erro ao verificar se o item pode ser removido',
        'Erro de Validação'
      );
    }
  }

  /**
   * Execute item deletion
   */
  private confirmarExclusao(item: TDto): void {
    const loadingKey = `delete-${item.id}`;
    
    this.loadingStateService.wrapAsync(
      'item-action',
      loadingKey,
      () => this.service.remover(item.id),
      `Excluindo ${this.entityDisplayName().toLowerCase()}...`
    ).subscribe({
      next: () => {
        this.feedbackService.showSuccess({
          message: `${this.entityDisplayName()} excluído com sucesso`,
          summary: 'Exclusão Realizada'
        });
        this.carregarItens();
      },
      error: (error) => {
        // Error is already handled by the service
      }
    });
  }



  /**
   * Check if in edit mode
   */
  isEditMode(): boolean {
    try {
      return this.selectedItem() !== null;
    } catch (error) {
      console.warn('Error checking edit mode:', error);
      return false;
    }
  }

  /**
   * Get dialog title based on mode
   */
  dialogTitle(): string {
    return this.isEditMode() 
      ? `Editar ${this.entityDisplayName()}` 
      : `Novo ${this.entityDisplayName()}`;
  }

  // Enhanced action management methods
  isActionLoading(action: string, itemId: number): boolean {
    const key = `${action}-${itemId}`;
    return this.actionLoadingStates().has(key);
  }

  hasAnyActionLoading(): boolean {
    return this.actionLoadingStates().size > 0;
  }

  setActionLoading(action: string, itemId: number, loading: boolean): void {
    const key = `${action}-${itemId}`;
    const currentStates = new Map(this.actionLoadingStates());
    
    if (loading) {
      currentStates.set(key, Date.now());
    } else {
      currentStates.delete(key);
    }
    
    this.actionLoadingStates.set(currentStates);
  }

  isRowHighlighted(rowIndex: number): boolean {
    return this.highlightedRowIndex() === rowIndex;
  }

  canDeactivateItem(item: TDto): boolean {
    return this.canPerformAction('deactivate', item);
  }

  canActivateItem(item: TDto): boolean {
    return this.canPerformAction('activate', item);
  }

  getDeactivateTooltip(item: TDto): string {
    return `Desativar ${this.entityDisplayName().toLowerCase()}`;
  }

  getActivateTooltip(item: TDto): string {
    return `Ativar ${this.entityDisplayName().toLowerCase()}`;
  }

  /**
   * Get dialog title
   */
  /**
   * Format date for display
   */
  formatarData(data: Date | string): string {
    if (!data) return '-';
    return new Date(data).toLocaleDateString('pt-BR');
  }

  /**
   * Get status label for display
   */
  getStatusLabel(ativo: boolean): string {
    return ativo ? 'Ativo' : 'Inativo';
  }

  /**
   * Get status severity for PrimeNG styling
   */
  getStatusSeverity(ativo: boolean): string {
    return ativo ? 'success' : 'danger';
  }

  /**
   * Get field value with null safety
   */
  getFieldValue(item: any, field: string): string {
    const value = field.split('.').reduce((obj, key) => obj?.[key], item);
    return value ?? '-';
  }

  /**
   * Get validation error message for form control
   */
  getErrorMessage(controlName: string): string {
    try {
      if (!this.form) return '';
      const control = this.form.get(controlName);
      if (!control) return '';
      return this.validationService.getErrorMessage(control, controlName);
    } catch (error) {
      console.warn(`Error getting error message for ${controlName}:`, error);
      return '';
    }
  }

  /**
   * Check if form control should show error
   */
  shouldShowError(controlName: string): boolean {
    try {
      if (!this.form) return false;
      const control = this.form.get(controlName);
      if (!control) return false;
      return this.validationService.shouldShowError(control);
    } catch (error) {
      console.warn(`Error checking should show error for ${controlName}:`, error);
      return false;
    }
  }

  // Enhanced responsive and state management methods
  isMobile(): boolean {
    return this.componentStateService.isMobile();
  }

  isTablet(): boolean {
    return this.componentStateService.isTablet();
  }

  isDesktop(): boolean {
    return this.componentStateService.isDesktop();
  }

  getVisibleColumns(): TableColumn[] {
    // Return cached computed visible columns to avoid creating new arrays on each CD cycle
    try {
      return this._visibleColumns();
    } catch (error) {
      // Fallback in case computed isn't available
      return this.componentStateService.getVisibleColumns(this.displayColumns());
    }
  }

  // Computed signal that derives visible columns from displayColumns() and component state
  private _visibleColumns = computed(() => {
    try {
      const cols = this.displayColumns();
      return this.componentStateService.getVisibleColumns(cols as TableColumn[]);
    } catch (error) {
      return [] as TableColumn[];
    }
  });

  private _dialogStyle = computed(() => {
    try {
      return this.componentStateService.getDialogStyle();
    } catch (error) {
      console.warn('Error computing dialog style:', error);
      return {};
    }
  });

  getDialogStyle(): { [key: string]: string } {
    return this._dialogStyle();
  }

  getPageReportTemplate(): string {
    return this.componentStateService.getPageReportTemplate(this.entityDisplayName());
  }

  // Computed signal for active filters summary
  private _activeFilters = computed(() => {
    try {
      return this.componentStateService.getActiveFiltersSummary(this.getComponentName());
    } catch (error) {
      console.warn('Error computing active filters summary:', error);
      return [];
    }
  });

  getActiveFiltersSummary(): ActiveFilter[] {
    return this._activeFilters();
  }

  hasActiveFilters(): boolean {
    return this.componentStateService.hasActiveFilters(this.getComponentName());
  }

  private getNestedValue(obj: any, path: string): any {
    return path.split('.').reduce((current, key) => current?.[key], obj);
  }

  /**
   * Simulate progress for better UX during loading
   */
  private simulateProgress(): void {
    this.loadingProgress.set(0);
    
    const progressInterval = setInterval(() => {
      const currentProgress = this.loadingProgress();
      if (currentProgress < 90) {
        this.loadingProgress.set(currentProgress + Math.random() * 15);
      } else {
        clearInterval(progressInterval);
      }
    }, 200);

    // Clear interval after 10 seconds to prevent memory leaks
    setTimeout(() => clearInterval(progressInterval), 10000);
  }

  /**
   * Enhanced filter state management
   */
  private maintainFilterState(): void {
    // Save filter state when filters change
    // Note: Signals don't have subscribe method, filter state is saved on change events
  }

  /**
   * Collect form validation errors
   */
  private collectFormErrors(): string[] {
    const errors: string[] = [];
    
    Object.keys(this.form.controls).forEach(key => {
      const control = this.form.get(key);
      if (control && control.invalid && control.touched) {
        const errorMessage = this.getErrorMessage(key);
        if (errorMessage) {
          errors.push(errorMessage);
        }
      }
    });
    
    return errors;
  }

  /**
   * Check if form has unsaved changes
   */
  hasUnsavedChanges(): boolean {
    return this.form.dirty && !this.formLoading();
  }

  /**
   * Show contextual help for the current entity
   */
  showHelp(): void {
    const helpMessage = this.getEntityHelpText();
    if (helpMessage) {
      this.feedbackService.showHelp(helpMessage, `Ajuda - ${this.entityDisplayName()}`);
    }
  }

  /**
   * Get entity-specific help text (override in child components)
   */
  protected getEntityHelpText(): string | null {
    return `${this.entityDisplayName()} são dados de referência utilizados em todo o sistema. Mantenha-os atualizados para garantir o funcionamento correto.`;
  }

  protected getStatusChangeWarning(item: TDto, activating: boolean): string | null {
    return null;
  }

  protected updateItemStatusOptimistically(item: TDto, newStatus: boolean): void {
    const items = this.items();
    const index = items.findIndex(i => i.id === item.id);
    if (index !== -1) {
      const updatedItem = { ...items[index], ativo: newStatus };
      const updatedItems = [...items];
      updatedItems[index] = updatedItem;
      this.items.set(updatedItems);
      this.applyFilters();
    }
  }

  protected highlightRowTemporarily(item: TDto): void {
    const index = this.filteredItems().findIndex(i => i.id === item.id);
    if (index !== -1) {
      this.highlightedRowIndex.set(index);
      setTimeout(() => {
        this.highlightedRowIndex.set(-1);
      }, 2000);
    }
  }

  protected refreshItemData(item: TDto): void {
    this.service.obterPorId(item.id).subscribe(freshItem => {
      const items = this.items();
      const index = items.findIndex(i => i.id === item.id);
      if (index !== -1) {
        const updatedItems = [...items];
        updatedItems[index] = freshItem;
        this.items.set(updatedItems);
        this.applyFilters();
      }
    });
  }
}
