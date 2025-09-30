import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { CachedReferenceService } from '../../../../shared/services/cached-reference.service';
import { MoedaDto, CriarMoedaDto, AtualizarMoedaDto } from '../../../../shared/models/reference.model';

/**
 * Service for managing Moedas (Currencies)
 * Extends CachedReferenceService for optimized CRUD operations with caching and performance monitoring
 */
@Injectable({
  providedIn: 'root'
})
export class MoedaService extends CachedReferenceService<
  MoedaDto,
  CriarMoedaDto,
  AtualizarMoedaDto
> {
  
  protected readonly entityName = 'Moeda';
  protected readonly apiEndpoint = 'referencias/moedas';
  
  constructor() {
    super('/api/referencias/moedas');
  }

  /**
   * Get moedas by codigo pattern (for search/autocomplete)
   */
  buscarPorCodigo(codigo: string): Observable<MoedaDto[]> {
    return this.buscarPorTermo(codigo);
  }

  /**
   * Check if codigo is unique (excluding current item if editing)
   */
  verificarCodigoUnico(codigo: string, idExcluir?: number): Observable<boolean> {
    const params: any = { codigo };
    if (idExcluir) {
      params.idExcluir = idExcluir;
    }
    
    const cacheKey = `verificar-codigo/${codigo}/${idExcluir || 'new'}`;
    
    return this.performanceService.createCachedObservable(
      cacheKey,
      () => this.http.get<{ isUnique: boolean }>(`${this.baseUrl}/verificar-codigo`, { params })
        .pipe(map(response => response.isUnique)),
      'referencias'
    ).pipe(
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
    
    const cacheKey = `verificar-nome/${nome}/${idExcluir || 'new'}`;
    
    return this.performanceService.createCachedObservable(
      cacheKey,
      () => this.http.get<{ isUnique: boolean }>(`${this.baseUrl}/verificar-nome`, { params })
        .pipe(map(response => response.isUnique)),
      'referencias'
    ).pipe(
      catchError(() => of(false))
    );
  }

  /**
   * Get moedas for dropdown usage (only active, minimal data)
   */
  obterParaDropdown(): Observable<{ id: number; codigo: string; nome: string; simbolo: string }[]> {
    const cacheKey = 'dropdown';
    
    return this.performanceService.createCachedObservable(
      cacheKey,
      () => this.http.get<{ id: number; codigo: string; nome: string; simbolo: string }[]>(`${this.baseUrl}/dropdown`),
      'referencias'
    );
  }
}