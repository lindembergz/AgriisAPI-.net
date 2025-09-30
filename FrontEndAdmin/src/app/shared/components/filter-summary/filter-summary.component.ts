
import { Component, Input, Output, EventEmitter, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { ActiveFilter } from '../../interfaces/component-template.interface';

/**
 * Filter Summary Component
 * Displays active filters as removable tags with summary information
 */
@Component({
  selector: 'app-filter-summary',
  standalone: true,
  imports: [
    CommonModule,
    TagModule,
    ButtonModule,
    TooltipModule
  ],
  templateUrl: './filter-summary.component.html',
  styleUrls: ['./filter-summary.component.scss']
})
export class FilterSummaryComponent implements OnInit {
  // Input Properties
  @Input() filters: ActiveFilter[] = [];
  @Input() showClearAll = true;
  @Input() showCount = true;
  @Input() maxVisible = 5;
  @Input() severity: 'success' | 'info' | 'warning' | 'danger' | 'secondary' | 'contrast' = 'info';
  @Input() size: 'small' | 'large' = 'small';
  @Input() rounded = false;
  @Input() outlined = false;
  @Input() icon = '';
  @Input() styleClass = '';
  @Input() emptyMessage = 'Nenhum filtro ativo';
  @Input() clearAllLabel = 'Limpar Todos';
  @Input() clearAllIcon = 'pi pi-filter-slash';
  @Input() moreLabel = 'mais';
  @Input() lessLabel = 'menos';
  @Input() expandable = true;
  @Input() collapsible = true;
  @Input() initiallyExpanded = false;
  @Input() animationDuration = 300;
  @Input() showAnimation = true;
  @Input() compactMode = false;
  @Input() verticalLayout = false;
  @Input() groupSimilar = false;
  @Input() sortFilters = true;
  @Input() customTemplate = false;

  // Output Events
  @Output() onFilterRemove = new EventEmitter<string>();
  @Output() onClearAll = new EventEmitter<void>();
  @Output() onExpand = new EventEmitter<boolean>();
  @Output() onFilterClick = new EventEmitter<ActiveFilter>();
  @Output() onFilterHover = new EventEmitter<ActiveFilter>();

  // Internal State
  protected isExpanded = signal(this.initiallyExpanded);
  protected visibleFilters = computed(() => this.isExpanded() ? this.sortedFilters() : this.sortedFilters().slice(0, this.maxVisible));
  protected hiddenCount = computed(() => this.sortedFilters().length - this.visibleFilters().length);

  private sortedFilters = computed(() => {
    if (this.sortFilters) {
      return [...this.filters].sort((a, b) => a.label.localeCompare(b.label));
    }
    return this.filters;
  });

  ngOnInit(): void {}

  removeFilter(key: string): void {
    this.onFilterRemove.emit(key);
  }

  clearAllFilters(): void {
    this.onClearAll.emit();
  }

  toggleExpand(): void {
    if (this.expandable || (this.isExpanded() && this.collapsible)) {
      this.isExpanded.set(!this.isExpanded());
      this.onExpand.emit(this.isExpanded());
    }
  }

  hasFilters(): boolean {
    return this.filters && this.filters.length > 0;
  }

  get showToggleButton(): boolean {
    return this.expandable && this.filters.length > this.maxVisible;
  }

  trackByFilter(index: number, filter: ActiveFilter): string {
    return filter.key;
  }

  containerClasses(): string {
    return `filter-summary-container ${this.styleClass}`;
  }

  getAnimationStyle(): any {
    return { transition: `all ${this.animationDuration}ms` };
  }

  isEmpty(): boolean {
    return !this.hasFilters();
  }

  isAnimating(): boolean {
    return this.showAnimation;
  }
}
