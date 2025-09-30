import { Injectable, inject } from '@angular/core';
import { Observable, Subject, BehaviorSubject, combineLatest } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap, map, startWith, shareReplay } from 'rxjs/operators';
import { PerformanceService } from './performance.service';
import { DEBOUNCE_CONFIG } from '../../core/config/api-optimization.config';

/**
 * Optimized search service with debouncing, caching, and performance monitoring
 */
@Injectable({
  providedIn: 'root'
})
export class OptimizedSearchService {
  private performanceService = inject(PerformanceService);
  
  // Search subjects for different components
  private searchSubjects = new Map<string, Subject<string>>();
  private filterSubjects = new Map<string, BehaviorSubject<any>>();
  
  /**
   * Create optimized search observable for a component
   */
  createSearch<T>(
    componentId: string,
    searchFn: (term: string, filters?: any) => Observable<T[]>,
    options: SearchOptions = {}
  ): SearchResult<T> {
    
    const searchSubject = this.getOrCreateSearchSubject(componentId);
    const filterSubject = this.getOrCreateFilterSubject(componentId);
    
    const config = {
      debounceMs: options.debounceMs || DEBOUNCE_CONFIG.search.delay,
      minLength: options.minLength || DEBOUNCE_CONFIG.search.minLength,
      cacheResults: options.cacheResults !== false
    };
    
    // Create optimized search stream
    const searchTerm$ = searchSubject.pipe(
      debounceTime(config.debounceMs),
      distinctUntilChanged(),
      startWith('')
    );
    
    const filters$ = filterSubject.pipe(
      startWith({})
    );
    
    // Combine search term and filters
    const results$ = combineLatest([searchTerm$, filters$]).pipe(
      switchMap(([term, filters]) => {
        // Skip search if term is too short
        if (term.length > 0 && term.length < config.minLength) {
          return of([]);
        }
        
        // Use performance service for caching if enabled
        if (config.cacheResults) {
          return this.performanceService.optimizeSearch(
            of(term),
            (searchTerm) => searchFn(searchTerm, filters),
            config.debounceMs
          );
        } else {
          return searchFn(term, filters);
        }
      }),
      shareReplay(1)
    );
    
    // Loading state
    const loading$ = new BehaviorSubject<boolean>(false);
    
    // Track loading state
    const resultsWithLoading$ = combineLatest([searchTerm$, filters$]).pipe(
      switchMap(([term, filters]) => {
        loading$.next(true);
        
        const search$ = config.cacheResults
          ? this.performanceService.optimizeSearch(
              of(term),
              (searchTerm) => searchFn(searchTerm, filters),
              config.debounceMs
            )
          : searchFn(term, filters);
        
        return search$.pipe(
          map(results => {
            loading$.next(false);
            return results;
          })
        );
      }),
      shareReplay(1)
    );
    
    return {
      results$: resultsWithLoading$,
      loading$: loading$.asObservable(),
      search: (term: string) => searchSubject.next(term),
      setFilters: (filters: any) => filterSubject.next(filters),
      clear: () => {
        searchSubject.next('');
        filterSubject.next({});
      }
    };
  }
  
  /**
   * Create optimized filter observable
   */
  createFilter<T>(
    componentId: string,
    filterFn: (filters: any) => Observable<T[]>,
    options: FilterOptions = {}
  ): FilterResult<T> {
    
    const filterSubject = this.getOrCreateFilterSubject(componentId);
    
    const config = {
      debounceMs: options.debounceMs || DEBOUNCE_CONFIG.filter.delay,
      cacheResults: options.cacheResults !== false
    };
    
    const results$ = filterSubject.pipe(
      debounceTime(config.debounceMs),
      distinctUntilChanged((prev, curr) => JSON.stringify(prev) === JSON.stringify(curr)),
      switchMap(filters => {
        if (config.cacheResults) {
          const cacheKey = `filter:${componentId}:${JSON.stringify(filters)}`;
          return this.performanceService.createCachedObservable(
            cacheKey,
            () => filterFn(filters),
            'search'
          );
        } else {
          return filterFn(filters);
        }
      }),
      shareReplay(1)
    );
    
    return {
      results$,
      setFilters: (filters: any) => filterSubject.next(filters),
      clearFilters: () => filterSubject.next({})
    };
  }
  
  /**
   * Create combined search and filter
   */
  createSearchWithFilters<T>(
    componentId: string,
    searchFn: (term: string, filters: any) => Observable<T[]>,
    options: SearchOptions & FilterOptions = {}
  ): SearchWithFiltersResult<T> {
    
    const searchResult = this.createSearch(componentId, searchFn, options);
    const filterSubject = this.getOrCreateFilterSubject(componentId);
    
    return {
      ...searchResult,
      setFilters: (filters: any) => filterSubject.next(filters),
      clearFilters: () => filterSubject.next({})
    };
  }
  
  /**
   * Preload search results for common terms
   */
  preloadCommonSearches<T>(
    componentId: string,
    searchFn: (term: string) => Observable<T[]>,
    commonTerms: string[]
  ): void {
    commonTerms.forEach(term => {
      const cacheKey = `search:${componentId}:${term}`;
      this.performanceService.createCachedObservable(
        cacheKey,
        () => searchFn(term),
        'search'
      ).subscribe(); // Execute to populate cache
    });
  }
  
  /**
   * Clear search cache for component
   */
  clearSearchCache(componentId: string): void {
    const pattern = new RegExp(`^search:${componentId}:`);
    // This would need to be implemented in the cache service
    console.log(`Clearing search cache for component: ${componentId}`);
  }
  
  /**
   * Get search performance metrics
   */
  getSearchMetrics(componentId: string): Observable<SearchMetrics> {
    return this.performanceService.getPerformanceMetrics().pipe(
      map(metrics => ({
        componentId,
        averageResponseTime: metrics.averageResponseTime,
        cacheHitRate: metrics.cacheHitCount / (metrics.apiCallCount || 1),
        totalSearches: metrics.apiCallCount,
        slowSearches: metrics.slowOperations.length
      }))
    );
  }
  
  private getOrCreateSearchSubject(componentId: string): Subject<string> {
    if (!this.searchSubjects.has(componentId)) {
      this.searchSubjects.set(componentId, new Subject<string>());
    }
    return this.searchSubjects.get(componentId)!;
  }
  
  private getOrCreateFilterSubject(componentId: string): BehaviorSubject<any> {
    if (!this.filterSubjects.has(componentId)) {
      this.filterSubjects.set(componentId, new BehaviorSubject<any>({}));
    }
    return this.filterSubjects.get(componentId)!;
  }
  
  /**
   * Cleanup resources for component
   */
  cleanup(componentId: string): void {
    const searchSubject = this.searchSubjects.get(componentId);
    if (searchSubject) {
      searchSubject.complete();
      this.searchSubjects.delete(componentId);
    }
    
    const filterSubject = this.filterSubjects.get(componentId);
    if (filterSubject) {
      filterSubject.complete();
      this.filterSubjects.delete(componentId);
    }
  }
}

// Interfaces
export interface SearchOptions {
  debounceMs?: number;
  minLength?: number;
  cacheResults?: boolean;
}

export interface FilterOptions {
  debounceMs?: number;
  cacheResults?: boolean;
}

export interface SearchResult<T> {
  results$: Observable<T[]>;
  loading$: Observable<boolean>;
  search: (term: string) => void;
  setFilters: (filters: any) => void;
  clear: () => void;
}

export interface FilterResult<T> {
  results$: Observable<T[]>;
  setFilters: (filters: any) => void;
  clearFilters: () => void;
}

export interface SearchWithFiltersResult<T> extends SearchResult<T> {
  clearFilters: () => void;
}

export interface SearchMetrics {
  componentId: string;
  averageResponseTime: number;
  cacheHitRate: number;
  totalSearches: number;
  slowSearches: number;
}

// Helper function for the 'of' operator
function of<T>(value: T): Observable<T> {
  return new Observable(subscriber => {
    subscriber.next(value);
    subscriber.complete();
  });
}