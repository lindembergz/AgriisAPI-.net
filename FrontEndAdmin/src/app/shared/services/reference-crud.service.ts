import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Observable, throwError, BehaviorSubject, of } from 'rxjs';
import { catchError, tap, shareReplay, map } from 'rxjs/operators';
import { MessageService } from 'primeng/api';
import { environment } from '../../../environments/environment';
import { BaseEntity } from '../models/base.model';

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

    return this.http.get<TDto[]>(this.baseUrl).pipe(
      tap(data => this.setCache(cacheKey, data)),
      catchError(error => this.handleError('obter todos', error)),
      shareReplay(1)
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

    return this.http.get<TDto[]>(`${this.baseUrl}/ativos`).pipe(
      tap(data => {
        this.setCache(cacheKey, data);
        this.activeItemsCache$.next(data);
      }),
      catchError(error => this.handleError('obter ativos', error)),
      shareReplay(1)
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

    return this.http.get<TDto>(`${this.baseUrl}/${id}`).pipe(
      tap(data => this.setCache(cacheKey, data)),
      catchError(error => this.handleError(`obter ${this.entityName} id=${id}`, error))
    );
  }

  /**
   * Create new entity
   */
  criar(dto: TCreateDto): Observable<TDto> {
    return this.http.post<TDto>(this.baseUrl, dto).pipe(
      tap(() => this.invalidateCache()),
      catchError(error => this.handleError(`criar ${this.entityName}`, error))
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

    return this.http.put<TDto>(`${this.baseUrl}/${id}`, dto, { headers }).pipe(
      tap(() => this.invalidateCache()),
      catchError(error => this.handleError(`atualizar ${this.entityName} id=${id}`, error))
    );
  }

  /**
   * Activate entity
   */
  ativar(id: number): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${id}/ativar`, {}).pipe(
      tap(() => this.invalidateCache()),
      catchError(error => this.handleError(`ativar ${this.entityName} id=${id}`, error))
    );
  }

  /**
   * Deactivate entity
   */
  desativar(id: number): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${id}/desativar`, {}).pipe(
      tap(() => this.invalidateCache()),
      catchError(error => this.handleError(`desativar ${this.entityName} id=${id}`, error))
    );
  }

  /**
   * Remove entity
   */
  remover(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`).pipe(
      tap(() => this.invalidateCache()),
      catchError(error => this.handleError(`remover ${this.entityName} id=${id}`, error))
    );
  }

  /**
   * Check if entity can be removed
   */
  podeRemover(id: number): Observable<boolean> {
    return this.http.get<{ canRemove: boolean }>(`${this.baseUrl}/${id}/pode-remover`).pipe(
      map(response => response.canRemove),
      catchError(error => this.handleError(`verificar remoção ${this.entityName} id=${id}`, error))
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

    return this.http.get<{ items: TDto[]; total: number }>(`${this.baseUrl}/buscar`, { params: httpParams }).pipe(
      catchError(error => this.handleError(`buscar ${this.entityName}`, error))
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
   * Standardized error handling with user-friendly messages
   */
  protected handleError(operation: string, error: HttpErrorResponse): Observable<never> {
    console.error(`${this.constructor.name}: ${operation} failed:`, error);
    
    let errorMessage = 'Ocorreu um erro inesperado';
    let severity: 'error' | 'warn' | 'info' = 'error';
    
    // Handle API error response format
    if (error.error && typeof error.error === 'object') {
      const apiError = error.error as ApiErrorResponse;
      
      if (apiError.errorCode) {
        switch (apiError.errorCode) {
          case 'ENTITY_NOT_FOUND':
            errorMessage = `${this.entityName} não encontrado(a)`;
            break;
          case 'DUPLICATE_CODE':
            errorMessage = 'Este código já está em uso. Por favor, escolha outro.';
            break;
          case 'DUPLICATE_NAME':
            errorMessage = 'Este nome já está em uso. Por favor, escolha outro.';
            break;
          case 'CANNOT_DELETE_REFERENCED':
            errorMessage = `Não é possível excluir este(a) ${this.entityName} pois está sendo usado(a) por outros registros.`;
            severity = 'warn';
            break;
          case 'CONCURRENCY_CONFLICT':
            errorMessage = 'Este registro foi modificado por outro usuário. Por favor, recarregue e tente novamente.';
            severity = 'warn';
            break;
          case 'VALIDATION_ERROR':
            if (apiError.validationErrors) {
              const validationMessages = Object.values(apiError.validationErrors).flat();
              errorMessage = validationMessages.join(', ');
            } else {
              errorMessage = 'Dados inválidos fornecidos';
            }
            break;
          default:
            errorMessage = apiError.errorDescription || errorMessage;
        }
      } else if (apiError.errorDescription) {
        errorMessage = apiError.errorDescription;
      }
    } else if (error.status) {
      // Handle HTTP status codes
      switch (error.status) {
        case 400:
          errorMessage = 'Dados inválidos fornecidos';
          break;
        case 401:
          errorMessage = 'Não autorizado. Faça login novamente';
          break;
        case 403:
          errorMessage = 'Acesso negado';
          break;
        case 404:
          errorMessage = `${this.entityName} não encontrado(a)`;
          break;
        case 409:
          errorMessage = `Já existe um(a) ${this.entityName} com estes dados`;
          break;
        case 412:
          errorMessage = 'Este registro foi modificado por outro usuário. Por favor, recarregue e tente novamente.';
          severity = 'warn';
          break;
        case 422:
          errorMessage = 'Dados de entrada inválidos';
          break;
        case 500:
          errorMessage = 'Erro interno do servidor';
          break;
        case 502:
        case 503:
        case 504:
          errorMessage = 'Serviço temporariamente indisponível';
          break;
        default:
          errorMessage = `Erro ${error.status}: ${error.statusText}`;
      }
    }

    // Show toast message
    this.messageService.add({
      severity,
      summary: severity === 'error' ? 'Erro' : 'Aviso',
      detail: errorMessage,
      life: severity === 'warn' ? 7000 : 5000
    });

    return throwError(() => ({
      message: errorMessage,
      originalError: error,
      operation
    }));
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