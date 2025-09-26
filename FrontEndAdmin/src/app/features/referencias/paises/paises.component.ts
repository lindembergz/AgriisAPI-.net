import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TooltipModule } from 'primeng/tooltip';
import { DialogModule } from 'primeng/dialog';
import { CheckboxModule } from 'primeng/checkbox';
import { ReferenceCrudBaseComponent } from '../../../shared/components/reference-crud-base/reference-crud-base.component';
import { PaisDto, CriarPaisDto, AtualizarPaisDto } from '../../../shared/models/reference.model';
import { PaisService } from './services/pais.service';
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
 * Component for managing Países (Countries) with CRUD operations
 * Shows UF dependency count and prevents deletion when UFs exist
 */
@Component({
  selector: 'app-paises',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    InputTextModule,
    TagModule,
    TableModule,
    ButtonModule,
    SelectModule,
    ConfirmDialogModule,
    ToastModule,
    ProgressSpinnerModule,
    TooltipModule,
    DialogModule,
    CheckboxModule
  ],
  template: `
    <div class="paises-container">
      <!-- Form fields slot -->
      <div slot="form-fields" class="form-fields">
        <!-- Código field -->
        <div class="field">
          <label for="codigo" class="required">Código</label>
          <input
            pInputText
            id="codigo"
            formControlName="codigo"
            placeholder="Ex: BR, US, AR"
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
            Código do país (ISO 3166-1 alpha-2 ou alpha-3)
          </small>
        </div>

        <!-- Nome field -->
        <div class="field">
          <label for="nome" class="required">Nome</label>
          <input
            pInputText
            id="nome"
            formControlName="nome"
            placeholder="Ex: Brasil, Estados Unidos, Argentina"
            maxlength="100"
            [class.ng-invalid]="shouldShowError('nome')"
            class="w-full" />
          <small 
            *ngIf="shouldShowError('nome')" 
            class="p-error">
            {{ getErrorMessage('nome') }}
          </small>
        </div>
      </div>

      <!-- Custom UF count column -->
      <ng-container slot="custom-ufsCount">
        <p-tag 
          value="0" 
          severity="secondary"
          class="uf-count-tag">
        </p-tag>
      </ng-container>
    </div>
  `,
  styleUrls: ['./paises.component.scss']
})
export class PaisesComponent extends ReferenceCrudBaseComponent<
  PaisDto,
  CriarPaisDto,
  AtualizarPaisDto
> {
  
  protected service = inject(PaisService);
  private fieldValidators = inject(FieldValidatorsUtil);

  // Entity configuration
  protected entityDisplayName = () => 'País';
  protected entityDescription = () => 'Gerenciar países do sistema';
  protected defaultSortField = () => 'nome';
  protected searchFields = () => ['codigo', 'nome'];

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
      field: 'ufsCount',
      header: 'UFs',
      sortable: true,
      width: '100px',
      type: 'custom',
      hideOnMobile: true
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
  }

  /**
   * Get UFs count display text
   */
  getUfsCountDisplay(item: any): string {
    const count = item.ufsCount || 0;
    return count === 0 ? 'Nenhuma UF' : `${count} UF${count > 1 ? 's' : ''}`;
  }

  /**
   * Get UFs count severity for styling
   */
  getUfsCountSeverity(count: number): string {
    if (count === 0) return 'secondary';
    if (count <= 5) return 'info';
    if (count <= 15) return 'success';
    return 'warning';
  }

  /**
   * Override excluir to check UF dependencies
   */
  override excluirItem(item: PaisDto): void {
    // Check if país has UFs before allowing deletion
    this.service.verificarDependenciasUf(item.id).subscribe({
      next: (temUfs) => {
        if (temUfs) {
          this.messageService.add({
            severity: 'warn',
            summary: 'Não é possível excluir',
            detail: 'Este país possui UFs cadastradas. Remova as UFs primeiro.',
            life: 5000
          });
          return;
        }
        
        // Proceed with normal deletion flow
        super.excluirItem(item);
      },
      error: (error) => {
        console.error('Erro ao verificar dependências:', error);
      }
    });
  }
}