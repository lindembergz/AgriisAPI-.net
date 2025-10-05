import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';

// PrimeNG imports
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { DividerModule } from 'primeng/divider';

// Components and interfaces
import { EnderecoMapComponent } from './endereco-map.component';
import { Endereco } from '../models/endereco.model';
import { Coordenadas } from '../interfaces/coordenadas.interface';

/**
 * Componente de exemplo para demonstrar o uso do EnderecoMapComponent
 */
@Component({
  selector: 'app-endereco-map-example',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    CardModule,
    ButtonModule,
    MessageModule,
    DividerModule,
    EnderecoMapComponent
  ],
  template: `
    <div class="grid">
      <!-- Exemplo 1: Modo de edição -->
      <div class="col-12 lg:col-6">
        <p-card header="Exemplo 1: Seleção de Endereço" styleClass="h-full">
          <p class="mb-3">
            Clique no mapa para selecionar uma localização e obter automaticamente 
            os dados do endereço através da geocodificação reversa.
          </p>
          
          <app-endereco-map
            [endereco]="enderecoExemplo1()"
            [readonly]="false"
            height="350px"
            (enderecoChange)="onEnderecoChange($event, 1)"
            (coordenadasChange)="onCoordenadasChange($event, 1)">
          </app-endereco-map>
          
          <p-divider></p-divider>
          
          <div class="mt-3">
            <h5>Dados Capturados:</h5>
            <pre class="surface-100 p-3 border-round text-sm">{{ formatarEndereco(enderecoExemplo1()) }}</pre>
          </div>
          
          <div class="flex gap-2 mt-3">
            <p-button
              label="Limpar"
              icon="pi pi-times"
              severity="secondary"
              size="small"
              (onClick)="limparEndereco(1)">
            </p-button>
            
            <p-button
              label="Exemplo SP"
              icon="pi pi-map-marker"
              severity="info"
              size="small"
              (onClick)="carregarExemploSP()">
            </p-button>
          </div>
        </p-card>
      </div>

      <!-- Exemplo 2: Modo somente leitura -->
      <div class="col-12 lg:col-6">
        <p-card header="Exemplo 2: Visualização (Somente Leitura)" styleClass="h-full">
          <p class="mb-3">
            Exemplo de visualização de um endereço existente em modo somente leitura.
          </p>
          
          <app-endereco-map
            [endereco]="enderecoExemplo2()"
            [readonly]="true"
            height="350px">
          </app-endereco-map>
          
          <p-divider></p-divider>
          
          <div class="mt-3">
            <h5>Endereço Fixo:</h5>
            <pre class="surface-100 p-3 border-round text-sm">{{ formatarEndereco(enderecoExemplo2()) }}</pre>
          </div>
          
          <div class="flex gap-2 mt-3">
            <p-button
              label="Alterar para Brasília"
              icon="pi pi-map"
              severity="help"
              size="small"
              (onClick)="carregarExemploBrasilia()">
            </p-button>
            
            <p-button
              label="Alterar para Rio"
              icon="pi pi-map"
              severity="success"
              size="small"
              (onClick)="carregarExemploRio()">
            </p-button>
          </div>
        </p-card>
      </div>

      <!-- Log de eventos -->
      <div class="col-12">
        <p-card header="Log de Eventos" styleClass="mt-3">
          <p class="mb-3">
            Acompanhe os eventos disparados pelos componentes:
          </p>
          
          <div class="surface-50 p-3 border-round" style="max-height: 200px; overflow-y: auto;">
            <div *ngFor="let evento of eventos(); let i = index" class="mb-2">
              <span class="text-sm text-500">{{ evento.timestamp }}</span>
              <span class="ml-2 font-medium">{{ evento.tipo }}</span>
              <span class="ml-2">{{ evento.descricao }}</span>
            </div>
            
            <div *ngIf="eventos().length === 0" class="text-center text-500">
              Nenhum evento registrado ainda...
            </div>
          </div>
          
          <div class="flex gap-2 mt-3">
            <p-button
              label="Limpar Log"
              icon="pi pi-trash"
              severity="secondary"
              size="small"
              (onClick)="limparEventos()">
            </p-button>
          </div>
        </p-card>
      </div>
    </div>
  `,
  styles: [`
    pre {
      font-size: 0.875rem;
      line-height: 1.4;
      white-space: pre-wrap;
      word-wrap: break-word;
    }
    
    .event-log {
      font-family: 'Courier New', monospace;
      font-size: 0.8rem;
    }
  `]
})
export class EnderecoMapExampleComponent {
  // Signals para estado reativo
  enderecoExemplo1 = signal<Endereco | null>(null);
  enderecoExemplo2 = signal<Endereco | null>(this.criarEnderecoExemplo());
  eventos = signal<Array<{ timestamp: string; tipo: string; descricao: string }>>([]);

  /**
   * Manipula mudanças no endereço
   */
  onEnderecoChange(endereco: Endereco, exemplo: number): void {
    if (exemplo === 1) {
      this.enderecoExemplo1.set(endereco);
    }
    
    this.adicionarEvento('ENDERECO_CHANGE', `Exemplo ${exemplo}: Endereço atualizado`);
  }

  /**
   * Manipula mudanças nas coordenadas
   */
  onCoordenadasChange(coordenadas: Coordenadas, exemplo: number): void {
    const desc = coordenadas.latitude && coordenadas.longitude 
      ? `Lat: ${coordenadas.latitude.toFixed(6)}, Lng: ${coordenadas.longitude.toFixed(6)}`
      : 'Coordenadas removidas';
      
    this.adicionarEvento('COORDENADAS_CHANGE', `Exemplo ${exemplo}: ${desc}`);
  }

  /**
   * Limpa o endereço do exemplo
   */
  limparEndereco(exemplo: number): void {
    if (exemplo === 1) {
      this.enderecoExemplo1.set(null);
    }
    
    this.adicionarEvento('CLEAR', `Exemplo ${exemplo}: Endereço limpo`);
  }

  /**
   * Carrega exemplo de São Paulo
   */
  carregarExemploSP(): void {
    const endereco: Endereco = {
      id: 1,
      logradouro: 'Avenida Paulista',
      numero: '1000',
      complemento: '',
      bairro: 'Bela Vista',
      cidade: 'São Paulo',
      uf: 'SP',
      cep: '01310-100',
      latitude: -23.5613,
      longitude: -46.6565,
      ativo: true,
      dataCriacao: new Date(),
      dataAtualizacao: new Date()
    };
    
    this.enderecoExemplo1.set(endereco);
    this.adicionarEvento('LOAD_EXAMPLE', 'Carregado exemplo de São Paulo');
  }

  /**
   * Carrega exemplo de Brasília
   */
  carregarExemploBrasilia(): void {
    const endereco: Endereco = {
      id: 2,
      logradouro: 'Esplanada dos Ministérios',
      numero: 'S/N',
      complemento: 'Palácio do Planalto',
      bairro: 'Zona Cívico-Administrativa',
      cidade: 'Brasília',
      uf: 'DF',
      cep: '70150-900',
      latitude: -15.7998,
      longitude: -47.8645,
      ativo: true,
      dataCriacao: new Date(),
      dataAtualizacao: new Date()
    };
    
    this.enderecoExemplo2.set(endereco);
    this.adicionarEvento('LOAD_EXAMPLE', 'Carregado exemplo de Brasília');
  }

  /**
   * Carrega exemplo do Rio de Janeiro
   */
  carregarExemploRio(): void {
    const endereco: Endereco = {
      id: 3,
      logradouro: 'Avenida Atlântica',
      numero: '1702',
      complemento: '',
      bairro: 'Copacabana',
      cidade: 'Rio de Janeiro',
      uf: 'RJ',
      cep: '22021-001',
      latitude: -22.9707,
      longitude: -43.1823,
      ativo: true,
      dataCriacao: new Date(),
      dataAtualizacao: new Date()
    };
    
    this.enderecoExemplo2.set(endereco);
    this.adicionarEvento('LOAD_EXAMPLE', 'Carregado exemplo do Rio de Janeiro');
  }

  /**
   * Cria um endereço de exemplo inicial
   */
  private criarEnderecoExemplo(): Endereco {
    return {
      id: 0,
      logradouro: 'Rua das Flores',
      numero: '123',
      complemento: 'Apto 45',
      bairro: 'Centro',
      cidade: 'São Paulo',
      uf: 'SP',
      cep: '01234-567',
      latitude: -23.5505,
      longitude: -46.6333,
      ativo: true,
      dataCriacao: new Date(),
      dataAtualizacao: new Date()
    };
  }

  /**
   * Formata endereço para exibição
   */
  formatarEndereco(endereco: Endereco | null): string {
    if (!endereco) {
      return 'Nenhum endereço selecionado';
    }

    return JSON.stringify({
      logradouro: endereco.logradouro,
      numero: endereco.numero,
      complemento: endereco.complemento,
      bairro: endereco.bairro,
      cidade: endereco.cidade,
      uf: endereco.uf,
      cep: endereco.cep,
      coordenadas: endereco.latitude && endereco.longitude 
        ? `${endereco.latitude.toFixed(6)}, ${endereco.longitude.toFixed(6)}`
        : 'Não definidas'
    }, null, 2);
  }

  /**
   * Adiciona evento ao log
   */
  private adicionarEvento(tipo: string, descricao: string): void {
    const evento = {
      timestamp: new Date().toLocaleTimeString(),
      tipo,
      descricao
    };
    
    const eventosAtuais = this.eventos();
    this.eventos.set([evento, ...eventosAtuais.slice(0, 19)]); // Manter apenas os últimos 20
  }

  /**
   * Limpa o log de eventos
   */
  limparEventos(): void {
    this.eventos.set([]);
  }
}