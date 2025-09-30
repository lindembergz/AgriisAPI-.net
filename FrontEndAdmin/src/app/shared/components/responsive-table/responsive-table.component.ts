
import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, signal, computed, TemplateRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { SkeletonModule } from 'primeng/skeleton';
import { TableColumn, SortConfig, PaginationConfig } from '../../interfaces/unified-component.interfaces';

/**
 * Responsive Table Component
 * Provides enhanced table functionality with responsive design,
 * virtual scrolling, and mobile-optimized layouts
 */
@Component({
  selector: 'app-responsive-table',
  standalone: true,
  imports: [
    CommonModule,
    TableModule,
    ButtonModule,
    TagModule,
    TooltipModule,
    ProgressSpinnerModule,
    SkeletonModule
  ],
  templateUrl: './responsive-table.component.html',
  styleUrls: ['./responsive-table.component.scss']
})
export class ResponsiveTableComponent implements OnInit, OnDestroy {
  // Input Properties
  @Input() data: any[] = [];
  @Input() columns: TableColumn[] = [];
  @Input() loading = false;
  @Input() paginator = true;
  @Input() rows = 10;
  @Input() rowsPerPageOptions = [5, 10, 20, 50];
  @Input() sortField = '';
  @Input() sortOrder = 1;
  @Input() multiSort = true;
  @Input() scrollable = true;
  @Input() scrollHeight = '60vh';
  @Input() resizableColumns = true;
  @Input() virtualScrolling = false;
  @Input() virtualScrollItemSize = 46;
  @Input() showCurrentPageReport = true;
  @Input() currentPageReportTemplate = 'Mostrando {first} a {last} de {totalRecords} registros';
  @Input() emptyMessage = 'Nenhum registro encontrado';
  @Input() loadingIcon = 'pi pi-spinner';
  @Input() responsiveLayout = 'scroll';
  @Input() breakpoint = '960px';
  @Input() styleClass = 'p-datatable-gridlines p-datatable-striped';
  @Input() selectionMode: 'single' | 'multiple' | null = null;
  @Input() selection: any | any[];
  @Input() dataKey = 'id';
  @Input() stateKey: string;
  @Input() stateStorage: 'session' | 'local' = 'session';
  @Input() actionsTemplate: TemplateRef<any> | null = null;

  // Output Events
  @Output() onSort = new EventEmitter<SortConfig>();
  @Output() onPage = new EventEmitter<PaginationConfig>();
  @Output() onRowSelect = new EventEmitter<any>();
  @Output() onRowUnselect = new EventEmitter<any>();
  @Output() onStateSave = new EventEmitter<any>();
  @Output() onStateRestore = new EventEmitter<any>();

  // Internal State
  private resizeObserver: ResizeObserver;
  protected isMobile = signal(false);
  // Selected columns for multiSelect (kept as simple array to avoid change-detection calls)
  selectedColumns: any[] = [];

  // Provide column options for any older templates that expect them
  columnOptions(): any[] {
    return (this.columns || []).map(col => ({ label: col.header, value: col }));
  }

  constructor() {}

  ngOnInit(): void {
    this.checkIfMobile();
    this.resizeObserver = new ResizeObserver(() => this.checkIfMobile());
    this.resizeObserver.observe(document.body);
  }

  ngOnDestroy(): void {
    this.resizeObserver.disconnect();
  }

  private checkIfMobile(): void {
    this.isMobile.set(window.innerWidth < parseInt(this.breakpoint, 10));
  }

  // trackBy to improve performance and avoid unnecessary re-renders
  trackBy(index: number, item: any): any {
    return item && (item.field || item.id) ? (item.field || item.id) : index;
  }

  isTablet(): boolean {
    return window.innerWidth > parseInt(this.breakpoint, 10) && window.innerWidth < 1200;
  }

  showSkeleton(): boolean {
    return this.loading && this.data.length === 0;
  }

  handleSort(event: any): void {
    this.onSort.emit({ sortField: event.field, sortOrder: event.order });
  }

  handlePage(event: any): void {
    this.onPage.emit({ first: event.first, rows: event.rows });
  }

  handleRowSelect(event: any): void {
    this.onRowSelect.emit(event.data);
  }

  handleRowUnselect(event: any): void {
    this.onRowUnselect.emit(event.data);
  }

  handleStateSave(event: any): void {
    this.onStateSave.emit(event);
  }

  handleStateRestore(event: any): void {
    this.onStateRestore.emit(event);
  }
}
