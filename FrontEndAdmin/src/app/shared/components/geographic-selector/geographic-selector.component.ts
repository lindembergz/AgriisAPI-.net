import { Component, OnInit, inject, signal, input, output, forwardRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, NG_VALUE_ACCESSOR, FormControl, ReactiveFormsModule } from '@angular/forms';
import { SelectModule } from 'primeng/select';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MessageModule } from 'primeng/message';
import { TooltipModule } from 'primeng/tooltip';
import { ReferenceCrudService } from '../../services/reference-crud.service';
import { PaisDto, UfDto, MunicipioDto, GeographicData } from '../../models/reference.model';

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
          [disabled]="disabled() || ufsLoading() || !selectedPais()"
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
  
  // Output events
  paisChange = output<PaisDto | null>();
  ufChange = output<UfDto | null>();
  municipioChange = output<MunicipioDto | null>();
  selectionChange = output<GeographicData>();

  // Services (these would be injected concrete implementations)
  // For now, we'll use abstract services that need to be provided
  private paisService!: ReferenceCrudService<PaisDto, any, any>;
  private ufService!: ReferenceCrudService<UfDto, any, any>;
  private municipioService!: ReferenceCrudService<MunicipioDto, any, any>;

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
    if (this.showPais()) {
      this.loadPaises();
    }
  }

  /**
   * Load países from API
   */
  private loadPaises(): void {
    this.paisesLoading.set(true);
    this.paisesError.set(false);
    
    // This would use the actual service
    // For now, we'll simulate the call
    setTimeout(() => {
      // Simulated data - in real implementation, this would come from the service
      const mockPaises: PaisDto[] = [
        { id: 1, nome: 'Brasil', codigo: 'BR', ativo: true, dataCriacao: new Date(), dataAtualizacao: new Date() }
      ];
      
      this.paises.set(mockPaises);
      this.paisesLoading.set(false);
    }, 500);
  }

  /**
   * Load UFs for selected país
   */
  private loadUfs(paisId: number): void {
    this.ufsLoading.set(true);
    this.ufsError.set(false);
    this.ufs.set([]);
    
    // This would use the actual service with cascading endpoint
    // GET /api/referencias/ufs/pais/{paisId}
    setTimeout(() => {
      // Simulated data - in real implementation, this would come from the service
      const mockUfs: UfDto[] = [
        { id: 1, nome: 'São Paulo', codigo: 'SP', paisId: 1, ativo: true, dataCriacao: new Date(), dataAtualizacao: new Date() },
        { id: 2, nome: 'Rio de Janeiro', codigo: 'RJ', paisId: 1, ativo: true, dataCriacao: new Date(), dataAtualizacao: new Date() },
        { id: 3, nome: 'Minas Gerais', codigo: 'MG', paisId: 1, ativo: true, dataCriacao: new Date(), dataAtualizacao: new Date() }
      ];
      
      this.ufs.set(mockUfs);
      this.ufsLoading.set(false);
    }, 300);
  }

  /**
   * Load municípios for selected UF
   */
  private loadMunicipios(ufId: number): void {
    this.municipiosLoading.set(true);
    this.municipiosError.set(false);
    this.municipios.set([]);
    
    // This would use the actual service with cascading endpoint
    // GET /api/referencias/municipios/uf/{ufId}
    setTimeout(() => {
      // Simulated data - in real implementation, this would come from the service
      const mockMunicipios: MunicipioDto[] = [
        { id: 1, nome: 'São Paulo', codigoIbge: '3550308', ufId: 1, ativo: true, dataCriacao: new Date(), dataAtualizacao: new Date() },
        { id: 2, nome: 'Campinas', codigoIbge: '3509502', ufId: 1, ativo: true, dataCriacao: new Date(), dataAtualizacao: new Date() },
        { id: 3, nome: 'Santos', codigoIbge: '3548500', ufId: 1, ativo: true, dataCriacao: new Date(), dataAtualizacao: new Date() }
      ];
      
      this.municipios.set(mockMunicipios);
      this.municipiosLoading.set(false);
    }, 300);
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
    this.emitChange();
    
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
    this.emitChange();
    
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
    this.emitChange();
    
    // Emit individual change event
    const municipio = this.municipios().find(m => m.id === municipioId) || null;
    this.municipioChange.emit(municipio);
  }

  /**
   * Emit combined change event
   */
  private emitChange(): void {
    const data: GeographicData = {
      pais: this.selectedPais() ? this.paises().find(p => p.id === this.selectedPais()) : undefined,
      uf: this.selectedUf() ? this.ufs().find(u => u.id === this.selectedUf()) : undefined,
      municipio: this.selectedMunicipio() ? this.municipios().find(m => m.id === this.selectedMunicipio()) : undefined
    };
    
    this.onChange(data);
    this.selectionChange.emit(data);
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
    return uf ? `${uf.nome} (${uf.codigo})` : '';
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
    if (value) {
      if (value.pais) {
        this.selectedPais.set(value.pais.id);
        if (this.showUf() && value.uf) {
          this.loadUfs(value.pais.id);
          this.selectedUf.set(value.uf.id);
          if (this.showMunicipio() && value.municipio) {
            this.loadMunicipios(value.uf.id);
            this.selectedMunicipio.set(value.municipio.id);
          }
        }
      }
    } else {
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
    this.emitChange();
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