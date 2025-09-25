import { Injectable, inject } from '@angular/core';
import { ErrorHandlerService } from './error-handler.service';

/**
 * Service for common notification patterns
 */
@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private errorHandler = inject(ErrorHandlerService);

  // Success messages
  showSaveSuccess(): void {
    this.errorHandler.handleSuccess('Dados salvos com sucesso!');
  }

  showDeleteSuccess(): void {
    this.errorHandler.handleSuccess('Registro excluído com sucesso!');
  }

  showUpdateSuccess(): void {
    this.errorHandler.handleSuccess('Dados atualizados com sucesso!');
  }

  showLoginSuccess(): void {
    this.errorHandler.handleSuccess('Login realizado com sucesso!');
  }

  // Warning messages
  showValidationWarning(message: string = 'Verifique os dados informados'): void {
    this.errorHandler.handleBusinessError(message);
  }

  showUnsavedChanges(): void {
    this.errorHandler.handleInfo('Você possui alterações não salvas');
  }

  // Error messages
  showSaveError(): void {
    this.errorHandler.showToast('error', 'Erro ao salvar os dados');
  }

  showDeleteError(): void {
    this.errorHandler.showToast('error', 'Erro ao excluir o registro');
  }

  showLoadError(): void {
    this.errorHandler.showToast('error', 'Erro ao carregar os dados');
  }

  showConnectionError(): void {
    this.errorHandler.showToast('error', 'Erro de conexão com o servidor');
  }

  // Info messages
  showLoading(): void {
    this.errorHandler.handleInfo('Carregando...');
  }

  showNoDataFound(): void {
    this.errorHandler.handleInfo('Nenhum registro encontrado');
  }

  // Custom messages
  showCustomSuccess(message: string): void {
    this.errorHandler.handleSuccess(message);
  }

  showCustomError(message: string): void {
    this.errorHandler.showToast('error', message);
  }

  showCustomWarning(message: string): void {
    this.errorHandler.handleBusinessError(message);
  }

  showCustomInfo(message: string): void {
    this.errorHandler.handleInfo(message);
  }

  // Progress and loading messages
  showProgress(message: string, progress?: number): void {
    const progressText = progress !== undefined ? ` (${progress}%)` : '';
    this.errorHandler.showToast('info', `${message}${progressText}`);
  }

  showOperationStarted(operation: string): void {
    this.errorHandler.showToast('info', `${operation} iniciado...`);
  }

  showOperationCompleted(operation: string): void {
    this.errorHandler.handleSuccess(`${operation} concluído com sucesso!`);
  }

  // Batch operation messages
  showBatchOperationStarted(count: number, operation: string): void {
    this.errorHandler.showToast('info', `Processando ${count} ${operation}...`);
  }

  showBatchOperationCompleted(successful: number, failed: number, operation: string): void {
    if (failed === 0) {
      this.errorHandler.handleSuccess(`${successful} ${operation} processados com sucesso!`);
    } else {
      this.errorHandler.showToast('warn', 
        `${successful} ${operation} processados com sucesso, ${failed} falharam`);
    }
  }

  // Form-specific messages
  showFormSaved(formName: string): void {
    this.errorHandler.handleSuccess(`${formName} salvo com sucesso!`);
  }

  showFormValidationError(errors: string[]): void {
    const message = errors.length === 1 
      ? errors[0] 
      : `${errors.length} erros de validação encontrados`;
    this.errorHandler.handleBusinessError(message);
  }

  showFormDirtyWarning(): void {
    this.errorHandler.showToast('warn', 'Você possui alterações não salvas que serão perdidas');
  }

  // Network and connectivity messages
  showNetworkError(): void {
    this.errorHandler.showToast('error', 'Erro de conexão. Verifique sua internet');
  }

  showTimeoutError(): void {
    this.errorHandler.showToast('error', 'Operação expirou. Tente novamente');
  }

  showRetryMessage(attempt: number, maxAttempts: number): void {
    this.errorHandler.showToast('info', `Tentativa ${attempt} de ${maxAttempts}...`);
  }

  // Permission and access messages
  showAccessDenied(): void {
    this.errorHandler.showToast('error', 'Acesso negado. Você não tem permissão para esta ação');
  }

  showSessionExpired(): void {
    this.errorHandler.showToast('warn', 'Sua sessão expirou. Faça login novamente');
  }

  // File operation messages
  showFileUploadStarted(fileName: string): void {
    this.errorHandler.showToast('info', `Enviando arquivo: ${fileName}`);
  }

  showFileUploadCompleted(fileName: string): void {
    this.errorHandler.handleSuccess(`Arquivo ${fileName} enviado com sucesso!`);
  }

  showFileUploadError(fileName: string, error?: string): void {
    const message = error 
      ? `Erro ao enviar ${fileName}: ${error}`
      : `Erro ao enviar arquivo: ${fileName}`;
    this.errorHandler.showToast('error', message);
  }

  showFileDownloadStarted(fileName: string): void {
    this.errorHandler.showToast('info', `Baixando arquivo: ${fileName}`);
  }

  showFileDownloadCompleted(fileName: string): void {
    this.errorHandler.handleSuccess(`Download de ${fileName} concluído!`);
  }

  // Data synchronization messages
  showSyncStarted(): void {
    this.errorHandler.showToast('info', 'Sincronizando dados...');
  }

  showSyncCompleted(): void {
    this.errorHandler.handleSuccess('Sincronização concluída!');
  }

  showSyncError(): void {
    this.errorHandler.showToast('error', 'Erro na sincronização de dados');
  }

  // Auto-save messages
  showAutoSaveStarted(): void {
    this.errorHandler.showToast('info', 'Salvamento automático...', 'Auto-save');
  }

  showAutoSaveCompleted(): void {
    this.errorHandler.showToast('success', 'Salvo automaticamente', 'Auto-save');
  }

  showAutoSaveError(): void {
    this.errorHandler.showToast('warn', 'Falha no salvamento automático', 'Auto-save');
  }

  // Clear all messages
  clearAll(): void {
    this.errorHandler.clearMessages();
  }
}