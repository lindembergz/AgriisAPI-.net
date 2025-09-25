import { Injectable, inject, signal, computed } from '@angular/core';
import { NotificationService } from '../../core/services/notification.service';
import { LoadingStateManager, LoadingOperations } from '../utils/loading-state.util';

/**
 * Comprehensive user feedback service that combines loading states with notifications
 */
@Injectable({
  providedIn: 'root'
})
export class UserFeedbackService {
  private notificationService = inject(NotificationService);
  private loadingManager = new LoadingStateManager();

  // Global loading state
  private globalLoading = signal(false);
  
  // Operation tracking
  private currentOperations = signal<Map<string, OperationInfo>>(new Map());

  /**
   * Check if any operation is currently loading
   */
  isLoading = computed(() => this.loadingManager.isAnyLoading()() || this.globalLoading());

  /**
   * Get loading state for specific operation
   */
  isOperationLoading(operation: string) {
    return this.loadingManager.isLoading(operation);
  }

  /**
   * Get current operations
   */
  getCurrentOperations = computed(() => Array.from(this.currentOperations().values()));

  /**
   * Start an operation with loading state and optional notification
   */
  startOperation(config: OperationConfig): void {
    const { operation, message, showNotification = false, timeout } = config;
    
    // Start loading state
    this.loadingManager.startLoading(operation);
    
    // Track operation info
    this.currentOperations.update(ops => {
      const newOps = new Map(ops);
      newOps.set(operation, {
        operation,
        message: message || this.getDefaultMessage(operation, 'start'),
        startTime: Date.now(),
        timeout
      });
      return newOps;
    });

    // Show notification if requested
    if (showNotification && message) {
      this.notificationService.showOperationStarted(message);
    }

    // Set timeout if specified
    if (timeout) {
      setTimeout(() => {
        if (this.isOperationLoading(operation)()) {
          this.failOperation({
            operation,
            error: 'Operação expirou',
            showNotification: true
          });
        }
      }, timeout);
    }
  }

  /**
   * Complete an operation successfully
   */
  completeOperation(config: CompleteOperationConfig): void {
    const { operation, message, showNotification = true, data } = config;
    
    // Stop loading state
    this.loadingManager.stopLoading(operation);
    
    // Get operation info
    const operationInfo = this.currentOperations().get(operation);
    const duration = operationInfo ? Date.now() - operationInfo.startTime : 0;
    
    // Remove from tracking
    this.currentOperations.update(ops => {
      const newOps = new Map(ops);
      newOps.delete(operation);
      return newOps;
    });

    // Show success notification
    if (showNotification) {
      const successMessage = message || this.getDefaultMessage(operation, 'success');
      this.notificationService.showOperationCompleted(successMessage);
    }

    // Log completion for debugging
    console.log(`Operation ${operation} completed in ${duration}ms`, data);
  }

  /**
   * Fail an operation with error handling
   */
  failOperation(config: FailOperationConfig): void {
    const { operation, error, showNotification = true, retry } = config;
    
    // Stop loading state
    this.loadingManager.stopLoading(operation);
    
    // Remove from tracking
    this.currentOperations.update(ops => {
      const newOps = new Map(ops);
      newOps.delete(operation);
      return newOps;
    });

    // Show error notification
    if (showNotification) {
      const errorMessage = typeof error === 'string' ? error : error.message;
      this.notificationService.showCustomError(errorMessage);
    }

    // Handle retry if provided
    if (retry) {
      setTimeout(() => {
        retry();
      }, retry.delay || 2000);
      
      if (retry.showRetryMessage) {
        this.notificationService.showRetryMessage(retry.attempt || 1, retry.maxAttempts || 3);
      }
    }

    // Log error for debugging
    console.error(`Operation ${operation} failed:`, error);
  }

  /**
   * Update operation progress
   */
  updateProgress(operation: string, progress: number, message?: string): void {
    this.currentOperations.update(ops => {
      const newOps = new Map(ops);
      const existing = newOps.get(operation);
      if (existing) {
        newOps.set(operation, {
          ...existing,
          progress,
          message: message || existing.message
        });
      }
      return newOps;
    });

    if (message) {
      this.notificationService.showProgress(message, progress);
    }
  }

  /**
   * Execute an async operation with automatic feedback
   */
  async executeOperation<T>(
    config: ExecuteOperationConfig<T>
  ): Promise<T> {
    const { operation, asyncFn, startMessage, successMessage, errorMessage, showNotifications = true } = config;
    
    try {
      // Start operation
      this.startOperation({
        operation,
        message: startMessage,
        showNotification: showNotifications,
        timeout: config.timeout
      });

      // Execute the async function
      const result = await asyncFn();

      // Complete operation
      this.completeOperation({
        operation,
        message: successMessage,
        showNotification: showNotifications,
        data: result
      });

      return result;
    } catch (error) {
      // Fail operation
      this.failOperation({
        operation,
        error: errorMessage || error as any,
        showNotification: showNotifications,
        retry: config.retry
      });
      
      throw error;
    }
  }

  /**
   * Execute multiple operations in parallel with batch feedback
   */
  async executeBatchOperation<T>(
    config: BatchOperationConfig<T>
  ): Promise<BatchResult<T>> {
    const { operations, batchName, showProgress = true } = config;
    const results: T[] = [];
    const errors: Error[] = [];
    
    if (showProgress) {
      this.notificationService.showBatchOperationStarted(operations.length, batchName);
    }

    // Execute operations in parallel
    const promises = operations.map(async (op, index) => {
      try {
        const result = await this.executeOperation({
          ...op,
          showNotifications: false // Don't show individual notifications
        });
        results[index] = result;
        
        if (showProgress) {
          const progress = Math.round(((index + 1) / operations.length) * 100);
          this.updateProgress('batch-operation', progress, `${index + 1}/${operations.length} concluído`);
        }
        
        return result;
      } catch (error) {
        errors[index] = error as Error;
        throw error;
      }
    });

    // Wait for all operations to complete
    const settledResults = await Promise.allSettled(promises);
    
    // Count successful and failed operations
    const successful = settledResults.filter(r => r.status === 'fulfilled').length;
    const failed = settledResults.filter(r => r.status === 'rejected').length;

    // Show batch completion notification
    if (showProgress) {
      this.notificationService.showBatchOperationCompleted(successful, failed, batchName);
    }

    return {
      results: results.filter(r => r !== undefined),
      errors: errors.filter(e => e !== undefined),
      successful,
      failed,
      total: operations.length
    };
  }

  /**
   * Set global loading state
   */
  setGlobalLoading(loading: boolean): void {
    this.globalLoading.set(loading);
  }

  /**
   * Clear all operations and loading states
   */
  clearAll(): void {
    this.loadingManager.clearAll();
    this.currentOperations.set(new Map());
    this.globalLoading.set(false);
    this.notificationService.clearAll();
  }

  /**
   * Get default message for operation and type
   */
  private getDefaultMessage(operation: string, type: 'start' | 'success' | 'error'): string {
    const operationNames: Record<string, string> = {
      [LoadingOperations.SAVE]: 'Salvamento',
      [LoadingOperations.UPDATE]: 'Atualização',
      [LoadingOperations.DELETE]: 'Exclusão',
      [LoadingOperations.LOAD]: 'Carregamento',
      [LoadingOperations.CREATE]: 'Criação',
      [LoadingOperations.LIST_LOAD]: 'Carregamento da lista',
      [LoadingOperations.LOGIN]: 'Autenticação'
    };

    const name = operationNames[operation] || operation;
    
    switch (type) {
      case 'start':
        return `${name} em andamento...`;
      case 'success':
        return `${name} concluído com sucesso!`;
      case 'error':
        return `Erro no ${name.toLowerCase()}`;
      default:
        return name;
    }
  }
}

// Interfaces for type safety
export interface OperationInfo {
  operation: string;
  message: string;
  startTime: number;
  progress?: number;
  timeout?: number;
}

export interface OperationConfig {
  operation: string;
  message?: string;
  showNotification?: boolean;
  timeout?: number;
}

export interface CompleteOperationConfig {
  operation: string;
  message?: string;
  showNotification?: boolean;
  data?: any;
}

export interface FailOperationConfig {
  operation: string;
  error: string | Error;
  showNotification?: boolean;
  retry?: {
    delay?: number;
    attempt?: number;
    maxAttempts?: number;
    showRetryMessage?: boolean;
    (): void;
  };
}

export interface ExecuteOperationConfig<T> {
  operation: string;
  asyncFn: () => Promise<T>;
  startMessage?: string;
  successMessage?: string;
  errorMessage?: string;
  showNotifications?: boolean;
  timeout?: number;
  retry?: FailOperationConfig['retry'];
}

export interface BatchOperationConfig<T> {
  operations: ExecuteOperationConfig<T>[];
  batchName: string;
  showProgress?: boolean;
}

export interface BatchResult<T> {
  results: T[];
  errors: Error[];
  successful: number;
  failed: number;
  total: number;
}