import { Injectable } from '@angular/core';
import { MessageService } from 'primeng/api';

/**
 * Service to provide consistent notification configuration across the application
 */
@Injectable({
  providedIn: 'root'
})
export class NotificationConfigService {

  constructor(private messageService: MessageService) {}

  /**
   * Show success notification with consistent styling
   */
  showSuccess(detail: string, summary: string = 'Sucesso', life: number = 5000): void {
    this.messageService.add({
      severity: 'success',
      summary,
      detail,
      life,
      styleClass: 'notification-success',
      contentStyleClass: 'notification-content'
    });
  }

  /**
   * Show error notification with consistent styling
   */
  showError(detail: string, summary: string = 'Erro', life: number = 8000): void {
    this.messageService.add({
      severity: 'error',
      summary,
      detail,
      life,
      styleClass: 'notification-error',
      contentStyleClass: 'notification-content'
    });
  }

  /**
   * Show warning notification with consistent styling
   */
  showWarning(detail: string, summary: string = 'Atenção', life: number = 6000): void {
    this.messageService.add({
      severity: 'warn',
      summary,
      detail,
      life,
      styleClass: 'notification-warning',
      contentStyleClass: 'notification-content'
    });
  }

  /**
   * Show info notification with consistent styling
   */
  showInfo(detail: string, summary: string = 'Informação', life: number = 5000): void {
    this.messageService.add({
      severity: 'info',
      summary,
      detail,
      life,
      styleClass: 'notification-info',
      contentStyleClass: 'notification-content'
    });
  }

  /**
   * Show operation success with undo functionality
   */
  showOperationSuccess(
    detail: string, 
    summary: string = 'Operação realizada',
    undoCallback?: () => void,
    life: number = 8000
  ): void {
    const message: any = {
      severity: 'success',
      summary,
      detail,
      life,
      styleClass: 'notification-success notification-with-action',
      contentStyleClass: 'notification-content'
    };

    if (undoCallback) {
      message.data = { undoCallback };
      message.styleClass += ' notification-undoable';
    }

    this.messageService.add(message);
  }

  /**
   * Show validation error with field-specific information
   */
  showValidationError(
    fieldName: string,
    errorMessage: string,
    summary: string = 'Erro de Validação'
  ): void {
    this.messageService.add({
      severity: 'error',
      summary,
      detail: `${fieldName}: ${errorMessage}`,
      life: 6000,
      styleClass: 'notification-error notification-validation',
      contentStyleClass: 'notification-content'
    });
  }

  /**
   * Show loading notification
   */
  showLoading(detail: string = 'Processando...', summary: string = 'Aguarde'): void {
    this.messageService.add({
      severity: 'info',
      summary,
      detail,
      life: 0, // Persistent until manually cleared
      styleClass: 'notification-loading',
      contentStyleClass: 'notification-content'
    });
  }

  /**
   * Clear all notifications
   */
  clearAll(): void {
    this.messageService.clear();
  }

  /**
   * Clear notifications by key
   */
  clearByKey(key: string): void {
    this.messageService.clear(key);
  }

  /**
   * Show confirmation-style notification (for operations that need user awareness)
   */
  showConfirmation(
    detail: string,
    summary: string = 'Confirmação',
    life: number = 10000
  ): void {
    this.messageService.add({
      severity: 'info',
      summary,
      detail,
      life,
      styleClass: 'notification-confirmation',
      contentStyleClass: 'notification-content',
      closable: true
    });
  }

  /**
   * Show network error notification
   */
  showNetworkError(
    detail: string = 'Verifique sua conexão com a internet e tente novamente.',
    summary: string = 'Erro de Conexão'
  ): void {
    this.messageService.add({
      severity: 'error',
      summary,
      detail,
      life: 10000,
      styleClass: 'notification-error notification-network',
      contentStyleClass: 'notification-content',
      closable: true
    });
  }

  /**
   * Show permission error notification
   */
  showPermissionError(
    detail: string = 'Você não tem permissão para realizar esta operação.',
    summary: string = 'Acesso Negado'
  ): void {
    this.messageService.add({
      severity: 'warn',
      summary,
      detail,
      life: 8000,
      styleClass: 'notification-warning notification-permission',
      contentStyleClass: 'notification-content'
    });
  }

  /**
   * Show bulk operation result
   */
  showBulkOperationResult(
    successCount: number,
    errorCount: number,
    operation: string = 'operação'
  ): void {
    if (errorCount === 0) {
      this.showSuccess(
        `${successCount} ${successCount === 1 ? 'item processado' : 'itens processados'} com sucesso.`,
        `${operation} concluída`
      );
    } else if (successCount === 0) {
      this.showError(
        `Falha ao processar ${errorCount} ${errorCount === 1 ? 'item' : 'itens'}.`,
        `Erro na ${operation}`
      );
    } else {
      this.showWarning(
        `${successCount} ${successCount === 1 ? 'item processado' : 'itens processados'} com sucesso, ${errorCount} com erro.`,
        `${operation} parcialmente concluída`
      );
    }
  }
}