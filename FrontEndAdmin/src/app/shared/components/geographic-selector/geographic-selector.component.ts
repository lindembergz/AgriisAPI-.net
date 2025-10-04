import { Component, OnInit, inject, signal, input, output, forwardRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, NG_VALUE_ACCESSOR, FormControl, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { SelectModule } from 'primeng/select';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MessageModule } from 'primeng/message';
import { TooltipModule } from 'primeng/tooltip';
import { ReferenceCrudService } from '../../services/reference-crud.service';
import { PaisDto, UfDto, MunicipioDto, GeographicData } from '../../models/reference.model';
import { UfService } from '../../../features/referencias/ufs/services/uf.service';
import { MunicipioService } from '../../../features/referencias/municipios/services/municipio.service';

/**
 * Geographic selector component for cascading País > UF > Município selection
 * Implements ControlValueAccessor for reactive forms integration
 */
@Component({
  selector: 'app-geographic-selector',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    SelectModule,
    ProgressSpinnerModule,
    MessageModule,
    TooltipModule
  ],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => GeographicSelectorComponent),
      multi: true
    }
  ],
  template: `
    <div class="geographic-selector">
      <!-- País Selection -->
      <div class="field" *ngIf="showPais()">
        <label for="pais" [class.required]="required()">
          País
          <span *ngIf="required()" class="required-asterisk">*</span>
        </label>
        <p-select
          id="pais"
          [options]="paises()"
          [ngModel]="selectedPais()"
          (onChange)="onPaisChange($event)"
          placeholder="Selecione um país"
          optionLabel="nome"
          optionValue="id"
          [loading]="paisesLoading()"
          [disabled]="disabled() || paisesLoading()"
          [filter]="true"
          filterBy="nome"
          [showClear]="!required()"
          class="w-full"
          [class.p-invalid]="hasError() && touched()"
          pTooltip="Selecione o país"
          tooltipPosition="top">
          
          <ng-template pTemplate="selectedItem" let-selectedOption>
            <div *ngIf="selectedOption" class="selected-item">
              <span>{{ getPaisNome(selectedOption) }}</span>
            </div>
          </ng-template>
          
          <ng-template pTemplate="item" let-pais>
            <div class="dropdown-item">
              <span class="item-name">{{ pais.nome }}</span>
              <small class="item-code">{{ pais.codigo }}</small>
            </div>
          </ng-template>
          
          <ng-template pTemplate="empty">
            <div class="empty-message">
              <i class="pi pi-info-circle"></i>
              <span>Nenhum país encontrado</span>
            </div>
          </ng-template>
        </p-select>
        
        <div *ngIf="paisesError()" class="error-container">
          <p-message severity="error" text="Erro ao carregar países"></p-message>
        </div>
      </div>

      <!-- UF Selection -->
      <div class="field" *ngIf="showUf()">
        <label for="uf" [class.required]="required()">
          Estado (UF)
          <span *ngIf="required()" class="required-asterisk">*</span>
        </label>
        <p-select
          id="uf"
          [options]="ufs()"
          [ngModel]="selectedUf()"
          (onChange)="onUfChange($event)"
          placeholder="Selecione um estado"
          optionLabel="nome"
          optionValue="id"
          [loading]="ufsLoading()"
          [disabled]="disabled() || ufsLoading()"
          [filter]="true"
          filterBy="nome,codigo"
          [showClear]="!required()"
          class="w-full"
          [class.p-invalid]="hasError() && touched()"
          pTooltip="Selecione o estado"
          tooltipPosition="top">
          
          <ng-template pTemplate="selectedItem" let-selectedOption>
            <div *ngIf="selectedOption" class="selected-item">
              <span>{{ getUfNome(selectedOption) }}</span>
            </div>
          </ng-template>
          
          <ng-template pTemplate="item" let-uf>
            <div class="dropdown-item">
              <span class="item-name">{{ uf.nome }}</span>
              <small class="item-code">{{ uf.codigo }}</small>
            </div>
          </ng-template>
          
          <ng-template pTemplate="empty">
            <div class="empty-message">
              <i class="pi pi-info-circle"></i>
              <span>
                @if (!selectedPais()) {
                  Selecione um país primeiro
                } @else if (ufsLoading()) {
                  Carregando estados...
                } @else {
                  Nenhum estado encontrado
                }
              </span>
            </div>
          </ng-template>
        </p-select>
        
        <div *ngIf="ufsError()" class="error-container">
          <p-message severity="error" text="Erro ao carregar estados"></p-message>
        </div>
      </div>

      <!-- Município Selection -->
      <div class="field" *ngIf="showMunicipio()">
        <label for="municipio" [class.required]="required()">
          Município
          <span *ngIf="required()" class="required-asterisk">*</span>
        </label>
        <p-select
          id="municipio"
          [options]="municipios()"
          [ngModel]="selectedMunicipio()"
          (onChange)="onMunicipioChange($event)"
          placeholder="Selecione um município"
          optionLabel="nome"
          optionValue="id"
          [loading]="municipiosLoading()"
          [disabled]="disabled() || municipiosLoading() || !selectedUf()"
          [filter]="true"
          filterBy="nome,codigoIbge"
          [showClear]="!required()"
          class="w-full"
          [class.p-invalid]="hasError() && touched()"
          pTooltip="Selecione o município"
          tooltipPosition="top">
          
          <ng-template pTemplate="selectedItem" let-selectedOption>
            <div *ngIf="selectedOption" class="selected-item">
              <span>{{ getMunicipioNome(selectedOption) }}</span>
            </div>
          </ng-template>
          
          <ng-template pTemplate="item" let-municipio>
            <div class="dropdown-item">
              <span class="item-name">{{ municipio.nome }}</span>
              <small class="item-code">IBGE: {{ municipio.codigoIbge }}</small>
            </div>
          </ng-template>
          
          <ng-template pTemplate="empty">
            <div class="empty-message">
              <i class="pi pi-info-circle"></i>
              <span>
                @if (!selectedUf()) {
                  Selecione um estado primeiro
                } @else if (municipiosLoading()) {
                  Carregando municípios...
                } @else {
                  Nenhum município encontrado
                }
              </span>
            </div>
          </ng-template>
        </p-select>
        
        <div *ngIf="municipiosError()" class="error-container">
          <p-message severity="error" text="Erro ao carregar municípios"></p-message>
        </div>
      </div>

      <!-- Loading Indicator -->
      <div *ngIf="isLoading()" class="loading-indicator">
        <p-progressSpinner [style]="{ width: '20px', height: '20px' }"></p-progressSpinner>
        <span>Carregando dados geográficos...</span>
      </div>
    </div>
  `,
  styleUrls: ['./geographic-selector.component.scss']
})
export class GeographicSelectorComponent implements OnInit, ControlValueAccessor {
  
  // Input properties
  showPais = input<boolean>(true);
  showUf = input<boolean>(true);
  showMunicipio = input<boolean>(true);
  required = input<boolean>(false);
  disabled = input<boolean>(false);
  initialData = input<GeographicData | null>(null);
  
  // Output events
  paisChange = output<PaisDto | null>();
  ufChange = output<UfDto | null>();
  municipioChange = output<MunicipioDto | null>();
  selectionChange = output<GeographicData>();

  // Services - inject specific services
  private ufService = inject(UfService);
  private municipioService = inject(MunicipioService);

  // Signals for reactive state
  paises = signal<PaisDto[]>([]);
  ufs = signal<UfDto[]>([]);
  municipios = signal<MunicipioDto[]>([]);
  
  selectedPais = signal<number | null>(null);
  selectedUf = signal<number | null>(null);
  selectedMunicipio = signal<number | null>(null);
  
  paisesLoading = signal<boolean>(false);
  ufsLoading = signal<boolean>(false);
  municipiosLoading = signal<boolean>(false);
  
  paisesError = signal<boolean>(false);
  ufsError = signal<boolean>(false);
  municipiosError = signal<boolean>(false);
  
  touched = signal<boolean>(false);
  hasError = signal<boolean>(false);

  // ControlValueAccessor implementation
  private onChange = (value: GeographicData) => {};
  private onTouched = () => {};

  ngOnInit(): void {
    console.log('🌍 GeographicSelector ngOnInit - configurações:', {
      showPais: this.showPais(),
      showUf: this.showUf(),
      showMunicipio: this.showMunicipio(),
      initialData: this.initialData()
    });
    
    // Carregar dados iniciais se fornecidos
    const initial = this.initialData();
    if (initial) {
      console.log('📥 Processando dados iniciais:', initial);
      this.processInitialData(initial);
    } else {
      // Carregar dados padrão
      if (this.showPais()) {
        this.loadPaises();
      } else if (this.showUf()) {
        // Se não mostra país mas mostra UF, carregar UFs diretamente (assumindo Brasil)
        console.log('🏛️ Carregando UFs diretamente (Brasil assumido)');
        this.loadUfs(1);
      }
    }
  }

  /**
   * Load países from API
   */
  private loadPaises(): void {
    this.paisesLoading.set(true);
    this.paisesError.set(false);
    
    // Por enquanto, vamos usar dados fixos para o Brasil
    // Em uma implementação futura, isso viria de um serviço de países
    const mockPaises: PaisDto[] = [
      { 
        id: 1, 
        nome: 'Brasil', 
        codigo: 'BR', 
        ativo: true, 
        dataCriacao: new Date(), 
        dataAtualizacao: new Date(),
        estados: [], // Will be populated when UFs are loaded
        totalEstados: 0 // Will be updated when UFs are loaded
      }
    ];
    
    this.paises.set(mockPaises);
    this.paisesLoading.set(false);
    
    // Se não temos país selecionado, selecionar Brasil por padrão
    if (!this.selectedPais()) {
      this.selectedPais.set(1);
      if (this.showUf()) {
        this.loadUfs(1);
      }
    }
  }

  /**
   * Load UFs for selected país
   */
  private loadUfs(paisId: number): void {
    this.ufsLoading.set(true);
    this.ufsError.set(false);
    this.ufs.set([]);
    
    // Usar o serviço real de UFs
    this.ufService.obterAtivos().subscribe({
      next: (ufs) => {
        console.log('🏛️ UFs carregadas:', ufs);
        this.ufs.set(ufs);
        this.ufsLoading.set(false);
      },
      error: (error) => {
        console.error('❌ Erro ao carregar UFs:', error);
        this.ufsError.set(true);
        this.ufsLoading.set(false);
      }
    });
  }

  /**
   * Load municípios for selected UF
   */
  private loadMunicipios(ufId: number): void {
    this.municipiosLoading.set(true);
    this.municipiosError.set(false);
    this.municipios.set([]);
    
    // Usar o serviço real de municípios
    this.municipioService.obterAtivosPorUf(ufId).subscribe({
      next: (municipios) => {
        console.log('🏘️ Municípios carregados para UF', ufId, ':', municipios);
        this.municipios.set(municipios);
        this.municipiosLoading.set(false);
      },
      error: (error) => {
        console.error('❌ Erro ao carregar municípios:', error);
        this.municipiosError.set(true);
        this.municipiosLoading.set(false);
      }
    });
  }

  /**
   * Handle país selection change
   */
  onPaisChange(event: any): void {
    const paisId = event.value;
    this.selectedPais.set(paisId);
    
    // Reset dependent selections
    this.selectedUf.set(null);
    this.selectedMunicipio.set(null);
    this.ufs.set([]);
    this.municipios.set([]);
    
    if (paisId && this.showUf()) {
      this.loadUfs(paisId);
    }
    
    this.markAsTouched();
    this.emitSelectionChange();
    
    // Emit individual change event
    const pais = this.paises().find(p => p.id === paisId) || null;
    this.paisChange.emit(pais);
  }

  /**
   * Handle UF selection change
   */
  onUfChange(event: any): void {
    const ufId = event.value;
    this.selectedUf.set(ufId);
    
    // Reset dependent selections
    this.selectedMunicipio.set(null);
    this.municipios.set([]);
    
    if (ufId && this.showMunicipio()) {
      this.loadMunicipios(ufId);
    }
    
    this.markAsTouched();
    this.emitSelectionChange();
    
    // Emit individual change event
    const uf = this.ufs().find(u => u.id === ufId) || null;
    this.ufChange.emit(uf);
  }

  /**
   * Handle município selection change
   */
  onMunicipioChange(event: any): void {
    const municipioId = event.value;
    this.selectedMunicipio.set(municipioId);
    
    this.markAsTouched();
    this.emitSelectionChange();
    
    // Emit individual change event
    const municipio = this.municipios().find(m => m.id === municipioId) || null;
    this.municipioChange.emit(municipio);
  }



  /**
   * Mark component as touched
   */
  private markAsTouched(): void {
    this.touched.set(true);
    this.onTouched();
  }

  /**
   * Check if any loading is in progress
   */
  isLoading(): boolean {
    return this.paisesLoading() || this.ufsLoading() || this.municipiosLoading();
  }

  /**
   * Get país name by ID
   */
  getPaisNome(paisId: number): string {
    const pais = this.paises().find(p => p.id === paisId);
    return pais ? pais.nome : '';
  }

  /**
   * Get UF name by ID
   */
  getUfNome(ufId: number): string {
    const uf = this.ufs().find(u => u.id === ufId);
    return uf ? `${uf.nome} (${uf.uf})` : '';
  }

  /**
   * Get município name by ID
   */
  getMunicipioNome(municipioId: number): string {
    const municipio = this.municipios().find(m => m.id === municipioId);
    return municipio ? municipio.nome : '';
  }

  // ControlValueAccessor implementation
  writeValue(value: GeographicData | null): void {
    console.log('🔄 GeographicSelector writeValue called with:', value);
    
    if (value) {
      // Se temos dados geográficos, vamos configurar as seleções
      if (value.pais) {
        this.selectedPais.set(value.pais.id);
        console.log('🌍 País selecionado:', value.pais);
      }
      
      if (this.showUf() && value.uf) {
        console.log('🏛️ Configurando UF:', value.uf);
        
        // Carregar UFs se ainda não foram carregadas
        if (this.ufs().length === 0) {
          this.loadUfs(value.pais?.id || 1); // Assumir Brasil como padrão
        }
        
        // Aguardar um pouco para os UFs carregarem, então selecionar
        setTimeout(() => {
          this.selectedUf.set(value.uf!.id);
          console.log('✅ UF selecionada:', value.uf!.id);
          
          if (this.showMunicipio() && value.municipio) {
            console.log('🏘️ Configurando Município:', value.municipio);
            this.loadMunicipios(value.uf!.id);
            
            // Aguardar um pouco para os municípios carregarem, então selecionar
            setTimeout(() => {
              this.selectedMunicipio.set(value.municipio!.id);
              console.log('✅ Município selecionado:', value.municipio!.id);
            }, 200);
          }
        }, 200);
      }
    } else {
      console.log('🧹 Limpando seleções geográficas');
      this.selectedPais.set(null);
      this.selectedUf.set(null);
      this.selectedMunicipio.set(null);
      this.ufs.set([]);
      this.municipios.set([]);
    }
  }

  registerOnChange(fn: (value: GeographicData) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    // The disabled state is handled by the input signal
  }

  /**
   * Validate the current selection
   */
  validate(): boolean {
    let isValid = true;
    
    if (this.required()) {
      if (this.showPais() && !this.selectedPais()) isValid = false;
      if (this.showUf() && !this.selectedUf()) isValid = false;
      if (this.showMunicipio() && !this.selectedMunicipio()) isValid = false;
    }
    
    this.hasError.set(!isValid);
    return isValid;
  }

  /**
   * Process initial data and set selections
   */
  public processInitialData(data: GeographicData): void {
    console.log('🔄 Processando dados iniciais:', data);
    
    // Configurar país se disponível
    if (data.pais && this.showPais()) {
      this.selectedPais.set(data.pais.id);
      console.log('🌍 País inicial configurado:', data.pais.nome);
    }
    
    // Configurar UF se disponível
    if (data.uf && this.showUf()) {
      console.log('🏛️ Configurando UF inicial:', data.uf.nome);
      
      // Carregar UFs primeiro
      this.loadUfs(data.pais?.id || 1);
      
      // Aguardar carregamento e então selecionar
      setTimeout(() => {
        this.selectedUf.set(data.uf!.id);
        console.log('✅ UF inicial selecionada:', data.uf!.nome);
        
        // Configurar município se disponível
        if (data.municipio && this.showMunicipio()) {
          console.log('🏘️ Configurando município inicial:', data.municipio.nome);
          this.loadMunicipios(data.uf!.id);
          
          // Aguardar carregamento e então selecionar
          setTimeout(() => {
            this.selectedMunicipio.set(data.municipio!.id);
            console.log('✅ Município inicial selecionado:', data.municipio!.nome);
            
            // Emitir evento de mudança
            this.emitSelectionChange();
          }, 300);
        }
      }, 300);
    }
  }

  /**
   * Emit selection change event
   */
  private emitSelectionChange(): void {
    const currentData: GeographicData = {
      pais: this.paises().find(p => p.id === this.selectedPais()) || null,
      uf: this.ufs().find(u => u.id === this.selectedUf()) || null,
      municipio: this.municipios().find(m => m.id === this.selectedMunicipio()) || null
    };
    
    console.log('📤 Emitindo mudança de seleção:', currentData);
    this.selectionChange.emit(currentData);
    this.onChange(currentData);
  }

  /**
   * Reset all selections
   */
  reset(): void {
    this.selectedPais.set(null);
    this.selectedUf.set(null);
    this.selectedMunicipio.set(null);
    this.ufs.set([]);
    this.municipios.set([]);
    this.touched.set(false);
    this.hasError.set(false);
    this.emitSelectionChange();
  }

  /**
   * Check if component is ready to receive data
   */
  public isReady(): boolean {
    return this.showUf() ? this.ufs().length > 0 : true;
  }

  /**
   * Set specific selections programmatically
   */
  setSelection(data: Partial<GeographicData>): void {
    if (data.pais) {
      this.onPaisChange({ value: data.pais.id });
    }
    if (data.uf) {
      setTimeout(() => this.onUfChange({ value: data.uf!.id }), 100);
    }
    if (data.municipio) {
      setTimeout(() => this.onMunicipioChange({ value: data.municipio!.id }), 200);
    }
  }
}