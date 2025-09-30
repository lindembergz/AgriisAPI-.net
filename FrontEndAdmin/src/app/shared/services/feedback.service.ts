import { Injectable, inject } from '@angular/core';
import { MessageService, ConfirmationService } from 'primeng/api';
import { Observable, Subject } from 'rxjs';

/**
 * Feedback types for different user interactions
 */
export enum FeedbackType {
  SUCCESS = 'success',
  INFO = 'info',
  WARNING = 'warn',
  ERROR = 'error'
}

/**
 * Confirmation dialog configuration
 */
export interface ConfirmationConfig {
  message: string;
  header?: string;
  icon?: string;
  acceptLabel?: string;
  rejectLabel?: string;
  acceptButtonStyleClass?: string;
  rejectButtonStyleClass?: string;
  blockScroll?: boolean;
  closable?: boolean;
  dismissableMask?: boolean;
}

/**
 * Toast message configuration
 */
export interface ToastConfig {
  severity: FeedbackType;
  summary: string;
  detail: string;
  life?: number;
  sticky?: boolean;
  closable?: boolean;
  data?: any;
}

/**
 * Success feedback configuration
 */
export interface SuccessFeedbackConfig {
  message: string;
  summary?: string;
  showToast?: boolean;
  showConfirmation?: boolean;
  autoHide?: boolean;
  duration?: number;
  includeUndo?: boolean;
  undoAction?: () => void;
  undoLabel?: string;
}

/**
 * Form validation feedback configuration
 */
export interface ValidationFeedbackConfig {
  showInlineErrors?: boolean;
  showSummaryErrors?: boolean;
  highlightInvalidFields?: boolean;
  scrollToFirstError?: boolean;
  focusFirstError?: boolean;
}

/**
 * Comprehensive feedback service for user interactions
 */
@Injectable({
  providedIn: 'root'
})
export class FeedbackService {
  
  private messageService = inject(MessageService);
  private confirmationService = inject(ConfirmationService);
  
  // Subjects for feedback events
  private successFeedbackSubject = new Subject<SuccessFeedbackConfig>();
  private validationFeedbackSubject = new Subject<ValidationFeedbackConfig>();
  
  // Default configurations
  private defaultToastLife = 4000;
  private defaultSuccessLife = 3000;
  private defaultErrorLife = 6000;
  private defaultWarningLife = 5000;

  /**
   * Get success feedback observable
   */
  get successFeedback$(): Observable<SuccessFeedbackConfig> {
    return this.successFeedbackSubject.asObservable();
  }

  /**
   * Get validation feedback observable
   */
  get validationFeedback$(): Observable<ValidationFeedbackConfig> {
    return this.validationFeedbackSubject.asObservable();
  }

  /**
   * Show success message with optional undo functionality
   */
  showSuccess(config: SuccessFeedbackConfig): void {
    const {
      message,
      summary = 'Sucesso',
      showToast = true,
      autoHide = true,
      duration = this.defaultSuccessLife,
      includeUndo = false,
      undoAction,
      undoLabel = 'Desfazer'
    } = config;

    if (showToast) {
      const toastConfig: ToastConfig = {
        severity: FeedbackType.SUCCESS,
        summary,
        detail: message,
        life: autoHide ? duration : 0,
        sticky: !autoHide,
        closable: true
      };

      // Add undo functionality if requested
      if (includeUndo && undoAction) {
        toastConfig.data = {
          undoAction,
          undoLabel
        };
      }

      this.showToast(toastConfig);
    }

    // Emit success feedback event
    this.successFeedbackSubject.next(config);
  }

  /**
   * Show info message
   */
  showInfo(message: string, summary: string = 'Informação', life?: number): void {
    this.showToast({
      severity: FeedbackType.INFO,
      summary,
      detail: message,
      life: life || this.defaultToastLife
    });
  }

  /**
   * Show warning message
   */
  showWarning(message: string, summary: string = 'Aviso', life?: number): void {
    this.showToast({
      severity: FeedbackType.WARNING,
      summary,
      detail: message,
      life: life || this.defaultWarningLife
    });
  }

  /**
   * Show error message
   */
  showError(message: string, summary: string = 'Erro', life?: number): void {
    this.showToast({
      severity: FeedbackType.ERROR,
      summary,
      detail: message,
      life: life || this.defaultErrorLife,
      sticky: life === 0
    });
  }

  /**
   * Show toast message with full configuration
   */
  showToast(config: ToastConfig): void {
    this.messageService.add({
      severity: config.severity,
      summary: config.summary,
      detail: config.detail,
      life: config.life || this.defaultToastLife,
      sticky: config.sticky || false,
      closable: config.closable !== false,
      data: config.data
    });
  }

  /**
   * Show confirmation dialog
   */
  showConfirmation(config: ConfirmationConfig): Promise<boolean> {
    return new Promise((resolve) => {
      this.confirmationService.confirm({
        message: config.message,
        header: config.header || 'Confirmação',
        icon: config.icon || 'pi pi-question-circle',
        acceptLabel: config.acceptLabel || 'Sim',
        rejectLabel: config.rejectLabel || 'Não',
        acceptButtonStyleClass: config.acceptButtonStyleClass || 'p-button-primary',
        rejectButtonStyleClass: config.rejectButtonStyleClass || 'p-button-text',
        blockScroll: config.blockScroll !== false,
        closable: config.closable !== false,
        dismissableMask: config.dismissableMask !== false,
        accept: () => resolve(true),
        reject: () => resolve(false)
      });
    });
  }

  /**
   * Show delete confirmation with standard styling
   */
  showDeleteConfirmation(
    itemName: string,
    customMessage?: string
  ): Promise<boolean> {
    const message = customMessage || 
      `Tem certeza que deseja excluir ${itemName}? Esta ação não pode ser desfeita.`;

    return this.showConfirmation({
      message,
      header: 'Confirmar Exclusão',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Excluir',
      rejectLabel: 'Cancelar',
      acceptButtonStyleClass: 'p-button-danger',
      rejectButtonStyleClass: 'p-button-text'
    });
  }

  /**
   * Show deactivation confirmation
   */
  showDeactivationConfirmation(itemName: string): Promise<boolean> {
    return this.showConfirmation({
      message: `Tem certeza que deseja desativar ${itemName}?`,
      header: 'Confirmar Desativação',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Desativar',
      rejectLabel: 'Cancelar',
      acceptButtonStyleClass: 'p-button-warning',
      rejectButtonStyleClass: 'p-button-text'
    });
  }

  /**
   * Show activation confirmation
   */
  showActivationConfirmation(itemName: string): Promise<boolean> {
    return this.showConfirmation({
      message: `Tem certeza que deseja ativar ${itemName}?`,
      header: 'Confirmar Ativação',
      icon: 'pi pi-question-circle',
      acceptLabel: 'Ativar',
      rejectLabel: 'Cancelar',
      acceptButtonStyleClass: 'p-button-success',
      rejectButtonStyleClass: 'p-button-text'
    });
  }

  /**
   * Show unsaved changes confirmation
   */
  showUnsavedChangesConfirmation(): Promise<boolean> {
    return this.showConfirmation({
      message: 'Você tem alterações não salvas. Deseja continuar sem salvar?',
      header: 'Alterações Não Salvas',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Continuar',
      rejectLabel: 'Cancelar',
      acceptButtonStyleClass: 'p-button-warning',
      rejectButtonStyleClass: 'p-button-primary'
    });
  }

  /**
   * Show form validation feedback
   */
  showValidationFeedback(
    errors: string[],
    config: ValidationFeedbackConfig = {}
  ): void {
    const {
      showSummaryErrors = true,
      scrollToFirstError = true,
      focusFirstError = true
    } = config;

    if (showSummaryErrors && errors.length > 0) {
      const errorMessage = errors.length === 1 
        ? errors[0]
        : `Foram encontrados ${errors.length} erros no formulário:\n• ${errors.join('\n• ')}`;

      this.showError(errorMessage, 'Formulário Inválido', 8000);
    }

    // Emit validation feedback event
    this.validationFeedbackSubject.next(config);

    // Scroll to first error if requested
    if (scrollToFirstError) {
      this.scrollToFirstError();
    }

    // Focus first error if requested
    if (focusFirstError) {
      this.focusFirstError();
    }
  }

  /**
   * Show operation progress feedback
   */
  showProgress(
    message: string,
    progress?: number,
    summary: string = 'Processando'
  ): void {
    const detail = progress !== undefined 
      ? `${message} (${Math.round(progress)}%)`
      : message;

    this.showToast({
      severity: FeedbackType.INFO,
      summary,
      detail,
      life: 2000
    });
  }

  /**
   * Show bulk operation feedback
   */
  showBulkOperationFeedback(
    operation: string,
    successCount: number,
    totalCount: number,
    errors?: string[]
  ): void {
    if (successCount === totalCount) {
      // All successful
      this.showSuccess({
        message: `${operation} realizada com sucesso em ${successCount} item(s)`,
        summary: 'Operação Concluída'
      });
    } else if (successCount > 0) {
      // Partial success
      const errorCount = totalCount - successCount;
      this.showWarning(
        `${operation} realizada em ${successCount} de ${totalCount} item(s). ${errorCount} item(s) apresentaram erro.`,
        'Operação Parcialmente Concluída',
        8000
      );
    } else {
      // All failed
      this.showError(
        `Falha ao realizar ${operation} em todos os ${totalCount} item(s).`,
        'Operação Falhou',
        10000
      );
    }

    // Show detailed errors if provided
    if (errors && errors.length > 0) {
      setTimeout(() => {
        this.showValidationFeedback(errors);
      }, 1000);
    }
  }

  /**
   * Show network status feedback
   */
  showNetworkStatus(isOnline: boolean): void {
    if (isOnline) {
      this.showSuccess({
        message: 'Conexão com a internet restaurada',
        summary: 'Online',
        duration: 2000
      });
    } else {
      this.showError(
        'Conexão com a internet perdida. Algumas funcionalidades podem não estar disponíveis.',
        'Offline',
        0 // Sticky until online
      );
    }
  }

  /**
   * Clear all toast messages
   */
  clearAll(): void {
    this.messageService.clear();
  }

  /**
   * Clear toast messages by key
   */
  clearByKey(key: string): void {
    this.messageService.clear(key);
  }

  /**
   * Scroll to first error element
   */
  private scrollToFirstError(): void {
    setTimeout(() => {
      const firstError = document.querySelector('.ng-invalid:not(form), .p-invalid');
      if (firstError) {
        firstError.scrollIntoView({
          behavior: 'smooth',
          block: 'center'
        });
      }
    }, 100);
  }

  /**
   * Focus first error element
   */
  private focusFirstError(): void {
    setTimeout(() => {
      const firstError = document.querySelector('.ng-invalid:not(form) input, .ng-invalid:not(form) select, .ng-invalid:not(form) textarea, .p-invalid input, .p-invalid select, .p-invalid textarea') as HTMLElement;
      if (firstError && typeof firstError.focus === 'function') {
        firstError.focus();
      }
    }, 150);
  }

  /**
   * Create success feedback with undo functionality
   */
  createUndoableSuccess(
    message: string,
    undoAction: () => void,
    undoLabel: string = 'Desfazer'
  ): SuccessFeedbackConfig {
    return {
      message,
      includeUndo: true,
      undoAction,
      undoLabel,
      duration: 8000 // Longer duration for undo actions
    };
  }

  /**
   * Show contextual help message
   */
  showHelp(message: string, title: string = 'Ajuda'): void {
    this.showInfo(message, title, 6000);
  }

  /**
   * Show feature announcement
   */
  showAnnouncement(
    message: string,
    title: string = 'Nova Funcionalidade',
    persistent: boolean = false
  ): void {
    this.showToast({
      severity: FeedbackType.INFO,
      summary: title,
      detail: message,
      life: persistent ? 0 : 10000,
      sticky: persistent
    });
  }
}