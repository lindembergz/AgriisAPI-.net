import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap, map } from 'rxjs/operators';
import { CacheService } from '../../core/services/cache.service';
import { PerformanceService } from './performance.service';
import { ReferenceCrudService } from './reference-crud.service';
import { BaseEntity } from '../models/base.model';

/**
 * Enhanced base service for reference data with caching and performance optimization
 * Extends ReferenceCrudService to maintain compatibility
 */
@Injectable()
export abstract class CachedReferenceService<
  TDto extends BaseEntity, 
  TCreateDto, 
  TUpdateDto
> extends ReferenceCrudService<TDto, TCreateDto, TUpdateDto> {
  protected cacheService = inject(CacheService);
  protected performanceService = inject(PerformanceService);

  constructor(apiEndpoint: string) {
    super();
    // Set the apiEndpoint for the parent class
    (this as any).apiEndpoint = apiEndpoint.replace('/api/', '');
  }

  /**
   * Override parent methods to add enhanced caching
   */
  override obterTodos(): Observable<TDto[]> {
    const cacheKey = `${this.baseUrl}/todos`;
    
    return this.performanceService.createCachedObservable(
      cacheKey,
      () => super.obterTodos(),
      'referencias'
    );
  }

  override obterAtivos(): Observable<TDto[]> {
    const cacheKey = `${this.baseUrl}/ativos`;
    
    return this.performanceService.createCachedObservable(
      cacheKey,
      () => super.obterAtivos(),
      'referencias'
    );
  }

  override obterPorId(id: number): Observable<TDto> {
    const cacheKey = `${this.baseUrl}/${id}`;
    
    return this.performanceService.createCachedObservable(
      cacheKey,
      () => super.obterPorId(id),
      'referencias'
    );
  }

  override criar(dto: TCreateDto): Observable<TDto> {
    return super.criar(dto).pipe(
      tap(() => this.invalidateEnhancedCache())
    );
  }

  override atualizar(id: number, dto: TUpdateDto, rowVersion?: string): Observable<TDto> {
    return super.atualizar(id, dto, rowVersion).pipe(
      tap(() => this.invalidateEnhancedCache())
    );
  }

  override remover(id: number): Observable<void> {
    return super.remover(id).pipe(
      tap(() => this.invalidateEnhancedCache())
    );
  }

  override ativar(id: number): Observable<void> {
    return super.ativar(id).pipe(
      tap(() => this.invalidateEnhancedCache())
    );
  }

  override desativar(id: number): Observable<void> {
    return super.desativar(id).pipe(
      tap(() => this.invalidateEnhancedCache())
    );
  }

   /**
   * Override search method to maintain compatibility
   */
  override buscar(params: {
    termo?: string;
    ativo?: boolean;
    pagina?: number;
    tamanhoPagina?: number;
    ordenacao?: string;
  }): Observable<{ items: TDto[]; total: number }> {
    const cacheKey = `buscar/${JSON.stringify(params)}`;
    
    return this.performanceService.createCachedObservable(
      cacheKey,
      () => super.buscar(params),
      'search'
    );
  }

  /**
   * Simple search by term (for backward compatibility)
   */
  buscarPorTermo(termo: string): Observable<TDto[]> {
    if (!termo || termo.length < 2) {
      return this.obterAtivos();
    }

    return this.buscar({ termo }).pipe(
      map(result => result.items)
    );
  }

  /**
   * Get paginated results with caching
   */
  obterPaginado(
    page: number = 1, 
    pageSize: number = 10, 
    search?: string,
    filters?: any
  ): Observable<PaginatedResult<TDto>> {
    const params: any = { page, pageSize };
    
    if (search) params.search = search;
    if (filters) Object.assign(params, filters);
    
    const cacheKey = `${this.baseUrl}/paginado/${JSON.stringify(params)}`;
    
    return this.performanceService.createCachedObservable(
      cacheKey,
      () => this.http.get<PaginatedResult<TDto>>(`${this.baseUrl}/paginado`, { params }),
      'referencias'
    );
  }

  /**
   * Preload commonly used data
   */
  preload(): void {
    // Preload active items
    this.obterAtivos().subscribe();
    
    // Preload first page
    this.obterPaginado(1, 20).subscribe();
  }

  /**
   * Invalidate enhanced cache entries for this service
   */
  protected invalidateEnhancedCache(): void {
    // Invalidate parent cache
    super.invalidateCache();
    
    // Invalidate enhanced cache
    const pattern = new RegExp(`^${this.baseUrl.replace(/\//g, '\\/')}`);
    this.cacheService.invalidatePattern(pattern, 'referencias');
    this.cacheService.invalidatePattern(pattern, 'search');
  }

  /**
   * Get cache statistics for this service
   */
  getCacheStats(): any {
    return this.cacheService.getStats();
  }

  /**
   * Clear cache for this service
   */
  clearCache(): void {
    this.invalidateEnhancedCache();
  }
}

/**
 * Paginated result interface
 */
export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}