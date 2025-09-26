import { Component, Input, Output, EventEmitter, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GoogleMapsModule } from '@angular/google-maps';

/**
 * Interface for coordinate data
 */
export interface Coordenadas {
  latitude: number | null;
  longitude: number | null;
}

/**
 * Simple Google Maps component for coordinate selection
 */
@Component({
  selector: 'app-simple-map',
  standalone: true,
  imports: [
    CommonModule,
    GoogleMapsModule
  ],
  template: `
    <div class="map-container">
      <h3>Coordenadas Geográficas</h3>
      
      <!-- Loading state -->
      <div *ngIf="loading()" class="text-center p-4">
        <p>Carregando mapa...</p>
      </div>

      <!-- Error state -->
      <div *ngIf="error()" class="error-message">
        {{ error() }}
      </div>

      <!-- Map container -->
      <div *ngIf="!loading() && !error()" class="map-wrapper">
        <!-- Instructions -->
        <div class="instructions">
          <p *ngIf="!readonly">Clique no mapa para selecionar as coordenadas geográficas</p>
          
          <!-- Current coordinates display -->
          <div class="coordinate-display">
            <strong>{{ getCoordenadasDisplay() }}</strong>
          </div>
        </div>

        <!-- Google Map -->
        <google-map
          [height]="height"
          [width]="width"
          [options]="mapOptions"
          (mapClick)="onMapClick($event)"
          (mapLoad)="onMapLoad()"
          class="google-map">
          
          <!-- Marker for selected coordinates -->
          <map-marker
            *ngIf="markerPosition"
            [position]="markerPosition"
            [options]="markerOptions">
          </map-marker>
        </google-map>

        <!-- Action buttons -->
        <div class="actions" *ngIf="!readonly">
          <button
            type="button"
            [disabled]="!hasValidCoordenadas()"
            (click)="clearCoordenadas()"
            class="btn btn-secondary">
            Limpar Coordenadas
          </button>
          
          <button
            type="button"
            [disabled]="!hasValidCoordenadas()"
            (click)="centerOnCoordenadas()"
            class="btn btn-primary">
            Centralizar no Ponto
          </button>
        </div>

        <!-- Read-only mode indicator -->
        <div *ngIf="readonly" class="readonly-indicator">
          <p>Modo somente leitura - coordenadas não podem ser alteradas</p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .map-container {
      padding: 1rem;
      border: 1px solid #ddd;
      border-radius: 8px;
      background: white;
    }

    .coordinate-display {
      font-family: 'Courier New', monospace;
      font-size: 0.9rem;
      text-align: center;
      background-color: #f8f9fa;
      border: 1px solid #dee2e6;
      padding: 0.5rem;
      margin: 0.5rem 0;
      border-radius: 4px;
    }

    .google-map {
      display: block;
      border: 2px solid #dee2e6;
      border-radius: 4px;
      margin: 1rem 0;
    }

    .actions {
      display: flex;
      gap: 0.5rem;
      margin-top: 1rem;
    }

    .btn {
      padding: 0.5rem 1rem;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      font-size: 0.875rem;
    }

    .btn:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    .btn-secondary {
      background-color: #dc3545;
      color: white;
    }

    .btn-secondary:hover:not(:disabled) {
      background-color: #c82333;
    }

    .btn-primary {
      background-color: #007bff;
      color: white;
    }

    .btn-primary:hover:not(:disabled) {
      background-color: #0056b3;
    }

    .error-message {
      color: #dc3545;
      background-color: #f8d7da;
      border: 1px solid #f5c6cb;
      padding: 0.75rem;
      border-radius: 4px;
      margin: 1rem 0;
    }

    .readonly-indicator {
      background-color: #fff3cd;
      border: 1px solid #ffeaa7;
      color: #856404;
      padding: 0.75rem;
      border-radius: 4px;
      margin-top: 1rem;
    }

    .instructions {
      margin-bottom: 1rem;
    }

    .text-center {
      text-align: center;
    }

    .p-4 {
      padding: 1rem;
    }
  `]
})
export class SimpleMapComponent implements OnInit {
  /**
   * Initial coordinates to display on the map
   */
  @Input() coordenadas: Coordenadas | null = null;

  /**
   * Map height in pixels
   */
  @Input() height: string = '400px';

  /**
   * Map width in pixels or percentage
   */
  @Input() width: string = '100%';

  /**
   * Whether the component is in read-only mode
   */
  @Input() readonly: boolean = false;

  /**
   * Event emitted when coordinates are selected
   */
  @Output() coordenadasChange = new EventEmitter<Coordenadas>();

  /**
   * Event emitted when coordinates are cleared
   */
  @Output() coordenadasClear = new EventEmitter<void>();

  // Signals for reactive state management
  loading = signal(true);
  error = signal<string | null>(null);

  // Map configuration
  mapOptions: google.maps.MapOptions = {
    center: { lat: -14.235, lng: -51.9253 }, // Center of Brazil
    zoom: 4,
    mapTypeId: google.maps.MapTypeId.HYBRID,
    streetViewControl: false,
    fullscreenControl: true,
    mapTypeControl: true,
    zoomControl: true
  };

  // Current marker position
  markerPosition: google.maps.LatLngLiteral | null = null;

  // Marker options
  markerOptions: google.maps.MarkerOptions = {
    draggable: false,
    animation: google.maps.Animation.DROP
  };

  ngOnInit(): void {
    this.initializeMap();
  }

  /**
   * Initialize the map with existing coordinates if available
   */
  private initializeMap(): void {
    try {
      if (this.coordenadas?.latitude && this.coordenadas?.longitude) {
        this.markerPosition = {
          lat: this.coordenadas.latitude,
          lng: this.coordenadas.longitude
        };
        
        // Center map on existing coordinates
        this.mapOptions = {
          ...this.mapOptions,
          center: this.markerPosition,
          zoom: 15
        };
      }
      
      this.loading.set(false);
      this.error.set(null);
    } catch (err) {
      this.handleError('Erro ao inicializar o mapa');
    }
  }

  /**
   * Handle map click events to capture coordinates
   */
  onMapClick(event: google.maps.MapMouseEvent): void {
    if (this.readonly) {
      return;
    }

    const clickedLocation = event.latLng;
    if (!clickedLocation) {
      return;
    }

    try {
      const lat = clickedLocation.lat();
      const lng = clickedLocation.lng();

      // Update marker position
      this.markerPosition = { lat, lng };

      // Emit coordinate change event
      const coordenadas: Coordenadas = {
        latitude: lat,
        longitude: lng
      };

      this.coordenadasChange.emit(coordenadas);
      this.error.set(null);
    } catch (err) {
      this.handleError('Erro ao capturar coordenadas');
    }
  }

  /**
   * Handle map load event
   */
  onMapLoad(): void {
    this.loading.set(false);
  }

  /**
   * Clear selected coordinates
   */
  clearCoordenadas(): void {
    if (this.readonly) {
      return;
    }

    this.markerPosition = null;
    this.coordenadasClear.emit();
    this.error.set(null);
  }

  /**
   * Center map on current coordinates
   */
  centerOnCoordenadas(): void {
    if (this.markerPosition) {
      this.mapOptions = {
        ...this.mapOptions,
        center: this.markerPosition,
        zoom: 15
      };
    }
  }

  /**
   * Get formatted coordinate display
   */
  getCoordenadasDisplay(): string {
    if (!this.markerPosition) {
      return 'Nenhuma coordenada selecionada';
    }

    const lat = this.markerPosition.lat.toFixed(6);
    const lng = this.markerPosition.lng.toFixed(6);
    return `Lat: ${lat}, Lng: ${lng}`;
  }

  /**
   * Handle errors and display appropriate messages
   */
  private handleError(message: string): void {
    console.error(message);
    this.error.set(message);
    this.loading.set(false);
  }

  /**
   * Check if coordinates are valid
   */
  hasValidCoordenadas(): boolean {
    return this.markerPosition !== null;
  }
}