import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Observable, throwError, BehaviorSubject, of } from 'rxjs';
import { catchError, tap, shareReplay, map, retry, delay } from 'rxjs/operators';
import { MessageService } from 'primeng/api';
import { environment } from '../../../environments/environment';
import { BaseEntity } from '../models/base.model';
import { ErrorHandlingService } from './error-handling.service';

/**
 * API Error Response interface for standardized error handling
 */
export interface ApiErrorResponse {
  errorCode: string;
  errorDescription: string;
  validationErrors?: { [key: string]: string[] };
  traceId?: string;
  timestamp: Date;
}

/**
 * Concurrency conflict error interface
 */
export interface ConcurrencyConflictError {
  message: string;
  currentRowVersion: string;
  conflictingData: any;
}

/**
 * Cache configuration interface
 */
export interface CacheConfig {
  enabled: boolean;
  ttlMinutes: number;
  maxSize: number;
}

/**
 * Cache entry interface
 */
interface CacheEntry<T> {
  data: T;
  timestamp: number;
  ttl: number;
}

/**
 * Base CRUD service for reference entities with caching and concurrency control
 */
@Injectable({
  providedIn: 'root'
})
export abstract class ReferenceCrudService<
  TDto extends BaseEntity,
  TCreateDto,
  TUpdateDto
> {
  protected http = inject(HttpClient);
  protected messageService = inject(MessageService);
  protected errorHandlingService = inject(ErrorHandlingService);
  
  protected abstract readonly entityName: string;
  protected abstract readonly apiEndpoint: string;
  
  // Cache configuration
  protected cacheConfig: CacheConfig = {
    enabled: true,
    ttlMinutes: 5,
    maxSize: 100
  };
  
  // Cache storage
  private cache = new Map<string, CacheEntry<any>>();
  private activeItemsCache$ = new BehaviorSubject<TDto[] | null>(null);
  
  protected get baseUrl(): string {
    return `${environment.apiUrl}/${this.apiEndpoint}`;
  }

  /**
   * Get all entities
   */
  obterTodos(): Observable<TDto[]> {
    const cacheKey = 'all';
    const cached = this.getFromCache<TDto[]>(cacheKey);
    
    if (cached) {
      return of(cached);
    }

    // Debug: trace calls to obterTodos so we can confirm at runtime whether the request is issued
    console.debug(`[ReferenceCrudService] obterTodos called for ${this.entityName} -> ${this.baseUrl}`);

    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.get<TDto[]>(this.baseUrl).pipe(
        tap(data => {
          console.debug(`[ReferenceCrudService] obterTodos response for ${this.entityName}:`, data?.length ?? 0);
          this.setCache(cacheKey, data);
        }),
        shareReplay(1)
      ),
      `obter todos ${this.entityName}`,
      this.entityName,
      { maxRetries: 2, delayMs: 1000 }
    );
  }

  /**
   * Get only active entities with caching
   */
  obterAtivos(): Observable<TDto[]> {
    const cacheKey = 'active';
    const cached = this.getFromCache<TDto[]>(cacheKey);
    
    if (cached) {
      return of(cached);
    }

    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.get<TDto[]>(`${this.baseUrl}/ativos`).pipe(
        tap(data => {
          this.setCache(cacheKey, data);
          this.activeItemsCache$.next(data);
        }),
        shareReplay(1)
      ),
      `obter ${this.entityName} ativos`,
      this.entityName,
      { maxRetries: 2, delayMs: 1000 }
    );
  }

  /**
   * Get entity by ID
   */
  obterPorId(id: number): Observable<TDto> {
    const cacheKey = `item-${id}`;
    const cached = this.getFromCache<TDto>(cacheKey);
    
    if (cached) {
      return of(cached);
    }

    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.get<TDto>(`${this.baseUrl}/${id}`).pipe(
        tap(data => this.setCache(cacheKey, data))
      ),
      `obter ${this.entityName} por ID`,
      this.entityName,
      { maxRetries: 1, delayMs: 500 }
    );
  }

  /**
   * Create new entity
   */
  criar(dto: TCreateDto): Observable<TDto> {
    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.post<TDto>(this.baseUrl, dto).pipe(
        tap(() => this.invalidateCache())
      ),
      `criar ${this.entityName}`,
      this.entityName,
      { maxRetries: 0 } // Don't retry create operations
    );
  }

  /**
   * Update entity with concurrency control
   */
  atualizar(id: number, dto: TUpdateDto, rowVersion?: string): Observable<TDto> {
    let headers: any = {};
    
    if (rowVersion) {
      headers['If-Match'] = rowVersion;
    }

    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.put<TDto>(`${this.baseUrl}/${id}`, dto, { headers }).pipe(
        tap(() => this.invalidateCache())
      ),
      `atualizar ${this.entityName}`,
      this.entityName,
      { maxRetries: 0 } // Don't retry update operations due to concurrency
    );
  }

  /**
   * Activate entity
   */
  ativar(id: number): Observable<void> {
    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.patch<void>(`${this.baseUrl}/${id}/ativar`, {}).pipe(
        tap(() => this.invalidateCache())
      ),
      `ativar ${this.entityName}`,
      this.entityName,
      { maxRetries: 1, delayMs: 500 }
    );
  }

  /**
   * Deactivate entity
   */
  desativar(id: number): Observable<void> {
    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.patch<void>(`${this.baseUrl}/${id}/desativar`, {}).pipe(
        tap(() => this.invalidateCache())
      ),
      `desativar ${this.entityName}`,
      this.entityName,
      { maxRetries: 1, delayMs: 500 }
    );
  }

  /**
   * Remove entity
   */
  remover(id: number): Observable<void> {
    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.delete<void>(`${this.baseUrl}/${id}`).pipe(
        tap(() => this.invalidateCache())
      ),
      `remover ${this.entityName}`,
      this.entityName,
      { maxRetries: 0 } // Don't retry delete operations
    );
  }

  /**
   * Search entities with pagination and filters
   */
  buscar(params: {
    termo?: string;
    ativo?: boolean;
    pagina?: number;
    tamanhoPagina?: number;
    ordenacao?: string;
  }): Observable<{ items: TDto[]; total: number }> {
    let httpParams = new HttpParams();
    
    if (params.termo) httpParams = httpParams.set('termo', params.termo);
    if (params.ativo !== undefined) httpParams = httpParams.set('ativo', params.ativo.toString());
    if (params.pagina) httpParams = httpParams.set('pagina', params.pagina.toString());
    if (params.tamanhoPagina) httpParams = httpParams.set('tamanhoPagina', params.tamanhoPagina.toString());
    if (params.ordenacao) httpParams = httpParams.set('ordenacao', params.ordenacao);

    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.get<{ items: TDto[]; total: number }>(`${this.baseUrl}/buscar`, { params: httpParams }),
      `buscar ${this.entityName}`,
      this.entityName,
      { maxRetries: 2, delayMs: 1000 }
    );
  }

  /**
   * Get cached active items observable
   */
  getActiveItemsCache(): Observable<TDto[] | null> {
    return this.activeItemsCache$.asObservable();
  }

  /**
   * Refresh cache for active items
   */
  refreshActiveItems(): Observable<TDto[]> {
    this.invalidateCache('active');
    return this.obterAtivos();
  }

  /**
   * Cache management methods
   */
  protected getFromCache<T>(key: string): T | null {
    if (!this.cacheConfig.enabled) return null;
    
    const entry = this.cache.get(key);
    if (!entry) return null;
    
    const now = Date.now();
    if (now > entry.timestamp + entry.ttl) {
      this.cache.delete(key);
      return null;
    }
    
    return entry.data as T;
  }

  protected setCache<T>(key: string, data: T): void {
    if (!this.cacheConfig.enabled) return;
    
    // Remove oldest entries if cache is full
    if (this.cache.size >= this.cacheConfig.maxSize) {
      const oldestKey = this.cache.keys().next().value;
      this.cache.delete(oldestKey);
    }
    
    this.cache.set(key, {
      data,
      timestamp: Date.now(),
      ttl: this.cacheConfig.ttlMinutes * 60 * 1000
    });
  }

  protected invalidateCache(pattern?: string): void {
    if (pattern) {
      // Remove specific cache entries
      for (const key of this.cache.keys()) {
        if (key.includes(pattern)) {
          this.cache.delete(key);
        }
      }
    } else {
      // Clear all cache
      this.cache.clear();
      this.activeItemsCache$.next(null);
    }
  }



  /**
   * Configure cache settings
   */
  configurarCache(config: Partial<CacheConfig>): void {
    this.cacheConfig = { ...this.cacheConfig, ...config };
  }

  /**
   * Clear all cache
   */
  limparCache(): void {
    this.invalidateCache();
  }
}