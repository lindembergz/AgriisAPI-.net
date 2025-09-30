import { Injectable, inject } from '@angular/core';
import { Observable, fromEvent, debounceTime, distinctUntilChanged, BehaviorSubject, of } from 'rxjs';
import { tap, switchMap } from 'rxjs/operators';
import { CacheService } from '../../core/services/cache.service';

/**
 * Service for performance optimizations and monitoring
 */
@Injectable({
  providedIn: 'root'
})
export class PerformanceService {
  private cacheService = inject(CacheService);
  private performanceMetrics$ = new BehaviorSubject<PerformanceMetrics>({
    apiCallCount: 0,
    cacheHitCount: 0,
    averageResponseTime: 0,
    slowOperations: []
  });

  /**
   * Debounce search input to reduce API calls
   */
  debounceSearch(searchInput: Observable<string>, debounceMs: number = 300): Observable<string> {
    return searchInput.pipe(
      debounceTime(debounceMs),
      distinctUntilChanged()
    );
  }

  /**
   * Create debounced scroll event for infinite scrolling
   */
  createDebouncedScrollEvent(element: Element, debounceMs: number = 100): Observable<Event> {
    return fromEvent(element, 'scroll').pipe(
      debounceTime(debounceMs)
    );
  }

  /**
   * Measure and log performance metrics
   */
  measurePerformance(operationName: string, operation: () => void): void {
    const startTime = performance.now();
    operation();
    const endTime = performance.now();
    
    console.log(`Performance: ${operationName} took ${endTime - startTime} milliseconds`);
    
    // In production, you might want to send this to an analytics service
    if (endTime - startTime > 1000) {
      console.warn(`Slow operation detected: ${operationName} took ${endTime - startTime}ms`);
    }
  }

  /**
   * Measure async operation performance
   */
  async measureAsyncPerformance<T>(operationName: string, operation: () => Promise<T>): Promise<T> {
    const startTime = performance.now();
    try {
      const result = await operation();
      const endTime = performance.now();
      
      console.log(`Performance: ${operationName} took ${endTime - startTime} milliseconds`);
      
      if (endTime - startTime > 2000) {
        console.warn(`Slow async operation detected: ${operationName} took ${endTime - startTime}ms`);
      }
      
      return result;
    } catch (error) {
      const endTime = performance.now();
      console.error(`Performance: ${operationName} failed after ${endTime - startTime} milliseconds`, error);
      throw error;
    }
  }

  /**
   * Check if the user is on a slow connection
   */
  isSlowConnection(): boolean {
    // @ts-ignore - navigator.connection is not in all TypeScript definitions
    const connection = navigator.connection || navigator.mozConnection || navigator.webkitConnection;
    
    if (!connection) {
      return false; // Assume fast connection if we can't detect
    }
    
    // Consider 2G or slow-2g as slow connections
    return connection.effectiveType === '2g' || connection.effectiveType === 'slow-2g';
  }

  /**
   * Get memory usage information (if available)
   */
  getMemoryUsage(): any {
    // @ts-ignore - performance.memory is not in all TypeScript definitions
    if ('memory' in performance) {
      // @ts-ignore
      return {
        // @ts-ignore
        used: performance.memory.usedJSHeapSize,
        // @ts-ignore
        total: performance.memory.totalJSHeapSize,
        // @ts-ignore
        limit: performance.memory.jsHeapSizeLimit
      };
    }
    return null;
  }

  /**
   * Log memory usage for debugging
   */
  logMemoryUsage(context: string): void {
    const memory = this.getMemoryUsage();
    if (memory) {
      console.log(`Memory usage (${context}):`, {
        used: `${(memory.used / 1024 / 1024).toFixed(2)} MB`,
        total: `${(memory.total / 1024 / 1024).toFixed(2)} MB`,
        limit: `${(memory.limit / 1024 / 1024).toFixed(2)} MB`
      });
    }
  }

  /**
   * Create cached observable for API calls
   */
  createCachedObservable<T>(
    key: string,
    apiCall: () => Observable<T>,
    cacheType: 'referencias' | 'usuarios' | 'search' | 'static' = 'referencias'
  ): Observable<T> {
    const startTime = performance.now();
    
    return this.cacheService.get(key, () => {
      this.incrementApiCallCount();
      return apiCall().pipe(
        tap(() => {
          const endTime = performance.now();
          this.recordResponseTime(endTime - startTime);
        })
      );
    }, { type: cacheType }).pipe(
      tap(() => {
        // Check if this was a cache hit
        if (this.cacheService.has(key)) {
          this.incrementCacheHitCount();
        }
      })
    );
  }

  /**
   * Optimize search with debouncing and caching
   */
  optimizeSearch<T>(
    searchTerm$: Observable<string>,
    searchFn: (term: string) => Observable<T>,
    debounceMs: number = 300
  ): Observable<T> {
    return searchTerm$.pipe(
      debounceTime(debounceMs),
      distinctUntilChanged(),
      switchMap(term => {
        if (!term || term.length < 2) {
          return of([] as any);
        }
        
        const cacheKey = `search:${term}`;
        return this.createCachedObservable(cacheKey, () => searchFn(term), 'search');
      })
    );
  }

  /**
   * Preload critical reference data
   */
  preloadCriticalData(): void {
    // Preload commonly used reference data
    const criticalData = [
      'paises-ativos',
      'ufs-brasil', 
      'unidades-medida-tipos',
      'moedas-ativas'
    ];

    criticalData.forEach(key => {
      // The actual services would be injected and called here
      console.log(`Preloading critical data: ${key}`);
    });
  }

  /**
   * Get performance metrics
   */
  getPerformanceMetrics(): Observable<PerformanceMetrics> {
    return this.performanceMetrics$.asObservable();
  }

  /**
   * Clear performance-related caches
   */
  clearPerformanceCaches(): void {
    this.cacheService.clear('search');
    this.resetMetrics();
  }

  /**
   * Get cache statistics
   */
  getCacheStats() {
    return this.cacheService.getStats();
  }

  private incrementApiCallCount(): void {
    const current = this.performanceMetrics$.value;
    this.performanceMetrics$.next({
      ...current,
      apiCallCount: current.apiCallCount + 1
    });
  }

  private incrementCacheHitCount(): void {
    const current = this.performanceMetrics$.value;
    this.performanceMetrics$.next({
      ...current,
      cacheHitCount: current.cacheHitCount + 1
    });
  }

  private recordResponseTime(time: number): void {
    const current = this.performanceMetrics$.value;
    const newAverage = (current.averageResponseTime + time) / 2;
    
    const updates: Partial<PerformanceMetrics> = {
      averageResponseTime: newAverage
    };

    // Track slow operations (> 2 seconds)
    if (time > 2000) {
      updates.slowOperations = [
        ...current.slowOperations.slice(-9), // Keep last 10
        { timestamp: Date.now(), duration: time }
      ];
    }

    this.performanceMetrics$.next({ ...current, ...updates });
  }

  private resetMetrics(): void {
    this.performanceMetrics$.next({
      apiCallCount: 0,
      cacheHitCount: 0,
      averageResponseTime: 0,
      slowOperations: []
    });
  }
}

/**
 * Performance metrics interface
 */
export interface PerformanceMetrics {
  apiCallCount: number;
  cacheHitCount: number;
  averageResponseTime: number;
  slowOperations: Array<{ timestamp: number; duration: number }>;
}