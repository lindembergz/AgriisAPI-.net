import { ComponentFixture, TestBed } from '@angular/core/testing';
import { GoogleMapsModule } from '@angular/google-maps';
import { CoordenadasMapComponent, Coordenadas } from './coordenadas-map.component';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Message } from 'primeng/message';

describe('CoordenadasMapComponent', () => {
  let component: CoordenadasMapComponent;
  let fixture: ComponentFixture<CoordenadasMapComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        CoordenadasMapComponent,
        GoogleMapsModule,
        Button,
        Card,
        Message
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CoordenadasMapComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with default values', () => {
    expect(component.coordenadas).toBeNull();
    expect(component.height).toBe('400px');
    expect(component.width).toBe('100%');
    expect(component.readonly).toBeFalse();
    expect(component.loading()).toBeTruthy();
    expect(component.error()).toBeNull();
    expect(component.mapLoaded()).toBeFalse();
  });

  it('should set marker position when coordinates are provided', () => {
    const coordenadas: Coordenadas = { latitude: -14.235, longitude: -51.9253 };
    component.coordenadas = coordenadas;
    component.ngOnInit();

    expect(component.markerPosition).toEqual({
      lat: coordenadas.latitude!,
      lng: coordenadas.longitude!
    });
  });

  it('should emit coordinates when map is clicked', () => {
    spyOn(component.coordenadasChange, 'emit');
    
    const mockEvent = {
      latLng: {
        lat: () => -14.235,
        lng: () => -51.9253
      }
    } as google.maps.MapMouseEvent;

    component.onMapClick(mockEvent);

    expect(component.coordenadasChange.emit).toHaveBeenCalledWith({
      latitude: -14.235,
      longitude: -51.9253
    });
  });

  it('should not handle click events in readonly mode', () => {
    component.readonly = true;
    spyOn(component.coordenadasChange, 'emit');
    
    const mockEvent = {
      latLng: {
        lat: () => -14.235,
        lng: () => -51.9253
      }
    } as google.maps.MapMouseEvent;

    component.onMapClick(mockEvent);

    expect(component.coordenadasChange.emit).not.toHaveBeenCalled();
  });

  it('should clear coordinates', () => {
    component.markerPosition = { lat: -14.235, lng: -51.9253 };
    spyOn(component.coordenadasClear, 'emit');

    component.clearCoordenadas();

    expect(component.markerPosition).toBeNull();
    expect(component.coordenadasClear.emit).toHaveBeenCalled();
  });

  it('should not clear coordinates in readonly mode', () => {
    component.readonly = true;
    component.markerPosition = { lat: -14.235, lng: -51.9253 };
    spyOn(component.coordenadasClear, 'emit');

    component.clearCoordenadas();

    expect(component.markerPosition).not.toBeNull();
    expect(component.coordenadasClear.emit).not.toHaveBeenCalled();
  });

  it('should return correct coordinate display', () => {
    expect(component.getCoordenadasDisplay()).toBe('Nenhuma coordenada selecionada');

    component.markerPosition = { lat: -14.235123, lng: -51.925345 };
    expect(component.getCoordenadasDisplay()).toBe('Lat: -14.235123, Lng: -51.925345');
  });

  it('should validate coordinates correctly', () => {
    expect(component.hasValidCoordenadas()).toBeFalse();

    component.markerPosition = { lat: -14.235, lng: -51.9253 };
    expect(component.hasValidCoordenadas()).toBeTruthy();
  });
});