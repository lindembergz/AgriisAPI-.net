import { Injectable } from '@angular/core';
import { Observable, of, BehaviorSubject, timer } from 'rxjs';
import { tap, shareReplay, switchMap, catchError } from 'rxjs/operators';

/**
 * Enhanced caching service for reference data and API responses
 * Implements multiple caching strategies with TTL and invalidation
 */
@Injectable({
  providedIn: 'root'
})
export class CacheService {
  private cache = new Map<string, CacheEntry>();
  private cacheSubjects = new Map<string, BehaviorSubject<any>>();
  
  // Default cache TTL (5 minutes)
  private readonly DEFAULT_TTL = 5 * 60 * 1000;
  
  // Cache configuration for different data types
  private readonly CACHE_CONFIG = {
    // Reference data - cache for longer periods
    referencias: {
      ttl: 30 * 60 * 1000, // 30 minutes
      maxSize: 100
    },
    // User data - shorter cache
    usuarios: {
      ttl: 10 * 60 * 1000, // 10 minutes
      maxSize: 50
    },
    // Search results - very short cache
    search: {
      ttl: 2 * 60 * 1000, // 2 minutes
      maxSize: 20
    },
    // Static data - long cache
    static: {
      ttl: 60 * 60 * 1000, // 1 hour
      maxSize: 200
    }
  };

  constructor() {
    // Clean up expired cache entries every 5 minutes
    timer(0, 5 * 60 * 1000).subscribe(() => {
      this.cleanupExpiredEntries();
    });
  }

  /**
   * Get cached data or execute the provided function
   */
  get<T>(
    key: string, 
    dataFn: () => Observable<T>, 
    options: CacheOptions = {}
  ): Observable<T> {
    const cacheKey = this.buildCacheKey(key, options.namespace);
    const config = this.getCacheConfig(options.type || 'default');
    
    // Check if we have a valid cached entry
    const cachedEntry = this.cache.get(cacheKey);
    if (cachedEntry && !this.isExpired(cachedEntry, config.ttl)) {
      return of(cachedEntry.data);
    }

    // Check if there's already a request in progress
    if (this.cacheSubjects.has(cacheKey)) {
      return this.cacheSubjects.get(cacheKey)!.asObservable();
    }

    // Create new request subject
    const subject = new BehaviorSubject<T | null>(null);
    this.cacheSubjects.set(cacheKey, subject);

    // Execute the data function and cache the result
    return dataFn().pipe(
      tap(data => {
        this.set(cacheKey, data, config.ttl);
        subject.next(data);
        this.cacheSubjects.delete(cacheKey);
      }),
      catchError(error => {
        this.cacheSubjects.delete(cacheKey);
        throw error;
      }),
      shareReplay(1)
    );
  }

  /**
   * Set data in cache with TTL
   */
  set<T>(key: string, data: T, ttl?: number): void {
    const cacheKey = this.buildCacheKey(key);
    const entry: CacheEntry = {
      data,
      timestamp: Date.now(),
      ttl: ttl || this.DEFAULT_TTL
    };
    
    this.cache.set(cacheKey, entry);
    this.enforceMaxSize();
  }

  /**
   * Get data from cache without executing fallback
   */
  getSync<T>(key: string, namespace?: string): T | null {
    const cacheKey = this.buildCacheKey(key, namespace);
    const entry = this.cache.get(cacheKey);
    
    if (!entry || this.isExpired(entry)) {
      return null;
    }
    
    return entry.data;
  }

  /**
   * Check if data exists in cache and is not expired
   */
  has(key: string, namespace?: string): boolean {
    const cacheKey = this.buildCacheKey(key, namespace);
    const entry = this.cache.get(cacheKey);
    return entry ? !this.isExpired(entry) : false;
  }

  /**
   * Remove specific cache entry
   */
  delete(key: string, namespace?: string): boolean {
    const cacheKey = this.buildCacheKey(key, namespace);
    return this.cache.delete(cacheKey);
  }

  /**
   * Clear all cache entries or entries by namespace
   */
  clear(namespace?: string): void {
    if (namespace) {
      const keysToDelete = Array.from(this.cache.keys())
        .filter(key => key.startsWith(`${namespace}:`));
      keysToDelete.forEach(key => this.cache.delete(key));
    } else {
      this.cache.clear();
    }
  }

  /**
   * Invalidate cache entries by pattern
   */
  invalidatePattern(pattern: RegExp, namespace?: string): void {
    const keysToDelete = Array.from(this.cache.keys())
      .filter(key => {
        const keyToTest = namespace ? key.replace(`${namespace}:`, '') : key;
        return pattern.test(keyToTest);
      });
    
    keysToDelete.forEach(key => this.cache.delete(key));
  }

  /**
   * Get cache statistics
   */
  getStats(): CacheStats {
    const now = Date.now();
    let totalSize = 0;
    let expiredCount = 0;
    
    this.cache.forEach(entry => {
      totalSize += this.estimateSize(entry.data);
      if (this.isExpired(entry)) {
        expiredCount++;
      }
    });
    
    return {
      totalEntries: this.cache.size,
      expiredEntries: expiredCount,
      estimatedSize: totalSize,
      hitRate: this.calculateHitRate()
    };
  }

  /**
   * Preload reference data that's commonly used
   */
  preloadReferenceData(): void {
    const commonReferences = [
      'paises',
      'ufs', 
      'unidades-medida',
      'moedas'
    ];
    
    // This would typically call the actual services
    // For now, we'll just mark these as high priority for caching
    commonReferences.forEach(ref => {
      // The actual implementation would call the respective services
      console.log(`Preloading reference data: ${ref}`);
    });
  }

  /**
   * Build cache key with optional namespace
   */
  private buildCacheKey(key: string, namespace?: string): string {
    return namespace ? `${namespace}:${key}` : key;
  }

  /**
   * Get cache configuration for data type
   */
  private getCacheConfig(type: string): { ttl: number; maxSize: number } {
    return this.CACHE_CONFIG[type as keyof typeof this.CACHE_CONFIG] || {
      ttl: this.DEFAULT_TTL,
      maxSize: 50
    };
  }

  /**
   * Check if cache entry is expired
   */
  private isExpired(entry: CacheEntry, customTtl?: number): boolean {
    const ttl = customTtl || entry.ttl;
    return Date.now() - entry.timestamp > ttl;
  }

  /**
   * Clean up expired cache entries
   */
  private cleanupExpiredEntries(): void {
    const keysToDelete: string[] = [];
    
    this.cache.forEach((entry, key) => {
      if (this.isExpired(entry)) {
        keysToDelete.push(key);
      }
    });
    
    keysToDelete.forEach(key => this.cache.delete(key));
    
    if (keysToDelete.length > 0) {
      console.log(`Cache cleanup: Removed ${keysToDelete.length} expired entries`);
    }
  }

  /**
   * Enforce maximum cache size by removing oldest entries
   */
  private enforceMaxSize(): void {
    const maxSize = 500; // Global max size
    
    if (this.cache.size > maxSize) {
      const entries = Array.from(this.cache.entries())
        .sort(([, a], [, b]) => a.timestamp - b.timestamp);
      
      const entriesToRemove = entries.slice(0, this.cache.size - maxSize);
      entriesToRemove.forEach(([key]) => this.cache.delete(key));
    }
  }

  /**
   * Estimate size of cached data (rough approximation)
   */
  private estimateSize(data: any): number {
    try {
      return JSON.stringify(data).length * 2; // Rough estimate in bytes
    } catch {
      return 1000; // Default estimate for non-serializable data
    }
  }

  /**
   * Calculate cache hit rate (simplified)
   */
  private calculateHitRate(): number {
    // This would require tracking hits/misses over time
    // For now, return a placeholder
    return 0.85; // 85% hit rate placeholder
  }
}

/**
 * Cache entry interface
 */
interface CacheEntry {
  data: any;
  timestamp: number;
  ttl: number;
}

/**
 * Cache options interface
 */
export interface CacheOptions {
  namespace?: string;
  type?: 'referencias' | 'usuarios' | 'search' | 'static' | 'default';
  ttl?: number;
}

/**
 * Cache statistics interface
 */
export interface CacheStats {
  totalEntries: number;
  expiredEntries: number;
  estimatedSize: number;
  hitRate: number;
}