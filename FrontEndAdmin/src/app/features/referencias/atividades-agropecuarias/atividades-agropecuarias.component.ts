import { Component, inject, OnInit } from '@angular/core';
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
import { ReferenceCrudBaseComponent } from '../../../shared/components/reference-crud-base/reference-crud-base.component';
import { 
  AtividadeAgropecuariaDto, 
  CriarAtividadeAgropecuariaDto, 
  AtualizarAtividadeAgropecuariaDto,
  TipoAtividadeAgropecuaria 
} from '../../../shared/models/reference.model';
import { AtividadeAgropecuariaService } from './services/atividade-agropecuaria.service';

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
    CheckboxModule
  ],
  template: `
    <div class="reference-crud-container">
      <!-- Header with title and actions -->
      <div class="header-section">
        <div class="title-section">
          <h2>{{ entityDisplayName() }}</h2>
          <p class="subtitle">{{ entityDescription() }}</p>
        </div>
        
        <div class="actions-section">
          <!-- Custom toolbar for type filter -->
          <div class="custom-toolbar">
            <div class="filter-section">
              <label for="tipoFilter">Filtrar por Tipo:</label>
              <p-select
                id="tipoFilter"
                [options]="tipoOptions"
                [(ngModel)]="selectedTipoFilter"
                (onChange)="onTipoFilterChange()"
                optionLabel="label"
                optionValue="value"
                placeholder="Todos os tipos"
                [showClear]="true"
                class="tipo-filter-dropdown">
              </p-select>
            </div>
          </div>
          
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
          [totalRecords]="items().length"
          [loading]="tableLoading()"
          [sortMode]="'multiple'"
          [multiSortMeta]="multiSortMeta()"
          (onSort)="onSort($event)"
          responsiveLayout="scroll"
          class="p-datatable-sm">
          
          <ng-template pTemplate="header">
            <tr>
              <th *ngFor="let col of displayColumns()" 
                  [pSortableColumn]="col.sortable ? col.field : null"
                  [style.width]="col.width"
                  [class.hide-on-mobile]="col.hideOnMobile"
                  [class.hide-on-tablet]="col.hideOnTablet">
                {{ col.header }}
                <p-sortIcon *ngIf="col.sortable" [field]="col.field"></p-sortIcon>
              </th>
              <th style="width: 120px">Ações</th>
            </tr>
          </ng-template>
          
          <ng-template pTemplate="body" let-item let-rowIndex="rowIndex">
            <tr [class.highlighted-row]="highlightedRowIndex() === rowIndex">
              <td *ngFor="let col of displayColumns()" 
                  [class.hide-on-mobile]="col.hideOnMobile"
                  [class.hide-on-tablet]="col.hideOnTablet">
                <ng-container [ngSwitch]="col.type">
                  <span *ngSwitchCase="'boolean'">
                    <p-tag 
                      [value]="item[col.field] ? 'Ativo' : 'Inativo'"
                      [severity]="item[col.field] ? 'success' : 'danger'">
                    </p-tag>
                  </span>
                  <span *ngSwitchDefault>{{ item[col.field] }}</span>
                </ng-container>
              </td>
              <td>
                <div class="action-buttons">
                  <p-button
                    icon="pi pi-pencil"
                    [pTooltip]="'Editar ' + entityDisplayName().toLowerCase()"
                    tooltipPosition="top"
                    (onClick)="editarItem(item)"
                    [loading]="isActionLoading('edit', item.id)"
                    class="p-button-rounded p-button-text p-button-sm">
                  </p-button>
                  <p-button
                    icon="pi pi-trash"
                    [pTooltip]="'Excluir ' + entityDisplayName().toLowerCase()"
                    tooltipPosition="top"
                    (onClick)="confirmarExclusao(item)"
                    [loading]="isActionLoading('delete', item.id)"
                    class="p-button-rounded p-button-text p-button-sm p-button-danger">
                  </p-button>
                </div>
              </td>
            </tr>
          </ng-template>
          
          <ng-template pTemplate="emptymessage">
            <tr>
              <td [attr.colspan]="displayColumns().length + 1" class="text-center">
                <div class="empty-state">
                  <i class="pi pi-info-circle" style="font-size: 2rem; color: var(--text-color-secondary);"></i>
                  <p>Nenhum {{ entityDisplayName().toLowerCase() }} encontrado.</p>
                </div>
              </td>
            </tr>
          </ng-template>
        </p-table>
      </div>

      <!-- Form Dialog -->
      <p-dialog
        [header]="selectedItem() ? 'Editar ' + entityDisplayName() : 'Novo ' + entityDisplayName()"
[visible]="showForm()"
        (onHide)="cancelarEdicao()"
        [modal]="true"
        [closable]="true"
        [draggable]="false"
        [resizable]="false"
        styleClass="form-dialog"
        [style]="{ width: '600px' }">
        
        <form [formGroup]="form" (ngSubmit)="salvarItem()">
          <div class="form-fields">
            <!-- Código field -->
            <div class="field">
              <label for="codigo" class="required">Código</label>
              <input
                pInputText
                id="codigo"
                formControlName="codigo"
                placeholder="Ex: AGR001, PEC002"
                maxlength="10"
                [class.ng-invalid]="shouldShowError('codigo')"
                [disabled]="isEditMode()"
                class="w-full" />
              <small 
                *ngIf="shouldShowError('codigo')" 
                class="p-error">
                {{ getErrorMessage('codigo') }}
              </small>
              <small class="field-help">
                Código único da atividade agropecuária (2-10 caracteres)
              </small>
            </div>

            <!-- Descrição field -->
            <div class="field">
              <label for="descricao" class="required">Descrição</label>
              <input
                pInputText
                id="descricao"
                formControlName="descricao"
                placeholder="Ex: Cultivo de Soja, Criação de Bovinos"
                maxlength="200"
                [class.ng-invalid]="shouldShowError('descricao')"
                class="w-full" />
              <small 
                *ngIf="shouldShowError('descricao')" 
                class="p-error">
                {{ getErrorMessage('descricao') }}
              </small>
              <small class="field-help">
                Descrição detalhada da atividade (5-200 caracteres)
              </small>
            </div>

            <!-- Tipo field -->
            <div class="field">
              <label for="tipo" class="required">Tipo de Atividade</label>
              <p-select
                id="tipo"
                formControlName="tipo"
                [options]="tipoOptions"
                optionLabel="label"
                optionValue="value"
                placeholder="Selecione o tipo"
                [class.ng-invalid]="shouldShowError('tipo')"
                class="w-full">
              </p-select>
              <small 
                *ngIf="shouldShowError('tipo')" 
                class="p-error">
                {{ getErrorMessage('tipo') }}
              </small>
              <small class="field-help">
                Tipo da atividade: Agricultura, Pecuária ou Mista
              </small>
            </div>

            <!-- Ativo field -->
            <div class="field">
              <div class="flex align-items-center">
                <p-checkbox
                  id="ativo"
                  formControlName="ativo"
                  [binary]="true">
                </p-checkbox>
                <label for="ativo" class="ml-2">Ativo</label>
              </div>
              <small class="field-help">
                Indica se a atividade está ativa no sistema
              </small>
            </div>
          </div>
        </form>
        
        <ng-template pTemplate="footer">
          <div class="flex justify-content-end gap-2">
            <p-button
              label="Cancelar"
              icon="pi pi-times"
              (onClick)="cancelarEdicao()"
              class="p-button-text">
            </p-button>
            <p-button
              [label]="selectedItem() ? 'Atualizar' : 'Criar'"
              icon="pi pi-check"
              (onClick)="salvarItem()"
              [loading]="formLoading()"
              [disabled]="form.invalid"
              class="p-button-primary">
            </p-button>
          </div>
        </ng-template>
      </p-dialog>

      <!-- Confirmation Dialog -->
      <p-confirmDialog></p-confirmDialog>
      
      <!-- Toast Messages -->
      <p-toast></p-toast>
    </div>
  `,
  styleUrls: ['./atividades-agropecuarias.component.scss']
})
export class AtividadesAgropecuariasComponent extends ReferenceCrudBaseComponent<
  AtividadeAgropecuariaDto,
  CriarAtividadeAgropecuariaDto,
  AtualizarAtividadeAgropecuariaDto
> implements OnInit {
  
  protected service = inject(AtividadeAgropecuariaService);

  // Type filtering
  selectedTipoFilter: TipoAtividadeAgropecuaria | null = null;
  tipoOptions = this.service.getTipoOptions();
  
  // Grouped view
  showGroupedView = false;
  groupedActivities: { tipo: TipoAtividadeAgropecuaria; tipoDescricao: string; atividades: AtividadeAgropecuariaDto[] }[] = [];

  // Entity configuration
  protected entityDisplayName = () => 'Atividade Agropecuária';
  protected entityDescription = () => 'Gerenciar atividades agropecuárias do sistema';
  protected defaultSortField = () => 'codigo';
  protected searchFields = () => ['codigo', 'descricao'];

  // Table columns configuration
  protected displayColumns = (): TableColumn[] => [
    {
      field: 'codigo',
      header: 'Código',
      sortable: true,
      width: '120px'
    },
    {
      field: 'descricao',
      header: 'Descrição',
      sortable: true,
      width: '300px'
    },
    {
      field: 'tipoDescricao',
      header: 'Tipo',
      sortable: true,
      width: '120px'
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
    }
  ];

  override ngOnInit(): void {
    super.ngOnInit();
    this.loadGroupedActivities();
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
  onTipoFilterChange(): void {
    if (this.selectedTipoFilter !== null) {
      this.service.obterPorTipo(this.selectedTipoFilter).subscribe({
        next: (atividades) => {
          this.items.set(atividades);
          this.showGroupedView = false;
        },
        error: (error) => console.error('Erro ao filtrar atividades:', error)
      });
    } else {
      this.carregarItens();
      this.loadGroupedActivities();
    }
  }

  /**
   * Load activities grouped by type
   */
  private loadGroupedActivities(): void {
    this.service.obterAgrupadasPorTipo().subscribe({
      next: (grouped) => {
        this.groupedActivities = [
          {
            tipo: TipoAtividadeAgropecuaria.Agricultura,
            tipoDescricao: 'Agricultura',
            atividades: grouped[TipoAtividadeAgropecuaria.Agricultura] || []
          },
          {
            tipo: TipoAtividadeAgropecuaria.Pecuaria,
            tipoDescricao: 'Pecuária',
            atividades: grouped[TipoAtividadeAgropecuaria.Pecuaria] || []
          },
          {
            tipo: TipoAtividadeAgropecuaria.Mista,
            tipoDescricao: 'Mista',
            atividades: grouped[TipoAtividadeAgropecuaria.Mista] || []
          }
        ].filter(group => group.atividades.length > 0);
        
        this.showGroupedView = this.selectedTipoFilter === null;
      },
      error: (error) => console.error('Erro ao carregar atividades agrupadas:', error)
    });
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
   * Toggle between grouped and list view
   */
  toggleView(): void {
    this.showGroupedView = !this.showGroupedView;
    if (this.showGroupedView) {
      this.selectedTipoFilter = null;
      this.loadGroupedActivities();
    }
  }

  /**
   * Override carregarItens to refresh grouped view as well
   */
  override carregarItens(): void {
    super.carregarItens();
    if (this.selectedTipoFilter === null) {
      this.loadGroupedActivities();
    }
  }
}