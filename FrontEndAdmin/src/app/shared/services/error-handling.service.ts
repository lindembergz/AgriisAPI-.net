import { Injectable, inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { MessageService } from 'primeng/api';
import { Observable, throwError, timer, of } from 'rxjs';
import { retryWhen, mergeMap, finalize, tap, catchError } from 'rxjs/operators';

/**
 * Error types for categorization
 */
export enum ErrorType {
  NETWORK = 'NETWORK',
  VALIDATION = 'VALIDATION',
  AUTHORIZATION = 'AUTHORIZATION',
  NOT_FOUND = 'NOT_FOUND',
  CONFLICT = 'CONFLICT',
  SERVER = 'SERVER',
  UNKNOWN = 'UNKNOWN'
}

/**
 * Error severity levels
 */
export enum ErrorSeverity {
  LOW = 'info',
  MEDIUM = 'warn',
  HIGH = 'error'
}

/**
 * Standardized error interface
 */
export interface AppError {
  type: ErrorType;
  severity: ErrorSeverity;
  message: string;
  details?: string;
  code?: string;
  retryable: boolean;
  timestamp: Date;
  originalError?: any;
  context?: string;
}

/**
 * Retry configuration
 */
export interface RetryConfig {
  maxRetries: number;
  delayMs: number;
  backoffMultiplier: number;
  retryableErrors: number[];
}

/**
 * Error handling statistics
 */
interface ErrorStats {
  totalErrors: number;
  errorsByType: Map<ErrorType, number>;
  errorsByCode: Map<string, number>;
  lastError?: AppError;
}

/**
 * Comprehensive error handling service with retry mechanisms and user feedback
 */
@Injectable({
  providedIn: 'root'
})
export class ErrorHandlingService {
  
  private messageService = inject(MessageService);
  
  // Default retry configuration
  private defaultRetryConfig: RetryConfig = {
    maxRetries: 3,
    delayMs: 1000,
    backoffMultiplier: 2,
    retryableErrors: [0, 408, 429, 500, 502, 503, 504]
  };
  
  // Error statistics
  private errorStats: ErrorStats = {
    totalErrors: 0,
    errorsByType: new Map(),
    errorsByCode: new Map()
  };
  
  // Error message templates
  private errorMessages = new Map<string, string>([
    ['ENTITY_NOT_FOUND', 'Registro não encontrado'],
    ['DUPLICATE_CODE', 'Este código já está em uso'],
    ['DUPLICATE_NAME', 'Este nome já está em uso'],
    ['CANNOT_DELETE_REFERENCED', 'Não é possível excluir este registro pois está sendo usado por outros'],
    ['CONCURRENCY_CONFLICT', 'Este registro foi modificado por outro usuário'],
    ['VALIDATION_ERROR', 'Dados inválidos fornecidos'],
    ['INSUFFICIENT_PERMISSIONS', 'Permissões insuficientes para esta operação'],
    ['RATE_LIMIT_EXCEEDED', 'Muitas tentativas. Tente novamente em alguns minutos'],
    ['SERVICE_UNAVAILABLE', 'Serviço temporariamente indisponível']
  ]);

  /**
   * Handle HTTP errors with comprehensive error processing
   */
  handleError(
    error: HttpErrorResponse,
    context?: string,
    entityName?: string,
    showToast: boolean = true
  ): Observable<never> {
    const appError = this.processError(error, context, entityName);
    
    // Update statistics
    this.updateErrorStats(appError);
    
    // Show user feedback if requested
    if (showToast) {
      this.showErrorToast(appError);
    }
    
    // Log error for debugging
    this.logError(appError);
    
    return throwError(() => appError);
  }

  /**
   * Process raw HTTP error into standardized AppError
   */
  private processError(
    error: HttpErrorResponse,
    context?: string,
    entityName?: string
  ): AppError {
    const appError: AppError = {
      type: this.categorizeError(error),
      severity: this.determineSeverity(error),
      message: this.generateUserMessage(error, entityName),
      details: this.extractErrorDetails(error),
      code: this.extractErrorCode(error),
      retryable: this.isRetryable(error),
      timestamp: new Date(),
      originalError: error,
      context
    };
    
    return appError;
  }

  /**
   * Categorize error by type
   */
  private categorizeError(error: HttpErrorResponse): ErrorType {
    if (error.status === 0) {
      return ErrorType.NETWORK;
    }
    
    switch (error.status) {
      case 400:
      case 422:
        return ErrorType.VALIDATION;
      case 401:
      case 403:
        return ErrorType.AUTHORIZATION;
      case 404:
        return ErrorType.NOT_FOUND;
      case 409:
      case 412:
        return ErrorType.CONFLICT;
      case 500:
      case 502:
      case 503:
      case 504:
        return ErrorType.SERVER;
      default:
        return ErrorType.UNKNOWN;
    }
  }

  /**
   * Determine error severity
   */
  private determineSeverity(error: HttpErrorResponse): ErrorSeverity {
    if (error.status === 0 || error.status >= 500) {
      return ErrorSeverity.HIGH;
    }
    
    if (error.status === 401 || error.status === 403 || error.status === 409) {
      return ErrorSeverity.MEDIUM;
    }
    
    return ErrorSeverity.LOW;
  }

  /**
   * Generate user-friendly error message
   */
  private generateUserMessage(error: HttpErrorResponse, entityName?: string): string {
    // Try to extract API error message
    if (error.error && typeof error.error === 'object') {
      const apiError = error.error;
      
      if (apiError.errorCode && this.errorMessages.has(apiError.errorCode)) {
        let message = this.errorMessages.get(apiError.errorCode)!;
        if (entityName) {
          message = message.replace('registro', entityName.toLowerCase());
        }
        return message;
      }
      
      if (apiError.errorDescription) {
        return apiError.errorDescription;
      }
      
      if (apiError.validationErrors) {
        const validationMessages = Object.values(apiError.validationErrors).flat();
        return (validationMessages as string[]).join(', ');
      }
    }
    
    // Fallback to HTTP status messages
    switch (error.status) {
      case 0:
        return 'Erro de conexão. Verifique sua internet e tente novamente';
      case 400:
        return 'Dados inválidos fornecidos';
      case 401:
        return 'Sessão expirada. Faça login novamente';
      case 403:
        return 'Acesso negado para esta operação';
      case 404:
        return entityName ? `${entityName} não encontrado(a)` : 'Recurso não encontrado';
      case 409:
        return entityName ? `Já existe um(a) ${entityName.toLowerCase()} com estes dados` : 'Conflito de dados';
      case 412:
        return 'Este registro foi modificado por outro usuário. Recarregue e tente novamente';
      case 422:
        return 'Dados de entrada inválidos';
      case 429:
        return 'Muitas tentativas. Aguarde alguns minutos e tente novamente';
      case 500:
        return 'Erro interno do servidor. Tente novamente em alguns minutos';
      case 502:
      case 503:
      case 504:
        return 'Serviço temporariamente indisponível. Tente novamente em alguns minutos';
      default:
        return `Erro ${error.status}: ${error.statusText || 'Erro desconhecido'}`;
    }
  }

  /**
   * Extract error details for debugging
   */
  private extractErrorDetails(error: HttpErrorResponse): string | undefined {
    if (error.error && typeof error.error === 'object') {
      const apiError = error.error;
      
      if (apiError.traceId) {
        return `Trace ID: ${apiError.traceId}`;
      }
      
      if (apiError.details) {
        return apiError.details;
      }
    }
    
    return error.message;
  }

  /**
   * Extract error code
   */
  private extractErrorCode(error: HttpErrorResponse): string | undefined {
    if (error.error && typeof error.error === 'object') {
      return error.error.errorCode;
    }
    
    return error.status.toString();
  }

  /**
   * Check if error is retryable
   */
  private isRetryable(error: HttpErrorResponse): boolean {
    return this.defaultRetryConfig.retryableErrors.includes(error.status);
  }

  /**
   * Show error toast to user
   */
  private showErrorToast(appError: AppError): void {
    const life = appError.severity === ErrorSeverity.HIGH ? 8000 : 
                 appError.severity === ErrorSeverity.MEDIUM ? 6000 : 4000;
    
    this.messageService.add({
      severity: appError.severity,
      summary: this.getSummaryByType(appError.type),
      detail: appError.message,
      life,
      sticky: appError.severity === ErrorSeverity.HIGH
    });
  }

  /**
   * Get summary text by error type
   */
  private getSummaryByType(type: ErrorType): string {
    switch (type) {
      case ErrorType.NETWORK:
        return 'Erro de Conexão';
      case ErrorType.VALIDATION:
        return 'Dados Inválidos';
      case ErrorType.AUTHORIZATION:
        return 'Acesso Negado';
      case ErrorType.NOT_FOUND:
        return 'Não Encontrado';
      case ErrorType.CONFLICT:
        return 'Conflito';
      case ErrorType.SERVER:
        return 'Erro do Servidor';
      default:
        return 'Erro';
    }
  }

  /**
   * Log error for debugging
   */
  private logError(appError: AppError): void {
    const logLevel = appError.severity === ErrorSeverity.HIGH ? 'error' : 
                     appError.severity === ErrorSeverity.MEDIUM ? 'warn' : 'info';
    
    const logData = {
      type: appError.type,
      message: appError.message,
      code: appError.code,
      context: appError.context,
      timestamp: appError.timestamp,
      details: appError.details,
      originalError: appError.originalError
    };
    
    console[logLevel]('Application Error:', logData);
  }

  /**
   * Update error statistics
   */
  private updateErrorStats(appError: AppError): void {
    this.errorStats.totalErrors++;
    this.errorStats.lastError = appError;
    
    // Update type statistics
    const typeCount = this.errorStats.errorsByType.get(appError.type) || 0;
    this.errorStats.errorsByType.set(appError.type, typeCount + 1);
    
    // Update code statistics
    if (appError.code) {
      const codeCount = this.errorStats.errorsByCode.get(appError.code) || 0;
      this.errorStats.errorsByCode.set(appError.code, codeCount + 1);
    }
  }

  /**
   * Create retry operator with exponential backoff
   */
  createRetryOperator<T>(config?: Partial<RetryConfig>) {
    const retryConfig = { ...this.defaultRetryConfig, ...config };
    
    return retryWhen<T>((errors: Observable<any>) =>
      errors.pipe(
        mergeMap((error, index) => {
          const retryAttempt = index + 1;
          
          // Check if we should retry
          if (retryAttempt > retryConfig.maxRetries || !this.isRetryable(error)) {
            return throwError(() => error);
          }
          
          // Calculate delay with exponential backoff
          const delay = retryConfig.delayMs * Math.pow(retryConfig.backoffMultiplier, index);
          
          console.warn(`Retry attempt ${retryAttempt}/${retryConfig.maxRetries} after ${delay}ms`, error);
          
          // Show retry notification
          this.messageService.add({
            severity: 'info',
            summary: 'Tentando Novamente',
            detail: `Tentativa ${retryAttempt} de ${retryConfig.maxRetries}...`,
            life: 2000
          });
          
          return timer(delay);
        })
      )
    );
  }

  /**
   * Wrap observable with error handling and retry
   */
  wrapWithErrorHandling<T>(
    source: Observable<T>,
    context?: string,
    entityName?: string,
    retryConfig?: Partial<RetryConfig>
  ): Observable<T> {
    return source.pipe(
      this.createRetryOperator(retryConfig),
      catchError((error) => {
        if (error instanceof HttpErrorResponse) {
          this.handleError(error, context, entityName, true).subscribe();
        }
        return throwError(() => error);
      })
    );
  }

  /**
   * Get error statistics
   */
  getErrorStats(): ErrorStats {
    return { ...this.errorStats };
  }

  /**
   * Clear error statistics
   */
  clearErrorStats(): void {
    this.errorStats = {
      totalErrors: 0,
      errorsByType: new Map(),
      errorsByCode: new Map()
    };
  }

  /**
   * Check if specific error type is frequent
   */
  isErrorTypeFrequent(type: ErrorType, threshold: number = 5): boolean {
    const count = this.errorStats.errorsByType.get(type) || 0;
    return count >= threshold;
  }

  /**
   * Get user-friendly error message for specific error code
   */
  getErrorMessage(code: string, entityName?: string): string {
    let message = this.errorMessages.get(code) || 'Erro desconhecido';
    
    if (entityName) {
      message = message.replace('registro', entityName.toLowerCase());
    }
    
    return message;
  }

  /**
   * Register custom error message
   */
  registerErrorMessage(code: string, message: string): void {
    this.errorMessages.set(code, message);
  }

  /**
   * Show success message (for consistency)
   */
  showSuccess(message: string, summary: string = 'Sucesso'): void {
    this.messageService.add({
      severity: 'success',
      summary,
      detail: message,
      life: 3000
    });
  }

  /**
   * Show warning message
   */
  showWarning(message: string, summary: string = 'Aviso'): void {
    this.messageService.add({
      severity: 'warn',
      summary,
      detail: message,
      life: 5000
    });
  }

  /**
   * Show info message
   */
  showInfo(message: string, summary: string = 'Informação'): void {
    this.messageService.add({
      severity: 'info',
      summary,
      detail: message,
      life: 4000
    });
  }
}