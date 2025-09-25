import { Injectable, OnDestroy, Directive } from '@angular/core';
import { Subject, Subscription } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

/**
 * Memory management utilities for preventing memory leaks
 * Provides automatic cleanup for subscriptions, event listeners, and Google Maps
 */

/**
 * Base class for components that need automatic cleanup
 */
@Directive()
export abstract class BaseComponent implements OnDestroy {
  protected destroy$ = new Subject<void>();

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Helper method to automatically unsubscribe from observables
   */
  protected takeUntilDestroy() {
    return takeUntil(this.destroy$);
  }
}

/**
 * Subscription manager for manual subscription handling
 */
export class SubscriptionManager {
  private subscriptions: Subscription[] = [];

  /**
   * Add subscription to be managed
   */
  add(subscription: Subscription): void {
    this.subscriptions.push(subscription);
  }

  /**
   * Unsubscribe from all managed subscriptions
   */
  unsubscribeAll(): void {
    this.subscriptions.forEach(sub => {
      if (sub && !sub.closed) {
        sub.unsubscribe();
      }
    });
    this.subscriptions = [];
  }

  /**
   * Get count of active subscriptions
   */
  getActiveCount(): number {
    return this.subscriptions.filter(sub => sub && !sub.closed).length;
  }
}

/**
 * Event listener manager for DOM events
 */
export class EventListenerManager {
  private listeners: Array<{
    element: Element | Window | Document;
    event: string;
    handler: EventListener;
    options?: boolean | AddEventListenerOptions;
  }> = [];

  /**
   * Add event listener with automatic cleanup tracking
   */
  addEventListener(
    element: Element | Window | Document,
    event: string,
    handler: EventListener,
    options?: boolean | AddEventListenerOptions
  ): void {
    element.addEventListener(event, handler, options);
    this.listeners.push({ element, event, handler, options });
  }

  /**
   * Remove specific event listener
   */
  removeEventListener(
    element: Element | Window | Document,
    event: string,
    handler: EventListener
  ): void {
    element.removeEventListener(event, handler);
    this.listeners = this.listeners.filter(
      listener => !(listener.element === element && 
                   listener.event === event && 
                   listener.handler === handler)
    );
  }

  /**
   * Remove all tracked event listeners
   */
  removeAllListeners(): void {
    this.listeners.forEach(({ element, event, handler }) => {
      element.removeEventListener(event, handler);
    });
    this.listeners = [];
  }

  /**
   * Get count of active listeners
   */
  getActiveCount(): number {
    return this.listeners.length;
  }
}

/**
 * Google Maps memory manager
 */
export class GoogleMapsManager {
  private mapInstances: google.maps.Map[] = [];
  private markerInstances: google.maps.Marker[] = [];
  private listenerInstances: google.maps.MapsEventListener[] = [];
  private overlayInstances: google.maps.OverlayView[] = [];

  /**
   * Register map instance for cleanup
   */
  registerMap(map: google.maps.Map): void {
    this.mapInstances.push(map);
  }

  /**
   * Register marker instance for cleanup
   */
  registerMarker(marker: google.maps.Marker): void {
    this.markerInstances.push(marker);
  }

  /**
   * Register event listener for cleanup
   */
  registerListener(listener: google.maps.MapsEventListener): void {
    this.listenerInstances.push(listener);
  }

  /**
   * Register overlay for cleanup
   */
  registerOverlay(overlay: google.maps.OverlayView): void {
    this.overlayInstances.push(overlay);
  }

  /**
   * Add event listener with automatic registration
   */
  addListener(
    instance: any,
    eventName: string,
    handler: (...args: any[]) => void
  ): google.maps.MapsEventListener {
    const listener = google.maps.event.addListener(instance, eventName, handler);
    this.registerListener(listener);
    return listener;
  }

  /**
   * Create marker with automatic registration
   */
  createMarker(options: google.maps.MarkerOptions): google.maps.Marker {
    const marker = new google.maps.Marker(options);
    this.registerMarker(marker);
    return marker;
  }

  /**
   * Clean up all Google Maps resources
   */
  cleanup(): void {
    // Remove all event listeners
    this.listenerInstances.forEach(listener => {
      if (listener) {
        google.maps.event.removeListener(listener);
      }
    });
    this.listenerInstances = [];

    // Clear all markers
    this.markerInstances.forEach(marker => {
      if (marker) {
        marker.setMap(null);
      }
    });
    this.markerInstances = [];

    // Clear all overlays
    this.overlayInstances.forEach(overlay => {
      if (overlay) {
        overlay.setMap(null);
      }
    });
    this.overlayInstances = [];

    // Clear maps (this should be done last)
    this.mapInstances.forEach(map => {
      if (map) {
        // Clear all overlays and markers from map
        google.maps.event.clearInstanceListeners(map);
      }
    });
    this.mapInstances = [];
  }

  /**
   * Get memory usage statistics
   */
  getStats(): GoogleMapsStats {
    return {
      mapCount: this.mapInstances.length,
      markerCount: this.markerInstances.length,
      listenerCount: this.listenerInstances.length,
      overlayCount: this.overlayInstances.length
    };
  }
}

/**
 * Timer manager for setTimeout and setInterval
 */
export class TimerManager {
  private timeouts: NodeJS.Timeout[] = [];
  private intervals: NodeJS.Timeout[] = [];

  /**
   * Create timeout with automatic cleanup tracking
   */
  setTimeout(callback: () => void, delay: number): NodeJS.Timeout {
    const timeout = setTimeout(() => {
      callback();
      this.removeTimeout(timeout);
    }, delay);
    
    this.timeouts.push(timeout);
    return timeout;
  }

  /**
   * Create interval with automatic cleanup tracking
   */
  setInterval(callback: () => void, delay: number): NodeJS.Timeout {
    const interval = setInterval(callback, delay);
    this.intervals.push(interval);
    return interval;
  }

  /**
   * Clear specific timeout
   */
  clearTimeout(timeout: NodeJS.Timeout): void {
    clearTimeout(timeout);
    this.removeTimeout(timeout);
  }

  /**
   * Clear specific interval
   */
  clearInterval(interval: NodeJS.Timeout): void {
    clearInterval(interval);
    this.removeInterval(interval);
  }

  /**
   * Clear all timers
   */
  clearAll(): void {
    this.timeouts.forEach(timeout => clearTimeout(timeout));
    this.intervals.forEach(interval => clearInterval(interval));
    this.timeouts = [];
    this.intervals = [];
  }

  private removeTimeout(timeout: NodeJS.Timeout): void {
    const index = this.timeouts.indexOf(timeout);
    if (index > -1) {
      this.timeouts.splice(index, 1);
    }
  }

  private removeInterval(interval: NodeJS.Timeout): void {
    const index = this.intervals.indexOf(interval);
    if (index > -1) {
      this.intervals.splice(index, 1);
    }
  }

  /**
   * Get active timer counts
   */
  getStats(): TimerStats {
    return {
      timeoutCount: this.timeouts.length,
      intervalCount: this.intervals.length
    };
  }
}

/**
 * Comprehensive memory manager that combines all utilities
 */
@Injectable({
  providedIn: 'root'
})
export class MemoryManager {
  private subscriptionManager = new SubscriptionManager();
  private eventListenerManager = new EventListenerManager();
  private googleMapsManager = new GoogleMapsManager();
  private timerManager = new TimerManager();

  /**
   * Get subscription manager
   */
  getSubscriptionManager(): SubscriptionManager {
    return this.subscriptionManager;
  }

  /**
   * Get event listener manager
   */
  getEventListenerManager(): EventListenerManager {
    return this.eventListenerManager;
  }

  /**
   * Get Google Maps manager
   */
  getGoogleMapsManager(): GoogleMapsManager {
    return this.googleMapsManager;
  }

  /**
   * Get timer manager
   */
  getTimerManager(): TimerManager {
    return this.timerManager;
  }

  /**
   * Perform complete cleanup of all managed resources
   */
  performFullCleanup(): void {
    this.subscriptionManager.unsubscribeAll();
    this.eventListenerManager.removeAllListeners();
    this.googleMapsManager.cleanup();
    this.timerManager.clearAll();
  }

  /**
   * Get comprehensive memory usage statistics
   */
  getMemoryStats(): MemoryStats {
    return {
      subscriptions: this.subscriptionManager.getActiveCount(),
      eventListeners: this.eventListenerManager.getActiveCount(),
      googleMaps: this.googleMapsManager.getStats(),
      timers: this.timerManager.getStats()
    };
  }

  /**
   * Monitor memory usage and warn if thresholds are exceeded
   */
  monitorMemoryUsage(): void {
    const stats = this.getMemoryStats();
    
    // Warn about potential memory leaks
    if (stats.subscriptions > 50) {
      console.warn(`High subscription count detected: ${stats.subscriptions}`);
    }
    
    if (stats.eventListeners > 100) {
      console.warn(`High event listener count detected: ${stats.eventListeners}`);
    }
    
    if (stats.googleMaps.markerCount > 1000) {
      console.warn(`High Google Maps marker count detected: ${stats.googleMaps.markerCount}`);
    }
  }
}

/**
 * Interfaces for statistics
 */
export interface GoogleMapsStats {
  mapCount: number;
  markerCount: number;
  listenerCount: number;
  overlayCount: number;
}

export interface TimerStats {
  timeoutCount: number;
  intervalCount: number;
}

export interface MemoryStats {
  subscriptions: number;
  eventListeners: number;
  googleMaps: GoogleMapsStats;
  timers: TimerStats;
}

/**
 * Decorator for automatic cleanup
 */
export function AutoCleanup(constructor: Function) {
  const originalOnDestroy = constructor.prototype.ngOnDestroy;

  if (typeof originalOnDestroy !== 'function') {
    console.warn(`AutoCleanup decorator is used on a class that does not have a ngOnDestroy method: ${constructor.name}`);
  }

  constructor.prototype.ngOnDestroy = function () {
    if (this.memoryManager && typeof this.memoryManager.performFullCleanup === 'function') {
      this.memoryManager.performFullCleanup();
    }

    if (originalOnDestroy) {
      originalOnDestroy.apply(this, arguments);
    }
  };
}