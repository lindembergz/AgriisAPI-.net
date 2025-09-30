import { Injectable, inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormGroup } from '@angular/forms';
import { Observable, throwError, timer } from 'rxjs';
import { retry, catchError, switchMap } from 'rxjs/operators';
import { MessageService } from 'primeng/api';
import { ComponentTemplate, ValidationErrorSummary, RetryConfig } from '../interfaces/component-template.interface';
import { ValidationService } from './validation.service';

interface ErrorConfig {
  title: string;
  message: string;
  actions: string[];
}

interface ComponentErrorConfig {
  load: ErrorConfig;
  save: ErrorConfig;
  delete: ErrorConfig;
  validation: { [fieldName: string]: { [errorType: string]: string } };
}

/**
 * Unified error handling service for all reference components
 */
@Injectable({
  providedIn: 'root'
})
export class UnifiedErrorHandlingService {
  
  private messageService = inject(MessageService);
  private validationService = inject(ValidationService);

  // Error configurations for each component
  private readonly ERROR_CONFIGS: { [componentName: string]: ComponentErrorConfig } = {
    'unidades-medida': {
      load: {
        title: 'Erro ao Carregar Unidades',
        message: 'Não foi possível carregar as unidades de medida.',
        actions: ['retry', 'refresh']
      },
      save: {
        title: 'Erro ao Salvar Unidade',
        message: 'Não foi possível salvar a unidade de medida.',
        actions: ['retry', 'cancel']
      },
      delete: {
        title: 'Erro ao Excluir Unidade',
        message: 'Não foi possível excluir a unidade de medida.',
        actions: ['retry', 'cancel']
      },
      validation: {
        simbolo: {
          required: 'Símbolo é obrigatório',
          unique: 'Este símbolo já está em uso',
          pattern: 'Símbolo deve conter apenas letras, números e símbolos de unidade'
        },
        nome: {
          required: 'Nome é obrigatório',
          unique: 'Este nome já está em uso',
          minlength: 'Nome deve ter pelo menos 2 caracteres'
        }
      }
    },
    'ufs': {
      load: {
        title: 'Erro ao Carregar UFs',
        message: 'Não foi possível carregar as UFs.',
        actions: ['retry', 'refresh']
      },
      save: {
        title: 'Erro ao Salvar UF',
        message: 'Não foi possível salvar a UF.',
        actions: ['retry', 'cancel']
      },
      delete: {
        title: 'Erro ao Excluir UF',
        message: 'Não foi possível excluir a UF.',
        actions: ['retry', 'cancel']
      },
      validation: {
        codigo: {
          required: 'Código é obrigatório',
          unique: 'Este código já está em uso',
          minlength: 'Código deve ter 2 caracteres',
          maxlength: 'Código deve ter 2 caracteres'
        },
        nome: {
          required: 'Nome é obrigatório',
          unique: 'Este nome já está em uso'
        },
        paisId: {
          required: 'País é obrigatório'
        }
      }
    },
    'moedas': {
      load: {
        title: 'Erro ao Carregar Moedas',
        message: 'Não foi possível carregar as moedas.',
        actions: ['retry', 'refresh']
      },
      save: {
        title: 'Erro ao Salvar Moeda',
        message: 'Não foi possível salvar a moeda.',
        actions: ['retry', 'cancel']
      },
      delete: {
        title: 'Erro ao Excluir Moeda',
        message: 'Não foi possível excluir a moeda.',
        actions: ['retry', 'cancel']
      },
      validation: {
        codigo: {
          required: 'Código é obrigatório',
          unique: 'Este código já está em uso',
          minlength: 'Código deve ter 3 caracteres',
          maxlength: 'Código deve ter 3 caracteres'
        },
        nome: {
          required: 'Nome é obrigatório',
          unique: 'Este nome já está em uso'
        },
        simbolo: {
          required: 'Símbolo é obrigatório',
          unique: 'Este símbolo já está em uso'
        }
      }
    },
    'paises': {
      load: {
        title: 'Erro ao Carregar Países',
        message: 'Não foi possível carregar os países.',
        actions: ['retry', 'refresh']
      },
      save: {
        title: 'Erro ao Salvar País',
        message: 'Não foi possível salvar o país.',
        actions: ['retry', 'cancel']
      },
      delete: {
        title: 'Erro ao Excluir País',
        message: 'Não foi possível excluir o país.',
        actions: ['retry', 'cancel']
      },
      validation: {
        codigo: {
          required: 'Código é obrigatório',
          unique: 'Este código já está em uso'
        },
        nome: {
          required: 'Nome é obrigatório',
          unique: 'Este nome já está em uso'
        }
      }
    }
  };

  /**
   * Handle component-level errors with standardized messages
   */
  handleComponentError(
    component: string, 
    operation: string, 
    error: any, 
    context?: any
  ): void {
    const errorConfig = this.getErrorConfig(component, operation);
    const userMessage = this.getUserFriendlyMessage(error, errorConfig);
    
    this.showErrorFeedback(userMessage, errorConfig);
    this.logError(component, operation, error, context);
  }

  /**
   * Standardized validation error handling
   */
  handleValidationErrors(
    formGroup: FormGroup, 
    component: string
  ): ValidationErrorSummary {
    const errors = this.collectFormErrors(formGroup, component);
    const summary = this.createErrorSummary(errors, component);
    
    this.showValidationFeedback(summary);
    return summary;
  }

  /**
   * Handle API errors with retry mechanisms
   */
  handleApiError(
    endpoint: string, 
    error: HttpErrorResponse, 
    retryConfig?: RetryConfig
  ): Observable<any> {
    const errorType = this.classifyApiError(error);
    const userMessage = this.getApiErrorMessage(errorType, endpoint);
    
    if (retryConfig && this.shouldRetry(errorType)) {
      return this.createRetryObservable(endpoint, retryConfig, error);
    }
    
    this.showApiErrorFeedback(userMessage, errorType);
    return throwError(() => error);
  }

  /**
   * Get error configuration for component and operation
   */
  private getErrorConfig(component: string, operation: string): ErrorConfig {
    const componentConfig = this.ERROR_CONFIGS[component];
    if (!componentConfig) {
      return {
        title: 'Erro',
        message: 'Ocorreu um erro inesperado.',
        actions: ['retry']
      };
    }

    return componentConfig[operation as keyof ComponentErrorConfig] as ErrorConfig || {
      title: 'Erro',
      message: 'Ocorreu um erro inesperado.',
      actions: ['retry']
    };
  }

  /**
   * Get user-friendly error message
   */
  private getUserFriendlyMessage(error: any, config: ErrorConfig): string {
    if (error?.error?.message) {
      return error.error.message;
    }
    
    if (error?.message) {
      return error.message;
    }

    return config.message;
  }

  /**
   * Show error feedback to user
   */
  private showErrorFeedback(message: string, config: ErrorConfig): void {
    this.messageService.add({
      severity: 'error',
      summary: config.title,
      detail: message,
      life: 5000
    });
  }

  /**
   * Show validation feedback to user
   */
  private showValidationFeedback(summary: ValidationErrorSummary): void {
    if (!summary.isValid && summary.errors.length > 0) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Erro de Validação',
        detail: summary.errors[0].message, // Show first error
        life: 4000
      });
    }
  }

  /**
   * Show API error feedback
   */
  private showApiErrorFeedback(message: string, errorType: string): void {
    const severity = errorType === 'network' ? 'error' : 'warn';
    
    this.messageService.add({
      severity,
      summary: 'Erro de Comunicação',
      detail: message,
      life: 5000
    });
  }

  /**
   * Log error for debugging
   */
  private logError(component: string, operation: string, error: any, context?: any): void {
    console.error(`[${component}] ${operation} error:`, {
      error,
      context,
      timestamp: new Date().toISOString()
    });
  }

  /**
   * Collect form errors with component-specific messages
   */
  private collectFormErrors(formGroup: FormGroup, component: string): string[] {
    const errors: string[] = [];
    const componentConfig = this.ERROR_CONFIGS[component];
    
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      if (control && control.invalid && control.touched) {
        const controlErrors = control.errors;
        if (controlErrors) {
          const errorKey = Object.keys(controlErrors)[0];
          
          // Try component-specific message first
          if (componentConfig?.validation?.[key]?.[errorKey]) {
            errors.push(componentConfig.validation[key][errorKey]);
          } else {
            // Fallback to validation service
            errors.push(this.validationService.getErrorMessage(control, key));
          }
        }
      }
    });

    return errors;
  }

  /**
   * Create error summary
   */
  private createErrorSummary(errors: string[], component: string): ValidationErrorSummary {
    const errorMessages = Object.values(errors);
    
    return {
      isValid: errorMessages.length === 0,
      errors: errorMessages.map(m => ({field: '', message: m}))
    };
  }

  /**
   * Classify API error type
   */
  private classifyApiError(error: HttpErrorResponse): string {
    if (error.status === 0) return 'network';
    if (error.status >= 400 && error.status < 500) return 'client';
    if (error.status >= 500) return 'server';
    return 'unknown';
  }

  /**
   * Get API error message based on type
   */
  private getApiErrorMessage(errorType: string, endpoint: string): string {
    switch (errorType) {
      case 'network':
        return 'Erro de conexão. Verifique sua internet e tente novamente.';
      case 'client':
        return 'Dados inválidos ou operação não permitida.';
      case 'server':
        return 'Erro interno do servidor. Tente novamente em alguns instantes.';
      default:
        return 'Erro desconhecido. Tente novamente.';
    }
  }

  /**
   * Check if error should be retried
   */
  private shouldRetry(errorType: string): boolean {
    return errorType === 'network' || errorType === 'server';
  }

  /**
   * Create retry observable
   */
  private createRetryObservable(endpoint: string, config: RetryConfig, originalError: HttpErrorResponse): Observable<any> {
    return timer(config.delay).pipe(
      switchMap(() => throwError(() => originalError)),
      retry({
        count: config.count,
        delay: (error, retryCount) => {
          const delay = config.delay * Math.pow(2, retryCount - 1);
          
          return timer(delay);
        }
      }),
      catchError(error => {
        this.showApiErrorFeedback(
          `Falha após ${config.count} tentativas. Tente novamente mais tarde.`,
          'retry-failed'
        );
        return throwError(() => error);
      })
    );
  }

  /**
   * Get validation error message for specific field and component
   */
  getValidationErrorMessage(component: string, fieldName: string, errorType: string): string {
    const componentConfig = this.ERROR_CONFIGS[component];
    
    if (componentConfig?.validation?.[fieldName]?.[errorType]) {
      return componentConfig.validation[fieldName][errorType];
    }

    // Fallback to generic messages
    switch (errorType) {
      case 'required':
        return `${fieldName} é obrigatório`;
      case 'unique':
        return `Este ${fieldName} já está em uso`;
      case 'minlength':
        return `${fieldName} muito curto`;
      case 'maxlength':
        return `${fieldName} muito longo`;
      default:
        return `${fieldName} inválido`;
    }
  }
}