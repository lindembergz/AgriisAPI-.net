import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, ReactiveFormsModule, Validators, FormsModule } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { DialogModule } from 'primeng/dialog';
import { CheckboxModule } from 'primeng/checkbox';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ReferenceCrudBaseComponent } from '../../../shared/components/reference-crud-base/reference-crud-base.component';
import { 
  AtividadeAgropecuariaDto, 
  CriarAtividadeAgropecuariaDto, 
  AtualizarAtividadeAgropecuariaDto,
  TipoAtividadeAgropecuaria 
} from '../../../shared/models/reference.model';
import { CustomFilter, CustomAction, EmptyStateConfig, LoadingStateConfig, TableColumn } from '../../../shared/interfaces/component-template.interface';
import { AtividadeAgropecuariaService } from './services/atividade-agropecuaria.service';
import { map } from 'rxjs/operators';
import { of } from 'rxjs';
import { FieldErrorComponent } from '../../../shared/components/field-error/field-error.component';
import { FilterSummaryComponent } from '../../../shared/components/filter-summary/filter-summary.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';



/**
 * Component for managing Atividades Agropecuárias (Agricultural Activities) with CRUD operations
 * Extends ReferenceCrudBaseComponent for consistent behavior
 * Features type filtering and grouping functionality
 */
@Component({
  selector: 'app-atividades-agropecuarias',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    InputTextModule,
    SelectModule,
    TableModule,
    ButtonModule,
    ConfirmDialogModule,
    ToastModule,
    ProgressSpinnerModule,
    TagModule,
    TooltipModule,
    DialogModule,
    CheckboxModule,
    FieldErrorComponent,
    FilterSummaryComponent,
    LoadingSpinnerComponent
  ],
  providers: [ConfirmationService, MessageService],
  templateUrl: './atividades-agropecuarias.component.html',
  styleUrls: ['./atividades-agropecuarias.component.scss']
})
export class AtividadesAgropecuariasComponent extends ReferenceCrudBaseComponent<
  AtividadeAgropecuariaDto,
  CriarAtividadeAgropecuariaDto,
  AtualizarAtividadeAgropecuariaDto
> implements OnInit {
  
  protected service = inject(AtividadeAgropecuariaService);

  // Type filtering signals
  selectedTipoFilter = signal<TipoAtividadeAgropecuaria | null>(null);
  tipoOptions = signal(this.service.getTipoOptions());
  
  // Grouped view signals
  showGroupedView = signal<boolean>(false);
  groupedActivities = signal<{ tipo: TipoAtividadeAgropecuaria; tipoDescricao: string; atividades: AtividadeAgropecuariaDto[] }[]>([]);
  
  // Current item for template access
  currentItem: AtividadeAgropecuariaDto | null = null;

  // Entity configuration
  protected entityDisplayName = () => 'Atividade Agropecuária';
  protected entityDescription = () => 'Gerenciar atividades agropecuárias com agrupamento por tipo';
  protected defaultSortField = () => 'codigo';
  protected searchFields = () => ['codigo', 'descricao'];

  protected getCustomFilters(): CustomFilter[] {
    return [
      {
        key: 'tipo',
        label: 'Tipo de Atividade',
        placeholder: 'Selecione um tipo',
        type: 'select',
        visible: true,
        options: [
          { label: 'Todos os tipos', value: null },
          ...this.tipoOptions().map(tipo => ({
            label: tipo.label,
            value: tipo.value
          }))
        ]
      }
    ];
  }

  protected getCustomActions(): CustomAction[] {
    return [
      {
        key: 'toggle-view',
        label: this.showGroupedView() ? 'Visualização Lista' : 'Visualização Agrupada',
        icon: this.showGroupedView() ? 'pi pi-list' : 'pi pi-th-large',
        tooltip: 'Alternar entre visualização agrupada e lista'
      }
    ];
  }

  protected getEmptyStateConfig(): EmptyStateConfig {
    return {
      icon: 'pi pi-briefcase',
      title: 'Nenhuma atividade agropecuária encontrada',
      description: this.hasActiveFilters() 
        ? 'Não há atividades que atendam aos filtros aplicados.'
        : 'Não há atividades agropecuárias cadastradas no sistema.',
      primaryAction: {
        label: 'Nova Atividade Agropecuária',
        icon: 'pi pi-plus',
        action: () => this.novoItem()
      },
      secondaryActions: this.hasActiveFilters() ? [{
        label: 'Limpar Filtros',
        icon: 'pi pi-filter-slash',
        action: () => this.clearAllFilters()
      }] : []
    };
  }

  protected getLoadingStateConfig(): LoadingStateConfig {
    return {
      message: 'Carregando atividades agropecuárias...',
      showProgress: false
    };
  }

  // Table columns configuration
  protected displayColumns = (): TableColumn[] => [
    {
      field: 'codigo',
      header: 'Código',
      sortable: true,
      width: '120px',
      type: 'text'
    },
    {
      field: 'descricao',
      header: 'Descrição',
      sortable: true,
      width: '300px',
      type: 'text'
    },
    {
      field: 'tipoDescricao',
      header: 'Tipo',
      sortable: true,
      width: '120px',
      type: 'text'
    }/*,
    {
      field: 'ativo',
      header: 'Status',
      sortable: true,
      width: '100px',
      type: 'boolean',
      hideOnMobile: true
    },
    {
      field: 'dataCriacao',
      header: 'Criado em',
      sortable: true,
      width: '150px',
      type: 'date',
      hideOnMobile: true,
      hideOnTablet: true
    }*/
  ];

  ngOnInit(): void {
    super.ngOnInit();
    this.carregarItens(); // Explicitly call to load items
    this.setupCustomFilters();
  }

  protected setupCustomFilters(): void {
    // Setup tipo filter behavior
  }

  /**
   * Create reactive form with validation
   */
  protected createFormGroup(): FormGroup {
    return this.fb.group({
      codigo: ['', [
        Validators.required,
        Validators.minLength(2),
        Validators.maxLength(10),
        Validators.pattern(/^[A-Z0-9]+$/)
      ]],
      descricao: ['', [
        Validators.required,
        Validators.minLength(5),
        Validators.maxLength(200)
      ]],
      tipo: [null, [
        Validators.required
      ]],
      ativo: [true]
    });
  }

  /**
   * Map form values to create DTO
   */
  protected mapToCreateDto(formValue: any): CriarAtividadeAgropecuariaDto {
    return {
      codigo: formValue.codigo?.toUpperCase().trim(),
      descricao: formValue.descricao?.trim(),
      tipo: formValue.tipo
    };
  }

  /**
   * Map form values to update DTO
   */
  protected mapToUpdateDto(formValue: any): AtualizarAtividadeAgropecuariaDto {
    return {
      descricao: formValue.descricao?.trim(),
      tipo: formValue.tipo,
      ativo: formValue.ativo
    };
  }

  /**
   * Populate form with entity data for editing
   */
protected populateForm(item: AtividadeAgropecuariaDto): void {
    this.form.patchValue({
      codigo: item.codigo,
      descricao: item.descricao,
      tipo: item.tipo,
      ativo: item.ativo
    });
  }

  /**
   * Handle tipo filter change
   */
  onTipoFilterChange(event?: any): void {
    const value = event?.value !== undefined ? event.value : this.selectedTipoFilter();
    this.selectedTipoFilter.set(value);
    this.applyItemFilters();
  }

  /**
   * Load activities grouped by type
   */
  private loadGroupedActivities(): void {
    this.updateGroupedActivities(this.items());
    // Keep grouped view state as is, don't auto-change
  }

  /**
   * Get icon for activity type
   */
  getTypeIcon(tipo: TipoAtividadeAgropecuaria): string {
    switch (tipo) {
      case TipoAtividadeAgropecuaria.Agricultura:
        return 'pi pi-sun';
      case TipoAtividadeAgropecuaria.Pecuaria:
        return 'pi pi-heart';
      case TipoAtividadeAgropecuaria.Mista:
        return 'pi pi-star';
      default:
        return 'pi pi-circle';
    }
  }

  /**
   * Get color for activity type
   */
  getTypeColor(tipo: TipoAtividadeAgropecuaria): string {
    switch (tipo) {
      case TipoAtividadeAgropecuaria.Agricultura:
        return '#4CAF50';
      case TipoAtividadeAgropecuaria.Pecuaria:
        return '#FF9800';
      case TipoAtividadeAgropecuaria.Mista:
        return '#9C27B0';
      default:
        return '#757575';
    }
  }

  /**
   * Get tipo filter options
   */
  tipoFilterOptions = () => [
    { label: 'Todos os tipos', value: null },
    ...this.tipoOptions().map(tipo => ({
      label: tipo.label,
      value: tipo.value
    }))
  ];

  /**
   * Toggle between grouped and list view
   */
  toggleView(): void {
    const newGroupedView = !this.showGroupedView();
    this.showGroupedView.set(newGroupedView);
    
    if (newGroupedView) {
      this.loadGroupedActivities();
    }
  }

  /**
   * Override carregarItens to refresh grouped view as well
   */
  public carregarItens(): void {
    this.loading.set(true);
    this.tableLoading.set(true);
    
    this.service.obterTodos().subscribe({
      next: (items: AtividadeAgropecuariaDto[]) => {
        this.items.set(items);
        this.applyItemFilters();
        this.loading.set(false);
        this.tableLoading.set(false);
        
        // Refresh grouped view
        this.updateGroupedActivities(items);
      },
      error: (error: any) => {
        console.error('Erro ao carregar atividades agropecuárias:', error);
        this.messageService.add({ severity: 'error', summary: 'Erro', detail: 'Erro ao carregar atividades agropecuárias. Tente novamente.' });
        this.loading.set(false);
        this.tableLoading.set(false);
      }
    });
  }

  /**
   * Update grouped activities based on current items
   */
  private updateGroupedActivities(items: AtividadeAgropecuariaDto[]): void {
    const grupos = [
      {
        tipo: TipoAtividadeAgropecuaria.Agricultura,
        tipoDescricao: 'Agricultura',
        atividades: items.filter(item => item.tipo === TipoAtividadeAgropecuaria.Agricultura)
      },
      {
        tipo: TipoAtividadeAgropecuaria.Pecuaria,
        tipoDescricao: 'Pecuária',
        atividades: items.filter(item => item.tipo === TipoAtividadeAgropecuaria.Pecuaria)
      },
      {
        tipo: TipoAtividadeAgropecuaria.Mista,
        tipoDescricao: 'Mista',
        atividades: items.filter(item => item.tipo === TipoAtividadeAgropecuaria.Mista)
      }
    ].filter(group => group.atividades.length > 0);
    
    this.groupedActivities.set(grupos);
  }

  /**
   * Override clearAllFilters to handle custom filters
   */
  public clearAllFilters(): void {
    this.selectedTipoFilter.set(null);
    this.searchTerm.set('');
    this.selectedStatusFilter.set('todas');
    this.applyItemFilters();
  }

  /**
   * Override hasActiveFilters to include custom filters
   */
  public hasActiveFilters(): boolean {
    return this.selectedTipoFilter() !== null || 
           this.searchTerm() !== '' || 
           this.selectedStatusFilter() !== 'todas';
  }

  // Additional methods needed by template

  /**
   * Get dialog title
   */
  dialogTitle(): string {
    return this.selectedItem() ? `Editar ${this.entityDisplayName()}` : `Nova ${this.entityDisplayName()}`;
  }

  /**
   * Check if is edit mode
   */
  isEditMode(): boolean {
    return this.selectedItem() !== null;
  }

  /**
   * Get dialog style
   */
  getDialogStyle(): any {
    return { width: '500px' };
  }

  /**
   * Get page report template
   */
  getPageReportTemplate(): string {
    return 'Mostrando {first} a {last} de {totalRecords} atividades';
  }

  /**
   * Check if mobile
   */
  isMobile(): boolean {
    return window.innerWidth < 768;
  }

  /**
   * Check if tablet
   */
  isTablet(): boolean {
    return window.innerWidth >= 768 && window.innerWidth < 1024;
  }

  /**
   * Check if row is highlighted
   */
  isRowHighlighted(rowIndex: number): boolean {
    return this.highlightedRowIndex() === rowIndex;
  }

  /**
   * Get status label
   */
  getStatusLabel(ativo: boolean): string {
    return ativo ? 'Ativo' : 'Inativo';
  }

  /**
   * Get status severity
   */
  getStatusSeverity(ativo: boolean): string {
    return ativo ? 'success' : 'danger';
  }

  /**
   * Format date
   */
  formatarData(data: string | Date): string {
    if (!data) return '-';
    const date = new Date(data);
    return date.toLocaleDateString('pt-BR');
  }

  /**
   * Get field value
   */
  getFieldValue(item: any, field: string): string {
    const value = field.split('.').reduce((obj, key) => obj?.[key], item);
    return value || '-';
  }

  /**
   * Check if action is loading
   */
  isActionLoading(action: string, itemId: number): boolean {
    return this.actionLoadingStates().get(`${action}-${itemId}`) !== undefined;
  }

  /**
   * Check if any action is loading
   */
  hasAnyActionLoading(): boolean {
    return this.actionLoadingStates().size > 0;
  }

  /**
   * Get active filters summary
   */
  getActiveFiltersSummary(): any[] {
    const filters = [];
    
    if (this.searchTerm()) {
      filters.push({
        key: 'search',
        label: `Busca: "${this.searchTerm()}"`,
        removable: true
      });
    }
    
    if (this.selectedStatusFilter() !== 'todas') {
      const statusLabel = this.statusFilterOptions.find(s => s.value === this.selectedStatusFilter())?.label;
      filters.push({
        key: 'status',
        label: `Status: ${statusLabel}`,
        removable: true
      });
    }
    
    const tipoFilter = this.selectedTipoFilter();
    if (tipoFilter !== null) {
      const tipoLabel = this.service.getTipoDescricao(tipoFilter);
      filters.push({
        key: 'tipo',
        label: `Tipo: ${tipoLabel}`,
        removable: true
      });
    }
    
    return filters;
  }

  /**
   * Remove specific filter
   */
  removeFilter(filterKey: string): void {
    switch (filterKey) {
      case 'search':
        this.clearSearch();
        break;
      case 'status':
        this.selectedStatusFilter.set('todas');
        break;
      case 'tipo':
        this.selectedTipoFilter.set(null);
        this.onTipoFilterChange();
        break;
    }
  }

  /**
   * Handle search change
   */
  onSearchChange(event: any): void {
    this.searchTerm.set(event.target.value);
    this.applyItemFilters();
  }

  /**
   * Clear search
   */
  clearSearch(): void {
    this.searchTerm.set('');
  }

  /**
   * Handle status filter change
   */
  onStatusFilterChange(event: any): void {
    this.selectedStatusFilter.set(event.value);
    this.applyItemFilters();
  }

  // Additional signals needed by template (these should be in base component)
  items = signal<AtividadeAgropecuariaDto[]>([]);
  loading = signal<boolean>(false);
  pageSize = signal<number>(10);
  multiSortMeta = signal<any[]>([]);
  actionLoadingStates = signal<Map<string, number>>(new Map());
  highlightedRowIndex = signal<number>(-1);
  currentLoadingMessage = signal<string>('Carregando atividades...');
  tableLoading = signal<boolean>(false);
  showForm = signal<boolean>(false);
  formLoading = signal<boolean>(false);
  selectedItem = signal<AtividadeAgropecuariaDto | null>(null);
  searchTerm = signal<string>('');
  selectedStatusFilter = signal<string>('todas');
  
  // Status filter options
  statusFilterOptions = [
    { label: 'Todas', value: 'todas' },
    { label: 'Ativas', value: 'ativas' },
    { label: 'Inativas', value: 'inativas' }
  ];

  // Filtered items signal
  filteredItems = signal<AtividadeAgropecuariaDto[]>([]);

  /**
   * Apply filters to items
   */
  private applyItemFilters(): void {
    let items = this.items();
    
    // Apply search filter
    const search = this.searchTerm().toLowerCase();
    if (search) {
      items = items.filter(item => 
        item.codigo.toLowerCase().includes(search) ||
        item.descricao.toLowerCase().includes(search)
      );
    }
    
    // Apply status filter
    const statusFilter = this.selectedStatusFilter();
    if (statusFilter === 'ativas') {
      items = items.filter(item => item.ativo);
    } else if (statusFilter === 'inativas') {
      items = items.filter(item => !item.ativo);
    }
    
    // Apply tipo filter
    const tipoFilter = this.selectedTipoFilter();
    if (tipoFilter !== null) {
      items = items.filter(item => item.tipo === tipoFilter);
    }
    
    this.filteredItems.set(items);
  }

  /**
   * Handle sort event
   */
  onSort(event: any): void {
    this.multiSortMeta.set(event.multiSortMeta || []);
  }

  /**
   * Handle page change event
   */
  onPageChange(event: any): void {
    this.pageSize.set(event.rows);
  }

  /**
   * Get visible columns
   */
  getVisibleColumns() {
    return this.displayColumns();
  }

  /**
   * Check if can save form
   */
  canSaveForm(): boolean {
    return this.form.valid && !this.formLoading();
  }

  /**
   * Cancel editing
   */
  cancelarEdicao(): void {
    this.showForm.set(false);
    this.selectedItem.set(null);
    this.form.reset();
  }

  /**
   * Save item
   */
  async salvarItem(): Promise<void> {
    if (!this.canSaveForm()) return;

    this.formLoading.set(true);
    
    try {
      const formValue = this.form.value;
      
      if (this.isEditMode()) {
        const updateDto = this.mapToUpdateDto(formValue);
        await this.service.atualizar(this.selectedItem()!.id, updateDto).toPromise();
        this.messageService.add({ severity: 'success', summary: 'Sucesso', detail: 'Atividade atualizada com sucesso!' });
      } else {
        const createDto = this.mapToCreateDto(formValue);
        await this.service.criar(createDto).toPromise();
        this.messageService.add({ severity: 'success', summary: 'Sucesso', detail: 'Atividade criada com sucesso!' });
      }
      
      this.cancelarEdicao();
      this.carregarItens();
    } catch (error) {
      console.error('Erro ao salvar atividade:', error);
      this.messageService.add({ severity: 'error', summary: 'Erro', detail: 'Erro ao salvar atividade. Tente novamente.' });
    } finally {
      this.formLoading.set(false);
    }
  }

  /**
   * New item
   */
  novoItem(): void {
    this.selectedItem.set(null);
    this.form.reset({ ativo: true });
    this.showForm.set(true);
  }

  /**
   * Edit item
   */
  editarItem(item: AtividadeAgropecuariaDto): void {
    this.selectedItem.set(item);
    this.populateForm(item);
    this.showForm.set(true);
  }

  /**
   * Deactivate item
   */
  async desativarItem(item: AtividadeAgropecuariaDto): Promise<void> {
    this.confirmationService.confirm({
      message: `Tem certeza que deseja desativar a atividade "${item.descricao}"?`,
      header: 'Confirmar Desativação',
      icon: 'pi pi-exclamation-triangle',
      accept: async () => {
        try {
          const actionKey = `deactivate-${item.id}`;
          this.actionLoadingStates().set(actionKey, Date.now());
          
          const updateDto: AtualizarAtividadeAgropecuariaDto = {
            descricao: item.descricao,
            tipo: item.tipo,
            ativo: false
          };
          
          await this.service.atualizar(item.id, updateDto).toPromise();
          this.messageService.add({ severity: 'success', summary: 'Sucesso', detail: 'Atividade desativada com sucesso!' });
          this.carregarItens();
        } catch (error) {
          console.error('Erro ao desativar atividade:', error);
          this.feedbackService.showError('Erro ao desativar atividade. Tente novamente.');
        } finally {
          const actionKey = `deactivate-${item.id}`;
          const states = new Map(this.actionLoadingStates());
          states.delete(actionKey);
          this.actionLoadingStates.set(states);
        }
      }
    });
  }

  /**
   * Activate item
   */
  async ativarItem(item: AtividadeAgropecuariaDto): Promise<void> {
    try {
      const actionKey = `activate-${item.id}`;
      this.actionLoadingStates().set(actionKey, Date.now());
      
      const updateDto: AtualizarAtividadeAgropecuariaDto = {
        descricao: item.descricao,
        tipo: item.tipo,
        ativo: true
      };
      
      await this.service.atualizar(item.id, updateDto).toPromise();
      this.messageService.add({ severity: 'success', summary: 'Sucesso', detail: 'Atividade ativada com sucesso!' });
      this.carregarItens();
    } catch (error) {
      console.error('Erro ao ativar atividade:', error);
      this.feedbackService.showError('Erro ao ativar atividade. Tente novamente.');
    } finally {
      const actionKey = `activate-${item.id}`;
      const states = new Map(this.actionLoadingStates());
      states.delete(actionKey);
      this.actionLoadingStates.set(states);
    }
  }

  /**
   * Delete item (not implemented yet)
   */
  async excluirItem(item: AtividadeAgropecuariaDto): Promise<void> {
    this.messageService.add({ severity: 'warn', summary: 'Aviso', detail: 'Funcionalidade de exclusão não implementada ainda.' });
  }
}