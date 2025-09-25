import { Component, Input, Output, EventEmitter, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';

/**
 * Enhanced button component with loading state
 */
@Component({
  selector: 'app-loading-button',
  standalone: true,
  imports: [CommonModule, ButtonModule, TooltipModule],
  template: `
    <p-button
      [label]="displayLabel()"
      [icon]="displayIcon()"
      [loading]="loading"
      [disabled]="disabled || loading"
      [severity]="severity"
      [size]="size"
      [outlined]="outlined"
      [text]="text"
      [raised]="raised"
      [rounded]="rounded"
      [styleClass]="styleClass"
      (onClick)="handleClick()"
      [pTooltip]="tooltip"
      [tooltipPosition]="tooltipPosition">
    </p-button>
  `
})
export class LoadingButtonComponent {
  @Input() label = '';
  @Input() icon = '';
  @Input() loading = false;
  @Input() disabled = false;
  @Input() severity: 'primary' | 'secondary' | 'success' | 'info' | 'warn' | 'danger' | 'help' | 'contrast' = 'primary';
  @Input() size: 'small' | 'large' | undefined = undefined;
  @Input() outlined = false;
  @Input() text = false;
  @Input() raised = false;
  @Input() rounded = false;
  @Input() styleClass = '';
  @Input() tooltip = '';
  @Input() tooltipPosition: 'top' | 'bottom' | 'left' | 'right' = 'top';
  
  // Loading state customization
  @Input() loadingLabel = '';
  @Input() loadingIcon = '';
  
  @Output() onClick = new EventEmitter<Event>();

  // Computed properties for dynamic display
  displayLabel = computed(() => {
    if (this.loading && this.loadingLabel) {
      return this.loadingLabel;
    }
    return this.label;
  });

  displayIcon = computed(() => {
    if (this.loading && this.loadingIcon) {
      return this.loadingIcon;
    }
    return this.icon;
  });

  handleClick(event?: Event): void {
    if (!this.loading && !this.disabled) {
      this.onClick.emit(event);
    }
  }
}