import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { ReferenceCrudService } from '../../../../shared/services/reference-crud.service';
import { PaisDto, CriarPaisDto, AtualizarPaisDto, UfDto } from '../../../../shared/models/reference.model';

/**
 * Service for managing Países (Countries)
 * Extends ReferenceCrudService with UF dependency management
 */
@Injectable({
  providedIn: 'root'
})
export class PaisService extends ReferenceCrudService<
  PaisDto,
  CriarPaisDto,
  AtualizarPaisDto
> {
  
  protected readonly entityName = 'País';
  protected readonly apiEndpoint = 'api/referencias/paises';

  /**
   * Get países with UF count for dependency display
   */
  override obterTodos(): Observable<PaisDto[]> {
    return this.http.get<PaisDto[]>(`${this.baseUrl}/com-contadores`).pipe(
      catchError(error => this.handleError('obter países com contadores', error))
    );
  }

  /**
   * Get países with UF count (active only)
   */
  override obterAtivos(): Observable<PaisDto[]> {
    return this.http.get<PaisDto[]>(`${this.baseUrl}/ativos/com-contadores`).pipe(
      catchError(error => this.handleError('obter países ativos com contadores', error))
    );
  }

  /**
   * Get UFs for a specific país
   */
  obterUfsPorPais(paisId: number): Observable<UfDto[]> {
    return this.http.get<UfDto[]>(`${this.baseUrl}/${paisId}/ufs`).pipe(
      catchError(error => this.handleError(`obter UFs do país ${paisId}`, error))
    );
  }

  /**
   * Check if país has UF dependencies
   */
  verificarDependenciasUf(paisId: number): Observable<boolean> {
    return this.http.get<{ hasUfs: boolean }>(`${this.baseUrl}/${paisId}/tem-ufs`).pipe(
      map(response => response.hasUfs),
      catchError(() => of(false))
    );
  }

  /**
   * Get países for dropdown usage (only active, minimal data)
   */
  obterParaDropdown(): Observable<{ id: number; codigo: string; nome: string }[]> {
    return this.http.get<{ id: number; codigo: string; nome: string }[]>(`${this.baseUrl}/dropdown`).pipe(
      catchError(error => this.handleError('obter países para dropdown', error))
    );
  }

  /**
   * Check if codigo is unique (excluding current item if editing)
   */
  verificarCodigoUnico(codigo: string, idExcluir?: number): Observable<boolean> {
    const params: any = { codigo };
    if (idExcluir) {
      params.idExcluir = idExcluir;
    }
    
    return this.http.get<{ isUnique: boolean }>(`${this.baseUrl}/verificar-codigo`, { params }).pipe(
      map(response => response.isUnique),
      catchError(() => of(false))
    );
  }

  /**
   * Check if nome is unique (excluding current item if editing)
   */
  verificarNomeUnico(nome: string, idExcluir?: number): Observable<boolean> {
    const params: any = { nome };
    if (idExcluir) {
      params.idExcluir = idExcluir;
    }
    
    return this.http.get<{ isUnique: boolean }>(`${this.baseUrl}/verificar-nome`, { params }).pipe(
      map(response => response.isUnique),
      catchError(() => of(false))
    );
  }

  /**
   * Get país statistics (UFs count, municipalities count, etc.)
   */
  obterEstatisticas(paisId: number): Observable<{
    ufsCount: number;
    municipiosCount: number;
    fornecedoresCount: number;
  }> {
    return this.http.get<{
      ufsCount: number;
      municipiosCount: number;
      fornecedoresCount: number;
    }>(`${this.baseUrl}/${paisId}/estatisticas`).pipe(
      catchError(error => this.handleError(`obter estatísticas do país ${paisId}`, error))
    );
  }

  /**
   * Configure cache for países (they change very infrequently)
   */
  constructor() {
    super();
    
    // Países change very infrequently, so we can cache for longer
    this.configurarCache({
      enabled: true,
      ttlMinutes: 60, // 1 hour cache
      maxSize: 20
    });
  }
}