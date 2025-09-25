import { Component, Input, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

/**
 * Reusable loading overlay component
 */
@Component({
  selector: 'app-loading-overlay',
  standalone: true,
  imports: [CommonModule, ProgressSpinnerModule],
  template: `
    <div 
      *ngIf="visible()" 
      class="loading-overlay"
      [class.loading-overlay-fullscreen]="fullscreen"
      [class.loading-overlay-transparent]="transparent">
      
      <div class="loading-content">
        <p-progressSpinner 
          [style]="spinnerStyle()"
          [strokeWidth]="strokeWidth"
          [animationDuration]="animationDuration">
        </p-progressSpinner>
        
        <div *ngIf="message" class="loading-message">
          {{ message }}
        </div>
        
        <div *ngIf="showProgress && progress !== null" class="loading-progress">
          <div class="progress-bar">
            <div 
              class="progress-fill" 
              [style.width.%]="progress">
            </div>
          </div>
          <span class="progress-text">{{ progress }}%</span>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .loading-overlay {
      position: absolute;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(255, 255, 255, 0.8);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 1000;
      border-radius: inherit;
      backdrop-filter: blur(2px);
      transition: all 0.2s ease-in-out;
    }

    .loading-overlay-fullscreen {
      position: fixed;
      z-index: 9999;
      border-radius: 0;
    }

    .loading-overlay-transparent {
      background: rgba(255, 255, 255, 0.5);
    }

    .loading-content {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 1rem;
      text-align: center;
      padding: 2rem;
      background: var(--surface-card);
      border-radius: 8px;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
      max-width: 300px;
    }

    .loading-message {
      color: var(--text-color);
      font-size: 0.875rem;
      font-weight: 500;
      margin: 0;
    }

    .loading-progress {
      width: 100%;
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }

    .progress-bar {
      width: 100%;
      height: 4px;
      background: var(--surface-border);
      border-radius: 2px;
      overflow: hidden;
    }

    .progress-fill {
      height: 100%;
      background: var(--primary-color);
      border-radius: 2px;
      transition: width 0.3s ease;
    }

    .progress-text {
      font-size: 0.75rem;
      color: var(--text-color-secondary);
      align-self: center;
    }

    // Dark theme support
    @media (prefers-color-scheme: dark) {
      .loading-overlay {
        background: rgba(0, 0, 0, 0.8);
      }

      .loading-overlay-transparent {
        background: rgba(0, 0, 0, 0.5);
      }
    }

    // Reduced motion support
    @media (prefers-reduced-motion: reduce) {
      .loading-overlay {
        transition: none;
      }

      .progress-fill {
        transition: none;
      }
    }

    // High contrast mode
    @media (prefers-contrast: high) {
      .loading-overlay {
        background: rgba(255, 255, 255, 0.95);
        border: 2px solid var(--primary-color);
      }

      .loading-content {
        border: 2px solid var(--text-color);
      }
    }
  `]
})
export class LoadingOverlayComponent {
  @Input() loading = false;
  @Input() message?: string;
  @Input() fullscreen = false;
  @Input() transparent = false;
  @Input() size: 'small' | 'medium' | 'large' = 'medium';
  @Input() strokeWidth = '2';
  @Input() animationDuration = '2s';
  @Input() showProgress = false;
  @Input() progress: number | null = null;

  // Computed signals
  visible = computed(() => this.loading);

  spinnerStyle = computed(() => {
    const sizeMap = {
      small: { width: '30px', height: '30px' },
      medium: { width: '50px', height: '50px' },
      large: { width: '70px', height: '70px' }
    };
    return sizeMap[this.size];
  });
}