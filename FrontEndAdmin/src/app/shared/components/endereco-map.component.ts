import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GoogleMapsModule } from '@angular/google-maps';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';

// PrimeNG imports
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Message } from 'primeng/message';
import { InputTextModule } from 'primeng/inputtext';
import { InputMaskModule } from 'primeng/inputmask';
import { SelectModule } from 'primeng/select';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TooltipModule } from 'primeng/tooltip';

// Services and models
import { GeocodingService, GeocodingResult } from '../services/geocoding.service';
import { BaseComponent, GoogleMapsManager } from '../utils/memory-management.util';
import { PerformanceService } from '../../core/services/performance.service';
import { Endereco } from '../models/endereco.model';

// Interfaces
import { Coordenadas, CONSTANTES_GEOGRAFICAS } from '../interfaces/coordenadas.interface';

/**
 * Componente avançado para seleção de endereço com mapa e geocodificação reversa
 */
@Component({
  selector: 'app-endereco-map',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    GoogleMapsModule,
    Button,
    Card,
    Message,
    InputTextModule,
    InputMaskModule,
    SelectModule,
    ProgressSpinnerModule,
    TooltipModule
  ],
  template: `
    <p-card header="Seleção de Endereço com Mapa" styleClass="mb-3">
      <!-- Loading state -->
      <div *ngIf="loading()" class="text-center p-4">
        <p-progressSpinner styleClass="w-4rem h-4rem"></p-progressSpinner>
        <p class="mt-2">Carregando mapa...</p>
      </div>

      <!-- Error state -->
      <p-message 
        *ngIf="error()" 
        severity="error" 
        [text]="error()!"
        styleClass="mb-3">
      </p-message>

      <!-- Main content -->
      <div *ngIf="!loading() && !error()">
        <!-- Instructions -->
        <div class="mb-3">
          <p-message 
            *ngIf="!readonly" 
            severity="info" 
            text="Clique no mapa para selecionar a localização e obter automaticamente os dados do endereço"
            styleClass="mb-2">
          </p-message>
        </div>

        <!-- Map container -->
        <div class="map-container mb-3">
          <google-map
            [height]="height"
            [width]="width"
            [options]="mapOptions"
            (mapClick)="onMapClick($event)"
            (mapLoad)="onMapLoad()"
            class="border-round overflow-hidden">
            
            <!-- Marker for selected coordinates -->
            <map-marker
              *ngIf="markerPosition"
              [position]="markerPosition"
              [options]="markerOptions">
            </map-marker>
          </google-map>

          <!-- Map action buttons -->
          <div class="flex gap-2 mt-2" *ngIf="!readonly">
            <p-button
              label="Limpar Seleção"
              icon="pi pi-times"
              severity="secondary"
              size="small"
              [disabled]="!hasValidCoordenadas()"
              (onClick)="clearSelection()">
            </p-button>
            
            <p-button
              label="Centralizar"
              icon="pi pi-map-marker"
              severity="info"
              size="small"
              [disabled]="!hasValidCoordenadas()"
              (onClick)="centerOnLocation()">
            </p-button>

            <p-button
              label="Buscar Endereço"
              icon="pi pi-search"
              severity="help"
              size="small"
              [disabled]="!hasValidCoordenadas() || geocodingLoading()"
              [loading]="geocodingLoading()"
              (onClick)="buscarEnderecoPorCoordenadas()"
              pTooltip="Obter dados do endereço baseado na localização selecionada">
            </p-button>
          </div>
        </div>

        <!-- Address form -->
        <form [formGroup]="enderecoForm" class="grid">
          <div class="col-12 md:col-8">
            <label for="logradouro" class="block text-900 font-medium mb-2">Logradouro *</label>
            <input 
              pInputText 
              id="logradouro"
              formControlName="logradouro"
              placeholder="Ex: Rua das Flores"
              class="w-full"
              [readonly]="readonly">
          </div>

          <div class="col-12 md:col-4">
            <label for="numero" class="block text-900 font-medium mb-2">Número *</label>
            <input 
              pInputText 
              id="numero"
              formControlName="numero"
              placeholder="Ex: 123"
              class="w-full"
              [readonly]="readonly">
          </div>

          <div class="col-12 md:col-6">
            <label for="bairro" class="block text-900 font-medium mb-2">Bairro *</label>
            <input 
              pInputText 
              id="bairro"
              formControlName="bairro"
              placeholder="Ex: Centro"
              class="w-full"
              [readonly]="readonly">
          </div>

          <div class="col-12 md:col-6">
            <label for="cidade" class="block text-900 font-medium mb-2">Cidade *</label>
            <input 
              pInputText 
              id="cidade"
              formControlName="cidade"
              placeholder="Ex: São Paulo"
              class="w-full"
              [readonly]="readonly">
          </div>

          <div class="col-12 md:col-4">
            <label for="uf" class="block text-900 font-medium mb-2">UF *</label>
            <p-select
              id="uf"
              formControlName="uf"
              [options]="ufOptions"
              placeholder="Selecione"
              class="w-full"
              [disabled]="readonly">
            </p-select>
          </div>

          <div class="col-12 md:col-4">
            <label for="cep" class="block text-900 font-medium mb-2">CEP *</label>
            <p-inputMask
              id="cep"
              formControlName="cep"
              mask="99999-999"
              placeholder="00000-000"
              class="w-full"
              [readonly]="readonly">
            </p-inputMask>
          </div>

          <div class="col-12 md:col-4">
            <label for="complemento" class="block text-900 font-medium mb-2">Complemento</label>
            <input 
              pInputText 
              id="complemento"
              formControlName="complemento"
              placeholder="Ex: Apto 101"
              class="w-full"
              [readonly]="readonly">
          </div>

          <!-- Coordinates display -->
          <div class="col-12">
            <div class="surface-100 border-round p-3">
              <div class="flex align-items-center gap-2">
                <i class="pi pi-map-marker text-primary"></i>
                <strong>Coordenadas:</strong>
                <span>{{ getCoordenadasDisplay() }}</span>
                <span *ngIf="validarCoordenadasBrasil()" class="text-green-600">
                  <i class="pi pi-check-circle"></i> Brasil
                </span>
                <span *ngIf="!validarCoordenadasBrasil() && hasValidCoordenadas()" class="text-orange-600">
                  <i class="pi pi-exclamation-triangle"></i> Fora do Brasil
                </span>
              </div>
            </div>
          </div>
        </form>

        <!-- Geocoding status -->
        <div *ngIf="geocodingResult()" class="mt-3">
          <p-message 
            severity="success" 
            text="Endereço obtido automaticamente através das coordenadas"
            styleClass="w-full">
          </p-message>
        </div>

        <!-- Read-only mode indicator -->
        <div *ngIf="readonly" class="mt-2">
          <p-message 
            severity="warn" 
            text="Modo somente leitura - dados não podem ser alterados"
            styleClass="w-full">
          </p-message>
        </div>
      </div>
    </p-card>
  `,
  styleUrls: ['./endereco-map.component.scss']
})
export class EnderecoMapComponent extends BaseComponent implements OnInit, OnDestroy {
  private fb = inject(FormBuilder);
  private geocodingService = inject(GeocodingService);
  private performanceService = inject(PerformanceService);
  private googleMapsManager = new GoogleMapsManager();

  /**
   * Dados iniciais do endereço
   */
  @Input() endereco: Endereco | null = null;

  /**
   * Altura do mapa
   */
  @Input() height: string = '400px';

  /**
   * Largura do mapa
   */
  @Input() width: string = '100%';

  /**
   * Modo somente leitura
   */
  @Input() readonly: boolean = false;

  /**
   * Evento emitido quando o endereço é alterado
   */
  @Output() enderecoChange = new EventEmitter<Endereco>();

  /**
   * Evento emitido quando as coordenadas são selecionadas
   */
  @Output() coordenadasChange = new EventEmitter<Coordenadas>();

  // Signals para estado reativo
  loading = signal(true);
  error = signal<string | null>(null);
  mapLoaded = signal(false);
  geocodingLoading = signal(false);
  geocodingResult = signal<GeocodingResult | null>(null);

  // Configuração do mapa
  mapOptions: google.maps.MapOptions = {
    center: { 
      lat: CONSTANTES_GEOGRAFICAS.BRASIL_CENTRO.latitude!, 
      lng: CONSTANTES_GEOGRAFICAS.BRASIL_CENTRO.longitude! 
    },
    zoom: 4,
    mapTypeId: google.maps.MapTypeId.HYBRID,
    streetViewControl: false,
    fullscreenControl: true,
    mapTypeControl: true,
    zoomControl: true
  };

  // Posição do marcador
  markerPosition: google.maps.LatLngLiteral | null = null;

  // Opções do marcador
  markerOptions: google.maps.MarkerOptions = {
    draggable: false,
    animation: google.maps.Animation.DROP
  };

  // Formulário de endereço
  enderecoForm: FormGroup;

  // Opções de UF
  ufOptions = [
    { label: 'Acre', value: 'AC' },
    { label: 'Alagoas', value: 'AL' },
    { label: 'Amapá', value: 'AP' },
    { label: 'Amazonas', value: 'AM' },
    { label: 'Bahia', value: 'BA' },
    { label: 'Ceará', value: 'CE' },
    { label: 'Distrito Federal', value: 'DF' },
    { label: 'Espírito Santo', value: 'ES' },
    { label: 'Goiás', value: 'GO' },
    { label: 'Maranhão', value: 'MA' },
    { label: 'Mato Grosso', value: 'MT' },
    { label: 'Mato Grosso do Sul', value: 'MS' },
    { label: 'Minas Gerais', value: 'MG' },
    { label: 'Pará', value: 'PA' },
    { label: 'Paraíba', value: 'PB' },
    { label: 'Paraná', value: 'PR' },
    { label: 'Pernambuco', value: 'PE' },
    { label: 'Piauí', value: 'PI' },
    { label: 'Rio de Janeiro', value: 'RJ' },
    { label: 'Rio Grande do Norte', value: 'RN' },
    { label: 'Rio Grande do Sul', value: 'RS' },
    { label: 'Rondônia', value: 'RO' },
    { label: 'Roraima', value: 'RR' },
    { label: 'Santa Catarina', value: 'SC' },
    { label: 'São Paulo', value: 'SP' },
    { label: 'Sergipe', value: 'SE' },
    { label: 'Tocantins', value: 'TO' }
  ];

  constructor() {
    super();
    this.enderecoForm = this.createEnderecoForm();
  }

  ngOnInit(): void {
    this.performanceService.measureComponentLoad('EnderecoMapComponent', () => {
      this.initializeComponent();
    });
  }

  override ngOnDestroy(): void {
    this.googleMapsManager.cleanup();
    this.performanceService.registerCleanupTask(() => {
      this.googleMapsManager.cleanup();
    });
    super.ngOnDestroy();
  }

  /**
   * Inicializa o componente
   */
  private initializeComponent(): void {
    try {
      if (this.endereco) {
        this.loadEnderecoData();
      }
      
      // Observar mudanças no formulário
      this.enderecoForm.valueChanges.subscribe(() => {
        this.emitEnderecoChange();
      });
      
      this.loading.set(false);
      this.error.set(null);
    } catch (err) {
      this.handleError('Erro ao inicializar o componente');
    }
  }

  /**
   * Cria o formulário de endereço
   */
  private createEnderecoForm(): FormGroup {
    return this.fb.group({
      logradouro: ['', [Validators.required, Validators.minLength(3)]],
      numero: ['', Validators.required],
      complemento: [''],
      bairro: ['', [Validators.required, Validators.minLength(2)]],
      cidade: ['', [Validators.required, Validators.minLength(2)]],
      uf: ['', Validators.required],
      cep: ['', [Validators.required, Validators.pattern(/^\d{5}-?\d{3}$/)]],
      latitude: [null as number | null],
      longitude: [null as number | null]
    });
  }

  /**
   * Carrega dados do endereço no formulário
   */
  private loadEnderecoData(): void {
    if (!this.endereco) return;

    this.enderecoForm.patchValue({
      logradouro: this.endereco.logradouro || '',
      numero: this.endereco.numero || '',
      complemento: this.endereco.complemento || '',
      bairro: this.endereco.bairro || '',
      cidade: this.endereco.cidade || '',
      uf: this.endereco.uf || '',
      cep: this.endereco.cep || '',
      latitude: this.endereco.latitude,
      longitude: this.endereco.longitude
    });

    // Configurar marcador se houver coordenadas
    if (this.endereco.latitude && this.endereco.longitude) {
      this.markerPosition = {
        lat: this.endereco.latitude,
        lng: this.endereco.longitude
      };
      
      this.mapOptions = {
        ...this.mapOptions,
        center: this.markerPosition,
        zoom: 15
      };
    }
  }

  /**
   * Manipula cliques no mapa
   */
  onMapClick(event: google.maps.MapMouseEvent): void {
    if (this.readonly) return;

    const clickedLocation = event.latLng;
    if (!clickedLocation) return;

    try {
      const lat = clickedLocation.lat();
      const lng = clickedLocation.lng();

      // Atualizar posição do marcador
      this.markerPosition = { lat, lng };

      // Atualizar formulário
      this.enderecoForm.patchValue({
        latitude: lat,
        longitude: lng
      });

      // Emitir mudança de coordenadas
      this.coordenadasChange.emit({ latitude: lat, longitude: lng });

      // Buscar endereço automaticamente se estiver no Brasil
      if (this.geocodingService.validarCoordenadasBrasil(lat, lng)) {
        this.buscarEnderecoPorCoordenadas();
      }

      this.error.set(null);
    } catch (err) {
      this.handleError('Erro ao capturar coordenadas');
    }
  }

  /**
   * Manipula carregamento do mapa
   */
  onMapLoad(): void {
    this.mapLoaded.set(true);
    this.loading.set(false);
  }

  /**
   * Busca endereço por coordenadas usando geocodificação reversa
   */
  buscarEnderecoPorCoordenadas(): void {
    if (!this.markerPosition || this.geocodingLoading()) return;

    this.geocodingLoading.set(true);
    this.geocodingResult.set(null);

    this.geocodingService.obterEnderecoPorCoordenadas(
      this.markerPosition.lat,
      this.markerPosition.lng
    ).subscribe({
      next: (result) => {
        this.geocodingLoading.set(false);
        
        if (result) {
          this.geocodingResult.set(result);
          this.preencherFormularioComResultado(result);
        } else {
          this.error.set('Não foi possível obter o endereço para esta localização');
        }
      },
      error: (error) => {
        this.geocodingLoading.set(false);
        this.handleError('Erro ao buscar endereço: ' + error.message);
      }
    });
  }

  /**
   * Preenche o formulário com o resultado da geocodificação
   */
  private preencherFormularioComResultado(result: GeocodingResult): void {
    this.enderecoForm.patchValue({
      logradouro: result.logradouro || this.enderecoForm.get('logradouro')?.value,
      numero: result.numero || this.enderecoForm.get('numero')?.value,
      bairro: result.bairro || this.enderecoForm.get('bairro')?.value,
      cidade: result.cidade || this.enderecoForm.get('cidade')?.value,
      uf: result.uf || this.enderecoForm.get('uf')?.value,
      cep: result.cep || this.enderecoForm.get('cep')?.value
    });

    // Atualizar o endereço com os IDs resolvidos se disponíveis
    if (result.ufId || result.municipioId) {
      const enderecoAtualizado = {
        ...this.endereco,
        ufId: result.ufId,
        municipioId: result.municipioId,
        ufNome: result.cidade, // Nome da cidade para compatibilidade
        municipioNome: result.cidade,
        ufCodigo: result.uf
      };
      
      console.log('🆔 IDs geográficos resolvidos:', {
        ufId: result.ufId,
        municipioId: result.municipioId,
        uf: result.uf,
        cidade: result.cidade
      });
    }
  }

  /**
   * Limpa a seleção
   */
  clearSelection(): void {
    if (this.readonly) return;

    this.markerPosition = null;
    this.geocodingResult.set(null);
    this.enderecoForm.reset();
    this.error.set(null);
  }

  /**
   * Centraliza o mapa na localização
   */
  centerOnLocation(): void {
    if (this.markerPosition) {
      this.mapOptions = {
        ...this.mapOptions,
        center: this.markerPosition,
        zoom: 15
      };
    }
  }

  /**
   * Verifica se tem coordenadas válidas
   */
  hasValidCoordenadas(): boolean {
    return this.markerPosition !== null;
  }

  /**
   * Valida se as coordenadas estão no Brasil
   */
  validarCoordenadasBrasil(): boolean {
    if (!this.markerPosition) return false;
    
    return this.geocodingService.validarCoordenadasBrasil(
      this.markerPosition.lat,
      this.markerPosition.lng
    );
  }

  /**
   * Obtém display das coordenadas
   */
  getCoordenadasDisplay(): string {
    if (!this.markerPosition) {
      return 'Nenhuma localização selecionada';
    }

    return this.geocodingService.formatarCoordenadas(
      this.markerPosition.lat,
      this.markerPosition.lng
    );
  }

  /**
   * Emite mudança no endereço
   */
  private emitEnderecoChange(): void {
    const formValue = this.enderecoForm.value;
    const geocodingResult = this.geocodingResult();
    
    const endereco: Endereco = {
      id: this.endereco?.id || 0,
      logradouro: formValue.logradouro || '',
      numero: formValue.numero || '',
      complemento: formValue.complemento || '',
      bairro: formValue.bairro || '',
      cidade: formValue.cidade || '',
      uf: formValue.uf || '',
      cep: formValue.cep || '',
      latitude: formValue.latitude,
      longitude: formValue.longitude,
      // Incluir IDs resolvidos se disponíveis
      ufId: geocodingResult?.ufId || this.endereco?.ufId,
      municipioId: geocodingResult?.municipioId || this.endereco?.municipioId,
      ufNome: formValue.cidade || this.endereco?.ufNome,
      ufCodigo: formValue.uf || this.endereco?.ufCodigo,
      municipioNome: formValue.cidade || this.endereco?.municipioNome,
      ativo: this.endereco?.ativo ?? true,
      dataCriacao: this.endereco?.dataCriacao || new Date(),
      dataAtualizacao: new Date()
    };

    this.enderecoChange.emit(endereco);
  }

  /**
   * Manipula erros
   */
  private handleError(message: string): void {
    console.error(message);
    this.error.set(message);
    this.loading.set(false);
    this.geocodingLoading.set(false);
  }
}