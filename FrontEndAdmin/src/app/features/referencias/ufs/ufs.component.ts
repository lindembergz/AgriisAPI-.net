import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormsModule } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { DialogModule } from 'primeng/dialog';
import { CheckboxModule } from 'primeng/checkbox';

import { ReferenceCrudBaseComponent } from '../../../shared/components/reference-crud-base/reference-crud-base.component';
import { UfDto, CriarUfDto, AtualizarUfDto, PaisDto } from '../../../shared/models/reference.model';
import { UfService } from './services/uf.service';
import { PaisService } from '../paises/services/pais.service';
import { FieldValidatorsUtil } from '../../../shared/utils/field-validators.util';

import { 
  ComponentTemplate, 
  CustomAction, 
  TableColumn, 
  EmptyStateConfig, 
  LoadingStateConfig, 
  ResponsiveConfig,
  DialogConfig,
  DisplayMode
} from '../../../shared/interfaces/unified-component.interfaces';
import { CustomFilter } from '../../../shared/interfaces/component-template.interface';
import { FieldErrorComponent } from '../../../shared/components/field-error/field-error.component';



/**
 * Component for managing UFs (Unidades Federativas) with País selection and Município dependency
 * Shows País relationship and Município count, prevents deletion when Municípios exist
 */
@Component({
  selector: 'app-ufs',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    InputTextModule,
    SelectModule,
    ButtonModule,
    TableModule,
    ConfirmDialogModule,
    ToastModule,
    ProgressSpinnerModule,
    TagModule,
    TooltipModule,
    DialogModule,
    CheckboxModule,
    FieldErrorComponent
  ],
  providers: [],
  templateUrl: './ufs.component.html',
  styleUrls: ['./ufs.component.scss']
})
export class UfsComponent extends ReferenceCrudBaseComponent<
  UfDto,
  CriarUfDto,
  AtualizarUfDto
> implements OnInit {
  
  protected service = inject(UfService);
  private paisService = inject(PaisService);

  // Signals for reactive data
  paisesOptions = signal<PaisDto[]>([]);
  loadingPaises = signal<boolean>(false);

  // Computed options for pais filter to avoid spread syntax in template
  paisFilterOptions = () => [{ label: 'Todos os países', value: null }, ...this.paisesOptions().map(p => ({ label: p.nome, value: p.id }))];

  // Entity configuration
  protected entityDisplayName = () => 'UF';
  protected entityDescription = () => '';
  protected defaultSortField = () => 'codigo';
  protected searchFields = () => ['codigo', 'nome', 'pais.nome'];

  // =============================================================================
  // UNIFIED FRAMEWORK IMPLEMENTATION
  // =============================================================================

  displayColumns: () => TableColumn[] = () => [
    {
      field: 'uf',
      header: 'UF',
      sortable: true,
      width: '80px',
      align: 'center',
      type: 'text'
    },
    {
      field: 'nome',
      header: 'Nome',
      sortable: true,
      width: '200px',
      type: 'text'
    },
    {
      field: 'codigoIbge',
      header: 'Cód. IBGE',
      sortable: true,
      width: '100px',
      align: 'center',
      hideOnMobile: true,
      type: 'text'
    },
    {
      field: 'regiao',
      header: 'Região',
      sortable: true,
      width: '150px',
      hideOnMobile: true,
      type: 'text'
    },
    {
      field: 'pais.nome',
      header: 'País',
      sortable: true,
      width: '150px',
      hideOnMobile: true,
      hideOnTablet: true,
      type: 'text'
    }
  ];


  ngOnInit(): void {
    try {
      super.ngOnInit();
      this.carregarPaises();
      this.carregarItens(); // Explicitly call to load items
    } catch (error) {
      console.error('Error initializing UfsComponent:', error);
      // Try to initialize with minimal setup
      this.carregarPaises();
      this.carregarItens();
    }
  }

  // =============================================================================
  // UTILITY METHODS FOR TEMPLATE
  // =============================================================================





  /**
   * Get país name by ID
   */
  getPaisNome(paisId: number): string {
    const pais = this.paisesOptions().find(p => p.id === paisId);
    return pais ? pais.nome : 'N/A';
  }



  /**
   * Apply custom filter for país
   */
  protected override applyCustomFilter(items: UfDto[], filterKey: string, filterValue: any): UfDto[] {
    if (filterKey === 'pais') {
      return items.filter(item => item.paisId === filterValue);
    }
    return super.applyCustomFilter(items, filterKey, filterValue);
  }

  /**
   * Load países for dropdown
   */
  private carregarPaises(): void {
    this.loadingPaises.set(true);
    
    this.paisService.obterAtivos().subscribe({
      next: (paises) => {
        this.paisesOptions.set(paises);
        this.loadingPaises.set(false);
      },
      error: (error) => {
        console.error('Erro ao carregar países:', error);
        this.loadingPaises.set(false);
      }
    });
  }

  /**
   * Override carregarItens to include País information and load municípios count
   */
  override carregarItens(): void {
    this.loading.set(true);
    this.tableLoading.set(true);
    
    const filter = this.selectedStatusFilter();
    let request;

    if (filter === 'ativas') {
      request = this.service.obterAtivos();
    } else {
      request = this.service.obterComPais();
    }

    request.subscribe({
      next: (items) => {
        if (filter === 'inativas') {
          // Filter inactive items when showing only inactive ones
          this.items.set(items.filter(item => !item.ativo));
        } else {
          this.items.set(items);
        }
        
        this.applyItemFilters();
        
        // Load municípios count for each UF
        //this.carregarContagemMunicipios(items);
        
        this.loading.set(false);
        this.tableLoading.set(false);
      },
      error: (error) => {
        console.error('Erro ao carregar UFs:', error);
        this.loading.set(false);
        this.tableLoading.set(false);
      }
    });
  }

  /**
   * Load municípios count for UFs
   */
  private carregarContagemMunicipios(ufs: UfDto[]): void {
    ufs.forEach(uf => {
      this.service.obterContagemMunicipios(uf.id).subscribe({
        next: (count) => {
          // Update the UF object with municípios count
          (uf as any).municipiosCount = count;
        },
        error: (error) => {
          console.error(`Erro ao carregar contagem de municípios para UF ${uf.uf}:`, error);
          (uf as any).municipiosCount = 0;
        }
      });
    });
  }



  /**
   * Map form values to create DTO
   */
  protected mapToCreateDto(formValue: any): CriarUfDto {
    return {
      codigo: formValue.codigo?.toUpperCase().trim(),
      nome: formValue.nome?.trim(),
      codigoIbge: parseInt(formValue.codigoIbge),
      regiao: formValue.regiao?.trim(),
      paisId: parseInt(formValue.paisId)
    };
  }

  /**
   * Map form values to update DTO
   */
  protected mapToUpdateDto(formValue: any): AtualizarUfDto {
    return {
      nome: formValue.nome?.trim(),
      codigoIbge: parseInt(formValue.codigoIbge),
      regiao: formValue.regiao?.trim(),
      ativo: formValue.ativo
    };
  }

  /**
   * Populate form with entity data for editing
   */
  protected populateForm(item: UfDto): void {
    this.form.patchValue({
      codigo: item.uf,
      nome: item.nome,
      codigoIbge: item.codigoIbge,
      regiao: item.regiao,
      paisId: item.paisId,
      ativo: item.ativo
    });
  }

  /**
   * Get Municípios count display text
   */
  getMunicipiosCountDisplay(item: any): string {
    const count = item.municipiosCount || 0;
    return count === 0 ? 'Nenhum' : `${count}`;
  }

  /**
   * Get Municípios count severity for styling
   */
  getMunicipiosCountSeverity(count: number): string {
    if (count === 0) return 'secondary';
    if (count <= 10) return 'info';
    if (count <= 50) return 'success';
    if (count <= 100) return 'warning';
    return 'danger';
  }

  /**
   * Get tooltip text for municípios count
   */
  getMunicipiosTooltip(count: number): string {
    if (count === 0) return 'Nenhum município cadastrado';
    return `${count} município${count > 1 ? 's' : ''} cadastrado${count > 1 ? 's' : ''}`;
  }



  /**
   * Override to check dependencies before deactivation
   */
  protected override async checkDeactivationDependencies(item: UfDto): Promise<{
    canDeactivate: boolean;
    message?: string;
    warningMessage?: string;
  } | null> {
    try {
      const temMunicipios = await this.service.verificarDependenciasMunicipio(item.id).toPromise();
      const municipiosCount = (item as any).municipiosCount || 0;
      
      if (temMunicipios && municipiosCount > 0) {
        return {
          canDeactivate: false,
          message: `Esta UF possui ${municipiosCount} município${municipiosCount > 1 ? 's' : ''} cadastrado${municipiosCount > 1 ? 's' : ''}. Desative os municípios primeiro.`
        };
      }
      
      if (municipiosCount > 0) {
        return {
          canDeactivate: true,
          warningMessage: `Esta UF possui ${municipiosCount} município${municipiosCount > 1 ? 's' : ''} cadastrado${municipiosCount > 1 ? 's' : ''}. Eles também serão afetados pela desativação.`
        };
      }
      
      return { canDeactivate: true };
    } catch (error) {
      console.error('Erro ao verificar dependências de municípios:', error);
      return { canDeactivate: true };
    }
  }

  /**
   * Override excluir to check Município dependencies
   */
  override async excluirItem(item: UfDto): Promise<void> {
    try {
      // Check if UF has Municípios before allowing deletion
      const temMunicipios = await this.service.verificarDependenciasMunicipio(item.id).toPromise();
      
      if (temMunicipios) {
        const municipiosCount = (item as any).municipiosCount || 0;
        this.feedbackService.showWarning(
          `Esta UF possui ${municipiosCount} município${municipiosCount > 1 ? 's' : ''} cadastrado${municipiosCount > 1 ? 's' : ''}. Remova os municípios primeiro.`,
          'Exclusão Não Permitida'
        );
        return;
      }
      
      // Proceed with normal deletion flow
      await super.excluirItem(item);
    } catch (error) {
      console.error('Erro ao verificar dependências:', error);
      this.feedbackService.showError(
        'Erro ao verificar dependências. Tente novamente.',
        'Erro de Validação'
      );
    }
  }

  /**
   * Create reactive form with validation
   */
  protected createFormGroup(): FormGroup {
    return this.fb.group({
      codigo: ['', [
        Validators.required,
        Validators.minLength(2),
        Validators.maxLength(2),
        FieldValidatorsUtil.alphaNumeric(),
        FieldValidatorsUtil.upperCase()
      ]],
      nome: ['', [
        Validators.required,
        Validators.minLength(2),
        Validators.maxLength(100),
        FieldValidatorsUtil.noSpecialChars()
      ]],
      codigoIbge: ['', [
        Validators.required,
        Validators.min(1),
        Validators.max(99)
      ]],
      regiao: ['', [
        Validators.required,
        Validators.minLength(2),
        Validators.maxLength(50)
      ]],
      paisId: ['', [
        Validators.required
      ]],
      ativo: [true]
    });
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
    return { width: '450px' };
  }

  /**
   * Get page report template
   */
  getPageReportTemplate(): string {
    return 'Mostrando {first} a {last} de {totalRecords} UFs';
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
    
    const paisFilter = this.getCustomFilterValue('pais');
    if (paisFilter) {
      const paisLabel = this.paisesOptions().find(p => p.id === paisFilter)?.nome;
      filters.push({
        key: 'pais',
        label: `País: ${paisLabel}`,
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
      case 'pais':
        this.onCustomFilterChange('pais', null);
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

  /**
   * Get custom filter value
   */
  getCustomFilterValue(key: string): any {
    return this.customFilters().get(key);
  }

  /**
   * Handle custom filter change
   */
  onCustomFilterChange(key: string, value: any): void {
    const filters = new Map(this.customFilters());
    filters.set(key, value);
    this.customFilters.set(filters);
    this.applyItemFilters();
  }

  // Additional signals needed by template (these should be in base component)
  items = signal<UfDto[]>([]);
  loading = signal<boolean>(false);
  tableLoading = signal<boolean>(false);
  showForm = signal<boolean>(false);
  formLoading = signal<boolean>(false);
  selectedItem = signal<UfDto | null>(null);
  searchTerm = signal<string>('');
  selectedStatusFilter = signal<string>('todas');
  pageSize = signal<number>(10);
  multiSortMeta = signal<any[]>([]);
  actionLoadingStates = signal<Map<string, number>>(new Map());
  highlightedRowIndex = signal<number>(-1);
  currentLoadingMessage = signal<string>('Carregando UFs...');
  customFilters = signal<Map<string, any>>(new Map());

  // Filtered items signal
  filteredItems = signal<UfDto[]>([]);

  /**
   * Apply filters to items
   */
  private applyItemFilters(): void {
    let items = this.items();
    
    // Apply search filter
    const search = this.searchTerm().toLowerCase();
    if (search) {
      items = items.filter(item => 
        item.uf.toLowerCase().includes(search) ||
        item.nome.toLowerCase().includes(search) ||
        this.getPaisNome(item.paisId).toLowerCase().includes(search)
      );
    }
    
    // Apply status filter
    const statusFilter = this.selectedStatusFilter();
    if (statusFilter === 'ativas') {
      items = items.filter(item => item.ativo);
    } else if (statusFilter === 'inativas') {
      items = items.filter(item => !item.ativo);
    }
    
    // Apply custom filters
    const paisFilter = this.customFilters().get('pais');
    if (paisFilter) {
      items = items.filter(item => item.paisId === paisFilter);
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
        this.messageService.add({ severity: 'success', summary: 'Sucesso', detail: 'UF atualizada com sucesso!' });
      } else {
        const createDto = this.mapToCreateDto(formValue);
        await this.service.criar(createDto).toPromise();
        this.messageService.add({ severity: 'success', summary: 'Sucesso', detail: 'UF criada com sucesso!' });
      }
      
      this.cancelarEdicao();
      this.carregarItens();
    } catch (error) {
      console.error('Erro ao salvar UF:', error);
      this.feedbackService.showError('Erro ao salvar UF. Tente novamente.');
    } finally {
      this.formLoading.set(false);
    }
  }

  /**
   * New item
   */
  novoItem(): void {
    this.selectedItem.set(null);
    this.form.reset({ 
      ativo: true,
      paisId: 1 // Default para Brasil
    });
    this.showForm.set(true);
  }

  /**
   * Edit item
   */
  editarItem(item: UfDto): void {
    this.selectedItem.set(item);
    this.populateForm(item);
    this.showForm.set(true);
  }


}