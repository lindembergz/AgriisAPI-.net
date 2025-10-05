import { Component, inject, signal, OnInit, computed } from '@angular/core';
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
import { CustomFilter, CustomAction, EmptyStateConfig, LoadingStateConfig, TableColumn } from '../../../shared/interfaces/component-template.interface';
import { MunicipioService } from './services/municipio.service';
import { UfService } from '../ufs/services/uf.service';
import { PaisService } from '../paises/services/pais.service';
import { map } from 'rxjs/operators';
import { of } from 'rxjs';
import { FieldErrorComponent } from '../../../shared/components/field-error/field-error.component';

/**
 * Component for managing Municípios with UF cascading selection
 * Extends unified ReferenceCrudBaseComponent while maintaining relationship functionality
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
    CheckboxModule,
    FieldErrorComponent
  ],
  providers: [ConfirmationService, MessageService],
  templateUrl: './municipios.component.html',
  styleUrls: ['./municipios.component.scss']
})
export class MunicipiosComponent extends ReferenceCrudBaseComponent<MunicipioDto, CriarMunicipioDto, AtualizarMunicipioDto> implements OnInit {
  
  protected service = inject(MunicipioService);
  private ufService = inject(UfService);
  private paisService = inject(PaisService);

  // Relationship-specific signals
  paisesOptions = signal<PaisDto[]>([]);
  ufsOptions = signal<UfDto[]>([]);
  loadingPaises = signal<boolean>(false);
  loadingUfs = signal<boolean>(false);
  
  // Custom filter signals
  selectedPaisFilter = signal<number | null>(null);
  selectedUfFilter = signal<number | null>(null);
  
  // Current item for template access
  currentItem: MunicipioDto | null = null;

  readonly rowsPerPageOptions = [5, 10, 20, 50];

  // Entity configuration
  protected entityDisplayName = () => 'Municípios';
  protected entityDescription = () => '';
  protected defaultSortField = () => 'nome';
  protected searchFields = () => ['nome', 'codigoIbge', 'uf.nome', 'uf.codigo'];

  protected getCustomFilters = computed<CustomFilter[]>(() => {
    return [
      {
        key: 'pais',
        label: 'País',
        placeholder: 'Selecione um país',
        type: 'select',
        visible: true,
        options: [
          { label: 'Todos os países', value: null },
          ...this.paisesOptions().map(pais => ({
            label: pais.nome,
            value: pais.id
          }))
        ]
      },
      {
        key: 'uf',
        label: 'UF',
        placeholder: 'Selecione uma UF',
        type: 'select',
        visible: true,
        options: [
          { label: 'Todas as UFs', value: null },
          ...this.ufsOptions().map(uf => ({
            label: `${uf.uf} - ${uf.nome}`,
            value: uf.id
          }))
        ]
      }
    ];
  });

  // Template-friendly option providers to avoid complex expressions in HTML
  paisFilterOptions = computed(() => [{ label: 'Todos os países', value: null }, ...this.paisesOptions().map(pais => ({ label: pais.nome, value: pais.id }))]);

  ufFilterOptions = computed(() => [{ label: 'Todas as UFs', value: null }, ...this.ufsOptions().map(uf => ({ label: `${uf.uf} - ${uf.nome}`, value: uf.id }))]);

  protected getCustomActions(): CustomAction[] {
    return [];
  }

  protected getEmptyStateConfig(): EmptyStateConfig {
    return {
      icon: 'pi pi-map-marker',
      title: 'Nenhum município encontrado',
      description: this.hasActiveFilters() 
        ? 'Não há municípios que atendam aos filtros aplicados.'
        : 'Não há municípios cadastrados no sistema.',
      primaryAction: {
        label: 'Novo Município',
        icon: 'pi pi-plus',
        action: () => this.novoItem()
      },
      secondaryActions: this.hasActiveFilters() ? [{
        label: 'Limpar Filtros',
        icon: 'pi pi-filter-slash',
        action: () => this.clearAllFilters()
      }] : []
    };
  }

  protected getLoadingStateConfig(): LoadingStateConfig {
    return {
      message: 'Carregando municípios...',
      showProgress: false
    };
  }

  // Table columns configuration
  private readonly _displayColumns: TableColumn[] = [
    {
      field: 'nome',
      header: 'Nome',
      sortable: true,
      width: '300px',
      type: 'text'
    },
    {
      field: 'codigoIbge',
      header: 'Código IBGE',
      sortable: true,
      width: '150px',
      hideOnMobile: true,
      type: 'text'
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
    }/*,
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
    }*/
  ];

  protected displayColumns = (): TableColumn[] => this._displayColumns;

  ngOnInit(): void {
    super.ngOnInit();
    this.carregarPaises();
    this.setupCustomFilters();
  }

  protected setupCustomFilters(): void {
    // Os filtros customizados são gerenciados pelo componente base
    // através dos métodos onCustomFilterChange e applyCustomFilter
    console.log('🔧 Configurando filtros customizados para municípios');
  }

  /**
   * Load países for cascading dropdown
   */
  private carregarPaises(): void {

    this.loadingPaises.set(true);

    this.paisService.obterTodos().subscribe({
      next: (paises) => {
        
        this.paisesOptions.set(paises);
        this.loadingPaises.set(false);
        
        // Auto-select Brasil if available
        const brasil = paises.find(p => p.codigo === 'BR' || p.nome.toLowerCase().includes('brasil'));
        if (brasil) {
          // this.componentStateService.setCustomFilterValue('pais', brasil.id);
          this.carregarUfsPorPais(brasil.id);
        }
         
      },
      error: (error) => {
        this.unifiedErrorHandlingService.handleComponentError('municipios', 'load-paises', error);
        this.loadingPaises.set(false);
      }
    });
  }

  /**
   * Load UFs by País ID
   */
  carregarUfsPorPais(paisId: number): void {
    this.loadingUfs.set(true);
    
    this.ufService.obterAtivosPorPais(paisId).subscribe({
      next: (ufs) => {
        this.ufsOptions.set(ufs);
        this.loadingUfs.set(false);
      },
      error: (error) => {
        this.unifiedErrorHandlingService.handleComponentError('municipios', 'load-ufs', error);
        this.loadingUfs.set(false);
      }
    });
  }

  /**
   * Override carregarItens to implement UF filtering and search
   */
  public carregarItens(): void {
    this.loading.set(true);
    
    // Obter filtros customizados do componente base
    const ufId = this.getCustomFilterValue('uf');
    
    let request;

    if (ufId) {
      // Filter by UF - usar o filtro customizado
      console.log('🔍 Carregando municípios filtrados por UF:', ufId);
      request = this.service.obterAtivosPorUf(ufId);
    } else {
      // Get all with UF information
      console.log('🌍 Carregando todos os municípios');
      request = this.service.obterAtivos();
    }

    request.subscribe({
      next: (items: MunicipioDto[]) => {
        console.log('📊 Municípios carregados:', items.length);
        this.items.set(items);
        this.loading.set(false);
        
        // Aplicar filtros do componente base (busca, status, etc.)
        this.applyFilters();
      },
      error: (error: any) => {
        console.error('❌ Erro ao carregar municípios:', error);
        this.unifiedErrorHandlingService.handleComponentError('municipios', 'load', error);
        this.loading.set(false);
      }
    });
  }

  /**
   * Create reactive form with validation
   */
  protected createFormGroup(): FormGroup {
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
    //form.get('codigoIbge')?.setAsyncValidators([this.validarCodigoIbgeUnico.bind(this)]);
    //form.get('nome')?.setAsyncValidators([this.validarNomeUnico.bind(this)]);

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
        this.unifiedErrorHandlingService.handleComponentError('municipios', 'load-ufs-form', error);
        this.loadingUfs.set(false);
      }
    });
  }

  /**
   * Map form values to create DTO
   */
  protected mapToCreateDto(formValue: any): CriarMunicipioDto {
    return {
      nome: formValue.nome?.trim(),
      codigoIbge: formValue.codigoIbge?.trim(),
      estadoId: parseInt(formValue.ufId)
    };
  }

  /**
   * Map form values to update DTO
   */
  protected mapToUpdateDto(formValue: any): AtualizarMunicipioDto {
    return {
      nome: formValue.nome?.trim(),
      ativo: formValue.ativo
    };
  }

  /**
   * Populate form with entity data for editing
   */
  protected populateForm(item: MunicipioDto): void {
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
    return item.uf ? `${item.uf.uf} - ${item.uf.nome}` : 'N/A';
  }

  /**
   * Get País display text
   */
  getPaisDisplay(item: MunicipioDto): string {
    return item.uf?.pais ? item.uf.pais.nome : 'N/A';
  }

  /**
   * Override clearAllFilters to handle custom filters
   */
  public clearAllFilters(): void {
    super.clearAllFilters();
    this.selectedPaisFilter.set(null);
    this.selectedUfFilter.set(null);
    this.ufsOptions.set([]);
    // this.componentStateService.setCustomFilterValue('pais', null);
    // this.componentStateService.setCustomFilterValue('uf', null);
  }

  /**
   * Override hasActiveFilters to include custom filters
   */
  public hasActiveFilters(): boolean {
    return super.hasActiveFilters() || 
           this.selectedPaisFilter() !== null || 
           this.selectedUfFilter() !== null;
  }

  /**
   * Override applyCustomFilter to handle UF filtering
   */
  protected applyCustomFilter(items: MunicipioDto[], filterKey: string, filterValue: any): MunicipioDto[] {
    console.log('🔧 Aplicando filtro customizado:', filterKey, '=', filterValue);
    
    switch (filterKey) {
      case 'uf':
        if (filterValue) {
          const filtered = items.filter(item => item.ufId === filterValue);
          console.log('🏛️ Filtro UF aplicado:', filtered.length, 'de', items.length, 'municípios');
          return filtered;
        }
        break;
      case 'pais':
        if (filterValue) {
          const filtered = items.filter(item => item.uf?.paisId === filterValue);
          console.log('🌍 Filtro País aplicado:', filtered.length, 'de', items.length, 'municípios');
          return filtered;
        }
        break;
    }
    
    return items;
  }

  /**
   * Override onCustomFilterChange to handle cascading filters
   */
  onCustomFilterChange(filterKey: string, event: any): void {
    console.log('🔄 Filtro customizado alterado:', filterKey, '=', event.value);
    
    if (filterKey === 'pais') {
      // Reset UF filter when país changes
      const currentFilters = new Map(this.customFilters());
      currentFilters.set('uf', null);
      this.customFilters.set(currentFilters);
      
      // Load UFs for the selected país
      if (event.value) {
        this.carregarUfsPorPais(event.value);
      } else {
        this.ufsOptions.set([]);
      }
    }
    
    // Call parent method to handle the filter change
    super.onCustomFilterChange(filterKey, event);
  }


}