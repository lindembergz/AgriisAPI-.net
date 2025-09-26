import { Injectable } from '@angular/core';
import { Observable, map, catchError, of, shareReplay } from 'rxjs';
import { ReferenceCrudService } from '../../../../shared/services/reference-crud.service';
import { EmbalagemDto, CriarEmbalagemDto, AtualizarEmbalagemDto, TipoUnidadeMedida } from '../../../../shared/models/reference.model';

/**
 * Interface for UnidadeMedida dropdown option with type information
 */
export interface UnidadeDropdownOption {
  id: number;
  simbolo: string;
  nome: string;
  tipo: TipoUnidadeMedida;
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
 * Service for managing Embalagens (Packaging)
 * Extends ReferenceCrudService for standard CRUD operations with caching and error handling
 * Includes UnidadeMedida relationship methods and type filtering
 */
@Injectable({
  providedIn: 'root'
})
export class EmbalagemService extends ReferenceCrudService<
  EmbalagemDto,
  CriarEmbalagemDto,
  AtualizarEmbalagemDto
> {
  
  protected readonly entityName = 'Embalagem';
  protected readonly apiEndpoint = 'api/referencias/embalagens';

  /**
   * Get embalagens by nome pattern (for search/autocomplete)
   */
  buscarPorNome(nome: string): Observable<EmbalagemDto[]> {
    return this.buscar({ termo: nome }).pipe(
      map(response => response.items)
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
   * Get embalagens by UnidadeMedida ID
   */
  obterPorUnidadeMedida(unidadeMedidaId: number): Observable<EmbalagemDto[]> {
    return this.http.get<EmbalagemDto[]>(`${this.baseUrl}/unidade-medida/${unidadeMedidaId}`)
      .pipe(
        shareReplay(1),
        catchError(error => this.handleError('obter embalagens por unidade de medida', error))
      );
  }

  /**
   * Get embalagens by UnidadeMedida type
   */
  obterPorTipoUnidadeMedida(tipo: TipoUnidadeMedida): Observable<EmbalagemDto[]> {
    return this.http.get<EmbalagemDto[]>(`${this.baseUrl}/tipo-unidade/${tipo}`)
      .pipe(
        shareReplay(1),
        catchError(error => this.handleError('obter embalagens por tipo de unidade', error))
      );
  }

  /**
   * Get embalagens for dropdown usage by UnidadeMedida (only active, minimal data)
   */
  obterDropdownPorUnidadeMedida(unidadeMedidaId: number): Observable<{ id: number; nome: string }[]> {
    return this.http.get<{ id: number; nome: string }[]>(`${this.baseUrl}/unidade-medida/${unidadeMedidaId}/dropdown`)
      .pipe(
        shareReplay(1),
        catchError(error => this.handleError('obter embalagens para dropdown por unidade', error))
      );
  }

  /**
   * Get embalagens for dropdown usage by UnidadeMedida type (only active, minimal data)
   */
  obterDropdownPorTipoUnidade(tipo: TipoUnidadeMedida): Observable<{ id: number; nome: string }[]> {
    return this.http.get<{ id: number; nome: string }[]>(`${this.baseUrl}/tipo-unidade/${tipo}/dropdown`)
      .pipe(
        shareReplay(1),
        catchError(error => this.handleError('obter embalagens para dropdown por tipo', error))
      );
  }

  /**
   * Get all UnidadesMedida for dropdown usage (with type information)
   */
  obterUnidadesMedidaParaDropdown(): Observable<UnidadeDropdownOption[]> {
    return this.http.get<UnidadeDropdownOption[]>(`api/referencias/unidades-medida/dropdown-completo`)
      .pipe(
        shareReplay(1),
        catchError(error => this.handleError('obter unidades de medida para dropdown', error))
      );
  }

  /**
   * Get available unit types
   */
  obterTiposUnidade(): Observable<TipoUnidadeOption[]> {
    return this.http.get<TipoUnidadeOption[]>(`api/referencias/unidades-medida/tipos`)
      .pipe(
        shareReplay(1),
        catchError(error => this.handleError('obter tipos de unidade', error))
      );
  }

  /**
   * Get embalagem by nome
   */
  obterPorNome(nome: string): Observable<EmbalagemDto | null> {
    return this.http.get<EmbalagemDto>(`${this.baseUrl}/nome/${encodeURIComponent(nome)}`)
      .pipe(
        catchError(error => {
          if (error.status === 404) {
            return of(null);
          }
          return this.handleError('obter embalagem por nome', error);
        })
      );
  }

  /**
   * Get embalagens for dropdown usage (all, only active, minimal data)
   */
  obterParaDropdown(): Observable<{ id: number; nome: string }[]> {
    return this.obterAtivos().pipe(
      map(embalagens => embalagens.map(e => ({
        id: e.id,
        nome: e.nome
      }))),
      shareReplay(1),
      catchError(error => this.handleError('obter embalagens para dropdown', error))
    );
  }

  /**
   * Get tipo unidade description from enum value
   */
  getTipoUnidadeDescricao(tipo: TipoUnidadeMedida): string {
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
   * Validate UnidadeMedida relationship
   */
  validarUnidadeMedida(unidadeMedidaId: number): Observable<boolean> {
    return this.http.get<{ valida: boolean }>(`api/referencias/unidades-medida/${unidadeMedidaId}/valida`)
      .pipe(
        map(response => response.valida),
        catchError(() => of(false))
      );
  }

  /**
   * Get embalagens with complete UnidadeMedida information
   */
  obterComUnidadesMedida(): Observable<EmbalagemDto[]> {
    return this.http.get<EmbalagemDto[]>(`${this.baseUrl}/com-unidades`)
      .pipe(
        shareReplay(1),
        catchError(error => this.handleError('obter embalagens com unidades de medida', error))
      );
  }

  /**
   * Get statistics about embalagens by UnidadeMedida type
   */
  obterEstatisticasPorTipo(): Observable<{ tipo: TipoUnidadeMedida; quantidade: number; descricao: string }[]> {
    return this.http.get<{ tipo: TipoUnidadeMedida; quantidade: number; descricao: string }[]>(`${this.baseUrl}/estatisticas-tipo`)
      .pipe(
        shareReplay(1),
        catchError(error => this.handleError('obter estatísticas por tipo', error))
      );
  }

  /**
   * Configure cache for embalagens (they change moderately)
   */
  constructor() {
    super();
    
    // Embalagens change moderately, so we use a shorter cache
    this.configurarCache({
      enabled: true,
      ttlMinutes: 15, // 15 minutes cache
      maxSize: 50
    });
  }
}