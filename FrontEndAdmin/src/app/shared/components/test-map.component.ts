import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GoogleMapsModule } from '@angular/google-maps';

@Component({
  selector: 'app-test-map',
  standalone: true,
  imports: [CommonModule, GoogleMapsModule],
  template: `
    <div style="padding: 20px;">
      <h2>Teste Google Maps</h2>
      <google-map
        height="400px"
        width="100%"
        [options]="mapOptions">
      </google-map>
    </div>
  `
})
export class TestMapComponent {
  mapOptions: google.maps.MapOptions = {
    center: { lat: -14.235, lng: -51.9253 },
    zoom: 4
  };
}