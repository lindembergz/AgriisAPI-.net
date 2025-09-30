import { Component, inject, OnInit, signal, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, ReactiveFormsModule, Validators, FormsModule } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { MultiSelectModule } from 'primeng/multiselect';
import { InputNumberModule } from 'primeng/inputnumber';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { DialogModule } from 'primeng/dialog';
import { CheckboxModule } from 'primeng/checkbox';
import { MessageService } from 'primeng/api';

import { ReferenceCrudBaseComponent } from '../../../shared/components/reference-crud-base/reference-crud-base.component';
import { ResponsiveTableComponent } from '../../../shared/components/responsive-table/responsive-table.component';
import { FilterSummaryComponent } from '../../../shared/components/filter-summary/filter-summary.component';
import { FieldErrorComponent } from '../../../shared/components/field-error/field-error.component';

import { UnidadeMedidaDto, CriarUnidadeMedidaDto, AtualizarUnidadeMedidaDto, TipoUnidadeMedida } from '../../../shared/models/reference.model';
import { UnidadeMedidaService, TipoUnidadeOption, UnidadeDropdownOption } from './services/unidade-medida.service';

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

// Interface removed - now using unified TableColumn interface

/**
 * Component for managing Unidades de Medida (Units of Measure) with CRUD operations
 * Extends ReferenceCrudBaseComponent for consistent behavior
 * Includes type filtering and conversion calculator functionality
 */
@Component({
  selector: 'app-unidades-medida',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
  InputTextModule,
  SelectModule,
  MultiSelectModule,
    InputNumberModule,
    ButtonModule,
    TableModule,
    ConfirmDialogModule,
    ToastModule,
    ProgressSpinnerModule,
    TagModule,
    TooltipModule,
    DialogModule,
    CheckboxModule,
    ResponsiveTableComponent,
    FilterSummaryComponent,
    FieldErrorComponent
  ],
  providers: [MessageService],
  templateUrl: './unidades-medida.component.html',
  styleUrls: ['./unidades-medida.component.scss']
})
export class UnidadesMedidaComponent extends ReferenceCrudBaseComponent<
  UnidadeMedidaDto,
  CriarUnidadeMedidaDto,
  AtualizarUnidadeMedidaDto
> implements OnInit {

  protected service = inject(UnidadeMedidaService);
  protected messageService = inject(MessageService);

  // Type filtering
  tiposDisponiveis: TipoUnidadeOption[] = [];

  // Conversion calculator
  showConversionCalculator = false;
  unidadesParaConversao: UnidadeDropdownOption[] = [];

  // Entity configuration
  protected entityDisplayName = () => 'Unidade de Medida';
  protected entityDescription = () => 'Gerenciar unidades de medida utilizadas no sistema';
  protected defaultSortField = () => 'simbolo';
  protected searchFields = () => ['simbolo', 'nome'];

  public filteredData = signal<UnidadeMedidaDto[]>([]);
  public actionsTemplate = null;
  public typeOptions = signal<any[]>([]);
  // helper used by templates (PrimeNG expects plain arrays)
  public typeOptionsArray = () => this.typeOptions();
  public saving = signal(false);
  public showDialog = signal(false);

  // Template-friendly aliases (some templates expect different names)
  // Keep backwards compatibility with templates referencing Portuguese/English names
  editItem = (ev: any) => this.editarItem ? this.editarItem(ev) : undefined;
  deleteItem = (ev: any) => this.excluirItem ? this.excluirItem(ev) : undefined;
  loadData = (force: boolean) => this.carregarItens ? this.carregarItens() : undefined;

  constructor() {
    super();
    effect(() => {
      this.items(); // Register dependency on items signal
      this.applyFiltersLocal();
    });
  }

  override ngOnInit(): void {
    try {
      super.ngOnInit();
      this.carregarTipos();
      this.carregarUnidadesParaConversao();
      this.setupFormValidation();
    } catch (error) {
      console.error('Error initializing UnidadesMedidaComponent:', error);
      // Try to initialize with minimal setup
      this.carregarTipos();
    }
  }

  // =============================================================================
  // UNIFIED FRAMEWORK IMPLEMENTATION
  // =============================================================================



  /**
   * Get custom filters configuration
   */
  protected getCustomFilters(): CustomFilter[] {
    return [
      {
        key: 'tipo',
        label: 'Tipo',
        placeholder: 'Filtrar por tipo',
        type: 'select',
        visible: true,
        options: [
          { label: 'Todos os tipos', value: null },
          ...this.tiposDisponiveis.map(tipo => ({
            label: tipo.descricao,
            value: tipo.valor
          }))
        ]
      }
    ];
  }

  /**
   * Get custom actions configuration
   */
  protected getCustomActions(): CustomAction[] {
    return [
      {
        key: 'conversion-calculator',
        label: 'Calculadora',
        icon: 'pi pi-calculator',
        styleClass: 'p-button-outlined p-button-info',
        tooltip: 'Abrir calculadora de conversão'
        , action: () => { this.showConversionCalculator = true; }
      }
    ];
  }


  /**
   * Override to apply custom filters
   */
  protected override applyCustomFilter(items: UnidadeMedidaDto[], filterKey: string, filterValue: any): UnidadeMedidaDto[] {
    if (filterKey === 'tipo' && filterValue !== null && filterValue !== undefined) {
      return items.filter(item => item.tipo === filterValue);
    }
    return items;
  }

  /**
   * Setup additional form validation
   */
  private setupFormValidation(): void {
    try {
      if (!this.form) {
        console.warn('Form not available for validation setup');
        return;
      }

      // Add blur validation for simbolo uniqueness
      const simboloControl = this.form.get('simbolo');
      if (simboloControl) {
        simboloControl.valueChanges.subscribe(value => {
          if (value && value.length >= 1 && !simboloControl.hasError('required')) {
            this.validateSimboloUnique(value);
          }
        });
      }

      // Add blur validation for nome uniqueness
      const nomeControl = this.form.get('nome');
      if (nomeControl) {
        nomeControl.valueChanges.subscribe(value => {
          if (value && value.length >= 2 && !nomeControl.hasError('required')) {
            this.validateNomeUnique(value);
          }
        });
      }
    } catch (error) {
      console.error('Error setting up form validation:', error);
    }
  }

  /**
   * Validate simbolo uniqueness
   */
  private validateSimboloUnique(simbolo: string): void {
    const currentId = this.selectedItem()?.id;
    this.service.verificarSimboloUnico(simbolo, currentId).subscribe({
      next: (isUnique) => {
        const simboloControl = this.form.get('simbolo');
        if (simboloControl) {
          if (!isUnique) {
            simboloControl.setErrors({ ...simboloControl.errors, unique: true });
          } else {
            const errors = simboloControl.errors;
            if (errors) {
              delete errors['unique'];
              simboloControl.setErrors(Object.keys(errors).length ? errors : null);
            }
          }
        }
      },
      error: (error) => {
        console.error('Erro ao validar símbolo único:', error);
      }
    });
  }

  /**
   * Validate nome uniqueness
   */
  private validateNomeUnique(nome: string): void {
    const currentId = this.selectedItem()?.id;
    this.service.verificarNomeUnico(nome, currentId).subscribe({
      next: (isUnique) => {
        const nomeControl = this.form.get('nome');
        if (nomeControl) {
          if (!isUnique) {
            nomeControl.setErrors({ ...nomeControl.errors, unique: true });
          } else {
            const errors = nomeControl.errors;
            if (errors) {
              delete errors['unique'];
              nomeControl.setErrors(Object.keys(errors).length ? errors : null);
            }
          }
        }
      },
      error: (error) => {
        console.error('Erro ao validar nome único:', error);
      }
    });
  }

  // Table columns configuration
  protected displayColumns = (): TableColumn[] => [

            {
      field: 'id',
      header: 'Código',
      sortable: true,
      width: '50px',
      type: 'custom'
    },
        {
      field: 'tipoDescricao',
      header: 'Grandeza',
      sortable: true,
      width: '150px',
      type: 'custom'
    },
    {
      field: 'nome',
      header: 'Unidade',
      sortable: true,
      width: '250px',
      type: 'text'
    },
    {
      field: 'simbolo',
      header: 'Símbolo',
      sortable: true,
      width: '250px',
      type: 'text'
    },


    /*{
      field: 'fatorConversao',
      header: 'Fator Conversão',
      sortable: true,
      width: '150px',
      hideOnMobile: true,
      type: 'text'
    },
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

  /**
   * Create reactive form with validation
   */
  protected createFormGroup(): FormGroup {
    return this.fb.group({
      simbolo: ['', [
        Validators.required,
        Validators.minLength(1),
        Validators.maxLength(10),
        Validators.pattern(/^[A-Za-z0-9²³°]+$/) // Allow letters, numbers, and common unit symbols
      ]],
      nome: ['', [
        Validators.required,
        Validators.minLength(2),
        Validators.maxLength(100)
      ]],
      tipo: [null, [
        Validators.required
      ]],
      fatorConversao: [1, [
        Validators.min(0.000001),
        Validators.max(999999)
      ]],
      ativo: [true]
    });
  }

  /**
   * Map form values to create DTO
   */
  protected mapToCreateDto(formValue: any): CriarUnidadeMedidaDto {
    return {
      simbolo: formValue.simbolo?.trim(),
      nome: formValue.nome?.trim(),
      tipo: formValue.tipo,
      fatorConversao: formValue.fatorConversao || 1
    };
  }

  /**
   * Map form values to update DTO
   */
  protected mapToUpdateDto(formValue: any): AtualizarUnidadeMedidaDto {
    return {
      nome: formValue.nome?.trim(),
      tipo: formValue.tipo,
      fatorConversao: formValue.fatorConversao || 1,
      ativo: formValue.ativo
    };
  }

  /**
   * Populate form with entity data for editing
   */
  protected populateForm(item: UnidadeMedidaDto): void {
    this.form.patchValue({
      simbolo: item.simbolo,
      nome: item.nome,
      tipo: item.tipo,
      fatorConversao: item.fatorConversao || 1,
      ativo: item.ativo
    });
  }

  /**
   * Load available unit types
   */
  private carregarTipos(): void {
    this.service.obterTipos().subscribe({
      next: (tipos) => {
        this.tiposDisponiveis = tipos;
        this.typeOptions.set(tipos.map(t => ({ label: t.descricao, value: t.valor })));
      },
      error: (error) => {
        console.error('Erro ao carregar tipos de unidade:', error);
        // Use fallback types if service fails
        this.tiposDisponiveis = [
          { valor: 1, nome: 'Peso', descricao: 'Peso' },
          { valor: 2, nome: 'Volume', descricao: 'Volume' },
          { valor: 3, nome: 'Area', descricao: 'Área' },
          { valor: 4, nome: 'Unidade', descricao: 'Unidade' }
        ];
        this.typeOptions.set(this.tiposDisponiveis.map(t => ({ label: t.descricao, value: t.valor })));
      }
    });
  }

  /**
   * Load units for conversion calculator
   */
  private carregarUnidadesParaConversao(): void {
    this.service.obterParaDropdown().subscribe({
      next: (unidades) => {
        this.unidadesParaConversao = unidades;
      },
      error: (error) => {
        console.error('Erro ao carregar unidades para conversão:', error);
      }
    });
  }















  // =============================================================================
  // UTILITY METHODS FOR TEMPLATE
  // =============================================================================

  // Additional properties needed by template
  statusOptions = [
    { label: 'Todos os Status', value: 'todas' },
    { label: 'Apenas Ativos', value: 'ativas' },
    { label: 'Apenas Inativos', value: 'inativas' }
  ];

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
   * Handle status change
   */
  onStatusChange(value: any): void {
    this.selectedStatusFilter.set(value);
    this.applyFiltersLocal();
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
    this.applyFiltersLocal();
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
    
    const statusFilter = this.selectedStatusFilter();
    if (statusFilter !== 'todas' && statusFilter !== null) {
      const statusLabel = statusFilter === 'ativas' ? 'Ativos' : 'Inativos';
      filters.push({
        key: 'status',
        label: `Status: ${statusLabel}`,
        removable: true
      });
    }
    
    const tipoFilter = this.getCustomFilterValue('tipo');
    if (tipoFilter !== null && tipoFilter !== undefined) {
      const tipoLabel = this.tiposDisponiveis.find(t => t.valor === tipoFilter)?.descricao;
      filters.push({
        key: 'tipo',
        label: `Tipo: ${tipoLabel}`,
        removable: true
      });
    }
    
    return filters;
  }

  /**
   * Clear specific filter
   */
  clearFilter(filterKey: string): void {
    switch (filterKey) {
      case 'search':
        this.clearSearch();
        break;
      case 'status':
        this.selectedStatusFilter.set('todas');
        this.applyFiltersLocal();
        break;
      case 'tipo':
        this.onCustomFilterChange('tipo', null);
        break;
    }
  }

  /**
   * Clear all filters
   */
  clearAllFilters(): void {
    this.searchTerm.set('');
    this.selectedStatusFilter.set('todas');
    this.customFilters.set(new Map());
    this.applyFiltersLocal();
  }

  /**
   * Handle search change
   */
  onSearchChange(event: any): void {
    this.searchTerm.set(event.target.value);
    this.applyFiltersLocal();
  }

  /**
   * Clear search
   */
  clearSearch(): void {
    this.searchTerm.set('');
    this.applyFiltersLocal();
  }

  /**
   * Apply all filters to data
   */
  private applyFiltersLocal(): void {
    let filtered = [...this.items()];
    
    // Apply status filter
    const statusFilter = this.selectedStatusFilter();
    if (statusFilter === 'ativas') {
      filtered = filtered.filter(item => item.ativo === true);
    } else if (statusFilter === 'inativas') {
      filtered = filtered.filter(item => item.ativo === false);
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
    
    this.filteredData.set(filtered);
  }

  /**
   * Get field value from object
   */
  public getFieldValue(item: any, field: string): string {
    const value = field.split('.').reduce((obj, key) => obj?.[key], item);
    return value?.toString() || '';
  }



  // Additional methods needed by template that may be missing from base

  /**
   * Execute custom action
   */
  executeCustomAction(actionKey: string): void {
    const action = this.getCustomActions().find(a => a.key === actionKey);
    if (action && action.action) {
      action.action();
    }
  }

  /**
   * Override novoItem to ensure dialog opens
   */
  override novoItem(): void {
    super.novoItem();
    this.showDialog.set(true);
  }

  /**
   * Override editarItem to ensure dialog opens
   */
  override editarItem(item: UnidadeMedidaDto): void {
    super.editarItem(item);
    this.showDialog.set(true);
  }

  /**
   * Override hideDialog to properly close
   */
  hideDialog(): void {
    this.showForm.set(false);
    this.showDialog.set(false);
    this.selectedItem.set(null);
  }

  /**
   * Override save method
   */
  save(): void {
    if (this.form.valid) {
      this.saving.set(true);
      this.salvarItem();
    }
  }

  /**
   * Override salvarItem to handle dialog closing
   */
  override salvarItem(): void {
    this.saving.set(true);
    
    const formValue = this.form.value;
    const isEdit = this.isEditMode();
    
    const dto = isEdit ? this.mapToUpdateDto(formValue) : this.mapToCreateDto(formValue);
    const request = isEdit 
      ? this.service.atualizar(this.selectedItem()!.id, dto as AtualizarUnidadeMedidaDto)
      : this.service.criar(dto as CriarUnidadeMedidaDto);

    request.subscribe({
      next: (result) => {
        this.saving.set(false);
        this.hideDialog();
        this.carregarItens();
        
        const message = isEdit ? 'Unidade de medida atualizada com sucesso!' : 'Unidade de medida criada com sucesso!';
        this.messageService.add({
          severity: 'success',
          summary: 'Sucesso',
          detail: message
        });
      },
      error: (error) => {
        this.saving.set(false);
        console.error('Erro ao salvar unidade de medida:', error);
        
        this.messageService.add({
          severity: 'error',
          summary: 'Erro',
          detail: 'Erro ao salvar unidade de medida. Tente novamente.'
        });
      }
    });
  }
}