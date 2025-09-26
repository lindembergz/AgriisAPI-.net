import { Injectable } from '@angular/core';
import { Observable, map, catchError, of, shareReplay } from 'rxjs';
import { ReferenceCrudService } from '../../../../shared/services/reference-crud.service';
import { UnidadeMedidaDto, CriarUnidadeMedidaDto, AtualizarUnidadeMedidaDto, TipoUnidadeMedida } from '../../../../shared/models/reference.model';

/**
 * Interface for conversion result
 */
export interface ConversaoResult {
  quantidadeOriginal: number;
  unidadeOrigemId: number;
  unidadeDestinoId: number;
  quantidadeConvertida: number;
}

/**
 * Interface for unit type option
 */
export interface TipoUnidadeOption {
  valor: number;
  nome: string;
  descricao: string;
}

/**
 * Interface for dropdown option
 */
export interface UnidadeDropdownOption {
  id: number;
  simbolo: string;
  nome: string;
}

/**
 * Service for managing Unidades de Medida (Units of Measure)
 * Extends ReferenceCrudService for standard CRUD operations with caching and error handling
 */
@Injectable({
  providedIn: 'root'
})
export class UnidadeMedidaService extends ReferenceCrudService<
  UnidadeMedidaDto,
  CriarUnidadeMedidaDto,
  AtualizarUnidadeMedidaDto
> {
  
  protected readonly entityName = 'Unidade de Medida';
  protected readonly apiEndpoint = 'api/referencias/unidades-medida';

  /**
   * Get unidades by simbolo pattern (for search/autocomplete)
   */
  buscarPorSimbolo(simbolo: string): Observable<UnidadeMedidaDto[]> {
    return this.buscar({ termo: simbolo }).pipe(
      map(response => response.items)
    );
  }

  /**
   * Check if simbolo is unique (excluding current item if editing)
   */
  verificarSimboloUnico(simbolo: string, idExcluir?: number): Observable<boolean> {
    const params: any = {};
    if (idExcluir) {
      params.idExcluir = idExcluir;
    }
    
    return this.http.get<{ existe: boolean }>(`${this.baseUrl}/existe-simbolo/${encodeURIComponent(simbolo)}`, { params })
      .pipe(
        map(response => !response.existe),
        catchError(() => of(false))
      );
  }

  /**
   * Check if nome is unique (excluding current item if editing)
   */
  verificarNomeUnico(nome: string, idExcluir?: number): Observable<boolean> {
    const params: any = {};
    if (idExcluir) {
      params.idExcluir = idExcluir;
    }
    
    return this.http.get<{ existe: boolean }>(`${this.baseUrl}/existe-nome/${encodeURIComponent(nome)}`, { params })
      .pipe(
        map(response => !response.existe),
        catchError(() => of(false))
      );
  }

  /**
   * Get unidades by type
   */
  obterPorTipo(tipo: TipoUnidadeMedida): Observable<UnidadeMedidaDto[]> {
    return this.http.get<UnidadeMedidaDto[]>(`${this.baseUrl}/tipo/${tipo}`)
      .pipe(
        shareReplay(1),
        catchError(error => this.handleError('obter unidades por tipo', error))
      );
  }

  /**
   * Get unidades for dropdown usage by type (only active, minimal data)
   */
  obterDropdownPorTipo(tipo: TipoUnidadeMedida): Observable<UnidadeDropdownOption[]> {
    return this.http.get<UnidadeDropdownOption[]>(`${this.baseUrl}/tipo/${tipo}/dropdown`)
      .pipe(
        shareReplay(1),
        catchError(error => this.handleError('obter unidades para dropdown por tipo', error))
      );
  }

  /**
   * Get all available unit types
   */
  obterTipos(): Observable<TipoUnidadeOption[]> {
    return this.http.get<TipoUnidadeOption[]>(`${this.baseUrl}/tipos`)
      .pipe(
        shareReplay(1),
        catchError(error => this.handleError('obter tipos de unidade', error))
      );
  }

  /**
   * Convert quantity from one unit to another of the same type
   */
  converter(quantidade: number, unidadeOrigemId: number, unidadeDestinoId: number): Observable<ConversaoResult> {
    const params = {
      quantidade: quantidade.toString(),
      unidadeOrigemId: unidadeOrigemId.toString(),
      unidadeDestinoId: unidadeDestinoId.toString()
    };

    return this.http.get<ConversaoResult>(`${this.baseUrl}/converter`, { params })
      .pipe(
        catchError(error => this.handleError('converter unidades', error))
      );
  }

  /**
   * Get unidade by simbolo
   */
  obterPorSimbolo(simbolo: string): Observable<UnidadeMedidaDto | null> {
    return this.http.get<UnidadeMedidaDto>(`${this.baseUrl}/simbolo/${encodeURIComponent(simbolo)}`)
      .pipe(
        catchError(error => {
          if (error.status === 404) {
            return of(null);
          }
          return this.handleError('obter unidade por símbolo', error);
        })
      );
  }

  /**
   * Get unidades for dropdown usage (all types, only active, minimal data)
   */
  obterParaDropdown(): Observable<UnidadeDropdownOption[]> {
    return this.obterAtivos().pipe(
      map(unidades => unidades.map(u => ({
        id: u.id,
        simbolo: u.simbolo,
        nome: u.nome
      }))),
      shareReplay(1),
      catchError(error => this.handleError('obter unidades para dropdown', error))
    );
  }

  /**
   * Get tipo description from enum value
   */
  getTipoDescricao(tipo: TipoUnidadeMedida): string {
    switch (tipo) {
      case TipoUnidadeMedida.Peso:
        return 'Peso';
      case TipoUnidadeMedida.Volume:
        return 'Volume';
      case TipoUnidadeMedida.Area:
        return 'Área';
      case TipoUnidadeMedida.Unidade:
        return 'Unidade';
      default:
        return 'Desconhecido';
    }
  }

  /**
   * Configure cache for unidades (they change infrequently)
   */
  constructor() {
    super();
    
    // Unidades don't change frequently, so we can cache for longer
    this.configurarCache({
      enabled: true,
      ttlMinutes: 30, // 30 minutes cache
      maxSize: 100
    });
  }
}