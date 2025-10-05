import { Injectable, inject } from '@angular/core';
import { Observable, from, of, throwError } from 'rxjs';
import { map, catchError, timeout, switchMap } from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Coordenadas, CONSTANTES_GEOGRAFICAS } from '../interfaces/coordenadas.interface';

/**
 * Interface para resultado de geocodificação reversa
 */
export interface GeocodingResult {
  logradouro: string;
  numero: string;
  bairro: string;
  cidade: string;
  uf: string;
  cep: string;
  pais: string;
  enderecoCompleto: string;
  latitude: number;
  longitude: number;
  // IDs resolvidos do banco de dados
  ufId?: number;
  municipioId?: number;
}

/**
 * Interface para componentes de endereço do Google Maps
 */
interface AddressComponent {
  long_name: string;
  short_name: string;
  types: string[];
}

/**
 * Serviço para geocodificação usando Google Maps API
 */
@Injectable({
  providedIn: 'root'
})
export class GeocodingService {
  private geocoder: google.maps.Geocoder | null = null;
  private readonly TIMEOUT_MS = 10000; // 10 segundos
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api';

  constructor() {
    this.initializeGeocoder();
  }

  /**
   * Inicializa o geocoder do Google Maps
   */
  private initializeGeocoder(): void {
    if (typeof google !== 'undefined' && google.maps && google.maps.Geocoder) {
      this.geocoder = new google.maps.Geocoder();
    }
  }

  /**
   * Obtém informações de endereço através de coordenadas (geocodificação reversa)
   */
  obterEnderecoPorCoordenadas(latitude: number, longitude: number): Observable<GeocodingResult | null> {
    if (!this.geocoder) {
      this.initializeGeocoder();
      if (!this.geocoder) {
        return throwError(() => new Error('Google Maps Geocoder não está disponível'));
      }
    }

    if (!this.validarCoordenadas(latitude, longitude)) {
      return throwError(() => new Error('Coordenadas inválidas'));
    }

    const latlng = { lat: latitude, lng: longitude };

    return from(
      new Promise<google.maps.GeocoderResult[]>((resolve, reject) => {
        this.geocoder!.geocode(
          { 
            location: latlng,
            region: 'BR' // Priorizar resultados do Brasil
          },
          (results, status) => {
            if (status === 'OK' && results && results.length > 0) {
              resolve(results);
            } else {
              reject(new Error(`Geocoding falhou: ${status}`));
            }
          }
        );
      })
    ).pipe(
      timeout(this.TIMEOUT_MS),
      map(results => this.processarResultadoGeocodificacao(results, latitude, longitude)),
      switchMap(result => {
        if (result && result.uf && result.cidade) {
          return this.resolverIdsGeograficos(result);
        }
        return of(result);
      }),
      catchError(error => {
        console.error('Erro na geocodificação reversa:', error);
        return of(null);
      })
    );
  }

  /**
   * Obtém coordenadas através de um endereço (geocodificação direta)
   */
  obterCoordenadasPorEndereco(endereco: string): Observable<GeocodingResult | null> {
    if (!this.geocoder) {
      this.initializeGeocoder();
      if (!this.geocoder) {
        return throwError(() => new Error('Google Maps Geocoder não está disponível'));
      }
    }

    if (!endereco || endereco.trim().length < 5) {
      return throwError(() => new Error('Endereço deve ter pelo menos 5 caracteres'));
    }

    return from(
      new Promise<google.maps.GeocoderResult[]>((resolve, reject) => {
        this.geocoder!.geocode(
          { 
            address: endereco,
            region: 'BR', // Priorizar resultados do Brasil
            componentRestrictions: { country: 'BR' }
          },
          (results, status) => {
            if (status === 'OK' && results && results.length > 0) {
              resolve(results);
            } else {
              reject(new Error(`Geocoding falhou: ${status}`));
            }
          }
        );
      })
    ).pipe(
      timeout(this.TIMEOUT_MS),
      map(results => {
        const location = results[0].geometry.location;
        return this.processarResultadoGeocodificacao(results, location.lat(), location.lng());
      }),
      catchError(error => {
        console.error('Erro na geocodificação direta:', error);
        return of(null);
      })
    );
  }

  /**
   * Processa o resultado da geocodificação e extrai informações do endereço
   */
  private processarResultadoGeocodificacao(
    results: google.maps.GeocoderResult[], 
    latitude: number, 
    longitude: number
  ): GeocodingResult | null {
    if (!results || results.length === 0) {
      return null;
    }

    // Procurar pelo resultado mais específico (street_address ou premise)
    let melhorResultado = results.find(result => 
      result.types.includes('street_address') || 
      result.types.includes('premise')
    ) || results[0];

    const components = melhorResultado.address_components;
    const enderecoCompleto = melhorResultado.formatted_address;

    // Extrair componentes do endereço
    const logradouro = this.extrairComponente(components, ['route']) || '';
    const numero = this.extrairComponente(components, ['street_number']) || 'S/N';
    const bairro = this.extrairComponente(components, ['sublocality', 'sublocality_level_1', 'neighborhood']) || '';
    const cidade = this.extrairComponente(components, ['locality', 'administrative_area_level_2']) || '';
    const uf = this.extrairComponente(components, ['administrative_area_level_1'], true) || '';
    const cep = this.extrairComponente(components, ['postal_code']) || '';
    const pais = this.extrairComponente(components, ['country']) || 'Brasil';

    return {
      logradouro,
      numero,
      bairro,
      cidade,
      uf,
      cep,
      pais,
      enderecoCompleto,
      latitude,
      longitude
    };
  }

  /**
   * Extrai um componente específico do endereço
   */
  private extrairComponente(
    components: AddressComponent[], 
    types: string[], 
    shortName: boolean = false
  ): string {
    for (const component of components) {
      if (types.some(type => component.types.includes(type))) {
        return shortName ? component.short_name : component.long_name;
      }
    }
    return '';
  }

  /**
   * Valida se as coordenadas estão dentro do território brasileiro
   */
  validarCoordenadasBrasil(latitude: number, longitude: number): boolean {
    const bounds = CONSTANTES_GEOGRAFICAS.BRASIL_BOUNDS;

    return latitude >= bounds.sul && 
           latitude <= bounds.norte &&
           longitude >= bounds.oeste && 
           longitude <= bounds.leste;
  }

  /**
   * Valida coordenadas básicas
   */
  private validarCoordenadas(latitude: number, longitude: number): boolean {
    return latitude >= -90 && latitude <= 90 && 
           longitude >= -180 && longitude <= 180;
  }

  /**
   * Formata coordenadas para exibição
   */
  formatarCoordenadas(latitude: number, longitude: number, precisao: number = 6): string {
    return `${latitude.toFixed(precisao)}, ${longitude.toFixed(precisao)}`;
  }

  /**
   * Calcula a distância entre duas coordenadas em quilômetros
   */
  calcularDistancia(
    lat1: number, lng1: number, 
    lat2: number, lng2: number
  ): number {
    const R = CONSTANTES_GEOGRAFICAS.RAIO_TERRA_KM;
    const dLat = this.toRadians(lat2 - lat1);
    const dLng = this.toRadians(lng2 - lng1);
    
    const a = Math.sin(dLat / 2) * Math.sin(dLat / 2) +
              Math.cos(this.toRadians(lat1)) * Math.cos(this.toRadians(lat2)) *
              Math.sin(dLng / 2) * Math.sin(dLng / 2);
    
    const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
    return R * c;
  }

  /**
   * Converte graus para radianos
   */
  private toRadians(degrees: number): number {
    return degrees * CONSTANTES_GEOGRAFICAS.GRAUS_PARA_RADIANOS;
  }

  /**
   * Resolve os IDs de UF e Município baseados nos nomes obtidos da geocodificação
   */
  private resolverIdsGeograficos(result: GeocodingResult): Observable<GeocodingResult> {
    // Primeiro, buscar o UF pelo código
    return this.buscarUfPorCodigo(result.uf).pipe(
      switchMap(uf => {
        if (uf) {
          result.ufId = uf.id;
          // Agora buscar o município pelo nome e UF
          return this.buscarMunicipioPorNome(result.cidade, uf.id).pipe(
            map(municipio => {
              if (municipio) {
                result.municipioId = municipio.id;
              }
              return result;
            })
          );
        }
        return of(result);
      }),
      catchError(error => {
        console.warn('Erro ao resolver IDs geográficos:', error);
        return of(result); // Retorna o resultado sem os IDs em caso de erro
      })
    );
  }

  /**
   * Busca UF pelo código (ex: 'SP', 'RJ')
   */
  private buscarUfPorCodigo(ufCodigo: string): Observable<any> {

    //https://localhost:5000/api/enderecos/estados/uf/PB
    

    return this.http.get<any>(`${environment.apiUrl}/enderecos/estados/uf/${ufCodigo}`).pipe(
      catchError(error => {
        console.warn(`UF não encontrada: ${ufCodigo}`, error);
        return of(null);
      })
    );
  }

  /**
   * Busca município pelo nome e UF ID
   */
  private buscarMunicipioPorNome(nomeMunicipio: string, ufId: number): Observable<any> {
    return this.http.get<any[]>(`${environment.apiUrl}/enderecos/municipios/buscar`, {
      params: {
        nome: nomeMunicipio,
        ufId: ufId.toString()
      }
    }).pipe(
      map(municipios => {
        if (municipios && municipios.length > 0) {
          // Procurar por correspondência exata primeiro
          let municipio = municipios.find(m => 
            m.nome.toLowerCase() === nomeMunicipio.toLowerCase()
          );
          
          // Se não encontrar correspondência exata, pegar o primeiro resultado
          if (!municipio) {
            municipio = municipios[0];
          }
          
          return municipio;
        }
        return null;
      }),
      catchError(error => {
        console.warn(`Município não encontrado: ${nomeMunicipio}`, error);
        return of(null);
      })
    );
  }
}