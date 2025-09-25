import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SimpleMapComponent, Coordenadas } from './simple-map.component';

/**
 * Simple test component to verify CoordenadasMapComponent works
 */
@Component({
  selector: 'app-coordenadas-map-test',
  standalone: true,
  imports: [
    CommonModule,
    SimpleMapComponent
  ],
  template: `
    <div class="p-4">
      <h2>Teste do Componente de Coordenadas</h2>
      
      <div class="mb-4">
        <p><strong>Coordenadas Selecionadas:</strong></p>
        <p *ngIf="coordenadas(); else noCoordenadas">
          Latitude: {{ coordenadas()?.latitude }}<br>
          Longitude: {{ coordenadas()?.longitude }}
        </p>
        <ng-template #noCoordenadas>
          <p>Nenhuma coordenada selecionada</p>
        </ng-template>
      </div>

      <app-simple-map
        [coordenadas]="coordenadas()"
        [readonly]="false"
        height="400px"
        (coordenadasChange)="onCoordenadasChange($event)"
        (coordenadasClear)="onCoordenadasClear()">
      </app-simple-map>
    </div>
  `,
  styles: [`
    .p-4 {
      padding: 1rem;
    }
    .mb-4 {
      margin-bottom: 1rem;
    }
  `]
})
export class CoordenadasMapTestComponent {
  coordenadas = signal<Coordenadas | null>(null);

  onCoordenadasChange(coordenadas: Coordenadas): void {
    this.coordenadas.set(coordenadas);
    console.log('Coordenadas alteradas:', coordenadas);
  }

  onCoordenadasClear(): void {
    this.coordenadas.set(null);
    console.log('Coordenadas removidas');
  }
}