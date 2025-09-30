import { Component, Input, Output, EventEmitter, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { ProgressBarModule } from 'primeng/progressbar';
import { interval, Subscription } from 'rxjs';
import { take } from 'rxjs/operators';

/**
 * Custom toast component with undo functionality and countdown
 */
@Component({
  selector: 'app-undo-toast',
  standalone: true,
  imports: [
    CommonModule,
    ButtonModule,
    ProgressBarModule
  ],
  template: `
    <div class="undo-toast" [class.undo-toast-success]="severity === 'success'">
      <div class="undo-toast-content">
        <div class="undo-toast-icon">
          <i [class]="iconClass"></i>
        </div>
        
        <div class="undo-toast-text">
          <div class="undo-toast-summary">{{ summary }}</div>
          <div class="undo-toast-detail">{{ detail }}</div>
        </div>
        
        <div class="undo-toast-actions">
          <p-button
            [label]="undoLabel"
            icon="pi pi-undo"
            (onClick)="onUndo()"
            class="p-button-sm p-button-outlined undo-button">
          </p-button>
          
          <p-button
            icon="pi pi-times"
            (onClick)="onClose()"
            class="p-button-sm p-button-text close-button">
          </p-button>
        </div>
      </div>
      
      <!-- Countdown Progress Bar -->
      <div *ngIf="showCountdown" class="undo-toast-countdown">
        <p-progressBar 
          [value]="countdownProgress" 
          [showValue]="false"
          styleClass="countdown-progress">
        </p-progressBar>
        <small class="countdown-text">
          Desfazer em {{ Math.ceil(remainingTime / 1000) }}s
        </small>
      </div>
    </div>
  `,
  styleUrls: ['./undo-toast.component.scss']
})
export class UndoToastComponent implements OnInit, OnDestroy {
  
  @Input() severity: 'success' | 'info' | 'warn' | 'error' = 'success';
  @Input() summary: string = '';
  @Input() detail: string = '';
  @Input() undoLabel: string = 'Desfazer';
  @Input() duration: number = 8000; // 8 seconds default
  @Input() showCountdown: boolean = true;
  
  @Output() undo = new EventEmitter<void>();
  @Output() close = new EventEmitter<void>();
  @Output() autoClose = new EventEmitter<void>();
  
  countdownProgress: number = 100;
  remainingTime: number = 0;
  
  private countdownSubscription?: Subscription;
  private readonly updateInterval = 100; // Update every 100ms for smooth animation

  ngOnInit(): void {
    this.remainingTime = this.duration;
    this.startCountdown();
  }

  ngOnDestroy(): void {
    this.stopCountdown();
  }

  /**
   * Get icon class based on severity
   */
  get iconClass(): string {
    switch (this.severity) {
      case 'success':
        return 'pi pi-check-circle';
      case 'info':
        return 'pi pi-info-circle';
      case 'warn':
        return 'pi pi-exclamation-triangle';
      case 'error':
        return 'pi pi-times-circle';
      default:
        return 'pi pi-info-circle';
    }
  }

  /**
   * Handle undo action
   */
  onUndo(): void {
    this.stopCountdown();
    this.undo.emit();
  }

  /**
   * Handle close action
   */
  onClose(): void {
    this.stopCountdown();
    this.close.emit();
  }

  /**
   * Start countdown timer
   */
  private startCountdown(): void {
    if (this.duration <= 0) return;
    
    this.countdownSubscription = interval(this.updateInterval)
      .pipe(take(Math.ceil(this.duration / this.updateInterval)))
      .subscribe({
        next: (tick) => {
          this.remainingTime = this.duration - (tick * this.updateInterval);
          this.countdownProgress = (this.remainingTime / this.duration) * 100;
          
          if (this.remainingTime <= 0) {
            this.onAutoClose();
          }
        },
        complete: () => {
          this.onAutoClose();
        }
      });
  }

  /**
   * Stop countdown timer
   */
  private stopCountdown(): void {
    if (this.countdownSubscription) {
      this.countdownSubscription.unsubscribe();
      this.countdownSubscription = undefined;
    }
  }

  /**
   * Handle auto close when countdown expires
   */
  private onAutoClose(): void {
    this.stopCountdown();
    this.autoClose.emit();
  }
}