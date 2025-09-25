import { Component, OnInit, OnDestroy, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Card } from 'primeng/card';
import { Button } from 'primeng/button';
import { Badge } from 'primeng/badge';
import { ProgressBar } from 'primeng/progressbar';
import { Message } from 'primeng/message';
import { interval } from 'rxjs';

import { PerformanceService, PerformanceMetrics } from '../../core/services/performance.service';
import { MemoryManager, MemoryStats } from '../utils/memory-management.util';
import { BaseComponent } from '../utils/memory-management.util';

/**
 * Performance monitoring component for development and debugging
 * Displays real-time performance metrics and memory usage
 */
@Component({
  selector: 'app-performance-monitor',
  standalone: true,
  imports: [
    CommonModule,
    Card,
    Button,
    Badge,
    ProgressBar,
    Message
  ],
  template: `
    <p-card header="Performance Monitor" [style]="{ 'position': 'fixed', 'top': '10px', 'right': '10px', 'z-index': '9999', 'width': '300px' }" *ngIf="visible()">
      <div class="performance-metrics">
        <!-- Performance Metrics -->
        <div class="metric-group">
          <h4>Performance</h4>
          <div class="metric">
            <span>Navigation Time:</span>
            <p-badge [value]="formatTime(performanceMetrics().navigationTime)" severity="info"></p-badge>
          </div>
          <div class="metric">
            <span>Component Load:</span>
            <p-badge [value]="formatTime(performanceMetrics().componentLoadTime)" severity="info"></p-badge>
          </div>
          <div class="metric">
            <span>Memory Usage:</span>
            <p-badge [value]="formatMemory(performanceMetrics().memoryUsage)" 
                     [severity]="getMemorySeverity(performanceMetrics().memoryUsage)"></p-badge>
          </div>
        </div>

        <!-- Memory Statistics -->
        <div class="metric-group">
          <h4>Memory</h4>
          <div class="metric">
            <span>Subscriptions:</span>
            <p-badge [value]="memoryStats().subscriptions.toString()" 
                     [severity]="getCountSeverity(memoryStats().subscriptions, 50)"></p-badge>
          </div>
          <div class="metric">
            <span>Event Listeners:</span>
            <p-badge [value]="memoryStats().eventListeners.toString()" 
                     [severity]="getCountSeverity(memoryStats().eventListeners, 100)"></p-badge>
          </div>
          <div class="metric">
            <span>Maps Markers:</span>
            <p-badge [value]="memoryStats().googleMaps.markerCount.toString()" 
                     [severity]="getCountSeverity(memoryStats().googleMaps.markerCount, 1000)"></p-badge>
          </div>
          <div class="metric">
            <span>Timers:</span>
            <p-badge [value]="(memoryStats().timers.timeoutCount + memoryStats().timers.intervalCount).toString()" 
                     [severity]="getCountSeverity(memoryStats().timers.timeoutCount + memoryStats().timers.intervalCount, 20)"></p-badge>
          </div>
        </div>

        <!-- Bundle Information -->
        <div class="metric-group" *ngIf="bundleInfo()">
          <h4>Bundle</h4>
          <div class="metric">
            <span>Total Size:</span>
            <p-badge [value]="formatSize(bundleInfo()!.totalSize)" severity="info"></p-badge>
          </div>
          <div class="metric">
            <span>JS Size:</span>
            <p-badge [value]="formatSize(bundleInfo()!.jsSize)" severity="info"></p-badge>
          </div>
          <div class="metric">
            <span>Resources:</span>
            <p-badge [value]="bundleInfo()!.resourceCount.toString()" severity="info"></p-badge>
          </div>
        </div>

        <!-- Actions -->
        <div class="actions">
          <p-button label="Cleanup" icon="pi pi-trash" size="small" 
                    (onClick)="performCleanup()" severity="warn"></p-button>
          <p-button label="Refresh" icon="pi pi-refresh" size="small" 
                    (onClick)="refreshMetrics()" severity="info"></p-button>
          <p-button label="Hide" icon="pi pi-times" size="small" 
                    (onClick)="hide()" severity="secondary"></p-button>
        </div>

        <!-- Warnings -->
        <div class="warnings" *ngIf="warnings().length > 0">
          <p-message *ngFor="let warning of warnings()" 
                     [severity]="warning.severity" 
                     [text]="warning.message"></p-message>
        </div>
      </div>
    </p-card>

    <!-- Toggle Button (when hidden) -->
    <p-button *ngIf="!visible()" 
              icon="pi pi-chart-line" 
              [style]="{ 'position': 'fixed', 'top': '10px', 'right': '10px', 'z-index': '9999' }"
              (onClick)="show()" 
              severity="info" 
              size="small"
              [rounded]="true"></p-button>
  `,
  styles: [`
    .performance-metrics {
      font-size: 0.875rem;
    }

    .metric-group {
      margin-bottom: 1rem;
      padding-bottom: 0.5rem;
      border-bottom: 1px solid var(--surface-border);
    }

    .metric-group:last-of-type {
      border-bottom: none;
    }

    .metric-group h4 {
      margin: 0 0 0.5rem 0;
      font-size: 0.875rem;
      font-weight: 600;
      color: var(--text-color-secondary);
    }

    .metric {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 0.25rem;
    }

    .metric span {
      font-size: 0.75rem;
      color: var(--text-color-secondary);
    }

    .actions {
      display: flex;
      gap: 0.5rem;
      flex-wrap: wrap;
      margin-top: 1rem;
    }

    .warnings {
      margin-top: 1rem;
    }

    .warnings p-message {
      margin-bottom: 0.5rem;
    }
  `]
})
export class PerformanceMonitorComponent extends BaseComponent implements OnInit {
  private performanceService = inject(PerformanceService);
  private memoryManager = inject(MemoryManager);

  // Component state
  visible = signal(false);
  performanceMetrics = signal<PerformanceMetrics>({
    navigationTime: 0,
    componentLoadTime: 0,
    memoryUsage: 0,
    bundleSize: 0
  });
  memoryStats = signal<MemoryStats>({
    subscriptions: 0,
    eventListeners: 0,
    googleMaps: {
      mapCount: 0,
      markerCount: 0,
      listenerCount: 0,
      overlayCount: 0
    },
    timers: {
      timeoutCount: 0,
      intervalCount: 0
    }
  });
  bundleInfo = signal<any>(null);
  warnings = signal<Array<{ severity: string; message: string }>>([]);

  ngOnInit(): void {
    // Only show in development mode
    if (!this.isDevelopmentMode()) {
      return;
    }

    this.setupPerformanceMonitoring();
    this.refreshMetrics();
  }

  /**
   * Check if running in development mode
   */
  private isDevelopmentMode(): boolean {
    return !!(window as any)['ng'] || location.hostname === 'localhost';
  }

  /**
   * Setup performance monitoring subscriptions
   */
  private setupPerformanceMonitoring(): void {
    // Subscribe to performance metrics
    this.performanceService.getPerformanceMetrics()
      .pipe(this.takeUntilDestroy())
      .subscribe(metrics => {
        this.performanceMetrics.set(metrics as PerformanceMetrics);
        this.checkForWarnings(metrics as PerformanceMetrics);
      });

    // Refresh metrics periodically
    interval(5000) // Every 5 seconds
      .pipe(this.takeUntilDestroy())
      .subscribe(() => {
        this.refreshMetrics();
      });
  }

  /**
   * Refresh all metrics
   */
  refreshMetrics(): void {
    // Update memory stats
    this.memoryStats.set(this.memoryManager.getMemoryStats());

    // Update bundle info
    this.performanceService.getBundleInfo().then(info => {
      this.bundleInfo.set(info);
    });

    // Monitor memory usage
    this.performanceService.monitorMemoryUsage();
    this.memoryManager.monitorMemoryUsage();
  }

  /**
   * Check for performance warnings
   */
  private checkForWarnings(metrics: PerformanceMetrics): void {
    const warnings: Array<{ severity: string; message: string }> = [];
    const memStats = this.memoryStats();

    // Navigation time warnings
    if (metrics.navigationTime > 3000) {
      warnings.push({
        severity: 'warn',
        message: `Slow navigation: ${this.formatTime(metrics.navigationTime)}`
      });
    }

    // Memory warnings
    if (metrics.memoryUsage > 100) {
      warnings.push({
        severity: 'error',
        message: `High memory usage: ${this.formatMemory(metrics.memoryUsage)}`
      });
    }

    // Subscription warnings
    if (memStats.subscriptions > 50) {
      warnings.push({
        severity: 'warn',
        message: `High subscription count: ${memStats.subscriptions}`
      });
    }

    // Event listener warnings
    if (memStats.eventListeners > 100) {
      warnings.push({
        severity: 'warn',
        message: `High event listener count: ${memStats.eventListeners}`
      });
    }

    // Google Maps warnings
    if (memStats.googleMaps.markerCount > 1000) {
      warnings.push({
        severity: 'warn',
        message: `High marker count: ${memStats.googleMaps.markerCount}`
      });
    }

    this.warnings.set(warnings);
  }

  /**
   * Perform memory cleanup
   */
  performCleanup(): void {
    this.memoryManager.performFullCleanup();
    this.performanceService.executeCleanup();
    this.refreshMetrics();
  }

  /**
   * Show the monitor
   */
  show(): void {
    this.visible.set(true);
    this.refreshMetrics();
  }

  /**
   * Hide the monitor
   */
  hide(): void {
    this.visible.set(false);
  }

  /**
   * Format time in milliseconds
   */
  formatTime(ms: number): string {
    if (ms < 1000) {
      return `${Math.round(ms)}ms`;
    } else {
      return `${(ms / 1000).toFixed(1)}s`;
    }
  }

  /**
   * Format memory in MB
   */
  formatMemory(mb: number): string {
    return `${mb.toFixed(1)}MB`;
  }

  /**
   * Format size in KB
   */
  formatSize(kb: number): string {
    if (kb < 1024) {
      return `${Math.round(kb)}KB`;
    } else {
      return `${(kb / 1024).toFixed(1)}MB`;
    }
  }

  /**
   * Get severity for memory usage
   */
  getMemorySeverity(mb: number): 'danger' | 'warn' | 'success' {
    if (mb > 100) return 'danger';
    if (mb > 50) return 'warn';
    return 'success';
  }

  /**
   * Get severity for count metrics
   */
  getCountSeverity(count: number, threshold: number): 'danger' | 'warn' | 'success' {
    if (count > threshold) return 'danger';
    if (count > threshold * 0.7) return 'warn';
    return 'success';
  }
}