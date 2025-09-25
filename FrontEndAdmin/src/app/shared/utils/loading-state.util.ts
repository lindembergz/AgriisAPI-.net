import { signal, computed, Signal } from '@angular/core';

/**
 * Utility class for managing loading states with signals
 */
export class LoadingStateManager {
  private loadingStates = signal<Map<string, boolean>>(new Map());

  /**
   * Get loading state for a specific operation
   */
  isLoading(operation: string): Signal<boolean> {
    return computed(() => this.loadingStates().get(operation) || false);
  }

  /**
   * Get overall loading state (true if any operation is loading)
   */
  isAnyLoading(): Signal<boolean> {
    return computed(() => {
      const states = this.loadingStates();
      return Array.from(states.values()).some(loading => loading);
    });
  }

  /**
   * Set loading state for a specific operation
   */
  setLoading(operation: string, loading: boolean): void {
    this.loadingStates.update(states => {
      const newStates = new Map(states);
      if (loading) {
        newStates.set(operation, true);
      } else {
        newStates.delete(operation);
      }
      return newStates;
    });
  }

  /**
   * Start loading for an operation
   */
  startLoading(operation: string): void {
    this.setLoading(operation, true);
  }

  /**
   * Stop loading for an operation
   */
  stopLoading(operation: string): void {
    this.setLoading(operation, false);
  }

  /**
   * Clear all loading states
   */
  clearAll(): void {
    this.loadingStates.set(new Map());
  }

  /**
   * Get all current loading operations
   */
  getLoadingOperations(): Signal<string[]> {
    return computed(() => Array.from(this.loadingStates().keys()));
  }
}

/**
 * Common loading operation names
 */
export const LoadingOperations = {
  // CRUD operations
  SAVE: 'save',
  UPDATE: 'update',
  DELETE: 'delete',
  LOAD: 'load',
  CREATE: 'create',
  
  // List operations
  LIST_LOAD: 'list-load',
  LIST_REFRESH: 'list-refresh',
  LIST_FILTER: 'list-filter',
  LIST_SEARCH: 'list-search',
  
  // Form operations
  FORM_SUBMIT: 'form-submit',
  FORM_VALIDATE: 'form-validate',
  FORM_LOAD: 'form-load',
  
  // File operations
  FILE_UPLOAD: 'file-upload',
  FILE_DOWNLOAD: 'file-download',
  
  // Authentication
  LOGIN: 'login',
  LOGOUT: 'logout',
  REFRESH_TOKEN: 'refresh-token',
  
  // API operations
  API_CALL: 'api-call',
  EXTERNAL_API: 'external-api'
} as const;

/**
 * Decorator for automatic loading state management
 */
export function withLoadingState(operation: string, loadingManager: LoadingStateManager) {
  return function (target: any, propertyKey: string, descriptor: PropertyDescriptor) {
    const originalMethod = descriptor.value;

    descriptor.value = async function (...args: any[]) {
      loadingManager.startLoading(operation);
      
      try {
        const result = await originalMethod.apply(this, args);
        return result;
      } finally {
        loadingManager.stopLoading(operation);
      }
    };

    return descriptor;
  };
}

/**
 * Helper function to create a loading state manager
 */
export function createLoadingStateManager(): LoadingStateManager {
  return new LoadingStateManager();
}

/**
 * Loading state configuration interface
 */
export interface LoadingStateConfig {
  showSpinner?: boolean;
  showOverlay?: boolean;
  disableInteraction?: boolean;
  message?: string;
  timeout?: number;
}

/**
 * Default loading state configurations
 */
export const DefaultLoadingConfigs: Record<string, LoadingStateConfig> = {
  [LoadingOperations.SAVE]: {
    showSpinner: true,
    showOverlay: true,
    disableInteraction: true,
    message: 'Salvando...',
    timeout: 30000
  },
  [LoadingOperations.LOAD]: {
    showSpinner: true,
    showOverlay: false,
    disableInteraction: false,
    message: 'Carregando...',
    timeout: 15000
  },
  [LoadingOperations.DELETE]: {
    showSpinner: true,
    showOverlay: true,
    disableInteraction: true,
    message: 'Excluindo...',
    timeout: 10000
  },
  [LoadingOperations.LIST_LOAD]: {
    showSpinner: true,
    showOverlay: false,
    disableInteraction: false,
    message: 'Carregando lista...',
    timeout: 15000
  },
  [LoadingOperations.LOGIN]: {
    showSpinner: true,
    showOverlay: true,
    disableInteraction: true,
    message: 'Autenticando...',
    timeout: 10000
  }
};