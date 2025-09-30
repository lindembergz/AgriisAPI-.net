import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { EmptyStateConfig } from '../../interfaces/component-template.interface';

/**
 * Reusable empty state component for consistent empty state display
 */
@Component({
  selector: 'app-empty-state',
  standalone: true,
  imports: [CommonModule, ButtonModule],
  template: `
    <div class="empty-state">
      <div class="empty-content">
        <div class="empty-illustration">
          <i [class]="config.icon + ' empty-icon'"></i>
        </div>
        
        <div class="empty-text">
          <h3>{{ config.title }}</h3>
          <p class="empty-description">{{ config.description }}</p>
        </div>
        
        <div class="empty-actions" *ngIf="config.primaryAction || config.secondaryActions?.length">
          <!-- Primary action -->
          <p-button
            *ngIf="config.primaryAction"
            [label]="config.primaryAction.label"
            [icon]="config.primaryAction.icon"
            (onClick)="onPrimaryAction()"
            class="p-button-primary">
          </p-button>
          
          <!-- Secondary actions -->
          <div *ngIf="config.secondaryActions?.length" class="secondary-actions">
            <p-button
              *ngFor="let action of config.secondaryActions"
              [label]="action.label"
              [icon]="action.icon"
              (onClick)="onSecondaryAction(action)"
              class="p-button-outlined">
            </p-button>
          </div>
        </div>
        
        <!-- Help text -->
        <div *ngIf="config.helpText" class="empty-help">
          <small>{{ config.helpText }}</small>
        </div>
      </div>
    </div>
  `,
  styleUrls: ['./empty-state.component.scss']
})
export class EmptyStateComponent {
  @Input() config!: EmptyStateConfig;
  @Output() primaryAction = new EventEmitter<void>();
  @Output() secondaryAction = new EventEmitter<any>();

  onPrimaryAction(): void {
    if (this.config.primaryAction?.action) {
      this.config.primaryAction.action();
    }
    this.primaryAction.emit();
  }

  onSecondaryAction(action: any): void {
    if (action.action) {
      action.action();
    }
    this.secondaryAction.emit(action);
  }
}