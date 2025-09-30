
import { Component, Input, Output, EventEmitter, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ProgressBarModule } from 'primeng/progressbar';
import { SkeletonModule } from 'primeng/skeleton';

export type LoadingType = 'spinner' | 'bar' | 'skeleton' | 'dots' | 'pulse';
export type LoadingSize = 'small' | 'medium' | 'large' | 'xlarge';
export type LoadingPosition = 'center' | 'top' | 'bottom' | 'left' | 'right';

/**
 * Loading Spinner Component
 * Provides various loading indicators with customizable styles and animations
 */
@Component({
  selector: 'app-loading-spinner',
  standalone: true,
  imports: [
    CommonModule,
    ProgressSpinnerModule,
    ProgressBarModule,
    SkeletonModule
  ],
  templateUrl: './loading-spinner.component.html',
  styleUrls: ['./loading-spinner.component.scss']
})
export class LoadingSpinnerComponent implements OnInit {
  // Input Properties
  @Input() type: LoadingType = 'spinner';
  @Input() size: LoadingSize = 'medium';
  @Input() position: LoadingPosition = 'center';
  @Input() message = 'Carregando...';
  @Input() subMessage = '';
  @Input() showMessage = true;
  @Input() showProgress = false;
  @Input() progress = 0;
  @Input() indeterminate = true;
  @Input() overlay = false;
  @Input() fullScreen = false;
  @Input() transparent = false;
  @Input() color = '#007ad9';
  @Input() backgroundColor = 'rgba(255, 255, 255, 0.8)';
  @Input() strokeWidth = 4;
  @Input() animationDuration = '1s';
  @Input() customClass = '';
  @Input() zIndex = 1000;
  @Input() borderRadius = '8px';
  @Input() padding = '2rem';
  @Input() minHeight = '100px';
  @Input() maxWidth = '300px';
  @Input() showCancel = false;
  @Input() cancelLabel = 'Cancelar';
  @Input() cancelIcon = 'pi pi-times';
  @Input() disabled = false;
  @Input() skeleton = {
    rows: 5,
    height: '1rem',
    borderRadius: '4px',
    animation: 'wave'
  };
  @Input() dots = {
    count: 3,
    size: '1rem',
    spacing: '0.5rem'
  };
  @Input() pulse = {
    size: '5rem',
    borderWidth: '2px'
  };

  // Output Events
  @Output() onCancel = new EventEmitter<void>();

  // Internal State
  protected skeletonRows = computed(() => Array(this.skeleton.rows).fill(0));
  visible = true;

  constructor() {}

  ngOnInit(): void {}

  cancel(): void {
    this.onCancel.emit();
  }

  get spinnerStyle() {
    return {
      'width': this.sizeMap[this.size],
      'height': this.sizeMap[this.size],
      '--spinner-color': this.color,
      '--spinner-stroke-width': `${this.strokeWidth}px`,
      '--spinner-animation-duration': this.animationDuration
    };
  }

  get barStyle() {
    return {
      '--bar-color': this.color,
      '--bar-background': this.backgroundColor
    };
  }

  get skeletonStyle() {
    return {
      '--skeleton-height': this.skeleton.height,
      '--skeleton-border-radius': this.skeleton.borderRadius,
      '--skeleton-animation': this.skeleton.animation
    };
  }

  get dotsStyle() {
    return {
      '--dot-size': this.dots.size,
      '--dot-spacing': this.dots.spacing,
      '--dot-color': this.color
    };
  }

  get pulseStyle() {
    return {
      '--pulse-size': this.pulse.size,
      '--pulse-border-width': this.pulse.borderWidth,
      '--pulse-color': this.color
    };
  }

  private get sizeMap() {
    return {
      small: '2rem',
      medium: '4rem',
      large: '6rem',
      xlarge: '8rem'
    };
  }

  trackByIndex(index: number, item: any): number {
    return index;
  }
}
