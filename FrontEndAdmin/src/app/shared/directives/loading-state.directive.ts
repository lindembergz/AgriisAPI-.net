import { 
  Directive, 
  Input, 
  ElementRef, 
  Renderer2, 
  OnInit, 
  OnDestroy, 
  inject,
  effect,
  signal
} from '@angular/core';

/**
 * Directive to add loading state to any element
 * Usage: <div appLoadingState [loading]="isLoading" [message]="'Loading...'">Content</div>
 */
@Directive({
  selector: '[appLoadingState]',
  standalone: true
})
export class LoadingStateDirective implements OnInit, OnDestroy {
  private elementRef = inject(ElementRef);
  private renderer = inject(Renderer2);
  
  @Input() set loading(value: boolean) {
    this.loadingSignal.set(value);
  }
  
  @Input() message = 'Carregando...';
  @Input() showSpinner = true;
  @Input() showOverlay = true;
  @Input() disableElement = true;
  @Input() spinnerSize = '30px';
  @Input() overlayOpacity = '0.7';

  private loadingSignal = signal(false);
  private overlayElement?: HTMLElement;
  private originalPointerEvents?: string;
  private originalOpacity?: string;

  constructor() {
    // React to loading state changes
    effect(() => {
      const isLoading = this.loadingSignal();
      if (isLoading) {
        this.showLoadingState();
      } else {
        this.hideLoadingState();
      }
    });
  }

  ngOnInit(): void {
    // Store original styles
    this.originalPointerEvents = this.elementRef.nativeElement.style.pointerEvents;
    this.originalOpacity = this.elementRef.nativeElement.style.opacity;
    
    // Ensure element has relative positioning for overlay
    const position = getComputedStyle(this.elementRef.nativeElement).position;
    if (position === 'static') {
      this.renderer.setStyle(this.elementRef.nativeElement, 'position', 'relative');
    }
  }

  ngOnDestroy(): void {
    this.hideLoadingState();
  }

  private showLoadingState(): void {
    if (this.disableElement) {
      this.renderer.setStyle(this.elementRef.nativeElement, 'pointer-events', 'none');
      this.renderer.setStyle(this.elementRef.nativeElement, 'opacity', this.overlayOpacity);
    }

    if (this.showOverlay) {
      this.createOverlay();
    }

    // Add loading class for custom styling
    this.renderer.addClass(this.elementRef.nativeElement, 'loading-state-active');
  }

  private hideLoadingState(): void {
    // Restore original styles
    if (this.originalPointerEvents !== undefined) {
      this.renderer.setStyle(this.elementRef.nativeElement, 'pointer-events', this.originalPointerEvents);
    } else {
      this.renderer.removeStyle(this.elementRef.nativeElement, 'pointer-events');
    }

    if (this.originalOpacity !== undefined) {
      this.renderer.setStyle(this.elementRef.nativeElement, 'opacity', this.originalOpacity);
    } else {
      this.renderer.removeStyle(this.elementRef.nativeElement, 'opacity');
    }

    // Remove overlay
    if (this.overlayElement) {
      this.renderer.removeChild(this.elementRef.nativeElement, this.overlayElement);
      this.overlayElement = undefined;
    }

    // Remove loading class
    this.renderer.removeClass(this.elementRef.nativeElement, 'loading-state-active');
  }

  private createOverlay(): void {
    if (this.overlayElement) {
      return; // Overlay already exists
    }

    // Create overlay container
    this.overlayElement = this.renderer.createElement('div');
    this.renderer.addClass(this.overlayElement, 'loading-state-overlay');
    
    // Overlay styles
    this.renderer.setStyle(this.overlayElement, 'position', 'absolute');
    this.renderer.setStyle(this.overlayElement, 'top', '0');
    this.renderer.setStyle(this.overlayElement, 'left', '0');
    this.renderer.setStyle(this.overlayElement, 'right', '0');
    this.renderer.setStyle(this.overlayElement, 'bottom', '0');
    this.renderer.setStyle(this.overlayElement, 'background', 'rgba(255, 255, 255, 0.8)');
    this.renderer.setStyle(this.overlayElement, 'display', 'flex');
    this.renderer.setStyle(this.overlayElement, 'align-items', 'center');
    this.renderer.setStyle(this.overlayElement, 'justify-content', 'center');
    this.renderer.setStyle(this.overlayElement, 'z-index', '1000');
    this.renderer.setStyle(this.overlayElement, 'border-radius', 'inherit');
    this.renderer.setStyle(this.overlayElement, 'backdrop-filter', 'blur(1px)');

    // Create content container
    const contentContainer = this.renderer.createElement('div');
    this.renderer.setStyle(contentContainer, 'display', 'flex');
    this.renderer.setStyle(contentContainer, 'flex-direction', 'column');
    this.renderer.setStyle(contentContainer, 'align-items', 'center');
    this.renderer.setStyle(contentContainer, 'gap', '0.5rem');
    this.renderer.setStyle(contentContainer, 'padding', '1rem');
    this.renderer.setStyle(contentContainer, 'background', 'var(--surface-card)');
    this.renderer.setStyle(contentContainer, 'border-radius', '6px');
    this.renderer.setStyle(contentContainer, 'box-shadow', '0 2px 8px rgba(0, 0, 0, 0.15)');

    // Create spinner if enabled
    if (this.showSpinner) {
      const spinner = this.createSpinner();
      this.renderer.appendChild(contentContainer, spinner);
    }

    // Create message if provided
    if (this.message) {
      const messageElement = this.renderer.createElement('div');
      this.renderer.setStyle(messageElement, 'color', 'var(--text-color)');
      this.renderer.setStyle(messageElement, 'font-size', '0.875rem');
      this.renderer.setStyle(messageElement, 'font-weight', '500');
      this.renderer.setStyle(messageElement, 'text-align', 'center');
      const messageText = this.renderer.createText(this.message);
      this.renderer.appendChild(messageElement, messageText);
      this.renderer.appendChild(contentContainer, messageElement);
    }

    this.renderer.appendChild(this.overlayElement, contentContainer);
    this.renderer.appendChild(this.elementRef.nativeElement, this.overlayElement);
  }

  private createSpinner(): HTMLElement {
    const spinner = this.renderer.createElement('div');
    this.renderer.addClass(spinner, 'loading-state-spinner');
    
    // Spinner styles
    this.renderer.setStyle(spinner, 'width', this.spinnerSize);
    this.renderer.setStyle(spinner, 'height', this.spinnerSize);
    this.renderer.setStyle(spinner, 'border', '2px solid var(--surface-border)');
    this.renderer.setStyle(spinner, 'border-top', '2px solid var(--primary-color)');
    this.renderer.setStyle(spinner, 'border-radius', '50%');
    this.renderer.setStyle(spinner, 'animation', 'loading-state-spin 1s linear infinite');

    // Add keyframes for spinner animation
    this.addSpinnerKeyframes();

    return spinner;
  }

  private addSpinnerKeyframes(): void {
    // Check if keyframes already exist
    const existingStyle = document.getElementById('loading-state-keyframes');
    if (existingStyle) {
      return;
    }

    const style = this.renderer.createElement('style');
    this.renderer.setAttribute(style, 'id', 'loading-state-keyframes');
    const keyframes = `
      @keyframes loading-state-spin {
        0% { transform: rotate(0deg); }
        100% { transform: rotate(360deg); }
      }
    `;
    const keyframesText = this.renderer.createText(keyframes);
    this.renderer.appendChild(style, keyframesText);
    this.renderer.appendChild(document.head, style);
  }
}