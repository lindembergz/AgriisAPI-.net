import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GoogleMapsModule } from '@angular/google-maps';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Message } from 'primeng/message';
import { BaseComponent, GoogleMapsManager } from '../utils/memory-management.util';
import { PerformanceService } from '../../core/services/performance.service';

/**
 * Interface for coordinate data
 */
export interface Coordenadas {
  latitude: number | null;
  longitude: number | null;
}

/**
 * Reusable component for Google Maps coordinate selection
 * Can be used by both Produtores and Fornecedores modules
 */
@Component({
  selector: 'app-coordenadas-map',
  standalone: true,
  imports: [
    CommonModule,
    GoogleMapsModule,
    Button,
    Card,
    Message
  ],
  templateUrl: './coordenadas-map.component.html',
  styleUrls: ['./coordenadas-map.component.scss']
})
export class CoordenadasMapComponent extends BaseComponent implements OnInit {
  private performanceService = inject(PerformanceService);
  private googleMapsManager = new GoogleMapsManager();
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
   * Event emitted when coordinates are selected (alias for compatibility)
   */
  @Output() coordinatesSelected = new EventEmitter<Coordenadas>();

  /**
   * Event emitted when coordinates are cleared
   */
  @Output() coordenadasClear = new EventEmitter<void>();

  // Signals for reactive state management
  loading = signal(true);
  error = signal<string | null>(null);
  mapLoaded = signal(false);

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
    this.performanceService.measureComponentLoad('CoordenadasMapComponent', () => {
      this.initializeMap();
    });
  }

  override ngOnDestroy(): void {
    // Clean up Google Maps resources
    this.googleMapsManager.cleanup();
    
    // Register cleanup task with performance service
    this.performanceService.registerCleanupTask(() => {
      this.googleMapsManager.cleanup();
    });
    
    // Call parent cleanup
    super.ngOnDestroy();
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
      this.coordinatesSelected.emit(coordenadas);
      this.error.set(null);
    } catch (err) {
      this.handleError('Erro ao capturar coordenadas');
    }
  }

  /**
   * Handle map load event
   */
  onMapLoad(): void {
    this.mapLoaded.set(true);
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