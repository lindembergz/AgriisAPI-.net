import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, FormsModule, Validators } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { DialogModule } from 'primeng/dialog';
import { CheckboxModule } from 'primeng/checkbox';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ReferenceCrudBaseComponent } from '../../../shared/components/reference-crud-base/reference-crud-base.component';
import { MunicipioDto, CriarMunicipioDto, AtualizarMunicipioDto, UfDto, PaisDto } from '../../../shared/models/reference.model';
import { MunicipioService } from './services/municipio.service';
import { UfService } from '../ufs/services/uf.service';
import { PaisService } from '../paises/services/pais.service';
import { map } from 'rxjs/operators';
import { of } from 'rxjs';

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
 * Component for managing Municípios with UF cascading selection
 * Implements search functionality by name and UF filtering
 */
@Component({
  selector: 'app-municipios',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    InputTextModule,
    SelectModule,
    TagModule,
    TooltipModule,
    TableModule,
    ButtonModule,
    ConfirmDialogModule,
    ToastModule,
    DialogModule,
    CheckboxModule
  ],
  providers: [ConfirmationService, MessageService],
  templateUrl: './municipios.component.html',
  styleUrls: ['./municipios.component.scss']
})
export class MunicipiosComponent implements OnInit {
  
  private service = inject(MunicipioService);
  private ufService = inject(UfService);
  private paisService = inject(PaisService);
  private confirmationService = inject(ConfirmationService);
  private messageService = inject(MessageService);
  private fb = inject(FormBuilder);

  // Signals for reactive data
  paisesOptions = signal<PaisDto[]>([]);
  ufsOptions = signal<UfDto[]>([]);
  loadingPaises = signal<boolean>(false);
  loadingUfs = signal<boolean>(false);
  
  // Filter signals
  selectedPaisFilter = signal<number | null>(null);
  selectedUfFilter = signal<number | null>(null);
  searchTerm = signal<string>('');

  // Base component signals
  items = signal<MunicipioDto[]>([]);
  loading = signal<boolean>(false);
  tableLoading = signal<boolean>(false);
  formLoading = signal<boolean>(false);
  showForm = signal<boolean>(false);
  selectedItem = signal<MunicipioDto | null>(null);
  selectedStatusFilter = signal<string>('todas');
  pageSize = signal<number>(10);

  // Filter options
  statusFilterOptions = [
    { label: 'Todas', value: 'todas' },
    { label: 'Ativas', value: 'ativas' },
    { label: 'Inativas', value: 'inativas' }
  ];

  // Form
  form!: FormGroup;

  // Entity configuration
  entityDisplayName = () => 'Município';
  entityDescription = () => 'Gerenciar Municípios com seleção cascateada de UF';
  defaultSortField = () => 'nome';
  searchFields = () => ['nome', 'codigoIbge', 'uf.nome', 'uf.codigo'];

  // Table columns configuration
  displayColumns = (): TableColumn[] => [
    {
      field: 'nome',
      header: 'Nome',
      sortable: true,
      width: '300px'
    },
    {
      field: 'codigoIbge',
      header: 'Código IBGE',
      sortable: true,
      width: '150px',
      hideOnMobile: true
    },
    {
      field: 'uf',
      header: 'UF',
      sortable: true,
      width: '100px',
      type: 'custom'
    },
    {
      field: 'uf.pais',
      header: 'País',
      sortable: true,
      width: '150px',
      type: 'custom',
      hideOnMobile: true,
      hideOnTablet: true
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

  ngOnInit(): void {
    this.form = this.createFormGroup();
    this.carregarPaises();
    this.carregarItens();
    this.setupSearchSubscription();
  }

  /**
   * Load países for cascading dropdown
   */
  private carregarPaises(): void {
    this.loadingPaises.set(true);
    
    this.paisService.obterAtivos().subscribe({
      next: (paises) => {
        this.paisesOptions.set(paises);
        this.loadingPaises.set(false);
        
        // Auto-select Brasil if available
        const brasil = paises.find(p => p.codigo === 'BR' || p.nome.toLowerCase().includes('brasil'));
        if (brasil) {
          this.selectedPaisFilter.set(brasil.id);
          this.carregarUfsPorPais(brasil.id);
        }
      },
      error: (error) => {
        console.error('Erro ao carregar países:', error);
        this.loadingPaises.set(false);
      }
    });
  }

  /**
   * Load UFs by País ID
   */
  carregarUfsPorPais(paisId: number): void {
    this.loadingUfs.set(true);
    this.selectedUfFilter.set(null);
    
    this.ufService.obterAtivosPorPais(paisId).subscribe({
      next: (ufs) => {
        this.ufsOptions.set(ufs);
        this.loadingUfs.set(false);
      },
      error: (error) => {
        console.error('Erro ao carregar UFs:', error);
        this.loadingUfs.set(false);
      }
    });
  }

  /**
   * Handle país selection change
   */
  onPaisFilterChange(paisId: number | null): void {
    this.selectedPaisFilter.set(paisId);
    this.ufsOptions.set([]);
    this.selectedUfFilter.set(null);
    
    if (paisId) {
      this.carregarUfsPorPais(paisId);
    }
    
    this.carregarItens();
  }

  /**
   * Handle UF selection change
   */
  onUfFilterChange(ufId: number | null): void {
    this.selectedUfFilter.set(ufId);
    this.carregarItens();
  }

  /**
   * Setup search subscription with debounce
   */
  private setupSearchSubscription(): void {
    // This would be implemented with a search input field
    // For now, we'll handle it in the carregarItens method
  }

  /**
   * Load items with UF filtering and search
   */
  carregarItens(): void {
    this.loading.set(true);
    this.tableLoading.set(true);
    
    const filter = this.selectedStatusFilter();
    const ufId = this.selectedUfFilter();
    const search = this.searchTerm();
    
    let request;

    if (search && search.length >= 2) {
      // Use search functionality
      request = this.service.buscarPorNome(search, ufId || undefined);
    } else if (ufId) {
      // Filter by UF
      if (filter === 'ativas') {
        request = this.service.obterAtivosPorUf(ufId);
      } else {
        request = this.service.obterPorUf(ufId);
      }
    } else {
      // Get all with UF information
      if (filter === 'ativas') {
        request = this.service.obterAtivos();
      } else {
        request = this.service.obterComUf();
      }
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
        console.error('Erro ao carregar municípios:', error);
        this.loading.set(false);
        this.tableLoading.set(false);
      }
    });
  }

  /**
   * Create reactive form with validation
   */
  createFormGroup(): FormGroup {
    const form = this.fb.group({
      nome: ['', [
        Validators.required,
        Validators.minLength(2),
        Validators.maxLength(100)
      ]],
      codigoIbge: ['', [
        Validators.required,
        Validators.pattern(/^\d{7}$/)
      ]],
      paisId: ['', [
        Validators.required
      ]],
      ufId: ['', [
        Validators.required
      ]],
      ativo: [true]
    });

    // Add async validators
    form.get('codigoIbge')?.setAsyncValidators([this.validarCodigoIbgeUnico.bind(this)]);
    form.get('nome')?.setAsyncValidators([this.validarNomeUnico.bind(this)]);

    // Setup cascading dropdown behavior
    form.get('paisId')?.valueChanges.subscribe(paisId => {
      if (paisId) {
        this.carregarUfsParaFormulario(parseInt(paisId));
      } else {
        this.ufsOptions.set([]);
      }
      form.get('ufId')?.setValue('');
    });

    return form;
  }

  /**
   * Load UFs for form dropdown
   */
  private carregarUfsParaFormulario(paisId: number): void {
    this.loadingUfs.set(true);
    
    this.ufService.obterAtivosPorPais(paisId).subscribe({
      next: (ufs) => {
        this.ufsOptions.set(ufs);
        this.loadingUfs.set(false);
      },
      error: (error) => {
        console.error('Erro ao carregar UFs para formulário:', error);
        this.loadingUfs.set(false);
      }
    });
  }

  /**
   * Map form values to create DTO
   */
  mapToCreateDto(formValue: any): CriarMunicipioDto {
    return {
      nome: formValue.nome?.trim(),
      codigoIbge: formValue.codigoIbge?.trim(),
      ufId: parseInt(formValue.ufId)
    };
  }

  /**
   * Map form values to update DTO
   */
  mapToUpdateDto(formValue: any): AtualizarMunicipioDto {
    return {
      nome: formValue.nome?.trim(),
      ativo: formValue.ativo
    };
  }

  /**
   * Populate form with entity data for editing
   */
  populateForm(item: MunicipioDto): void {
    // First set país to load UFs
    if (item.uf?.paisId) {
      this.form.patchValue({
        paisId: item.uf.paisId
      });
      
      // Load UFs and then set the UF value
      this.carregarUfsParaFormulario(item.uf.paisId);
      
      // Set other values after a short delay to ensure UFs are loaded
      setTimeout(() => {
        this.form.patchValue({
          nome: item.nome,
          codigoIbge: item.codigoIbge,
          ufId: item.ufId,
          ativo: item.ativo
        });
      }, 100);
    } else {
      this.form.patchValue({
        nome: item.nome,
        codigoIbge: item.codigoIbge,
        ufId: item.ufId,
        ativo: item.ativo
      });
    }
  }

  /**
   * Custom validation for código IBGE uniqueness
   */
  private validarCodigoIbgeUnico(control: any) {
    if (!control.value) {
      return of(null);
    }

    const codigoIbge = control.value.trim();
    const municipioId = this.selectedItem()?.id;

    return this.service.validarCodigoIbgeUnico(codigoIbge, municipioId).pipe(
      map(isUnique => isUnique ? null : { codigoIbgeNaoUnico: true })
    );
  }

  /**
   * Custom validation for nome uniqueness within UF
   */
  private validarNomeUnico(control: any) {
    if (!control.value || !this.form?.get('ufId')?.value) {
      return of(null);
    }

    const nome = control.value.trim();
    const ufId = parseInt(this.form.get('ufId')?.value);
    const municipioId = this.selectedItem()?.id;

    return this.service.validarNomeUnico(nome, ufId, municipioId).pipe(
      map(isUnique => isUnique ? null : { nomeNaoUnico: true })
    );
  }

  /**
   * Get UF display text
   */
  getUfDisplay(item: MunicipioDto): string {
    return item.uf ? `${item.uf.codigo} - ${item.uf.nome}` : 'N/A';
  }

  /**
   * Get País display text
   */
  getPaisDisplay(item: MunicipioDto): string {
    return item.uf?.pais ? item.uf.pais.nome : 'N/A';
  }

  /**
   * Handle search input
   */
  onSearchChange(searchTerm: string): void {
    this.searchTerm.set(searchTerm);
    
    // Debounce search
    setTimeout(() => {
      if (this.searchTerm() === searchTerm) {
        this.carregarItens();
      }
    }, 300);
  }

  /**
   * Clear all filters
   */
  limparFiltros(): void {
    this.selectedPaisFilter.set(null);
    this.selectedUfFilter.set(null);
    this.searchTerm.set('');
    this.ufsOptions.set([]);
    this.carregarItens();
  }

  /**
   * Get filter summary text
   */
  getFilterSummary(): string {
    const filters = [];
    
    if (this.selectedUfFilter()) {
      const uf = this.ufsOptions().find(u => u.id === this.selectedUfFilter());
      if (uf) {
        filters.push(`UF: ${uf.codigo}`);
      }
    }
    
    if (this.searchTerm()) {
      filters.push(`Busca: "${this.searchTerm()}"`);
    }
    
    return filters.length > 0 ? `Filtros: ${filters.join(', ')}` : '';
  }

  /**
   * Handle status filter change
   */
  onStatusFilterChange(event: any): void {
    this.selectedStatusFilter.set(event.value);
    this.carregarItens();
  }

  /**
   * Open form for editing item
   */
  editarItem(item: MunicipioDto): void {
    this.selectedItem.set(item);
    this.populateForm(item);
    this.showForm.set(true);
  }

  /**
   * Open form for creating new item
   */
  novoItem(): void {
    this.selectedItem.set(null);
    this.form.reset();
    this.showForm.set(true);
  }

  /**
   * Save item (create or update)
   */
  salvarItem(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.formLoading.set(true);
    const formValue = this.form.value;
    
    if (this.isEditMode()) {
      // Update existing item
      const updateDto = this.mapToUpdateDto(formValue);
      const itemId = this.selectedItem()!.id;
      
      this.service.atualizar(itemId, updateDto).subscribe({
        next: () => {
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
          console.error('Erro ao atualizar:', error);
        }
      });
    } else {
      // Create new item
      const createDto = this.mapToCreateDto(formValue);
      
      this.service.criar(createDto).subscribe({
        next: () => {
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
          console.error('Erro ao criar:', error);
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
    this.form.reset();
  }

  /**
   * Activate item
   */
  ativarItem(item: MunicipioDto): void {
    this.service.ativar(item.id).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Sucesso',
          detail: `${this.entityDisplayName()} ativado com sucesso`,
          life: 3000
        });
        this.carregarItens();
      },
      error: (error) => {
        console.error('Erro ao ativar:', error);
      }
    });
  }

  /**
   * Deactivate item
   */
  desativarItem(item: MunicipioDto): void {
    this.confirmationService.confirm({
      message: `Tem certeza que deseja desativar este ${this.entityDisplayName().toLowerCase()}?`,
      header: 'Confirmar Desativação',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Sim',
      rejectLabel: 'Não',
      accept: () => {
        this.service.desativar(item.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Sucesso',
              detail: `${this.entityDisplayName()} desativado com sucesso`,
              life: 3000
            });
            this.carregarItens();
          },
          error: (error) => {
            console.error('Erro ao desativar:', error);
          }
        });
      }
    });
  }

  /**
   * Confirm and delete item
   */
  excluirItem(item: MunicipioDto): void {
    this.confirmationService.confirm({
      message: `Tem certeza que deseja excluir este ${this.entityDisplayName().toLowerCase()}?`,
      header: 'Confirmar Exclusão',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Sim',
      rejectLabel: 'Não',
      accept: () => {
        this.service.remover(item.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Sucesso',
              detail: `${this.entityDisplayName()} excluído com sucesso`,
              life: 3000
            });
            this.carregarItens();
          },
          error: (error) => {
            console.error('Erro ao excluir:', error);
          }
        });
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
}