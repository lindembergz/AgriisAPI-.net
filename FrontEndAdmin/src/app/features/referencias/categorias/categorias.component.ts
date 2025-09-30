import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule, AbstractControl, ValidationErrors, AsyncValidatorFn } from '@angular/forms';
import { Observable, of, timer } from 'rxjs';
import { map, switchMap, catchError } from 'rxjs/operators';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { SelectModule } from 'primeng/select';
import { CheckboxModule } from 'primeng/checkbox';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService } from 'primeng/api';
import { ConfirmationService } from 'primeng/api';

import { ReferenceCrudBaseComponent } from '../../../shared/components/reference-crud-base/reference-crud-base.component';
import { ResponsiveTableComponent } from '../../../shared/components/responsive-table/responsive-table.component';
import { FilterSummaryComponent } from '../../../shared/components/filter-summary/filter-summary.component';

import { 
  CategoriaDto, 
  CriarCategoriaDto, 
  AtualizarCategoriaDto, 
  CategoriaProduto 
} from '../../../shared/models/reference.model';
import { 
  CategoriaService, 
  CategoriaDropdownOption, 
  TipoOption
} from './services/categoria.service';
import { ValidationService } from '../../../shared/services/validation.service';
import { FieldErrorComponent } from '../../../shared/components/field-error/field-error.component';

import { 
  ComponentTemplate, 
  CustomFilter, 
  CustomAction, 
  TableColumn, 
  EmptyStateConfig, 
  LoadingStateConfig, 
  ResponsiveConfig,
  DialogConfig,
  DisplayMode
} from '../../../shared/interfaces/unified-component.interfaces';

@Component({
  selector: 'app-categorias',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
  DialogModule,
  ButtonModule,
  TableModule,
  InputTextModule,
    TextareaModule,
    SelectModule,
    CheckboxModule,
    ToastModule,
    ConfirmDialogModule,
    ProgressSpinnerModule,
    TagModule,
    TooltipModule
    ,
    FieldErrorComponent
    ,
    ResponsiveTableComponent,
    FilterSummaryComponent
  ],
  providers: [MessageService, ConfirmationService, CategoriaService],
  templateUrl: './categorias.component.html',
  styleUrls: ['./categorias.component.scss']
})
export class CategoriasComponent extends ReferenceCrudBaseComponent<
  CategoriaDto,
  CriarCategoriaDto,
  AtualizarCategoriaDto
> implements OnInit {
  
  protected service = inject(CategoriaService);
  protected validationService = inject(ValidationService);

  // (flat) Original data for display
  
  // Dropdown options
  categoriasParaDropdown = signal<CategoriaDropdownOption[]>([]);
  tiposDisponiveis: TipoOption[] = [];

  // Table columns configuration
  treeColumns = signal<TableColumn[]>([]);

  // Component configuration
  protected entityDisplayName = () => 'Categoria';
  protected entityDescription = () => 'Gerencie as categorias de produtos de forma hierárquica';
  protected defaultSortField = () => 'nome';
  protected searchFields = () => ['nome', 'descricao', 'categoriaPaiNome'];

  // Filter signals for tree-specific functionality
  searchText = signal<string>('');
  selectedTipoFilter = signal<CategoriaProduto | null>(null);
  
  // Filter options
  tipoFilterOptions: Array<{label: string, value: CategoriaProduto | null}> = [];
  
  // Original data for filtering
  originalCategorias = signal<CategoriaDto[]>([]);

  ngOnInit(): void {
    try {
      super.ngOnInit();
      this.inicializarDados();
      this.inicializarColunas();
    } catch (error) {
      console.error('Error initializing CategoriasComponent:', error);
      // Fallback initialization
      this.inicializarDados();
      this.inicializarColunas();
      this.carregarItens();
    }
  }

  protected createFormGroup(): FormGroup {
    const form = this.fb.group({
      nome: ['', 
        [Validators.required, Validators.minLength(2), Validators.maxLength(100)],
        [this.createNomeUniqueValidator()]
      ],
      descricao: ['', [Validators.maxLength(500)]],
      tipo: [null, [Validators.required]],
      categoriaPaiId: [null, [], [this.createCircularReferenceValidator()]],
      ordem: [0, [Validators.required, Validators.min(0), Validators.pattern(/^\d+$/)]],
      ativo: [true]
    });

    // Add cross-field validation for categoria pai
    form.get('categoriaPaiId')?.addValidators([this.createCategoriaPaiValidator()]);

    return form;
  }

  protected mapToCreateDto(formValue: any): CriarCategoriaDto {
    return {
      nome: formValue.nome,
      descricao: formValue.descricao || undefined,
      tipo: formValue.tipo,
      categoriaPaiId: formValue.categoriaPaiId || undefined,
      ordem: formValue.ordem || 0
    };
  }

  protected mapToUpdateDto(formValue: any): AtualizarCategoriaDto {
    return {
      nome: formValue.nome,
      descricao: formValue.descricao || undefined,
      tipo: formValue.tipo,
      categoriaPaiId: formValue.categoriaPaiId || undefined,
      ordem: formValue.ordem || 0,
      ativo: formValue.ativo
    };
  }

  private inicializarDados(): void {
    this.carregarTiposDisponiveis();
    this.carregarCategoriasParaDropdown();
    this.inicializarFiltroTipo();
  }

  private inicializarFiltroTipo(): void {
    this.tipoFilterOptions = [
      { label: 'Todos os tipos', value: null },
      ...this.service.obterTiposDisponiveis().map(tipo => ({
        label: tipo.label,
        value: tipo.value
      }))
    ];
  }

  displayColumns: () => TableColumn[] = () => [
    {
      field: 'id',
      header: 'Código',
      sortable: true,
      width: '10%',
      type: 'text',
      hideOnMobile: true
    },
    { 
      field: 'nome', 
      header: 'Nome', 
      sortable: true,
      width: '40%',
      type: 'text'
    },
    {
      field: 'descricao',
      header: 'Descrição',
      sortable: false,
      width: '35%',
      type: 'text',
      hideOnMobile: true
    }/*,
    { 
      field: 'tipo', 
      header: 'Tipo', 
      sortable: true, 
      width: '20%',
      type: 'custom',
      hideOnMobile: true 
    },
    { 
      field: 'ativo', 
      header: 'Status', 
      sortable: true, 
      width: '15%',
      type: 'boolean'
    },
    { 
      field: 'ordem', 
      header: 'Ordem', 
      sortable: true, 
      width: '10%',
      type: 'text',
      hideOnMobile: true 
    }
    { 
      field: 'acoes', 
      header: 'Ações', 
      sortable: false, 
      width: '15%',
      type: 'custom'
    }*/
  ];

  private inicializarColunas(): void {
    this.treeColumns.set(this.displayColumns());
  }

  private carregarTiposDisponiveis(): void {
    this.tiposDisponiveis = this.service.obterTiposDisponiveis();
  }

  private carregarCategoriasParaDropdown(): void {
    this.service.obterParaDropdown().subscribe({
      next: (categorias) => {
        this.categoriasParaDropdown.set(categorias);
      },
      error: (error) => {
        console.error('Erro ao carregar categorias para dropdown:', error);
      }
    });
  }

  carregarItens(): void {
    console.debug('[CategoriasComponent] carregarItens invoked');
    this.loading.set(true);
    this.tableLoading.set(true);

    this.service.obterTodos().subscribe({
      next: (items: CategoriaDto[]) => {
        console.log(items)
        console.debug('[CategoriasComponent] obterTodos next, items length =', items?.length ?? 0);
  this.originalCategorias.set(items);
  this.aplicarFiltros();
        this.loading.set(false);
        this.tableLoading.set(false);
        if (this.items().length === 0 && items.length > 0) {
          this.showInfoMessage(`${items.length} categorias carregadas com sucesso.`, 'Dados carregados');
        }
      },
      error: (error: any) => {
        console.debug('[CategoriasComponent] obterTodos error', error);
        this.handleCategoriaError('carregar categorias', error);
  this.loading.set(false);
  this.tableLoading.set(false);
  this.originalCategorias.set([]);
  this.items.set([]);
      }
    });
  }

  private aplicarFiltros(): void {
    let filteredItems = [...this.originalCategorias()];

    const statusFilter = this.selectedStatusFilter();
    if (statusFilter === 'ativas') {
      filteredItems = filteredItems.filter(item => item.ativo);
    } else if (statusFilter === 'inativas') {
      filteredItems = filteredItems.filter(item => !item.ativo);
    }

    const tipoFilter = this.selectedTipoFilter();
    if (tipoFilter !== null) {
      filteredItems = filteredItems.filter(item => item.tipo === tipoFilter);
    }

    const searchText = this.searchText().toLowerCase().trim();
    if (searchText) {
      filteredItems = this.aplicarFiltroTexto(filteredItems, searchText);
    }

    // Use a flat list - no hierarchy transformation
    this.items.set(filteredItems);
    this.filteredItems.set(filteredItems);

    // Debug: print the shapes so we can see what's being displayed
    try {
      console.debug('[CategoriasComponent] after aplicarFiltros — original length =', this.originalCategorias().length);
      console.debug('[CategoriasComponent] after aplicarFiltros — filtered length =', this.items().length, this.items());
    } catch (e) {
      console.debug('[CategoriasComponent] error logging filter shapes', e);
    }
  }

  private aplicarFiltroTexto(categorias: CategoriaDto[], searchText: string): CategoriaDto[] {
    const matchingCategories = new Set<number>();
    const allCategories = new Map<number, CategoriaDto>();
    
    this.originalCategorias().forEach(cat => {
      allCategories.set(cat.id, cat);
    });
    
    categorias.forEach(categoria => {
      if (categoria.nome.toLowerCase().includes(searchText) ||
          (categoria.descricao && categoria.descricao.toLowerCase().includes(searchText))) {
        matchingCategories.add(categoria.id);
        this.addParentsToSet(categoria, allCategories, matchingCategories);
        this.addChildrenToSet(categoria, allCategories, matchingCategories);
      }
    });
    
    return categorias.filter(cat => matchingCategories.has(cat.id));
  }

  private addParentsToSet(categoria: CategoriaDto, allCategories: Map<number, CategoriaDto>, matchingSet: Set<number>): void {
    if (categoria.categoriaPaiId) {
      const parent = allCategories.get(categoria.categoriaPaiId);
      if (parent && !matchingSet.has(parent.id)) {
        matchingSet.add(parent.id);
        this.addParentsToSet(parent, allCategories, matchingSet);
      }
    }
  }

  private addChildrenToSet(categoria: CategoriaDto, allCategories: Map<number, CategoriaDto>, matchingSet: Set<number>): void {
    allCategories.forEach(cat => {
      if (cat.categoriaPaiId === categoria.id && !matchingSet.has(cat.id)) {
        matchingSet.add(cat.id);
        this.addChildrenToSet(cat, allCategories, matchingSet);
      }
    });
  }

  obterLabelTipo(tipo: CategoriaProduto): string {
    return this.service.obterLabelTipo(tipo);
  }

  onSearchTextChange(event: any): void {
    const searchText = event.target?.value || '';
    this.searchText.set(searchText);
    this.aplicarFiltros();
  }

  onTipoFilterChange(event: any): void {
    this.selectedTipoFilter.set(event.value);
    this.aplicarFiltros();
  }

  onStatusFilterChange(event: any): void {
    this.selectedStatusFilter.set(event.value);
    this.aplicarFiltros();
  }

  limparFiltros(): void {
    this.searchText.set('');
    this.selectedTipoFilter.set(null);
    this.selectedStatusFilter.set('todas');
    this.aplicarFiltros();
  }

  temFiltrosAtivos(): boolean {
    return this.searchText() !== '' || 
           this.selectedTipoFilter() !== null || 
           this.selectedStatusFilter() !== 'todas';
  }



  getVisibleColumns(): TableColumn[] {
    const allColumns = this.treeColumns();
    const isMobile = this.isMobile();
    const isTablet = this.isTablet();

    return allColumns.filter(col => {
      if (isMobile && col.hideOnMobile) return false;
      if (isTablet && col.hideOnTablet) return false;
      return true;
    });
  }

  isMobile(): boolean {
    return window.innerWidth < 768;
  }

  isTablet(): boolean {
    return window.innerWidth >= 768 && window.innerWidth <= 1024;
  }

  onNodeExpand(event: any): void {}

  onNodeCollapse(event: any): void {}

  onTreeTableLoad(): void {}

  getColumnWidth(field: string): string {
    const column = this.treeColumns().find(col => col.field === field);
    return column?.width || 'auto';
  }

  shouldHideColumn(column: TableColumn): boolean {
    if (this.isMobile() && column.hideOnMobile) return true;
    if (this.isTablet() && column.hideOnTablet) return true;
    return false;
  }

  private createNomeUniqueValidator(): AsyncValidatorFn {
    return (control: AbstractControl): Observable<ValidationErrors | null> => {
      if (!control.value || control.value.length < 2) {
        return of(null);
      }

      const excludeId = this.isEditMode() ? this.selectedItem()?.id : undefined;

      return timer(300).pipe(
        switchMap(() => this.service.existeComNome(control.value, excludeId)),
        map(exists => exists ? { nomeJaExiste: { value: control.value } } : null),
        catchError(() => of(null))
      );
    };
  }

  private createCircularReferenceValidator(): AsyncValidatorFn {
    return (control: AbstractControl): Observable<ValidationErrors | null> => {
      if (!control.value || !this.isEditMode()) {
        return of(null);
      }

      const categoriaId = this.selectedItem()?.id;
      if (!categoriaId) {
        return of(null);
      }

      return timer(300).pipe(
        switchMap(() => this.service.validarReferenciaCircular(categoriaId, control.value)),
        map(isValid => isValid ? null : { referenciaCircular: { categoriaId, categoriaPaiId: control.value } }),
        catchError(() => of(null))
      );
    };
  }

  private createCategoriaPaiValidator() {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) {
        return null;
      }

      const categoriasDisponiveis = this.categoriasParaDropdown();
      const categoriaEncontrada = categoriasDisponiveis.find(cat => cat.id === control.value);

      if (!categoriaEncontrada) {
        return { categoriaPaiInvalida: { value: control.value } };
      }

      if (!categoriaEncontrada.ativo) {
        return { categoriaPaiInativa: { value: control.value } };
      }

      if (this.isEditMode()) {
        const categoriaAtual = this.selectedItem();
        if (categoriaAtual && categoriaAtual.id === control.value) {
          return { categoriaPaiPropriaCategoria: { value: control.value } };
        }
      }

      return null;
    };
  }

  getErrorMessage(fieldName: string): string {
    const control = this.form.get(fieldName);
    if (!control || !control.errors) {
      return '';
    }

    const errors = control.errors;

    if (errors['nomeJaExiste']) {
      return `O nome "${errors['nomeJaExiste'].value}" já está sendo usado por outra categoria`;
    }

    if (errors['referenciaCircular']) {
      return 'Esta seleção criaria uma referência circular na hierarquia';
    }

    if (errors['categoriaPaiInvalida']) {
      return 'Categoria pai selecionada não é válida';
    }

    if (errors['categoriaPaiInativa']) {
      return 'A categoria pai selecionada está inativa';
    }

    if (errors['categoriaPaiPropriaCategoria']) {
      return 'Uma categoria não pode ser pai de si mesma';
    }

    switch (fieldName) {
      case 'nome':
        if (errors['required']) return 'Nome é obrigatório';
        if (errors['minlength']) return 'Nome deve ter pelo menos 2 caracteres';
        if (errors['maxlength']) return 'Nome deve ter no máximo 100 caracteres';
        break;

      case 'descricao':
        if (errors['maxlength']) return 'Descrição deve ter no máximo 500 caracteres';
        break;

      case 'tipo':
        if (errors['required']) return 'Tipo é obrigatório';
        break;

      case 'ordem':
        if (errors['required']) return 'Ordem é obrigatória';
        if (errors['min']) return 'Ordem deve ser um número positivo';
        if (errors['pattern']) return 'Ordem deve ser um número inteiro';
        break;
    }

    return 'Campo inválido';
  }

  protected populateForm(item: CategoriaDto): void {
    this.form.patchValue({
      nome: item.nome,
      descricao: item.descricao,
      tipo: item.tipo,
      categoriaPaiId: item.categoriaPaiId,
      ordem: item.ordem,
      ativo: item.ativo
    });

    const nomeControl = this.form.get('nome');
    if (nomeControl) {
      nomeControl.clearAsyncValidators();
      nomeControl.setAsyncValidators([this.createNomeUniqueValidator()]);
      nomeControl.updateValueAndValidity();
    }

    const categoriaPaiControl = this.form.get('categoriaPaiId');
    if (categoriaPaiControl) {
      categoriaPaiControl.clearAsyncValidators();
      categoriaPaiControl.setAsyncValidators([this.createCircularReferenceValidator()]);
      categoriaPaiControl.updateValueAndValidity();
    }
  }

  private resetFormForCreate(): void {
    this.form.reset();
    
    this.form.patchValue({
      ordem: 0,
      ativo: true
    });

    const nomeControl = this.form.get('nome');
    if (nomeControl) {
      nomeControl.clearAsyncValidators();
      nomeControl.setAsyncValidators([this.createNomeUniqueValidator()]);
    }

    const categoriaPaiControl = this.form.get('categoriaPaiId');
    if (categoriaPaiControl) {
      categoriaPaiControl.clearAsyncValidators();
      categoriaPaiControl.setAsyncValidators([this.createCircularReferenceValidator()]);
    }
  }

  getCategoriasParaDropdownFiltradas(): CategoriaDropdownOption[] {
    const todasCategorias = this.categoriasParaDropdown();
    
    if (!this.isEditMode() || !this.selectedItem()) {
      return todasCategorias.filter(cat => cat.ativo);
    }

    const categoriaAtual = this.selectedItem()!;
    const categoriasExcluidas = this.getDescendantIds(categoriaAtual.id);
    categoriasExcluidas.add(categoriaAtual.id);

    return todasCategorias.filter(cat => 
      cat.ativo && !categoriasExcluidas.has(cat.id)
    );
  }

  private getDescendantIds(categoriaId: number): Set<number> {
    const descendants = new Set<number>();
    const allCategories = this.items();
    
    const findDescendants = (parentId: number) => {
      const children = allCategories.filter(cat => cat.categoriaPaiId === parentId);
      children.forEach(child => {
        descendants.add(child.id);
        findDescendants(child.id);
      });
    };

    findDescendants(categoriaId);
    return descendants;
  }

  private markFormGroupTouched(): void {
    Object.keys(this.form.controls).forEach(key => {
      const control = this.form.get(key);
      if (control) {
        control.markAsTouched();
        control.updateValueAndValidity();
      }
    });
  }

  shouldShowError(fieldName: string): boolean {
    const control = this.form.get(fieldName);
    return !!(control && control.invalid && (control.dirty || control.touched));
  }

  getFieldValidationClass(fieldName: string): string {
    const control = this.form.get(fieldName);
    if (!control) return '';
    
    if (control.pending) return 'p-pending';
    if (control.invalid && (control.dirty || control.touched)) return 'p-invalid';
    if (control.valid && control.dirty) return 'p-valid';
    
    return '';
  }

  isSaveDisabled(): boolean {
    return this.form.invalid || this.form.pending || this.formLoading();
  }

  novoItem(): void {
    this.carregarCategoriasParaDropdown();
    this.selectedItem.set(null);
    this.currentRowVersion.set(null);
    this.resetFormForCreate();
    this.showForm.set(true);
  }

  editarItem(item: CategoriaDto): void {
    this.setCustomActionLoading('edit', item.id, true);
    this.carregarCategoriasParaDropdown();
    this.service.obterPorId(item.id).subscribe({
      next: (freshItem) => {
        this.selectedItem.set(freshItem);
        this.currentRowVersion.set((freshItem as any).rowVersion);
        this.populateForm(freshItem);
        this.showForm.set(true);
        this.setCustomActionLoading('edit', item.id, false);
      },
      error: (error) => {
        console.error('Erro ao carregar categoria para edição:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Erro',
          detail: 'Erro ao carregar dados da categoria para edição',
          life: 5000
        });
        this.setCustomActionLoading('edit', item.id, false);
      }
    });
  }

  salvarItem(): void {
    if (this.form.invalid) {
      this.markFormGroupTouched();
      this.messageService.add({
        severity: 'warn',
        summary: 'Formulário Inválido',
        detail: 'Por favor, corrija os erros no formulário antes de salvar',
        life: 5000
      });
      return;
    }

    if (this.form.pending) {
      setTimeout(() => this.salvarItem(), 100);
      return;
    }

    this.formLoading.set(true);
    const formValue = this.form.value;
    
    if (this.isEditMode()) {
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
            detail: 'Categoria atualizada com sucesso',
            life: 3000
          });
          this.carregarItens();
          this.carregarCategoriasParaDropdown();
        },
        error: (error) => {
          this.formLoading.set(false);
          if (error.originalError?.status === 412) {
            this.messageService.add({
              severity: 'warn',
              summary: 'Dados Desatualizados',
              detail: 'Os dados foram alterados por outro usuário. Recarregando...', 
              life: 5000
            });
            this.editarItem(this.selectedItem()!);
          } else if (error.originalError?.status === 400) {
            this.messageService.add({
              severity: 'error',
              summary: 'Erro de Validação',
              detail: error.message || 'Dados inválidos para atualização',
              life: 5000
            });
          } else {
            this.messageService.add({
              severity: 'error',
              summary: 'Erro',
              detail: 'Erro ao atualizar categoria',
              life: 5000
            });
          }
        }
      });
    } else {
      const createDto = this.mapToCreateDto(formValue);
      
      this.service.criar(createDto).subscribe({
        next: (newItem) => {
          this.formLoading.set(false);
          this.showForm.set(false);
          this.messageService.add({
            severity: 'success',
            summary: 'Sucesso',
            detail: 'Categoria criada com sucesso',
            life: 3000
          });
          this.carregarItens();
          this.carregarCategoriasParaDropdown();
        },
        error: (error) => {
          this.formLoading.set(false);
          if (error.originalError?.status === 400) {
            this.messageService.add({
              severity: 'error',
              summary: 'Erro de Validação',
              detail: error.message || 'Dados inválidos para criação',
              life: 5000
            });
          } else if (error.originalError?.status === 409) {
            this.messageService.add({
              severity: 'error',
              summary: 'Conflito',
              detail: 'Já existe uma categoria com este nome',
              life: 5000
            });
          } else {
            this.messageService.add({
              severity: 'error',
              summary: 'Erro',
              detail: 'Erro ao criar categoria',
              life: 5000
            });
          }
        }
      });
    }
  }

  async ativarItem(item: CategoriaDto): Promise<void> {
    this.setCustomActionLoading('activate', item.id, true);
    
    this.service.ativar(item.id).subscribe({
      next: () => {
        this.setCustomActionLoading('activate', item.id, false);
        this.messageService.add({
          severity: 'success',
          summary: 'Sucesso',
          detail: 'Categoria ativada com sucesso',
          life: 3000
        });
        this.carregarItens();
        this.carregarCategoriasParaDropdown();
      },
      error: (error) => {
        this.setCustomActionLoading('activate', item.id, false);
        this.messageService.add({
          severity: 'error',
          summary: 'Erro',
          detail: 'Erro ao ativar categoria',
          life: 5000
        });
      }
    });
  }

  async desativarItem(item: CategoriaDto): Promise<void> {
    this.confirmationService.confirm({
      message: `Tem certeza que deseja desativar a categoria "${item.nome}"?`,
      header: 'Confirmar Desativação',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Sim, Desativar',
      rejectLabel: 'Cancelar',
      accept: () => {
        this.executarDesativacao(item);
      }
    });
  }

  private executarDesativacao(item: CategoriaDto): void {
    this.setCustomActionLoading('deactivate', item.id, true);
    
    this.service.desativar(item.id).subscribe({
      next: () => {
        this.setCustomActionLoading('deactivate', item.id, false);
        this.messageService.add({
          severity: 'success',
          summary: 'Sucesso',
          detail: 'Categoria desativada com sucesso',
          life: 3000
        });
        this.carregarItens();
        this.carregarCategoriasParaDropdown();
      },
      error: (error) => {
        this.setCustomActionLoading('deactivate', item.id, false);
        this.messageService.add({
          severity: 'error',
          summary: 'Erro',
          detail: 'Erro ao desativar categoria',
          life: 5000
        });
      }
    });
  }

  async excluirItem(item: CategoriaDto): Promise<void> {
    this.setCustomActionLoading('delete', item.id, true);
    
    this.service.podeRemover(item.id).subscribe({
      next: (canRemove) => {
        this.setCustomActionLoading('delete', item.id, false);
        
        if (!canRemove) {
          this.messageService.add({
            severity: 'warn',
            summary: 'Não é possível excluir',
            detail: `A categoria "${item.nome}" não pode ser excluída pois possui produtos associados ou subcategorias`,
            life: 7000
          });
          return;
        }

        this.confirmationService.confirm({
          message: `Tem certeza que deseja excluir permanentemente a categoria "${item.nome}"?`,
          header: 'Confirmar Exclusão',
          icon: 'pi pi-exclamation-triangle',
          acceptLabel: 'Sim, Excluir',
          rejectLabel: 'Cancelar',
          acceptButtonStyleClass: 'p-button-danger',
          accept: () => {
            this.executarExclusao(item);
          }
        });
      },
      error: (error) => {
        this.setCustomActionLoading('delete', item.id, false);
        console.error('Erro ao verificar se categoria pode ser removida:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Erro',
          detail: 'Erro ao verificar se a categoria pode ser excluída',
          life: 5000
        });
      }
    });
  }

  private executarExclusao(item: CategoriaDto): void {
    this.setCustomActionLoading('delete', item.id, true);
    
    this.service.remover(item.id).subscribe({
      next: () => {
        this.setCustomActionLoading('delete', item.id, false);
        this.messageService.add({
          severity: 'success',
          summary: 'Sucesso',
          detail: 'Categoria excluída com sucesso',
          life: 3000
        });
        this.carregarItens();
        this.carregarCategoriasParaDropdown();
      },
      error: (error) => {
        this.setCustomActionLoading('delete', item.id, false);
        if (error.originalError?.status === 409) {
          this.messageService.add({
            severity: 'error',
            summary: 'Não é possível excluir',
            detail: 'A categoria possui dependências e não pode ser excluída',
            life: 7000
          });
        } else {
          this.messageService.add({
            severity: 'error',
            summary: 'Erro',
            detail: 'Erro ao excluir categoria',
            life: 5000
          });
        }
      }
    });
  }

  private setCustomActionLoading(action: string, id: number, loading: boolean): void {
    const key = `${action}-${id}`;
    const currentStates = new Map(this.actionLoadingStates());
    
    if (loading) {
      currentStates.set(key, id);
    } else {
      currentStates.delete(key);
    }
    
    this.actionLoadingStates.set(currentStates);
  }

  isActionLoading(action: string, id: number): boolean {
    const key = `${action}-${id}`;
    return this.actionLoadingStates().has(key);
  }

  hasAnyActionLoading(): boolean {
    return this.actionLoadingStates().size > 0;
  }

  getStatusLabel(ativo: boolean): string {
    return ativo ? 'Ativo' : 'Inativo';
  }

  getStatusSeverity(ativo: boolean): string {
    return ativo ? 'success' : 'danger';
  }

  cancelarEdicao(): void {
    this.showForm.set(false);
    this.selectedItem.set(null);
    this.currentRowVersion.set(null);
    this.form.reset();
  }

  dialogTitle(): string {
    return this.isEditMode() ? 'Editar Categoria' : 'Nova Categoria';
  }

  isEditMode(): boolean {
    return this.selectedItem() !== null;
  }

  private handleCategoriaError(operation: string, error: any): void {
    console.error(`CategoriasComponent: ${operation} failed:`, error);
    
    let errorMessage = 'Ocorreu um erro inesperado';
    let severity: 'error' | 'warn' | 'info' = 'error';
    let summary = 'Erro';
    
    if (!navigator.onLine) {
      errorMessage = 'Sem conexão com a internet. Verifique sua conexão e tente novamente.';
      severity = 'warn';
      summary = 'Conexão';
    } else if (error.status === 0) {
      errorMessage = 'Não foi possível conectar ao servidor. Tente novamente em alguns instantes.';
      severity = 'warn';
      summary = 'Conexão';
    } else if (error.error && typeof error.error === 'object') {
      const apiError = error.error;
      
      if (apiError.errorCode) {
        switch (apiError.errorCode) {
          case 'CATEGORIA_NOT_FOUND':
            errorMessage = 'Categoria não encontrada. Ela pode ter sido removida por outro usuário.';
            severity = 'warn';
            summary = 'Categoria não encontrada';
            break;
          case 'CATEGORIA_NAME_DUPLICATE':
            errorMessage = 'Já existe uma categoria com este nome no mesmo nível hierárquico.';
            severity = 'warn';
            summary = 'Nome duplicado';
            break;
          case 'CIRCULAR_REFERENCE':
            errorMessage = 'Não é possível definir esta categoria como pai pois criaria uma referência circular.';
            severity = 'warn';
            summary = 'Referência circular';
            break;
          case 'CATEGORIA_HAS_PRODUCTS':
            errorMessage = 'Não é possível excluir esta categoria pois ela possui produtos associados. Desative-a ao invés de excluir.';
            severity = 'warn';
            summary = 'Categoria em uso';
            break;
          case 'CATEGORIA_HAS_SUBCATEGORIES':
            errorMessage = 'Não é possível excluir esta categoria pois ela possui subcategorias. Remova as subcategorias primeiro.';
            severity = 'warn';
            summary = 'Possui subcategorias';
            break;
          case 'PARENT_CATEGORIA_INACTIVE':
            errorMessage = 'A categoria pai selecionada está inativa. Selecione uma categoria pai ativa.';
            severity = 'warn';
            summary = 'Categoria pai inativa';
            break;
          case 'PARENT_CATEGORIA_NOT_FOUND':
            errorMessage = 'A categoria pai selecionada não foi encontrada. Ela pode ter sido removida.';
            severity = 'warn';
            summary = 'Categoria pai não encontrada';
            break;
          case 'INVALID_CATEGORIA_TYPE':
            errorMessage = 'Tipo de categoria inválido. Selecione um tipo válido.';
            severity = 'warn';
            summary = 'Tipo inválido';
            break;
          case 'CONCURRENCY_CONFLICT':
            errorMessage = 'Esta categoria foi modificada por outro usuário. A página será recarregada com os dados atuais.';
            severity = 'warn';
            summary = 'Conflito de concorrência';
            setTimeout(() => this.carregarItens(), 2000);
            break;
          case 'VALIDATION_ERROR':
            if (apiError.validationErrors) {
              const validationMessages = Object.values(apiError.validationErrors).flat();
              errorMessage = validationMessages.join(', ');
            } else {
              errorMessage = 'Os dados fornecidos são inválidos. Verifique os campos e tente novamente.';
            }
            severity = 'warn';
            summary = 'Dados inválidos';
            break;
          case 'BUSINESS_RULE_VIOLATION':
            errorMessage = apiError.errorDescription || 'Operação violou uma regra de negócio.';
            severity = 'warn';
            summary = 'Regra de negócio';
            break;
          default:
            errorMessage = apiError.errorDescription || errorMessage;
        }
      } else if (apiError.errorDescription) {
        errorMessage = apiError.errorDescription;
      }
    } else if (error.status) {
      switch (error.status) {
        case 400:
          errorMessage = 'Os dados fornecidos são inválidos. Verifique os campos obrigatórios.';
          severity = 'warn';
          summary = 'Dados inválidos';
          break;
        case 401:
          errorMessage = 'Sua sessão expirou. Você será redirecionado para o login.';
          severity = 'warn';
          summary = 'Sessão expirada';
          setTimeout(() => {
            window.location.href = '/login';
          }, 3000);
          break;
        case 403:
          errorMessage = 'Você não tem permissão para realizar esta operação.';
          severity = 'warn';
          summary = 'Acesso negado';
          break;
        case 404:
          errorMessage = 'Categoria não encontrada. Ela pode ter sido removida.';
          severity = 'warn';
          summary = 'Não encontrada';
          break;
        case 409:
          errorMessage = 'Já existe uma categoria com estes dados. Verifique o nome e tente novamente.';
          severity = 'warn';
          summary = 'Conflito de dados';
          break;
        case 412:
          errorMessage = 'Esta categoria foi modificada por outro usuário. A página será recarregada.';
          severity = 'warn';
          summary = 'Dados desatualizados';
          setTimeout(() => this.carregarItens(), 2000);
          break;
        case 422:
          errorMessage = 'Os dados fornecidos não atendem aos requisitos. Verifique os campos.';
          severity = 'warn';
          summary = 'Dados inválidos';
          break;
        case 500:
          errorMessage = 'Erro interno do servidor. Nossa equipe foi notificada.';
          break;
        case 502:
        case 503:
        case 504:
          errorMessage = 'Serviço temporariamente indisponível. Tente novamente em alguns instantes.';
          severity = 'warn';
          summary = 'Serviço indisponível';
          break;
        default:
          if (error.status >= 500) {
            errorMessage = 'Erro no servidor. Nossa equipe foi notificada.';
          } else {
            errorMessage = `Erro ${error.status}: ${error.statusText || 'Erro desconhecido'}`;
          }
      }
    }

    this.messageService.add({
      severity,
      summary,
      detail: errorMessage,
      life: severity === 'error' ? 8000 : 6000,
      sticky: severity === 'error' && error.status >= 500
    });
  }

  private showSuccessMessage(operation: string, categoria?: CategoriaDto): void {
    let message = '';
    let summary = 'Sucesso';
    
    switch (operation) {
      case 'create':
        message = `Categoria "${categoria?.nome}" criada com sucesso.`;
        summary = 'Categoria criada';
        break;
      case 'update':
        message = `Categoria "${categoria?.nome}" atualizada com sucesso.`;
        summary = 'Categoria atualizada';
        break;
      case 'activate':
        message = `Categoria "${categoria?.nome}" ativada com sucesso.`;
        summary = 'Categoria ativada';
        break;
      case 'deactivate':
        message = `Categoria "${categoria?.nome}" desativada com sucesso.`;
        summary = 'Categoria desativada';
        break;
      case 'delete':
        message = `Categoria "${categoria?.nome}" excluída com sucesso.`;
        summary = 'Categoria excluída';
        break;
      case 'load':
        message = 'Categorias carregadas com sucesso.';
        summary = 'Dados carregados';
        break;
      default:
        message = 'Operação realizada com sucesso.';
    }

    this.messageService.add({
      severity: 'success',
      summary,
      detail: message,
      life: 4000
    });
  }

  private showInfoMessage(message: string, summary: string = 'Informação'): void {
    this.messageService.add({
      severity: 'info',
      summary,
      detail: message,
      life: 5000
    });
  }

  private showWarningMessage(message: string, summary: string = 'Atenção'): void {
    this.messageService.add({
      severity: 'warn',
      summary,
      detail: message,
      life: 6000
    });
  }

  private validateFormBeforeSubmit(): boolean {
    if (this.form.valid) {
      return true;
    }

    this.validationService.markFormGroupTouched(this.form);

    const validationSummary = this.validationService.getFormValidationSummary(this.form);
    
    if (!validationSummary.isValid) {
      const errorCount = validationSummary.errors.length;
      const errorMessage = errorCount === 1 
        ? 'Há 1 erro no formulário que precisa ser corrigido.'
        : `Há ${errorCount} erros no formulário que precisam ser corrigidos.`;
      
      this.showWarningMessage(errorMessage, 'Formulário inválido');
      
      this.focusFirstInvalidField();
      
      return false;
    }

    return true;
  }

  private focusFirstInvalidField(): void {
    const firstInvalidField = document.querySelector('.ng-invalid:not(form)') as HTMLElement;
    if (firstInvalidField) {
      firstInvalidField.focus();
      firstInvalidField.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }
  }

  private handleBusinessRuleErrors(errors: string[]): void {
    if (errors.length === 0) return;

    const errorMessage = errors.length === 1
      ? errors[0]
      : `Múltiplas regras de negócio foram violadas:\n• ${errors.join('\n• ')}`;

    this.showWarningMessage(errorMessage, 'Regras de negócio');
  }
}
