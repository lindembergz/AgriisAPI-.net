import { Injectable, inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { MessageService } from 'primeng/api';

/**
 * Centralized error handling service
 */
@Injectable({
  providedIn: 'root'
})
export class ErrorHandlerService {
  private messageService = inject(MessageService);

  /**
   * Handle HTTP errors and display appropriate messages
   */
  handleHttpError(error: HttpErrorResponse): void {
    let errorMessage = 'Ocorreu um erro inesperado';
    let severity: 'error' | 'warn' | 'info' = 'error';

    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = `Erro de conexão: ${error.error.message}`;
    } else {
      // Server-side error
      switch (error.status) {
        case 400:
          errorMessage = 'Dados inválidos enviados';
          severity = 'warn';
          break;
        case 401:
          errorMessage = 'Credenciais inválidas ou sessão expirada';
          break;
        case 403:
          errorMessage = 'Acesso negado';
          break;
        case 404:
          errorMessage = 'Recurso não encontrado';
          severity = 'warn';
          break;
        case 409:
          errorMessage = 'Conflito de dados';
          severity = 'warn';
          break;
        case 422:
          errorMessage = 'Dados de entrada inválidos';
          severity = 'warn';
          break;
        case 500:
          errorMessage = 'Erro interno do servidor';
          break;
        case 502:
        case 503:
        case 504:
          errorMessage = 'Serviço temporariamente indisponível';
          break;
        default:
          if (error.error?.message) {
            errorMessage = error.error.message;
          } else if (error.message) {
            errorMessage = error.message;
          }
      }
    }

    this.showToast(severity, errorMessage);
    console.error('HTTP Error:', error);
  }

  /**
   * Handle validation errors from API
   */
  handleValidationErrors(errors: any): void {
    if (Array.isArray(errors)) {
      errors.forEach(error => {
        this.showToast('warn', error.message || error);
      });
    } else if (typeof errors === 'object') {
      Object.keys(errors).forEach(field => {
        const fieldErrors = Array.isArray(errors[field]) ? errors[field] : [errors[field]];
        fieldErrors.forEach((error: string) => {
          this.showToast('warn', `${field}: ${error}`);
        });
      });
    } else {
      this.showToast('warn', errors.toString());
    }
  }

  /**
   * Handle business logic errors
   */
  handleBusinessError(message: string): void {
    this.showToast('warn', message);
  }

  /**
   * Handle success messages
   */
  handleSuccess(message: string): void {
    this.showToast('success', message);
  }

  /**
   * Handle info messages
   */
  handleInfo(message: string): void {
    this.showToast('info', message);
  }

  /**
   * Show toast message using PrimeNG MessageService
   */
  showToast(severity: 'success' | 'info' | 'warn' | 'error', message: string, summary?: string): void {
    const summaryMap = {
      success: 'Sucesso',
      info: 'Informação',
      warn: 'Atenção',
      error: 'Erro'
    };

    this.messageService.add({
      severity,
      summary: summary || summaryMap[severity],
      detail: message,
      life: severity === 'error' ? 5000 : 3000 // Error messages stay longer
    });
  }

  /**
   * Clear all toast messages
   */
  clearMessages(): void {
    this.messageService.clear();
  }
}