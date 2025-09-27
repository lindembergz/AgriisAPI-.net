import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectModule } from 'primeng/select';
import { TextareaModule } from 'primeng/textarea';
import { CheckboxModule } from 'primeng/checkbox';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { DialogModule } from 'primeng/dialog';
import { FieldErrorComponent } from '../../shared/components/field-error.component';
import { HierarchicalSelectorComponent, HierarchicalItem } from '../../shared/components/hierarchical-selector/hierarchical-selector.component';
import { ReferenceCrudBaseComponent } from '../../shared/components/reference-crud-base/reference-crud-base.component';
import { ProdutoService, DropdownOption, ProdutoDisplayDto } from './services/produto.service';
import { ProdutoDto, CriarProdutoDto, AtualizarProdutoDto, TipoProduto, TipoCalculoPeso } from '../../shared/models/produto.model';
import { CategoriaDto } from '../../shared/models/reference.model';
import { ValidationService } from '../../shared/services/validation.service';
import { ApiValidationService } from '../../shared/services/api-validation.service';
import { produtoCodigoUniqueValidator, produtoNomeUniqueValidator, produtoReferencesValidator } from '../../shared/validators/async-validators';

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
 * Produtos CRUD component with reference dropdowns
 */
@Component({
  selector: 'app-produtos',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    InputTextModule,
    InputNumberModule,
    SelectModule,
    TextareaModule,
    CheckboxModule,
    ButtonModule,
    FieldErrorComponent,
    HierarchicalSelectorComponent,
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
    <div class="produtos-container">
      <!-- Filters Section -->
      <div class="filters-section" *ngIf="!loading()">
        <div class="filters-header">
          <h3>Filtros</h3>
          <p-button
            label="Limpar Filtros"
            icon="pi pi-times"
            (onClick)="limparFiltros()"
            class="p-button-text p-button-sm"
            [disabled]="!temFiltrosAtivos()">
          </p-button>
        </div>
        
        <div class="filters-grid">
          <!-- Categoria Filter -->
          <div class="filter-field">
            <label for="filtroCategoria">Categoria</label>
            <p-select
              id="filtroCategoria"
              [(ngModel)]="filtroCategoria"
              [options]="categoriasParaFiltro()"
              optionLabel="nome"
              optionValue="id"
              placeholder="Todas as categorias"
              (onChange)="aplicarFiltros()"
              [showClear]="true"
              [filter]="true"
              filterBy="nome">
            </p-select>
          </div>

          <!-- Unidade de Medida Filter -->
          <div class="filter-field">
            <label for="filtroUnidade">Unidade de Medida</label>
            <p-select
              id="filtroUnidade"
              [(ngModel)]="filtroUnidadeMedida"
              [options]="unidadesParaFiltro()"
              optionLabel="nome"
              optionValue="id"
              placeholder="Todas as unidades"
              (onChange)="aplicarFiltros()"
              [showClear]="true"
              [filter]="true"
              filterBy="nome">
            </p-select>
          </div>

          <!-- Atividade Agropecuária Filter -->
          <div class="filter-field">
            <label for="filtroAtividade">Atividade Agropecuária</label>
            <p-select
              id="filtroAtividade"
              [(ngModel)]="filtroAtividadeAgropecuaria"
              [options]="atividadesParaFiltro()"
              optionLabel="nome"
              optionValue="id"
              placeholder="Todas as atividades"
              (onChange)="aplicarFiltros()"
              [showClear]="true"
              [filter]="true"
              filterBy="nome">
            </p-select>
          </div>

          <!-- Search Field -->
          <div class="filter-field">
            <label for="filtroTexto">Buscar</label>
            <span class="p-input-icon-left">
              <i class="pi pi-search"></i>
              <input
                id="filtroTexto"
                type="text"
                pInputText
                [(ngModel)]="filtroTexto"
                (input)="onBuscaChange($event)"
                placeholder="Buscar por código, nome..."
                class="search-input">
            </span>
          </div>
        </div>
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
                  <span *ngSwitchCase="'custom'" [ngSwitch]="col.field">
                    <span *ngSwitchCase="'preco'" [class]="getPrecoClass(item.preco)">
                      {{ formatarPreco(item.preco) }}
                    </span>
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

        <!-- Form fields slot -->
        <div slot="form-fields" class="form-fields">
        <!-- Código -->
        <div class="field">
          <label for="codigo" class="required">Código</label>
          <input
            id="codigo"
            type="text"
            pInputText
            formControlName="codigo"
            placeholder="Digite o código do produto"
            [class.ng-invalid]="shouldShowError('codigo')"
            [disabled]="isEditMode()"
            maxlength="20">
          <app-field-error 
            *ngIf="shouldShowError('codigo')"
            [message]="getErrorMessage('codigo')">
          </app-field-error>
        </div>

        <!-- Nome -->
        <div class="field">
          <label for="nome" class="required">Nome</label>
          <input
            id="nome"
            type="text"
            pInputText
            formControlName="nome"
            placeholder="Digite o nome do produto"
            [class.ng-invalid]="shouldShowError('nome')"
            maxlength="200">
          <app-field-error 
            *ngIf="shouldShowError('nome')"
            [message]="getErrorMessage('nome')">
          </app-field-error>
        </div>

        <!-- Descrição -->
        <div class="field">
          <label for="descricao">Descrição</label>
          <textarea
            id="descricao"
            pTextarea
            formControlName="descricao"
            placeholder="Digite uma descrição para o produto"
            [class.ng-invalid]="shouldShowError('descricao')"
            rows="3"
            maxlength="1000">
          </textarea>
          <app-field-error 
            *ngIf="shouldShowError('descricao')"
            [message]="getErrorMessage('descricao')">
          </app-field-error>
        </div>

        <!-- Preço -->
        <div class="field">
          <label for="preco">Preço</label>
          <p-inputNumber
            id="preco"
            formControlName="preco"
            mode="currency"
            currency="BRL"
            locale="pt-BR"
            placeholder="0,00"
            [class.ng-invalid]="shouldShowError('preco')"
            [min]="0"
            [maxFractionDigits]="2">
          </p-inputNumber>
          <app-field-error 
            *ngIf="shouldShowError('preco')"
            [message]="getErrorMessage('preco')">
          </app-field-error>
        </div>

        <!-- Categoria (Hierarchical Selector) -->
        <div class="field">
          <label for="categoria" class="required">Categoria</label>
          <app-hierarchical-selector
            [items]="categoriasHierarchicas()"
            [selectionMode]="'single'"
            [loading]="loadingCategorias()"
            [error]="errorCategorias()"
            [showHeader]="true"
            [showSearch]="true"
            [scrollHeight]="'300px'"
            (selectionChange)="onCategoriaChange($event)"
            class="categoria-selector">
          </app-hierarchical-selector>
          <app-field-error 
            *ngIf="shouldShowError('categoriaId')"
            [message]="getErrorMessage('categoriaId')">
          </app-field-error>
        </div>

        <!-- Unidade de Medida -->
        <div class="field">
          <label for="unidadeMedida" class="required">Unidade de Medida</label>
          <p-select
            id="unidadeMedida"
            formControlName="unidadeMedidaId"
            [options]="unidadesMedida()"
            optionLabel="nome"
            optionValue="id"
            placeholder="Selecione a unidade de medida"
            [class.ng-invalid]="shouldShowError('unidadeMedidaId')"
            [loading]="loadingUnidades()"
            (onChange)="onUnidadeMedidaChange($event)"
            [filter]="true"
            filterBy="nome"
            [showClear]="false">
            <ng-template pTemplate="selectedItem" let-option>
              <div *ngIf="option">
                <strong>{{ option.codigo }}</strong> - {{ option.nome }}
              </div>
            </ng-template>
            <ng-template pTemplate="item" let-option>
              <div>
                <strong>{{ option.codigo }}</strong> - {{ option.nome }}
              </div>
            </ng-template>
          </p-select>
          <app-field-error 
            *ngIf="shouldShowError('unidadeMedidaId')"
            [message]="getErrorMessage('unidadeMedidaId')">
          </app-field-error>
        </div>

        <!-- Embalagem (filtered by UnidadeMedida) -->
        <div class="field">
          <label for="embalagem">Embalagem</label>
          <p-select
            id="embalagem"
            formControlName="embalagemId"
            [options]="embalagensDisponiveis()"
            optionLabel="nome"
            optionValue="id"
            placeholder="Selecione a embalagem"
            [class.ng-invalid]="shouldShowError('embalagemId')"
            [loading]="loadingEmbalagens()"
            [disabled]="!form.get('unidadeMedidaId')?.value"
            [filter]="true"
            filterBy="nome"
            [showClear]="true">
          </p-select>
          <small class="field-help" *ngIf="!form.get('unidadeMedidaId')?.value">
            Selecione uma unidade de medida primeiro
          </small>
          <app-field-error 
            *ngIf="shouldShowError('embalagemId')"
            [message]="getErrorMessage('embalagemId')">
          </app-field-error>
        </div>

        <!-- Atividade Agropecuária -->
        <div class="field">
          <label for="atividadeAgropecuaria">Atividade Agropecuária</label>
          <p-select
            id="atividadeAgropecuaria"
            formControlName="atividadeAgropecuariaId"
            [options]="atividadesAgropecuarias()"
            optionLabel="nome"
            optionValue="id"
            placeholder="Selecione a atividade agropecuária"
            [class.ng-invalid]="shouldShowError('atividadeAgropecuariaId')"
            [loading]="loadingAtividades()"
            [filter]="true"
            filterBy="nome"
            [showClear]="true">
            <ng-template pTemplate="selectedItem" let-option>
              <div *ngIf="option">
                <strong>{{ option.codigo }}</strong> - {{ option.nome }}
              </div>
            </ng-template>
            <ng-template pTemplate="item" let-option>
              <div>
                <strong>{{ option.codigo }}</strong> - {{ option.nome }}
              </div>
            </ng-template>
          </p-select>
          <app-field-error 
            *ngIf="shouldShowError('atividadeAgropecuariaId')"
            [message]="getErrorMessage('atividadeAgropecuariaId')">
          </app-field-error>
        </div>
      </div>
    </div>
  `,
  styleUrls: ['./produtos.component.scss']
})
export class ProdutosComponent extends ReferenceCrudBaseComponent<ProdutoDisplayDto, CriarProdutoDto, AtualizarProdutoDto> implements OnInit {
  
  protected service = inject(ProdutoService) as any;
  protected validationService = inject(ValidationService);
  private apiValidationService = inject(ApiValidationService);

  // Reference data signals
  unidadesMedida = signal<DropdownOption[]>([]);
  embalagensDisponiveis = signal<DropdownOption[]>([]);
  categoriasHierarchicas = signal<HierarchicalItem[]>([]);
  atividadesAgropecuarias = signal<DropdownOption[]>([]);
  fornecedores = signal<DropdownOption[]>([]);
  culturas = signal<DropdownOption[]>([]);
  produtosPais = signal<DropdownOption[]>([]);

  // Loading states
  loadingUnidades = signal<boolean>(false);
  loadingEmbalagens = signal<boolean>(false);
  loadingCategorias = signal<boolean>(false);
  loadingAtividades = signal<boolean>(false);
  loadingFornecedores = signal<boolean>(false);
  loadingCulturas = signal<boolean>(false);
  loadingProdutosPais = signal<boolean>(false);

  // Error states
  errorCategorias = signal<string | null>(null);

  // Selected categoria for form control
  selectedCategoria = signal<HierarchicalItem | null>(null);

  // Filter properties
  filtroCategoria: number | null = null;
  filtroUnidadeMedida: number | null = null;
  filtroAtividadeAgropecuaria: number | null = null;
  filtroTexto: string = '';

  // Filter options
  categoriasParaFiltro = signal<DropdownOption[]>([]);
  unidadesParaFiltro = signal<DropdownOption[]>([]);
  atividadesParaFiltro = signal<DropdownOption[]>([]);

  // All items for filtering
  private todosItens: ProdutoDisplayDto[] = [];
  
  // Search debounce timer
  private searchTimer: any;

  // Enum options for dropdowns
  tiposProduto = [
    { label: 'Semente', value: TipoProduto.Semente },
    { label: 'Defensivo', value: TipoProduto.Defensivo },
    { label: 'Fertilizante', value: TipoProduto.Fertilizante },
    { label: 'Inoculante', value: TipoProduto.Inoculante },
    { label: 'Adjuvante', value: TipoProduto.Adjuvante },
    { label: 'Equipamento', value: TipoProduto.Equipamento },
    { label: 'Serviço', value: TipoProduto.Servico },
    { label: 'Outro', value: TipoProduto.Outro }
  ];

  tiposCalculoPeso = [
    { label: 'Peso Nominal', value: TipoCalculoPeso.PesoNominal },
    { label: 'Peso Cubado', value: TipoCalculoPeso.PesoCubado },
    { label: 'Maior Peso', value: TipoCalculoPeso.MaiorPeso }
  ];

  protected entityDisplayName = () => 'Produto';
  protected entityDescription = () => 'Gerencie o catálogo de produtos agrícolas';
  protected defaultSortField = () => 'nome';
  protected searchFields = () => ['codigo', 'nome', 'categoriaNome', 'unidadeMedidaNome', 'atividadeAgropecuariaNome'];

  protected displayColumns = (): TableColumn[] => [
    { field: 'codigo', header: 'Código', sortable: true, width: '120px' },
    { field: 'nome', header: 'Nome', sortable: true },
    { field: 'categoriaNome', header: 'Categoria', sortable: true, hideOnMobile: true },
    { field: 'unidadeMedidaSimbolo', header: 'Unidade', sortable: true, width: '100px', hideOnMobile: true },
    { field: 'preco', header: 'Preço', sortable: true, width: '120px', type: 'custom', hideOnTablet: true },
    { field: 'ativo', header: 'Status', sortable: true, width: '100px', type: 'boolean' }
  ];

  ngOnInit(): void {
    super.ngOnInit();
    this.carregarDadosReferencia();
  }

  protected createFormGroup(): FormGroup {
    const form = this.fb.group({
      codigo: ['', 
        [Validators.required, Validators.maxLength(20)],
        [produtoCodigoUniqueValidator(this.apiValidationService, this.isEditMode() ? this.selectedItem()?.id : undefined)]
      ],
      nome: ['', 
        [Validators.required, Validators.maxLength(200)],
        [produtoNomeUniqueValidator(this.apiValidationService, this.isEditMode() ? this.selectedItem()?.id : undefined)]
      ],
      descricao: ['', [Validators.maxLength(1000)]],
      marca: ['', [Validators.maxLength(100)]],
      tipo: [0, [Validators.required]], // TipoProduto.Semente como padrão
      categoriaId: [null, [Validators.required]],
      unidadeMedidaId: [null, [Validators.required]],
      embalagemId: [null],
      atividadeAgropecuariaId: [null],
      tipoCalculoPeso: [0, [Validators.required]], // TipoCalculoPeso.PesoNominal como padrão
      produtoRestrito: [false],
      observacoesRestricao: [''],
      fornecedorId: [null, [Validators.required]],
      produtoPaiId: [null],
      culturasIds: [[]],
      // Dimensões obrigatórias
      dimensoes: this.fb.group({
        altura: [0, [Validators.required, Validators.min(0)]],
        largura: [0, [Validators.required, Validators.min(0)]],
        comprimento: [0, [Validators.required, Validators.min(0)]],
        pesoNominal: [0, [Validators.required, Validators.min(0)]],
        pesoEmbalagem: [0, [Validators.required, Validators.min(0)]],
        pms: [null],
        quantidadeMinima: [1, [Validators.required, Validators.min(1)]],
        embalagem: ['', [Validators.required, Validators.maxLength(100)]],
        faixaDensidadeInicial: [null],
        faixaDensidadeFinal: [null]
      }),
      ativo: [true]
    }, {
      asyncValidators: [produtoReferencesValidator(this.apiValidationService)]
    });

    return form;
  }

  protected mapToCreateDto(formValue: any): CriarProdutoDto {
    return {
      nome: formValue.nome,
      descricao: formValue.descricao || undefined,
      codigo: formValue.codigo,
      marca: formValue.marca || undefined,
      tipo: formValue.tipo,
      unidadeMedidaId: formValue.unidadeMedidaId,
      embalagemId: formValue.embalagemId || undefined,
      atividadeAgropecuariaId: formValue.atividadeAgropecuariaId || undefined,
      tipoCalculoPeso: formValue.tipoCalculoPeso,
      produtoRestrito: formValue.produtoRestrito,
      observacoesRestricao: formValue.observacoesRestricao || undefined,
      categoriaId: formValue.categoriaId,
      fornecedorId: formValue.fornecedorId,
      produtoPaiId: formValue.produtoPaiId || undefined,
      dimensoes: formValue.dimensoes,
      culturasIds: formValue.culturasIds || []
    };
  }

  protected mapToUpdateDto(formValue: any): AtualizarProdutoDto {
    return {
      nome: formValue.nome,
      descricao: formValue.descricao || undefined,
      codigo: formValue.codigo,
      marca: formValue.marca || undefined,
      unidadeMedidaId: formValue.unidadeMedidaId,
      embalagemId: formValue.embalagemId || undefined,
      atividadeAgropecuariaId: formValue.atividadeAgropecuariaId || undefined,
      tipoCalculoPeso: formValue.tipoCalculoPeso,
      produtoRestrito: formValue.produtoRestrito,
      observacoesRestricao: formValue.observacoesRestricao || undefined,
      categoriaId: formValue.categoriaId,
      dimensoes: formValue.dimensoes,
      culturasIds: formValue.culturasIds || []
    };
  }



  protected populateForm(item: ProdutoDisplayDto): void {
    this.form.patchValue({
      codigo: item.codigo,
      nome: item.nome,
      descricao: item.descricao,
      // preco: item.preco, // Removed as preco is not part of the form
      categoriaId: item.categoriaId,
      unidadeMedidaId: item.unidadeMedidaId,
      embalagemId: item.embalagemId,
      atividadeAgropecuariaId: item.atividadeAgropecuariaId,
      ativo: item.ativo
    });

    // Set selected categoria for hierarchical selector
    if (item.categoria) {
      this.selectedCategoria.set({
        id: item.categoria.id,
        nome: item.categoria.nome,
        parentId: item.categoria.categoriaPaiId
      });
    }

    // Load embalagens for selected unidade de medida
    if (item.unidadeMedidaId) {
      this.carregarEmbalagens(item.unidadeMedidaId);
    }
  }

  /**
   * Load all reference data needed for the form
   */
  private carregarDadosReferencia(): void {
    this.carregarUnidadesMedida();
    this.carregarCategorias();
    this.carregarAtividadesAgropecuarias();
    this.carregarFornecedores();
    this.carregarCulturas();
    this.carregarProdutosPais();
  }

  /**
   * Load unidades de medida
   */
  private carregarUnidadesMedida(): void {
    this.loadingUnidades.set(true);
    
    this.service.obterUnidadesMedida().subscribe({
      next: (unidades) => {
        this.unidadesMedida.set(unidades);
        this.loadingUnidades.set(false);
      },
      error: (error) => {
        console.error('Erro ao carregar unidades de medida:', error);
        this.loadingUnidades.set(false);
      }
    });
  }

  /**
   * Load categorias for hierarchical selector
   */
  private carregarCategorias(): void {
    this.loadingCategorias.set(true);
    this.errorCategorias.set(null);
    
    this.service.obterCategorias().subscribe({
      next: (categorias) => {
        // Convert to hierarchical items
        const hierarchicalItems: HierarchicalItem[] = categorias.map(cat => ({
          id: cat.id,
          nome: cat.nome,
          parentId: cat.categoriaPaiId,
          ativo: cat.ativo
        }));
        
        this.categoriasHierarchicas.set(hierarchicalItems);
        this.loadingCategorias.set(false);
      },
      error: (error) => {
        console.error('Erro ao carregar categorias:', error);
        this.errorCategorias.set('Erro ao carregar categorias');
        this.loadingCategorias.set(false);
      }
    });
  }

  /**
   * Load atividades agropecuarias
   */
  private carregarAtividadesAgropecuarias(): void {
    this.loadingAtividades.set(true);
    
    this.service.obterAtividadesAgropecuarias().subscribe({
      next: (atividades) => {
        this.atividadesAgropecuarias.set(atividades);
        this.loadingAtividades.set(false);
      },
      error: (error) => {
        console.error('Erro ao carregar atividades agropecuárias:', error);
        this.loadingAtividades.set(false);
      }
    });
  }

  /**
   * Load embalagens filtered by unidade de medida
   */
  private carregarEmbalagens(unidadeMedidaId?: number): void {
    this.loadingEmbalagens.set(true);
    
    this.service.obterEmbalagensPorUnidade(unidadeMedidaId).subscribe({
      next: (embalagens) => {
        this.embalagensDisponiveis.set(embalagens);
        this.loadingEmbalagens.set(false);
      },
      error: (error) => {
        console.error('Erro ao carregar embalagens:', error);
        this.loadingEmbalagens.set(false);
      }
    });
  }

  /**
   * Load fornecedores
   */
  private carregarFornecedores(): void {
    this.loadingFornecedores.set(true);
    
    this.service.obterFornecedores().subscribe({
      next: (fornecedores) => {
        this.fornecedores.set(fornecedores);
        this.loadingFornecedores.set(false);
      },
      error: (error) => {
        console.error('Erro ao carregar fornecedores:', error);
        this.loadingFornecedores.set(false);
      }
    });
  }

  /**
   * Load culturas
   */
  private carregarCulturas(): void {
    this.loadingCulturas.set(true);
    
    this.service.obterCulturas().subscribe({
      next: (culturas) => {
        this.culturas.set(culturas);
        this.loadingCulturas.set(false);
      },
      error: (error) => {
        console.error('Erro ao carregar culturas:', error);
        this.loadingCulturas.set(false);
      }
    });
  }

  /**
   * Load produtos pais
   */
  private carregarProdutosPais(): void {
    this.loadingProdutosPais.set(true);
    
    this.service.obterProdutosPais().subscribe({
      next: (produtos) => {
        this.produtosPais.set(produtos);
        this.loadingProdutosPais.set(false);
      },
      error: (error) => {
        console.error('Erro ao carregar produtos pais:', error);
        this.loadingProdutosPais.set(false);
      }
    });
  }

  /**
   * Handle categoria selection change
   */
  onCategoriaChange(event: any): void {
    const selectedItems = event.selectedItems;
    
    if (selectedItems && selectedItems.length > 0) {
      const categoria = selectedItems[0];
      this.form.patchValue({ categoriaId: categoria.id });
      this.selectedCategoria.set(categoria);
    } else {
      this.form.patchValue({ categoriaId: null });
      this.selectedCategoria.set(null);
    }
  }

  /**
   * Handle unidade de medida change
   */
  onUnidadeMedidaChange(event: any): void {
    const unidadeMedidaId = event.value;
    
    // Clear embalagem selection
    this.form.patchValue({ embalagemId: null });
    
    // Load embalagens for selected unidade
    if (unidadeMedidaId) {
      this.carregarEmbalagens(unidadeMedidaId);
    } else {
      this.embalagensDisponiveis.set([]);
    }
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

  /**
   * Override carregarItens to use display DTOs and apply filters
   */
  override carregarItens(): void {
    this.loading.set(true);
    this.tableLoading.set(true);
    
    const filter = this.selectedStatusFilter();
    let request;

    if (filter === 'ativas') {
      request = this.service.obterAtivosParaExibicao();
    } else {
      request = this.service.obterTodosParaExibicao();
    }

    request.subscribe({
      next: (items) => {
        let filteredItems = items;
        
        if (filter === 'inativas') {
          filteredItems = items.filter(item => !item.ativo);
        }
        
        // Store all items in a private property for filtering
        this.todosItens = filteredItems;
        this.aplicarFiltros();
        this.carregarOpcoesParaFiltros(filteredItems);
        this.loading.set(false);
        this.tableLoading.set(false);
      },
      error: (error) => {
        console.error('Erro ao carregar produtos:', error);
        this.loading.set(false);
        this.tableLoading.set(false);
      }
    });
  }

  /**
   * Load filter options from loaded items
   */
  private carregarOpcoesParaFiltros(items: ProdutoDisplayDto[]): void {
    // Extract unique categorias
    const categorias = Array.from(
      new Map(
        items
          .filter(item => item.categoriaId && item.categoriaNome)
          .map(item => [item.categoriaId, { id: item.categoriaId, nome: item.categoriaNome }])
      ).values()
    );
    this.categoriasParaFiltro.set(categorias);

    // Extract unique unidades de medida
    const unidades = Array.from(
      new Map(
        items
          .filter(item => item.unidadeMedidaId && item.unidadeMedidaNome)
          .map(item => [item.unidadeMedidaId, { 
            id: item.unidadeMedidaId, 
            nome: `${item.unidadeMedidaSimbolo} - ${item.unidadeMedidaNome}` 
          }])
      ).values()
    );
    this.unidadesParaFiltro.set(unidades);

    // Extract unique atividades agropecuárias
    const atividades = Array.from(
      new Map(
        items
          .filter(item => item.atividadeAgropecuariaId && item.atividadeAgropecuariaNome)
          .map(item => [item.atividadeAgropecuariaId, { 
            id: item.atividadeAgropecuariaId!, 
            nome: item.atividadeAgropecuariaCodigo 
              ? `${item.atividadeAgropecuariaCodigo} - ${item.atividadeAgropecuariaNome}`
              : item.atividadeAgropecuariaNome!
          }])
      ).values()
    );
    this.atividadesParaFiltro.set(atividades);
  }

  /**
   * Apply all active filters
   */
  aplicarFiltros(): void {
    let itemsFiltrados = [...this.todosItens];

    // Apply categoria filter
    if (this.filtroCategoria) {
      itemsFiltrados = itemsFiltrados.filter(item => item.categoriaId === this.filtroCategoria);
    }

    // Apply unidade de medida filter
    if (this.filtroUnidadeMedida) {
      itemsFiltrados = itemsFiltrados.filter(item => item.unidadeMedidaId === this.filtroUnidadeMedida);
    }

    // Apply atividade agropecuária filter
    if (this.filtroAtividadeAgropecuaria) {
      itemsFiltrados = itemsFiltrados.filter(item => item.atividadeAgropecuariaId === this.filtroAtividadeAgropecuaria);
    }

    // Apply text search filter
    if (this.filtroTexto.trim()) {
      const searchTerm = this.filtroTexto.toLowerCase().trim();
      itemsFiltrados = itemsFiltrados.filter(item =>
        item.codigo.toLowerCase().includes(searchTerm) ||
        item.nome.toLowerCase().includes(searchTerm) ||
        item.categoriaNome.toLowerCase().includes(searchTerm) ||
        item.unidadeMedidaNome.toLowerCase().includes(searchTerm) ||
        (item.atividadeAgropecuariaNome && item.atividadeAgropecuariaNome.toLowerCase().includes(searchTerm))
      );
    }

    // Update the base component's items signal
    this.items.set(itemsFiltrados);
  }

  /**
   * Handle search input change with debounce
   */
  onBuscaChange(event: any): void {
    this.filtroTexto = event.target.value;
    
    // Clear previous timer
    if (this.searchTimer) {
      clearTimeout(this.searchTimer);
    }
    
    // Set new timer
    this.searchTimer = setTimeout(() => {
      this.aplicarFiltros();
    }, 300);
  }

  /**
   * Clear all filters
   */
  limparFiltros(): void {
    this.filtroCategoria = null;
    this.filtroUnidadeMedida = null;
    this.filtroAtividadeAgropecuaria = null;
    this.filtroTexto = '';
    this.aplicarFiltros();
  }

  /**
   * Check if any filters are active
   */
  temFiltrosAtivos(): boolean {
    return !!(
      this.filtroCategoria ||
      this.filtroUnidadeMedida ||
      this.filtroAtividadeAgropecuaria ||
      this.filtroTexto.trim()
    );
  }

  /**
   * Format price for display
   */
  formatarPreco(preco?: number): string {
    if (!preco || preco === 0) {
      return 'Não informado';
    }
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    }).format(preco);
  }

  /**
   * Get CSS class for price display
   */
  getPrecoClass(preco?: number): string {
    if (!preco || preco === 0) {
      return 'price-display price-zero';
    }
    return 'price-display';
  }
}
