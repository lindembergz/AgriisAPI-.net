import { Injectable, inject } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { filter, map } from 'rxjs/operators';
import { BehaviorSubject, Observable } from 'rxjs';

/**
 * Performance monitoring and optimization service
 * Tracks application performance metrics and provides optimization utilities
 */
@Injectable({
  providedIn: 'root'
})
export class PerformanceService {
  private router = inject(Router);
  
  // Performance metrics
  private performanceMetrics$ = new BehaviorSubject<PerformanceMetrics>({
    navigationTime: 0,
    componentLoadTime: 0,
    memoryUsage: 0,
    bundleSize: 0
  });

  // Memory cleanup registry
  private cleanupTasks: (() => void)[] = [];
  
  // Performance observer for monitoring
  private performanceObserver?: PerformanceObserver;

  constructor() {
    this.initializePerformanceMonitoring();
    this.setupNavigationTracking();
  }

  /**
   * Initialize performance monitoring
   */
  private initializePerformanceMonitoring(): void {
    if ('PerformanceObserver' in window) {
      this.performanceObserver = new PerformanceObserver((list) => {
        const entries = list.getEntries();
        this.processPerformanceEntries(entries);
      });

      // Observe navigation and resource loading
      this.performanceObserver.observe({ 
        entryTypes: ['navigation', 'resource', 'measure'] 
      });
    }
  }

  /**
   * Setup navigation performance tracking
   */
  private setupNavigationTracking(): void {
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd),
      map(event => event as NavigationEnd)
    ).subscribe((event) => {
      this.trackNavigationPerformance(event.url);
    });
  }

  /**
   * Process performance entries
   */
  private processPerformanceEntries(entries: PerformanceEntry[]): void {
    entries.forEach(entry => {
      if (entry.entryType === 'navigation') {
        const navEntry = entry as PerformanceNavigationTiming;
        this.updateMetrics({
          navigationTime: navEntry.loadEventEnd - navEntry.startTime
        });
      }
    });
  }

  /**
   * Track navigation performance
   */
  private trackNavigationPerformance(url: string): void {
    const startTime = performance.now();
    
    // Use requestIdleCallback to measure when navigation is complete
    if ('requestIdleCallback' in window) {
      requestIdleCallback(() => {
        const endTime = performance.now();
        const navigationTime = endTime - startTime;
        
        this.updateMetrics({ navigationTime });
        this.logPerformanceMetric('Navigation', url, navigationTime);
      });
    }
  }

  /**
   * Update performance metrics
   */
  private updateMetrics(updates: Partial<PerformanceMetrics>): void {
    const current = this.performanceMetrics$.value;
    this.performanceMetrics$.next({ ...current, ...updates });
  }

  /**
   * Log performance metric
   */
  private logPerformanceMetric(type: string, context: string, duration: number): void {
    if (duration > 1000) { // Log slow operations (> 1 second)
      console.warn(`Slow ${type}: ${context} took ${duration.toFixed(2)}ms`);
    }
  }

  /**
   * Get current performance metrics
   */
  getPerformanceMetrics(): Observable<PerformanceMetrics> {
    return this.performanceMetrics$.asObservable();
  }

  /**
   * Measure component load time
   */
  measureComponentLoad<T>(componentName: string, loadFn: () => T): T {
    const startTime = performance.now();
    const result = loadFn();
    const endTime = performance.now();
    const loadTime = endTime - startTime;

    this.updateMetrics({ componentLoadTime: loadTime });
    this.logPerformanceMetric('Component Load', componentName, loadTime);

    return result;
  }

  /**
   * Register cleanup task for memory management
   */
  registerCleanupTask(cleanupFn: () => void): void {
    this.cleanupTasks.push(cleanupFn);
  }

  /**
   * Execute all registered cleanup tasks
   */
  executeCleanup(): void {
    this.cleanupTasks.forEach(task => {
      try {
        task();
      } catch (error) {
        console.error('Cleanup task failed:', error);
      }
    });
    this.cleanupTasks = [];
  }

  /**
   * Monitor memory usage
   */
  monitorMemoryUsage(): void {
    if ('memory' in performance) {
      const memInfo = (performance as any).memory;
      const memoryUsage = memInfo.usedJSHeapSize / 1024 / 1024; // MB
      
      this.updateMetrics({ memoryUsage });
      
      // Warn if memory usage is high
      if (memoryUsage > 100) { // > 100MB
        console.warn(`High memory usage detected: ${memoryUsage.toFixed(2)}MB`);
      }
    }
  }

  /**
   * Optimize images for better performance
   */
  optimizeImage(imageUrl: string, maxWidth: number = 800): Promise<string> {
    return new Promise((resolve) => {
      const img = new Image();
      img.onload = () => {
        const canvas = document.createElement('canvas');
        const ctx = canvas.getContext('2d');
        
        if (!ctx) {
          resolve(imageUrl);
          return;
        }

        // Calculate new dimensions
        const ratio = Math.min(maxWidth / img.width, maxWidth / img.height);
        canvas.width = img.width * ratio;
        canvas.height = img.height * ratio;

        // Draw and compress
        ctx.drawImage(img, 0, 0, canvas.width, canvas.height);
        const optimizedUrl = canvas.toDataURL('image/jpeg', 0.8);
        
        resolve(optimizedUrl);
      };
      img.src = imageUrl;
    });
  }

  /**
   * Preload critical resources
   */
  preloadCriticalResources(resources: string[]): void {
    resources.forEach(resource => {
      const link = document.createElement('link');
      link.rel = 'preload';
      link.href = resource;
      
      // Determine resource type
      if (resource.endsWith('.js')) {
        link.as = 'script';
      } else if (resource.endsWith('.css')) {
        link.as = 'style';
      } else if (resource.match(/\.(jpg|jpeg|png|webp)$/)) {
        link.as = 'image';
      }
      
      document.head.appendChild(link);
    });
  }

  /**
   * Lazy load non-critical resources
   */
  lazyLoadResource(resourceUrl: string, type: 'script' | 'style'): Promise<void> {
    return new Promise((resolve, reject) => {
      let element: HTMLScriptElement | HTMLLinkElement;
      
      if (type === 'script') {
        element = document.createElement('script');
        (element as HTMLScriptElement).src = resourceUrl;
        (element as HTMLScriptElement).async = true;
      } else {
        element = document.createElement('link');
        (element as HTMLLinkElement).rel = 'stylesheet';
        (element as HTMLLinkElement).href = resourceUrl;
      }
      
      element.onload = () => resolve();
      element.onerror = () => reject(new Error(`Failed to load ${resourceUrl}`));
      
      document.head.appendChild(element);
    });
  }

  /**
   * Debounce function for performance optimization
   */
  debounce<T extends (...args: any[]) => any>(
    func: T, 
    wait: number
  ): (...args: Parameters<T>) => void {
    let timeout: NodeJS.Timeout;
    
    return (...args: Parameters<T>) => {
      clearTimeout(timeout);
      timeout = setTimeout(() => func.apply(this, args), wait);
    };
  }

  /**
   * Throttle function for performance optimization
   */
  throttle<T extends (...args: any[]) => any>(
    func: T, 
    limit: number
  ): (...args: Parameters<T>) => void {
    let inThrottle: boolean;
    
    return (...args: Parameters<T>) => {
      if (!inThrottle) {
        func.apply(this, args);
        inThrottle = true;
        setTimeout(() => inThrottle = false, limit);
      }
    };
  }

  /**
   * Get bundle size information
   */
  getBundleInfo(): Promise<BundleInfo> {
    return new Promise((resolve) => {
      // Estimate bundle size from loaded resources
      const resources = performance.getEntriesByType('resource') as PerformanceResourceTiming[];
      let totalSize = 0;
      let jsSize = 0;
      let cssSize = 0;
      
      resources.forEach(resource => {
        const size = resource.transferSize || 0;
        totalSize += size;
        
        if (resource.name.endsWith('.js')) {
          jsSize += size;
        } else if (resource.name.endsWith('.css')) {
          cssSize += size;
        }
      });
      
      resolve({
        totalSize: totalSize / 1024, // KB
        jsSize: jsSize / 1024, // KB
        cssSize: cssSize / 1024, // KB
        resourceCount: resources.length
      });
    });
  }

  /**
   * Clean up performance monitoring
   */
  cleanup(): void {
    if (this.performanceObserver) {
      this.performanceObserver.disconnect();
    }
    this.executeCleanup();
  }
}

/**
 * Performance metrics interface
 */
export interface PerformanceMetrics {
  navigationTime: number;
  componentLoadTime: number;
  memoryUsage: number;
  bundleSize: number;
}

/**
 * Bundle information interface
 */
export interface BundleInfo {
  totalSize: number;
  jsSize: number;
  cssSize: number;
  resourceCount: number;
}