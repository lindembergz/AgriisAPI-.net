import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormsModule } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
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
 * Component for managing Moedas (Currencies) with CRUD operations
 * Extends ReferenceCrudBaseComponent for consistent behavior
 */
@Component({
  selector: 'app-moedas',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    InputTextModule,
    TableModule,
    ButtonModule,
    SelectModule,
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
            [label]="'Nova ' + entityDisplayName()"
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
                  <p>Nenhuma {{ entityDisplayName().toLowerCase() }} encontrada.</p>
                </div>
              </td>
            </tr>
          </ng-template>
        </p-table>
      </div>

      <!-- Form Dialog -->
      <p-dialog
        [header]="selectedItem() ? 'Editar ' + entityDisplayName() : 'Nova ' + entityDisplayName()"
        [visible]="showForm()"
        (onHide)="cancelarEdicao()"
        [modal]="true"
        [closable]="true"
        [draggable]="false"
        [resizable]="false"
        styleClass="form-dialog"
        [style]="{ width: '500px' }">
        
        <form [formGroup]="form" (ngSubmit)="salvarItem()">
          <div class="form-fields">
            <!-- Código field -->
            <div class="field">
              <label for="codigo" class="required">Código</label>
              <input
                pInputText
                id="codigo"
                formControlName="codigo"
                placeholder="Ex: BRL, USD, EUR"
                maxlength="3"
                [class.ng-invalid]="shouldShowError('codigo')"
                [disabled]="isEditMode()"
                class="w-full" />
              <small 
                *ngIf="shouldShowError('codigo')" 
                class="p-error">
                {{ getErrorMessage('codigo') }}
              </small>
              <small class="field-help">
                Código da moeda com 3 caracteres (ISO 4217)
              </small>
            </div>

            <!-- Nome field -->
            <div class="field">
              <label for="nome" class="required">Nome</label>
              <input
                pInputText
                id="nome"
                formControlName="nome"
                placeholder="Ex: Real Brasileiro, Dólar Americano"
                maxlength="100"
                [class.ng-invalid]="shouldShowError('nome')"
                class="w-full" />
              <small 
                *ngIf="shouldShowError('nome')" 
                class="p-error">
                {{ getErrorMessage('nome') }}
              </small>
            </div>

            <!-- Símbolo field -->
            <div class="field">
              <label for="simbolo" class="required">Símbolo</label>
              <input
                pInputText
                id="simbolo"
                formControlName="simbolo"
                placeholder="Ex: R$, US$, €"
                maxlength="5"
                [class.ng-invalid]="shouldShowError('simbolo')"
                class="w-full" />
              <small 
                *ngIf="shouldShowError('simbolo')" 
                class="p-error">
                {{ getErrorMessage('simbolo') }}
              </small>
              <small class="field-help">
                Símbolo da moeda para exibição
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
                Indica se a moeda está ativa no sistema
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
  styleUrls: ['./moedas.component.scss']
})
export class MoedasComponent extends ReferenceCrudBaseComponent<
  MoedaDto,
  CriarMoedaDto,
  AtualizarMoedaDto
> {
  
  protected service = inject(MoedaService);
  private fieldValidators = inject(FieldValidatorsUtil);

  // Entity configuration
  protected entityDisplayName = () => 'Moeda';
  protected entityDescription = () => 'Gerenciar moedas utilizadas no sistema';
  protected defaultSortField = () => 'codigo';
  protected searchFields = () => ['codigo', 'nome', 'simbolo'];

  // Table columns configuration
  protected displayColumns = (): TableColumn[] => [
    {
      field: 'codigo',
      header: 'Código',
      sortable: true,
      width: '120px'
    },
    {
      field: 'nome',
      header: 'Nome',
      sortable: true,
      width: '300px'
    },
    {
      field: 'simbolo',
      header: 'Símbolo',
      sortable: true,
      width: '100px'
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
}