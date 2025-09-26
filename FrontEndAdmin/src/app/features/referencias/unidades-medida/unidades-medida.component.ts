import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, ReactiveFormsModule, Validators, FormsModule } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { InputNumberModule } from 'primeng/inputnumber';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DividerModule } from 'primeng/divider';
import { TableModule } from 'primeng/table';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { DialogModule } from 'primeng/dialog';
import { CheckboxModule } from 'primeng/checkbox';
import { ReferenceCrudBaseComponent } from '../../../shared/components/reference-crud-base/reference-crud-base.component';
import { UnidadeMedidaDto, CriarUnidadeMedidaDto, AtualizarUnidadeMedidaDto, TipoUnidadeMedida } from '../../../shared/models/reference.model';
import { UnidadeMedidaService, TipoUnidadeOption, UnidadeDropdownOption, ConversaoResult } from './services/unidade-medida.service';

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
    InputNumberModule,
    ButtonModule,
    CardModule,
    DividerModule,
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
    <div class="unidades-medida-container">
      <!-- Custom toolbar slot -->
      <div slot="custom-toolbar" class="custom-toolbar">
        <!-- Type filter -->
        <div class="filter-group">
          <label for="tipoFilter">Filtrar por Tipo:</label>
          <p-select
            id="tipoFilter"
            [options]="tiposDisponiveis"
            [(ngModel)]="tipoSelecionado"
            (onChange)="onTipoFilterChange()"
            optionLabel="descricao"
            optionValue="valor"
            placeholder="Todos os tipos"
            [showClear]="true"
            class="filter-dropdown">
          </p-select>
        </div>

        <!-- Conversion calculator toggle -->
        <p-button
          label="Calculadora de Conversão"
          icon="pi pi-calculator"
          [outlined]="true"
          (onClick)="toggleConversionCalculator()"
          class="ml-2">
        </p-button>
      </div>

      <!-- Form fields slot -->
      <div slot="form-fields" class="form-fields">
        <!-- Símbolo field -->
        <div class="field">
          <label for="simbolo" class="required">Símbolo</label>
          <input
            pInputText
            id="simbolo"
            formControlName="simbolo"
            placeholder="Ex: kg, L, m², un"
            maxlength="10"
            [class.ng-invalid]="shouldShowError('simbolo')"
            [disabled]="isEditMode()"
            class="w-full" />
          <small 
            *ngIf="shouldShowError('simbolo')" 
            class="p-error">
            {{ getErrorMessage('simbolo') }}
          </small>
          <small class="field-help">
            Símbolo único da unidade de medida
          </small>
        </div>

        <!-- Nome field -->
        <div class="field">
          <label for="nome" class="required">Nome</label>
          <input
            pInputText
            id="nome"
            formControlName="nome"
            placeholder="Ex: Quilograma, Litro, Metro Quadrado"
            maxlength="100"
            [class.ng-invalid]="shouldShowError('nome')"
            class="w-full" />
          <small 
            *ngIf="shouldShowError('nome')" 
            class="p-error">
            {{ getErrorMessage('nome') }}
          </small>
        </div>

        <!-- Tipo field -->
        <div class="field">
          <label for="tipo" class="required">Tipo</label>
          <p-select
            id="tipo"
            formControlName="tipo"
            [options]="tiposDisponiveis"
            optionLabel="descricao"
            optionValue="valor"
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
            Categoria da unidade de medida
          </small>
        </div>

        <!-- Fator de Conversão field -->
        <div class="field">
          <label for="fatorConversao">Fator de Conversão</label>
          <p-inputNumber
            id="fatorConversao"
            formControlName="fatorConversao"
            placeholder="1.0"
            [minFractionDigits]="0"
            [maxFractionDigits]="6"
            [min]="0.000001"
            [class.ng-invalid]="shouldShowError('fatorConversao')"
            class="w-full">
          </p-inputNumber>
          <small 
            *ngIf="shouldShowError('fatorConversao')" 
            class="p-error">
            {{ getErrorMessage('fatorConversao') }}
          </small>
          <small class="field-help">
            Fator para conversão entre unidades do mesmo tipo (padrão: 1.0)
          </small>
        </div>
      </div>

      <!-- Custom content slot for conversion calculator -->
      <div slot="custom-content" *ngIf="showConversionCalculator" class="conversion-calculator">
        <p-card header="Calculadora de Conversão" class="mt-4">
          <div class="conversion-form">
            <div class="p-grid p-align-center">
              <!-- Quantidade origem -->
              <div class="p-col-12 p-md-3">
                <label for="quantidadeOrigem">Quantidade:</label>
                <p-inputNumber
                  id="quantidadeOrigem"
                  [(ngModel)]="conversaoForm.quantidade"
                  [minFractionDigits]="0"
                  [maxFractionDigits]="6"
                  [min]="0"
                  placeholder="0"
                  class="w-full">
                </p-inputNumber>
              </div>

              <!-- Unidade origem -->
              <div class="p-col-12 p-md-3">
                <label for="unidadeOrigem">De:</label>
                <p-select
                  id="unidadeOrigem"
                  [(ngModel)]="conversaoForm.unidadeOrigemId"
                  [options]="unidadesParaConversao"
                  optionLabel="nome"
                  optionValue="id"
                  placeholder="Selecione"
                  [filter]="true"
                  filterBy="nome,simbolo"
                  class="w-full">
                  <ng-template pTemplate="item" let-unidade>
                    <div class="dropdown-item">
                      <strong>{{ unidade.simbolo }}</strong> - {{ unidade.nome }}
                    </div>
                  </ng-template>
                </p-select>
              </div>

              <!-- Unidade destino -->
              <div class="p-col-12 p-md-3">
                <label for="unidadeDestino">Para:</label>
                <p-select
                  id="unidadeDestino"
                  [(ngModel)]="conversaoForm.unidadeDestinoId"
                  [options]="unidadesParaConversao"
                  optionLabel="nome"
                  optionValue="id"
                  placeholder="Selecione"
                  [filter]="true"
                  filterBy="nome,simbolo"
                  class="w-full">
                  <ng-template pTemplate="item" let-unidade>
                    <div class="dropdown-item">
                      <strong>{{ unidade.simbolo }}</strong> - {{ unidade.nome }}
                    </div>
                  </ng-template>
                </p-select>
              </div>

              <!-- Botão converter -->
              <div class="p-col-12 p-md-3">
                <p-button
                  label="Converter"
                  icon="pi pi-arrow-right"
                  (onClick)="calcularConversao()"
                  [disabled]="!podeCalcularConversao()"
                  [loading]="calculandoConversao"
                  class="w-full">
                </p-button>
              </div>
            </div>

            <!-- Resultado da conversão -->
            <div *ngIf="resultadoConversao" class="conversion-result mt-3">
              <p-divider></p-divider>
              <div class="result-display">
                <h4>Resultado da Conversão:</h4>
                <p class="result-text">
                  <strong>{{ resultadoConversao.quantidadeOriginal | number:'1.0-6' }}</strong>
                  {{ getUnidadeById(resultadoConversao.unidadeOrigemId)?.simbolo }}
                  =
                  <strong>{{ resultadoConversao.quantidadeConvertida | number:'1.0-6' }}</strong>
                  {{ getUnidadeById(resultadoConversao.unidadeDestinoId)?.simbolo }}
                </p>
              </div>
            </div>
          </div>
        </p-card>
      </div>
    </div>
  `,
  styleUrls: ['./unidades-medida.component.scss']
})
export class UnidadesMedidaComponent extends ReferenceCrudBaseComponent<
  UnidadeMedidaDto,
  CriarUnidadeMedidaDto,
  AtualizarUnidadeMedidaDto
> implements OnInit {
  
  protected service = inject(UnidadeMedidaService);

  // Type filtering
  tiposDisponiveis: TipoUnidadeOption[] = [];
  tipoSelecionado: TipoUnidadeMedida | null = null;
  filteredItems: UnidadeMedidaDto[] = [];

  // Conversion calculator
  showConversionCalculator = false;
  unidadesParaConversao: UnidadeDropdownOption[] = [];
  calculandoConversao = false;
  resultadoConversao: ConversaoResult | null = null;
  
  conversaoForm = {
    quantidade: 0,
    unidadeOrigemId: null as number | null,
    unidadeDestinoId: null as number | null
  };

  // Entity configuration
  protected entityDisplayName = () => 'Unidade de Medida';
  protected entityDescription = () => 'Gerenciar unidades de medida utilizadas no sistema';
  protected defaultSortField = () => 'simbolo';
  protected searchFields = () => ['simbolo', 'nome'];

  override ngOnInit(): void {
    super.ngOnInit();
    this.carregarTipos();
    this.carregarUnidadesParaConversao();
    
    // Initialize filtered items
    this.filteredItems = [...this.items()];
  }

  // Table columns configuration
  protected displayColumns = (): TableColumn[] => [
    {
      field: 'simbolo',
      header: 'Símbolo',
      sortable: true,
      width: '120px'
    },
    {
      field: 'nome',
      header: 'Nome',
      sortable: true,
      width: '250px'
    },
    {
      field: 'tipo',
      header: 'Tipo',
      sortable: true,
      width: '150px',
      type: 'custom'
    },
    {
      field: 'fatorConversao',
      header: 'Fator Conversão',
      sortable: true,
      width: '150px',
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
      simbolo: ['', [
        Validators.required,
        Validators.minLength(1),
        Validators.maxLength(10)
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
        Validators.min(0.000001)
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
      },
      error: (error) => {
        console.error('Erro ao carregar tipos de unidade:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Erro',
          detail: 'Erro ao carregar tipos de unidade de medida'
        });
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

  /**
   * Handle type filter change
   */
  onTipoFilterChange(): void {
    if (this.tipoSelecionado) {
      // Apply filter to the data
      this.applyCustomFilter('tipo', this.tipoSelecionado);
    } else {
      // Clear filter
      this.clearCustomFilter('tipo');
    }
  }

  /**
   * Toggle conversion calculator visibility
   */
  toggleConversionCalculator(): void {
    this.showConversionCalculator = !this.showConversionCalculator;
    if (this.showConversionCalculator) {
      this.resetConversaoForm();
    }
  }

  /**
   * Reset conversion form
   */
  private resetConversaoForm(): void {
    this.conversaoForm = {
      quantidade: 0,
      unidadeOrigemId: null,
      unidadeDestinoId: null
    };
    this.resultadoConversao = null;
  }

  /**
   * Check if conversion can be calculated
   */
  podeCalcularConversao(): boolean {
    return !!(
      this.conversaoForm.quantidade > 0 &&
      this.conversaoForm.unidadeOrigemId &&
      this.conversaoForm.unidadeDestinoId
    );
  }

  /**
   * Calculate unit conversion
   */
  calcularConversao(): void {
    if (!this.podeCalcularConversao()) {
      return;
    }

    this.calculandoConversao = true;
    this.resultadoConversao = null;

    this.service.converter(
      this.conversaoForm.quantidade,
      this.conversaoForm.unidadeOrigemId!,
      this.conversaoForm.unidadeDestinoId!
    ).subscribe({
      next: (resultado) => {
        this.resultadoConversao = resultado;
        this.calculandoConversao = false;
      },
      error: (error) => {
        console.error('Erro ao calcular conversão:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Erro na Conversão',
          detail: error.error?.errorDescription || 'Erro ao calcular conversão entre unidades'
        });
        this.calculandoConversao = false;
      }
    });
  }

  /**
   * Get unit by ID for display purposes
   */
  getUnidadeById(id: number): UnidadeDropdownOption | undefined {
    return this.unidadesParaConversao.find(u => u.id === id);
  }

  /**
   * Get tipo description for display
   */
  getTipoDescricao(tipo: TipoUnidadeMedida): string {
    return this.service.getTipoDescricao(tipo);
  }

  /**
   * Custom filter implementation
   */
  private applyCustomFilter(field: string, value: any): void {
    if (field === 'tipo' && value !== null) {
      this.filteredItems = this.items().filter(item => 
        (item as UnidadeMedidaDto).tipo === value
      );
    }
  }

  /**
   * Clear custom filter
   */
  private clearCustomFilter(_field: string): void {
    // Reset to show all items
    this.filteredItems = [...this.items()];
  }
}