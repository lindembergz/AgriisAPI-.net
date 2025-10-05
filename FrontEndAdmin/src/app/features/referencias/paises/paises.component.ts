import { Component, inject, OnInit, signal, computed } from '@angular/core';
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
import { PaisDto, CriarPaisDto, AtualizarPaisDto } from '../../../shared/models/reference.model';
import { PaisService } from './services/pais.service';
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
 * Component for managing Países (Countries) with ISO 3166 compliance
 * Shows UF dependency count and prevents deletion when UFs exist
 */
@Component({
  selector: 'app-paises',
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
  templateUrl: './paises.component.html',
  styleUrls: ['./paises.component.scss']
})
export class PaisesComponent extends ReferenceCrudBaseComponent<
  PaisDto,
  CriarPaisDto,
  AtualizarPaisDto
> implements OnInit {

  protected service = inject(PaisService);

  // No additional signals needed - estados data comes included in PaisDto

  constructor() {
    super();
    // No special setup needed - estados data comes included in the API response
  }

  // Entity configuration
  protected entityDisplayName = () => 'Países';
  protected entityDescription = () => '';
  protected defaultSortField = () => 'nome';
  protected searchFields = () => ['codigo', 'nome'];

  // =============================================================================
  // UNIFIED FRAMEWORK IMPLEMENTATION
  // =============================================================================

  private readonly _displayColumns: TableColumn[] = [
    {
      field: 'codigo',
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
    /*{
      field: 'ufsCount',
      header: 'UFs',
      sortable: false,
      width: '120px',
      type: 'custom',
      align: 'center',
      hideOnMobile: true
    },
    {
      field: 'ativo',
      header: 'Status',
      sortable: true,
      width: '100px',
      type: 'boolean',
      hideOnMobile: true,
      align: 'center'
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

  displayColumns = () => this._displayColumns;





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
      console.error('Error initializing PaisesComponent:', error);
    }
  }

  // =============================================================================
  // UTILITY METHODS FOR TEMPLATE
  // =============================================================================







  /**
   * Get UFs count for a país
   */
  getUfsCount(paisId: number): number {
    // Get count directly from the país object (estados are included in the API response)
    const pais = this.items().find(p => p.id === paisId);
    return pais?.totalEstados || 0;
  }



  /**
   * Get UFs count display text
   */
  getUfsCountDisplay(item: PaisDto): string {
    const count = this.getUfsCount(item.id);
    return count === 0 ? 'Nenhuma' : `${count}`;
  }

  /**
   * Get UFs count severity for styling
   */
  getUfsCountSeverity(count: number): string {
    if (count === 0) return 'secondary';
    if (count <= 5) return 'info';
    if (count <= 15) return 'success';
    if (count <= 30) return 'warning';
    return 'danger';
  }

  /**
   * Get tooltip text for UFs count
   */
  getUfsTooltip(count: number): string {
    if (count === 0) return 'Nenhuma UF cadastrada';
    return `${count} UF${count > 1 ? 's' : ''} cadastrada${count > 1 ? 's' : ''}`;
  }





  /**
   * Override excluir to check UF dependencies
   */
  override async excluirItem(item: PaisDto): Promise<void> {
    try {
      // Check if país has UFs before allowing deletion
      const temUfs = await this.service.verificarDependenciasUf(item.id).toPromise();
      const ufsCount = this.getUfsCount(item.id);

      if (temUfs && ufsCount > 0) {
        this.feedbackService.showWarning(
          `Este país possui ${ufsCount} UF${ufsCount > 1 ? 's' : ''} cadastrada${ufsCount > 1 ? 's' : ''}. Remova as UFs primeiro.`,
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
   * Override to check dependencies before deactivation
   */
  protected override async checkDeactivationDependencies(item: PaisDto): Promise<{
    canDeactivate: boolean;
    message?: string;
    warningMessage?: string;
  } | null> {
    try {
      const temUfs = await this.service.verificarDependenciasUf(item.id).toPromise();
      const ufsCount = this.getUfsCount(item.id);

      if (temUfs && ufsCount > 0) {
        return {
          canDeactivate: false,
          message: `Este país possui ${ufsCount} UF${ufsCount > 1 ? 's' : ''} cadastrada${ufsCount > 1 ? 's' : ''}. Desative as UFs primeiro.`
        };
      }

      return { canDeactivate: true };
    } catch (error) {
      console.error('Erro ao verificar dependências de UFs:', error);
      return { canDeactivate: true };
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
      ativo: [true]
    });
  }

  /**
   * Map form values to create DTO
   */
  protected mapToCreateDto(formValue: any): CriarPaisDto {
    return {
      codigo: formValue.codigo?.toUpperCase().trim(),
      nome: formValue.nome?.trim()
    };
  }

  /**
   * Map form values to update DTO
   */
  protected mapToUpdateDto(formValue: any): AtualizarPaisDto {
    return {
      nome: formValue.nome?.trim(),
      ativo: formValue.ativo
    };
  }

  /**
   * Populate form with entity data for editing
   */
  protected populateForm(item: PaisDto): void {
    this.form.patchValue({
      codigo: item.codigo,
      nome: item.nome,
      ativo: item.ativo
    });

    // Disable codigo field in edit mode
    if (this.isEditMode()) {
      this.form.get('codigo')?.disable();
    } else {
      this.form.get('codigo')?.enable();
    }
  }

  // Additional methods needed by template

  /**
   * Get dialog title
   */
  dialogTitle(): string {
    return this.selectedItem() ? `Editar ${this.entityDisplayName()}` : `Novo ${this.entityDisplayName()}`;
  }

  /**
   * Check if is edit mode
   */
  isEditMode(): boolean {
    return this.selectedItem() !== null;
  }

  /**
   * Override novoItem to ensure codigo field is enabled
   */
  override novoItem(): void {
    super.novoItem();
    // Ensure codigo field is enabled for new items
    this.form.get('codigo')?.enable();
  }

  /**
   * Get dialog style
   */
  private readonly _paisesDialogStyle = { width: '450px' };

  override getDialogStyle(): any {
    return this._paisesDialogStyle;
  }

  /**
   * Get page report template
   */
  getPageReportTemplate(): string {
    return 'Mostrando {first} a {last} de {totalRecords} países';
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
  private _paisesActiveFilters = computed(() => {
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
  });

  override getActiveFiltersSummary(): any[] {
    return this._paisesActiveFilters();
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

  // Additional signals needed by template (these should be in base component)
  pageSize = signal<number>(10);
  multiSortMeta = signal<any[]>([]);
  actionLoadingStates = signal<Map<string, number>>(new Map());
  highlightedRowIndex = signal<number>(-1);
  currentLoadingMessage = signal<string>('Carregando países...');

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