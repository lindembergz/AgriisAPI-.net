import { Component, OnInit, inject, signal, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { TableModule } from 'primeng/table';
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
import { ConfirmationService, MessageService } from 'primeng/api';
import { ReferenceCrudService } from '../../services/reference-crud.service';
import { BaseEntity } from '../../models/base.model';
import { ValidationService } from '../../services/validation.service';

interface StatusFilter {
  label: string;
  value: string;
}

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
    ButtonModule,
    SelectModule,
    ConfirmDialogModule,
    ToastModule,
    ProgressSpinnerModule,
    TagModule,
    TooltipModule,
    DialogModule,
    InputTextModule,
    CheckboxModule
  ],
  providers: [ConfirmationService, MessageService],
  template: `
    <div class="reference-crud-container">
      <!-- Header with title and actions -->
      <div class="header-section">
        <div class="title-section">
          <h2>{{ entityDisplayName() }}</h2>
          <p class="subtitle">{{ entityDescription() }}</p>
        </div>
        
        <div class="actions-section">
          <!-- Status Filter -->
          <p-select
            [options]="statusFilterOptions"
            [ngModel]="selectedStatusFilter()"
            (onChange)="onStatusFilterChange($event)"
            placeholder="Filtrar por status"
            optionLabel="label"
            optionValue="value"
            class="status-filter">
          </p-select>
          
          <!-- New Entity Button -->
          <p-button
            [label]="'Novo ' + entityDisplayName()"
            icon="pi pi-plus"
            (onClick)="novoItem()"
            class="p-button-primary new-item-btn">
          </p-button>
        </div>
      </div>

      <!-- Loading Spinner -->
      <div *ngIf="loading()" class="loading-container">
        <p-progressSpinner></p-progressSpinner>
        <p>Carregando {{ entityDisplayName().toLowerCase() }}...</p>
      </div>

      <!-- Items Table -->
      <div *ngIf="!loading()" class="table-container">
        <p-table
          [value]="items()"
          [paginator]="true"
          [rows]="pageSize()"
          [rowsPerPageOptions]="[5, 10, 20, 50]"
          [sortField]="defaultSortField()"
          [sortOrder]="1"
          [globalFilterFields]="searchFields()"
          responsiveLayout="scroll"
          styleClass="p-datatable-gridlines p-datatable-striped"
          [tableStyle]="{ 'min-width': '50rem' }"
          [showCurrentPageReport]="true"
          [currentPageReportTemplate]="'Mostrando {first} a {last} de {totalRecords} ' + entityDisplayName().toLowerCase()"
          [loading]="tableLoading()"
          loadingIcon="pi pi-spinner"
          [sortMode]="'multiple'"
          [multiSortMeta]="multiSortMeta()"
          (onSort)="onSort($event)"
          (onPage)="onPageChange($event)"
          [lazy]="false"
          [scrollable]="true"
          scrollHeight="60vh"
          [resizableColumns]="true"
          columnResizeMode="expand">
          
          <!-- Table Header -->
          <ng-template pTemplate="header">
            <tr>
              <th *ngFor="let col of displayColumns()" 
                  [pSortableColumn]="col.sortable ? col.field : null"
                  [style.width]="col.width"
                  [class.hide-mobile]="col.hideOnMobile"
                  [class.hide-tablet]="col.hideOnTablet"
                  pResizableColumn>
                <div class="header-content">
                  {{ col.header }}
                  <p-sortIcon *ngIf="col.sortable" [field]="col.field"></p-sortIcon>
                </div>
              </th>
              <th style="width: 150px" class="actions-column">
                <div class="header-content">Ações</div>
              </th>
            </tr>
          </ng-template>

          <!-- Table Body -->
          <ng-template pTemplate="body" let-item let-rowIndex="rowIndex">
            <tr [class.row-highlight]="isRowHighlighted(rowIndex)">
              <td *ngFor="let col of displayColumns()" 
                  [class.hide-mobile]="col.hideOnMobile"
                  [class.hide-tablet]="col.hideOnTablet">
                <span class="mobile-label">{{ col.header }}:</span>
                <ng-container [ngSwitch]="col.type">
                  <span *ngSwitchCase="'boolean'">
                    <p-tag
                      [value]="getStatusLabel(item[col.field])"
                      [severity]="getStatusSeverity(item[col.field])">
                    </p-tag>
                  </span>
                  <span *ngSwitchCase="'date'">
                    {{ formatarData(item[col.field]) }}
                  </span>
                  <ng-container *ngSwitchCase="'custom'">
                    <ng-content [select]="'[slot=custom-' + col.field + ']'"></ng-content>
                  </ng-container>
                  <span *ngSwitchDefault>
                    {{ getFieldValue(item, col.field) }}
                  </span>
                </ng-container>
              </td>
              <td class="actions-column">
                <div class="action-buttons">
                  <p-button
                    icon="pi pi-pencil"
                    (onClick)="editarItem(item)"
                    class="p-button-rounded p-button-text p-button-info"
                    pTooltip="Editar"
                    tooltipPosition="top"
                    [loading]="isActionLoading('edit', item.id)"
                    [disabled]="hasAnyActionLoading()">
                  </p-button>
                  <p-button
                    *ngIf="item.ativo"
                    icon="pi pi-times"
                    (onClick)="desativarItem(item)"
                    class="p-button-rounded p-button-text p-button-warning"
                    pTooltip="Desativar"
                    tooltipPosition="top"
                    [loading]="isActionLoading('deactivate', item.id)"
                    [disabled]="hasAnyActionLoading()">
                  </p-button>
                  <p-button
                    *ngIf="!item.ativo"
                    icon="pi pi-check"
                    (onClick)="ativarItem(item)"
                    class="p-button-rounded p-button-text p-button-success"
                    pTooltip="Ativar"
                    tooltipPosition="top"
                    [loading]="isActionLoading('activate', item.id)"
                    [disabled]="hasAnyActionLoading()">
                  </p-button>
                  <p-button
                    icon="pi pi-trash"
                    (onClick)="excluirItem(item)"
                    class="p-button-rounded p-button-text p-button-danger"
                    pTooltip="Excluir"
                    tooltipPosition="top"
                    [loading]="isActionLoading('delete', item.id)"
                    [disabled]="hasAnyActionLoading()">
                  </p-button>
                </div>
              </td>
            </tr>
          </ng-template>

          <!-- Empty State -->
          <ng-template pTemplate="emptymessage">
            <tr>
              <td [attr.colspan]="displayColumns().length + 1" class="empty-state">
                <div class="empty-content">
                  <i class="pi pi-info-circle empty-icon"></i>
                  <h3>Nenhum {{ entityDisplayName().toLowerCase() }} encontrado</h3>
                  <p>
                    @if (selectedStatusFilter() === 'todas') {
                      Não há {{ entityDisplayName().toLowerCase() }} cadastrados no sistema.
                    } @else if (selectedStatusFilter() === 'ativas') {
                      Não há {{ entityDisplayName().toLowerCase() }} ativos no momento.
                    } @else {
                      Não há {{ entityDisplayName().toLowerCase() }} inativos no momento.
                    }
                  </p>
                  <p-button
                    [label]="'Cadastrar Novo ' + entityDisplayName()"
                    icon="pi pi-plus"
                    (onClick)="novoItem()"
                    class="p-button-primary">
                  </p-button>
                </div>
              </td>
            </tr>
          </ng-template>
        </p-table>
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
        [closable]="!formLoading()"
        (onHide)="onDialogHide()">
        
        <form [formGroup]="form" (ngSubmit)="salvarItem()" class="form-container">
          <!-- Form fields will be projected here -->
          <ng-content select="[slot=form-fields]"></ng-content>
          
          <!-- Default Ativo field for edit mode -->
          <div *ngIf="isEditMode()" class="field">
            <label for="ativo">Status</label>
            <p-checkbox
              formControlName="ativo"
              binary="true"
              label="Ativo">
            </p-checkbox>
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
              label="Salvar"
              icon="pi pi-check"
              (onClick)="salvarItem()"
              class="p-button-primary"
              [loading]="formLoading()"
              [disabled]="form.invalid">
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
  styleUrls: ['./reference-crud-base.component.scss']
})
export abstract class ReferenceCrudBaseComponent<
  TDto extends BaseEntity,
  TCreateDto,
  TUpdateDto
> implements OnInit {
  
  // Injected services
  protected fb = inject(FormBuilder);
  protected router = inject(Router);
  protected confirmationService = inject(ConfirmationService);
  protected messageService = inject(MessageService);
  protected validationService = inject(ValidationService);

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

  // Signals for reactive state management
  items = signal<TDto[]>([]);
  loading = signal<boolean>(false);
  tableLoading = signal<boolean>(false);
  formLoading = signal<boolean>(false);
  showForm = signal<boolean>(false);
  selectedItem = signal<TDto | null>(null);
  selectedStatusFilter = signal<string>('todas');
  pageSize = signal<number>(10);
  multiSortMeta = signal<any[]>([]);
  actionLoadingStates = signal<Map<string, number>>(new Map());
  highlightedRowIndex = signal<number>(-1);
  currentRowVersion = signal<string | null>(null);

  // Form
  form!: FormGroup;

  // Filter options
  statusFilterOptions: StatusFilter[] = [
    { label: 'Todas', value: 'todas' },
    { label: 'Ativas', value: 'ativas' },
    { label: 'Inativas', value: 'inativas' }
  ];

  ngOnInit(): void {
    this.form = this.createFormGroup();
    this.carregarItens();
  }

  /**
   * Load items based on current filter
   */
  carregarItens(): void {
    this.loading.set(true);
    this.tableLoading.set(true);
    
    const filter = this.selectedStatusFilter();
    let request;

    if (filter === 'ativas') {
      request = this.service.obterAtivos();
    } else {
      request = this.service.obterTodos();
    }

    request.subscribe({
      next: (items) => {
        if (filter === 'inativas') {
          // Filter inactive items when showing only inactive ones
          this.items.set(items.filter(item => !item.ativo));
        } else {
          this.items.set(items);
        }
        this.loading.set(false);
        this.tableLoading.set(false);
      },
      error: (error) => {
        console.error(`Erro ao carregar ${this.entityDisplayName().toLowerCase()}:`, error);
        this.loading.set(false);
        this.tableLoading.set(false);
      }
    });
  }

  /**
   * Handle status filter change
   */
  onStatusFilterChange(event: any): void {
    this.selectedStatusFilter.set(event.value);
    this.carregarItens();
  }

  /**
   * Open form for creating new item
   */
  novoItem(): void {
    this.selectedItem.set(null);
    this.currentRowVersion.set(null);
    this.form.reset();
    this.showForm.set(true);
  }

  /**
   * Open form for editing item
   */
  editarItem(item: TDto): void {
    this.setActionLoading('edit', item.id, true);
    
    // Load fresh data to get current row version
    this.service.obterPorId(item.id).subscribe({
      next: (freshItem) => {
        this.selectedItem.set(freshItem);
        this.currentRowVersion.set((freshItem as any).rowVersion);
        this.populateForm(freshItem);
        this.showForm.set(true);
        this.setActionLoading('edit', item.id, false);
      },
      error: (error) => {
        console.error('Erro ao carregar item para edição:', error);
        this.setActionLoading('edit', item.id, false);
      }
    });
  }

  /**
   * Save item (create or update)
   */
  salvarItem(): void {
    if (this.form.invalid) {
      this.validationService.markFormGroupTouched(this.form);
      return;
    }

    this.formLoading.set(true);
    const formValue = this.form.value;
    
    if (this.isEditMode()) {
      // Update existing item
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
            detail: `${this.entityDisplayName()} atualizado com sucesso`,
            life: 3000
          });
          this.carregarItens();
        },
        error: (error) => {
          this.formLoading.set(false);
          if (error.originalError?.status === 412) {
            // Concurrency conflict - reload item
            this.editarItem(this.selectedItem()!);
          }
        }
      });
    } else {
      // Create new item
      const createDto = this.mapToCreateDto(formValue);
      
      this.service.criar(createDto).subscribe({
        next: (newItem) => {
          this.formLoading.set(false);
          this.showForm.set(false);
          this.messageService.add({
            severity: 'success',
            summary: 'Sucesso',
            detail: `${this.entityDisplayName()} criado com sucesso`,
            life: 3000
          });
          this.carregarItens();
        },
        error: (error) => {
          this.formLoading.set(false);
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
   * Activate item
   */
  ativarItem(item: TDto): void {
    this.setActionLoading('activate', item.id, true);
    
    this.service.ativar(item.id).subscribe({
      next: () => {
        this.setActionLoading('activate', item.id, false);
        this.messageService.add({
          severity: 'success',
          summary: 'Sucesso',
          detail: `${this.entityDisplayName()} ativado com sucesso`,
          life: 3000
        });
        this.carregarItens();
      },
      error: (error) => {
        this.setActionLoading('activate', item.id, false);
      }
    });
  }

  /**
   * Deactivate item
   */
  desativarItem(item: TDto): void {
    this.confirmationService.confirm({
      message: `Tem certeza que deseja desativar este ${this.entityDisplayName().toLowerCase()}?`,
      header: 'Confirmar Desativação',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Sim',
      rejectLabel: 'Não',
      accept: () => {
        this.confirmarDesativacao(item);
      }
    });
  }

  /**
   * Execute item deactivation
   */
  private confirmarDesativacao(item: TDto): void {
    this.setActionLoading('deactivate', item.id, true);
    
    this.service.desativar(item.id).subscribe({
      next: () => {
        this.setActionLoading('deactivate', item.id, false);
        this.messageService.add({
          severity: 'success',
          summary: 'Sucesso',
          detail: `${this.entityDisplayName()} desativado com sucesso`,
          life: 3000
        });
        this.carregarItens();
      },
      error: (error) => {
        this.setActionLoading('deactivate', item.id, false);
      }
    });
  }

  /**
   * Confirm and delete item
   */
  excluirItem(item: TDto): void {
    // First check if item can be removed
    this.service.podeRemover(item.id).subscribe({
      next: (canRemove) => {
        if (!canRemove) {
          this.messageService.add({
            severity: 'warn',
            summary: 'Não é possível excluir',
            detail: `Este ${this.entityDisplayName().toLowerCase()} está sendo usado por outros registros`,
            life: 5000
          });
          return;
        }

        this.confirmationService.confirm({
          message: `Tem certeza que deseja excluir este ${this.entityDisplayName().toLowerCase()}?`,
          header: 'Confirmar Exclusão',
          icon: 'pi pi-exclamation-triangle',
          acceptLabel: 'Sim',
          rejectLabel: 'Não',
          accept: () => {
            this.confirmarExclusao(item);
          }
        });
      },
      error: (error) => {
        console.error('Erro ao verificar se pode remover:', error);
      }
    });
  }

  /**
   * Execute item deletion
   */
  private confirmarExclusao(item: TDto): void {
    this.setActionLoading('delete', item.id, true);
    
    this.service.remover(item.id).subscribe({
      next: () => {
        this.setActionLoading('delete', item.id, false);
        this.messageService.add({
          severity: 'success',
          summary: 'Sucesso',
          detail: `${this.entityDisplayName()} excluído com sucesso`,
          life: 3000
        });
        this.carregarItens();
      },
      error: (error) => {
        this.setActionLoading('delete', item.id, false);
      }
    });
  }

  /**
   * Handle dialog hide event
   */
  onDialogHide(): void {
    if (!this.formLoading()) {
      this.cancelarEdicao();
    }
  }

  /**
   * Check if in edit mode
   */
  isEditMode(): boolean {
    return this.selectedItem() !== null;
  }

  /**
   * Get dialog title
   */
  dialogTitle(): string {
    return this.isEditMode() 
      ? `Editar ${this.entityDisplayName()}` 
      : `Novo ${this.entityDisplayName()}`;
  }

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
   * Set action loading state
   */
  private setActionLoading(action: string, id: number, loading: boolean): void {
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
   * Check if specific action is loading
   */
  isActionLoading(action: string, id: number): boolean {
    const key = `${action}-${id}`;
    return this.actionLoadingStates().has(key);
  }

  /**
   * Check if any action is loading
   */
  hasAnyActionLoading(): boolean {
    return this.actionLoadingStates().size > 0;
  }

  /**
   * Check if row should be highlighted
   */
  isRowHighlighted(rowIndex: number): boolean {
    return this.highlightedRowIndex() === rowIndex;
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
}