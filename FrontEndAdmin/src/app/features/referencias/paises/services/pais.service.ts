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
  protected readonly apiEndpoint = 'referencias/paises';

  /**
   * Get países with UF count for dependency display
   */
  override obterTodos(): Observable<PaisDto[]> {
    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.get<PaisDto[]>(`${this.baseUrl}`),
      'obter países',
      'país'
    );
  }

  /**
   * Get países with UF count (active only)
   */
  override obterAtivos(): Observable<PaisDto[]> {
    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.get<PaisDto[]>(`${this.baseUrl}/ativos`),
      'obter países ativos',
      'país'
    );
  }

  /**
   * Get UFs for a specific país
   */
  obterUfsPorPais(paisId: number): Observable<UfDto[]> {
    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.get<UfDto[]>(`${this.baseUrl}/${paisId}/ufs`),
      `obter UFs do país ${paisId}`,
      'país'
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
    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.get<{ id: number; codigo: string; nome: string }[]>(`${this.baseUrl}/dropdown`),
      'obter países para dropdown',
      'país'
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
    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.get<{
        ufsCount: number;
        municipiosCount: number;
        fornecedoresCount: number;
      }>(`${this.baseUrl}/${paisId}/estatisticas`),
      `obter estatísticas do país ${paisId}`,
      'país'
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