import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormsModule } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MessageService, ConfirmationService } from 'primeng/api';

import { ReferenceCrudBaseComponent } from '../../../shared/components/reference-crud-base/reference-crud-base.component';
import { EmbalagemDto, CriarEmbalagemDto, AtualizarEmbalagemDto, TipoUnidadeMedida } from '../../../shared/models/reference.model';
import { CustomFilter, CustomAction, EmptyStateConfig, LoadingStateConfig, TableColumn } from '../../../shared/interfaces/component-template.interface';
import { EmbalagemService, UnidadeDropdownOption, TipoUnidadeOption } from './services/embalagem.service';
import { map } from 'rxjs/operators';
import { of } from 'rxjs';
import { FieldErrorComponent } from '../../../shared/components/field-error/field-error.component';

@Component({
  selector: 'app-embalagens',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    InputTextModule,
    SelectModule,
    ButtonModule,
    CheckboxModule,
    TableModule,
    DialogModule,
    ToastModule,
    ConfirmDialogModule,
    TagModule,
    TooltipModule,
    ProgressSpinnerModule,
    FieldErrorComponent
  ],
  providers: [ConfirmationService, MessageService],
  templateUrl: './embalagens.component.html',
  styleUrls: ['./embalagens.component.scss']
})
export class EmbalagensComponent extends ReferenceCrudBaseComponent<
  EmbalagemDto,
  CriarEmbalagemDto,
  AtualizarEmbalagemDto
> implements OnInit {

  // Service injection
  protected service = inject(EmbalagemService);

  // Entity configuration
  protected entityDisplayName = () => 'Embalagem';
  protected entityDescription = () => 'Gerenciar tipos de embalagem com unidades de medida associadas';
  protected defaultSortField = () => 'nome';
  protected searchFields = () => ['nome', 'descricao'];

  // Table columns
  protected displayColumns = (): TableColumn[] => [
    { field: 'nome', header: 'Nome', sortable: true, width: '25%', type: 'text' },
    { field: 'descricao', header: 'Descrição', sortable: true, width: '30%', hideOnMobile: true, type: 'text' },
    { field: 'unidadeMedida', header: 'Unidade de Medida', width: '25%', type: 'custom', sortable: false },
    //{ field: 'ativo', header: 'Status', sortable: true, width: '15%', type: 'boolean', hideOnMobile: true },
    //{ field: 'dataCriacao', header: 'Criado em', sortable: true, width: '150px', type: 'date', hideOnMobile: true, hideOnTablet: true }
  ];

  // UnidadeMedida filtering signals
  unidadesDisponiveis = signal<UnidadeDropdownOption[]>([]);
  unidadesFiltradas = signal<UnidadeDropdownOption[]>([]);
  tiposUnidadeDisponiveis = signal<TipoUnidadeOption[]>([]);
  tipoUnidadeSelecionado = signal<TipoUnidadeMedida | null>(null);
  unidadeMedidaSelecionada = signal<number | null>(null);

  // Current item for template access
  currentItem: EmbalagemDto | null = null;

  ngOnInit(): void {
    super.ngOnInit();
    this.carregarUnidadesMedida();
    this.carregarTiposUnidade();
    this.setupCustomFilters();
  }

  protected getCustomFilters(): CustomFilter[] {
    return [
      {
        key: 'tipoUnidade',
        label: 'Tipo de Unidade',
        placeholder: 'Selecione um tipo',
        type: 'select',
        visible: true,
        options: [
          { label: 'Todos os tipos', value: null },
          ...this.tiposUnidadeDisponiveis().map(tipo => ({
            label: tipo.descricao,
            value: tipo.valor
          }))
        ]
      },
      {
        key: 'unidadeMedida',
        label: 'Unidade de Medida',
        placeholder: 'Selecione uma unidade',
        type: 'select',
        visible: true,
        options: [
          { label: 'Todas as unidades', value: null },
          ...this.unidadesFiltradas().map(unidade => ({
            label: `${unidade.simbolo} - ${unidade.nome}`,
            value: unidade.id
          }))
        ]
      }
    ];
  }

  protected getCustomActions(): CustomAction[] {
    return [];
  }

  protected getEmptyStateConfig(): EmptyStateConfig {
    return {
      icon: 'pi pi-box',
      title: 'Nenhuma embalagem encontrada',
      description: this.hasActiveFilters()
        ? 'Não há embalagens que atendam aos filtros aplicados.'
        : 'Não há embalagens cadastradas no sistema.',
      primaryAction: {
        label: 'Nova Embalagem',
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
      message: 'Carregando embalagens...',
      showProgress: false
    };
  }

  protected setupCustomFilters(): void {
    // // Setup tipo unidade filter behavior
    // this.componentStateService.getCustomFilterValue('tipoUnidade').subscribe(tipoValue => {
    //   if (tipoValue !== this.tipoUnidadeSelecionado()) {
    //     this.tipoUnidadeSelecionado.set(tipoValue);
    //     this.onTipoUnidadeFilterChange();
    //   }
    // });

    // // Setup unidade medida filter behavior
    // this.componentStateService.getCustomFilterValue('unidadeMedida').subscribe(unidadeValue => {
    //   if (unidadeValue !== this.unidadeMedidaSelecionada()) {
    //     this.unidadeMedidaSelecionada.set(unidadeValue);
    //     this.onUnidadeMedidaFilterChange();
    //   }
    // });
  }

  /**
   * Create reactive form
   */
  protected createFormGroup(): FormGroup {
    return this.fb.group({
      nome: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      descricao: ['', [Validators.maxLength(500)]],
      unidadeMedidaId: [null, [Validators.required]],
      ativo: [true]
    });
  }

  /**
   * Map form value to create DTO
   */
  protected mapToCreateDto(formValue: any): CriarEmbalagemDto {
    return {
      nome: formValue.nome?.trim(),
      descricao: formValue.descricao?.trim() || undefined,
      unidadeMedidaId: formValue.unidadeMedidaId
    };
  }

  /**
   * Map form value to update DTO
   */
  protected mapToUpdateDto(formValue: any): AtualizarEmbalagemDto {
    return {
      nome: formValue.nome?.trim(),
      descricao: formValue.descricao?.trim() || undefined,
      unidadeMedidaId: formValue.unidadeMedidaId,
      ativo: formValue.ativo
    };
  }

  /**
   * Populate form with item data
   */
  protected populateForm(item: EmbalagemDto): void {
    this.form.patchValue({
      nome: item.nome,
      descricao: item.descricao || '',
      unidadeMedidaId: item.unidadeMedidaId,
      ativo: item.ativo
    });
  }

  /**
   * Load unidades de medida
   */
  private carregarUnidadesMedida(): void {
    this.service.obterUnidadesMedidaParaDropdown().subscribe({
      next: (unidades) => {
        this.unidadesDisponiveis.set(unidades);
        this.unidadesFiltradas.set([...unidades]);
      },
      error: (error) => {
        this.unifiedErrorHandlingService.handleComponentError('embalagens', 'load-unidades', error);
      }
    });
  }

  /**
   * Load tipos de unidade
   */
  private carregarTiposUnidade(): void {
    this.service.obterTiposUnidade().subscribe({
      next: (tipos) => {
        this.tiposUnidadeDisponiveis.set(tipos);
      },
      error: (error) => {
        this.unifiedErrorHandlingService.handleComponentError('embalagens', 'load-tipos', error);
      }
    });
  }

  /**
   * Handle tipo unidade filter change
   */
  onTipoUnidadeFilterChange(): void {
    const tipoSelecionado = this.tipoUnidadeSelecionado();

    if (tipoSelecionado) {
      const unidadesFiltradas = this.unidadesDisponiveis().filter(
        unidade => unidade.tipo === tipoSelecionado
      );
      this.unidadesFiltradas.set(unidadesFiltradas);
    } else {
      this.unidadesFiltradas.set([...this.unidadesDisponiveis()]);
    }

    // Clear unidade selection if it doesn't match the type filter
    const unidadeSelecionada = this.unidadeMedidaSelecionada();
    if (unidadeSelecionada && tipoSelecionado) {
      const selectedUnidade = this.unidadesDisponiveis().find(u => u.id === unidadeSelecionada);
      if (selectedUnidade && selectedUnidade.tipo !== tipoSelecionado) {
        this.unidadeMedidaSelecionada.set(null);
      }
    }

    this.applyItemFilters();
  }

  /**
   * Handle unidade medida filter change
   */
  onUnidadeMedidaFilterChange(): void {
    this.applyItemFilters();
  }

  /**
   * Override carregarItens to implement filtering
   */
  public carregarItens(): void {
    this.loading.set(true);
    this.tableLoading.set(true);

    this.service.obterTodos().subscribe({
      next: (items: EmbalagemDto[]) => {
        console.log( items)
        this.items.set(items);
        this.applyItemFilters();
        this.loading.set(false);
        this.tableLoading.set(false);
      },
      error: (error: any) => {
        console.error('Erro ao carregar embalagens:', error);
        this.loading.set(false);
        this.tableLoading.set(false);
      }
    });
  }

  /**
   * Override clearAllFilters to handle custom filters
   */
  public clearAllFilters(): void {
    this.searchTerm.set('');
    this.selectedStatusFilter.set('todas');
    this.tipoUnidadeSelecionado.set(null);
    this.unidadeMedidaSelecionada.set(null);
    this.unidadesFiltradas.set([...this.unidadesDisponiveis()]);
    this.applyItemFilters();
  }

  /**
   * Override hasActiveFilters to include custom filters
   */
  public hasActiveFilters(): boolean {
    return this.searchTerm() !== '' ||
           this.selectedStatusFilter() !== 'todas' ||
           this.tipoUnidadeSelecionado() !== null ||
           this.unidadeMedidaSelecionada() !== null;
  }

  /**
   * Get tipo unidade description
   */
  getTipoUnidadeDescricao(tipo: TipoUnidadeMedida): string {
    return this.service.getTipoUnidadeDescricao(tipo);
  }

  /**
   * Get unidade medida display for table
   */
  getUnidadeMedidaDisplay(item: EmbalagemDto): string {
    if (!item.unidadeMedidaNome) return '-';
    return `${item.unidadeMedidaSimbolo} - ${item.unidadeMedidaNome}`;
  }

  /**
   * Get tipo info for unidade medida
   */
  getTipoInfo(item: EmbalagemDto): string {
    // Como não temos o tipo na resposta da API, retornamos string vazia
    // Se necessário, pode ser adicionado ao DTO da API
    return '';
  }

  // Additional methods needed by the template

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
    return 'Mostrando {first} a {last} de {totalRecords} embalagens';
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
    }
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
   * Clear search
   */
  clearSearch(): void {
    this.searchTerm.set('');
  }

  /**
   * Handle search change
   */
  onSearchChange(event: any): void {
    this.searchTerm.set(event.target.value);
    this.applyItemFilters();
  }

  /**
   * Handle status filter change
   */
  onStatusFilterChange(event: any): void {
    this.selectedStatusFilter.set(event.value);
    this.applyItemFilters();
  }

  // Additional signals needed by template (these should be in base component)
  items = signal<EmbalagemDto[]>([]);
  loading = signal<boolean>(false);
  tableLoading = signal<boolean>(false);
  showForm = signal<boolean>(false);
  formLoading = signal<boolean>(false);
  selectedItem = signal<EmbalagemDto | null>(null);
  searchTerm = signal<string>('');
  selectedStatusFilter = signal<string>('todas');
  pageSize = signal<number>(10);
  multiSortMeta = signal<any[]>([]);
  actionLoadingStates = signal<Map<string, number>>(new Map());
  highlightedRowIndex = signal<number>(-1);
  currentLoadingMessage = signal<string>('Carregando embalagens...');
  
  // Status filter options
  statusFilterOptions = [
    { label: 'Todas', value: 'todas' },
    { label: 'Ativas', value: 'ativas' },
    { label: 'Inativas', value: 'inativas' }
  ];

  // Filtered items signal
  filteredItems = signal<EmbalagemDto[]>([]);

  /**
   * Apply filters to items
   */
  private applyItemFilters(): void {
    let items = this.items();
    
    // Apply search filter
    const search = this.searchTerm().toLowerCase();
    if (search) {
      items = items.filter(item => 
        item.nome.toLowerCase().includes(search) ||
        (item.descricao && item.descricao.toLowerCase().includes(search))
      );
    }
    
    // Apply status filter
    const statusFilter = this.selectedStatusFilter();
    if (statusFilter === 'ativas') {
      items = items.filter(item => item.ativo);
    } else if (statusFilter === 'inativas') {
      items = items.filter(item => !item.ativo);
    }
    
    // Apply unidade medida filter
    const unidadeFilter = this.unidadeMedidaSelecionada();
    if (unidadeFilter) {
      items = items.filter(item => item.unidadeMedidaId === unidadeFilter);
    }
    
    this.filteredItems.set(items);
  }
}