import { Component, inject, OnInit, signal } from '@angular/core';
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
import { MoedaDto, CriarMoedaDto, AtualizarMoedaDto } from '../../../shared/models/reference.model';
import { MoedaService } from './services/moeda.service';
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



/**
 * Component for managing Moedas (Currencies) with ISO 4217 compliance
 * Shows currency symbols and validates currency codes
 */
import { FieldErrorComponent } from '../../../shared/components/field-error/field-error.component';

@Component({
  selector: 'app-moedas',
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
  templateUrl: './moedas.component.html',
  styleUrls: ['./moedas.component.scss']
})
export class MoedasComponent extends ReferenceCrudBaseComponent<
  MoedaDto,
  CriarMoedaDto,
  AtualizarMoedaDto
> implements OnInit {

  protected service = inject(MoedaService);

  // Entity configuration
  protected entityDisplayName = () => 'Moeda';
  protected entityDescription = () => 'Gerenciar moedas utilizadas no sistema';
  protected defaultSortField = () => 'codigo';
  protected searchFields = () => ['codigo', 'nome', 'simbolo'];

  // =============================================================================
  // UNIFIED FRAMEWORK IMPLEMENTATION
  // =============================================================================

  displayColumns: () => TableColumn[] = () => [
        {
      field: 'id',
      header: 'Código',
      sortable: true,
      width: '120px',
      align: 'center',
      type: 'text'
    },

    {
      field: 'nome',
      header: 'Nome',
      sortable: true,
      width: '300px',
      type: 'text'
    },
    {
      field: 'simbolo',
      header: 'Símbolo',
      sortable: true,
      width: '100px',
      type: 'custom',
      align: 'center'
    },
    {
      field: 'codigo',
      header: 'Cód Transação comercial da moeda',
      sortable: true,
      width: '120px',
      align: 'center',
      type: 'text'
    },
  ];

  /**
   * Get custom filters configuration
   */
  protected getCustomFilters(): CustomFilter[] {
    return [];
  }

  /**
   * Get custom actions configuration
   */
  protected getCustomActions(): CustomAction[] {
    return [];
  }

  ngOnInit(): void {
    try {
      super.ngOnInit();
    } catch (error) {
      console.error('Error initializing MoedasComponent:', error);
    }
  }

  // =============================================================================
  // UTILITY METHODS FOR TEMPLATE
  // =============================================================================

  /**
   * Create reactive form with validation
   */
  protected createFormGroup(): FormGroup {
    return this.fb.group({
      codigo: ['', [
        Validators.required,
        Validators.minLength(3),
        Validators.maxLength(3),
        FieldValidatorsUtil.alphaNumeric(),
        FieldValidatorsUtil.upperCase()
      ]],
      nome: ['', [
        Validators.required,
        Validators.minLength(2),
        Validators.maxLength(100),
        FieldValidatorsUtil.noSpecialChars()
      ]],
      simbolo: ['', [
        Validators.required,
        Validators.minLength(1),
        Validators.maxLength(5)
      ]],
      ativo: [true]
    });
  }

  /**
   * Map form values to create DTO
   */
  protected mapToCreateDto(formValue: any): CriarMoedaDto {
    return {
      codigo: formValue.codigo?.toUpperCase().trim(),
      nome: formValue.nome?.trim(),
      simbolo: formValue.simbolo?.trim()
    };
  }

  /**
   * Map form values to update DTO
   */
  protected mapToUpdateDto(formValue: any): AtualizarMoedaDto {
    return {
      nome: formValue.nome?.trim(),
      simbolo: formValue.simbolo?.trim(),
      ativo: formValue.ativo
    };
  }

  /**
   * Populate form with entity data for editing
   */
  protected populateForm(item: MoedaDto): void {
    this.form.patchValue({
      codigo: item.codigo,
      nome: item.nome,
      simbolo: item.simbolo,
      ativo: item.ativo
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
    return 'Mostrando {first} a {last} de {totalRecords} moedas';
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
   * Handle search change
   */
  onSearchChange(event: any): void {
    this.searchTerm.set(event.target.value);
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
  }

  /**
   * Get currency symbol display
   */
  getCurrencySymbolDisplay(simbolo: string): string {
    return simbolo || '-';
  }

  /**
   * Get currency symbol class
   */
  getCurrencySymbolClass(codigo: string): string {
    return `currency-${codigo?.toLowerCase() || 'default'}`;
  }

  // Additional signals needed by template (these should be in base component)
  pageSize = signal<number>(10);
  multiSortMeta = signal<any[]>([]);
  actionLoadingStates = signal<Map<string, number>>(new Map());
  highlightedRowIndex = signal<number>(-1);
  currentLoadingMessage = signal<string>('Carregando moedas...');

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
}