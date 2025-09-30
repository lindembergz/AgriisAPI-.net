import { Injectable, signal } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { ActiveFilter, ResponsiveConfig } from '../interfaces/component-template.interface';

/**
 * Service to manage component state across the application
 */
@Injectable({
  providedIn: 'root'
})
export class ComponentStateService {
  
  private readonly STORAGE_KEY = 'reference-component-state';
  
  // Responsive breakpoints
  private readonly defaultResponsiveConfig: ResponsiveConfig = {
    mobile: {
      breakpoint: 768,
      hiddenColumns: ['dataCriacao', 'dataAtualizacao'],
      compactMode: true,
      cardView: true
    },
    tablet: {
      breakpoint: 1024,
      hiddenColumns: ['dataCriacao'],
      compactMode: false
    },
    desktop: {
      breakpoint: 1200,
      features: ['resizable-columns', 'sortable-columns']
    }
  };

  // Current viewport size
  private viewportSize = signal<'mobile' | 'tablet' | 'desktop'>('desktop');
  
  // Filter state management
  private filterStates = new Map<string, Map<string, any>>();
  
  constructor() {
    this.initializeResponsiveDetection();
    this.loadFilterStates();
  }

  /**
   * Initialize responsive detection
   */
  private initializeResponsiveDetection(): void {
    if (typeof window !== 'undefined') {
      this.updateViewportSize();
      window.addEventListener('resize', () => this.updateViewportSize());
    }
  }

  /**
   * Update viewport size based on window width
   */
  private updateViewportSize(): void {
    const width = window.innerWidth;
    const config = this.defaultResponsiveConfig;
    
    if (width < config.mobile.breakpoint) {
      this.viewportSize.set('mobile');
    } else if (width < config.tablet.breakpoint) {
      this.viewportSize.set('tablet');
    } else {
      this.viewportSize.set('desktop');
    }
  }

  /**
   * Get current viewport size
   */
  getViewportSize(): 'mobile' | 'tablet' | 'desktop' {
    return this.viewportSize();
  }

  /**
   * Check if current viewport is mobile
   */
  isMobile(): boolean {
    return this.viewportSize() === 'mobile';
  }

  /**
   * Check if current viewport is tablet
   */
  isTablet(): boolean {
    return this.viewportSize() === 'tablet';
  }

  /**
   * Check if current viewport is desktop
   */
  isDesktop(): boolean {
    return this.viewportSize() === 'desktop';
  }

  /**
   * Get responsive configuration
   */
  getResponsiveConfig(): ResponsiveConfig {
    return this.defaultResponsiveConfig;
  }

  /**
   * Save filter state for a component
   */
  saveFilterState(componentName: string, filters: Map<string, any>): void {
    this.filterStates.set(componentName, new Map(filters));
    this.persistFilterStates();
  }

  /**
   * Restore filter state for a component
   */
  restoreFilterState(componentName: string): Map<string, any> | null {
    return this.filterStates.get(componentName) || null;
  }

  /**
   * Clear filter state for a component
   */
  clearFilterState(componentName: string): void {
    this.filterStates.delete(componentName);
    this.persistFilterStates();
  }

  /**
   * Get active filters summary for a component
   */
  getActiveFiltersSummary(componentName: string): ActiveFilter[] {
    const filters = this.filterStates.get(componentName);
    if (!filters) return [];

    const activeFilters: ActiveFilter[] = [];
    
    for (const [key, value] of filters.entries()) {
      if (value !== null && value !== undefined && value !== '') {
        activeFilters.push({
          key,
          label: this.getFilterLabel(key, value),
          value
        });
      }
    }

    return activeFilters;
  }

  /**
   * Check if component has active filters
   */
  hasActiveFilters(componentName: string): boolean {
    return this.getActiveFiltersSummary(componentName).length > 0;
  }

  /**
   * Get filter label for display
   */
  private getFilterLabel(key: string, value: any): string {
    switch (key) {
      case 'search':
        return `Busca: ${value}`;
      case 'status':
        return `Status: ${value === 'ativas' ? 'Ativas' : value === 'inativas' ? 'Inativas' : 'Todas'}`;
      case 'tipo':
        return `Tipo: ${value}`;
      case 'pais':
        return `País: ${value}`;
      default:
        return `${key}: ${value}`;
    }
  }

  /**
   * Load filter states from localStorage
   */
  private loadFilterStates(): void {
    if (typeof window === 'undefined') return;

    try {
      const stored = localStorage.getItem(this.STORAGE_KEY);
      if (stored) {
        const data = JSON.parse(stored);
        for (const [componentName, filters] of Object.entries(data)) {
          this.filterStates.set(componentName, new Map(Object.entries(filters as any)));
        }
      }
    } catch (error) {
      console.warn('Failed to load filter states from localStorage:', error);
    }
  }

  /**
   * Persist filter states to localStorage
   */
  private persistFilterStates(): void {
    if (typeof window === 'undefined') return;

    try {
      const data: { [key: string]: { [key: string]: any } } = {};
      
      for (const [componentName, filters] of this.filterStates.entries()) {
        data[componentName] = Object.fromEntries(filters.entries());
      }

      localStorage.setItem(this.STORAGE_KEY, JSON.stringify(data));
    } catch (error) {
      console.warn('Failed to persist filter states to localStorage:', error);
    }
  }

  /**
   * Get page report template for pagination
   */
  getPageReportTemplate(entityName: string): string {
    return `Mostrando {first} a {last} de {totalRecords} ${entityName.toLowerCase()}`;
  }

  /**
   * Get dialog style based on viewport
   */
  getDialogStyle(): { [key: string]: string } {
    const viewport = this.viewportSize();
    
    switch (viewport) {
      case 'mobile':
        return { width: '95vw', height: '90vh' };
      case 'tablet':
        return { width: '80vw', maxHeight: '80vh' };
      default:
        return { width: '50vw', maxHeight: '80vh' };
    }
  }

  /**
   * Get visible columns based on viewport
   */
  getVisibleColumns(columns: any[], viewport?: 'mobile' | 'tablet' | 'desktop'): any[] {
    const currentViewport = viewport || this.viewportSize();
    
    return columns.filter(col => {
      if (currentViewport === 'mobile' && col.hideOnMobile) return false;
      if (currentViewport === 'tablet' && col.hideOnTablet) return false;
      return true;
    });
  }

  /**
   * Format field value for display
   */
  formatFieldValue(value: any, type?: string, format?: string): string {
    if (value === null || value === undefined) return '';

    switch (type) {
      case 'date':
        return this.formatDate(value, format);
      case 'number':
        return this.formatNumber(value, format);
      case 'currency':
        return this.formatCurrency(value, format);
      case 'boolean':
        return value ? 'Sim' : 'Não';
      default:
        return String(value);
    }
  }

  /**
   * Format date value
   */
  private formatDate(value: any, format?: string): string {
    if (!value) return '';
    
    try {
      const date = new Date(value);
      if (isNaN(date.getTime())) return String(value);
      
      return date.toLocaleDateString('pt-BR', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric'
      });
    } catch {
      return String(value);
    }
  }

  /**
   * Format number value
   */
  private formatNumber(value: any, format?: string): string {
    if (typeof value !== 'number') return String(value);
    
    return value.toLocaleString('pt-BR');
  }

  /**
   * Format currency value
   */
  private formatCurrency(value: any, format?: string): string {
    if (typeof value !== 'number') return String(value);
    
    return value.toLocaleString('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    });
  }
}