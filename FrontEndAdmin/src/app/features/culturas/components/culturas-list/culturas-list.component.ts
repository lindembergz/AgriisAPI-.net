import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmationService, MessageService } from 'primeng/api';
import { CulturaService } from '../../services/cultura.service';
import { CulturaDto } from '../../models/cultura.interface';

interface StatusFilter {
  label: string;
  value: string;
}

/**
 * Component for listing Culturas with CRUD operations
 * Implements standalone component with PrimeNG Table
 */
@Component({
  selector: 'app-culturas-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TableModule,
    ButtonModule,
    SelectModule,
    ConfirmDialogModule,
    ToastModule,
    ProgressSpinnerModule,
    TagModule,
    TooltipModule
  ],
  providers: [ConfirmationService, MessageService],
  templateUrl: './culturas-list.component.html',
  styleUrls: ['./culturas-list.component.scss']
})
export class CulturasListComponent implements OnInit {
  private culturaService = inject(CulturaService);
  private router = inject(Router);
  private confirmationService = inject(ConfirmationService);
  private messageService = inject(MessageService);

  // Signals for reactive state management
  culturas = signal<CulturaDto[]>([]);
  loading = signal<boolean>(false);
  tableLoading = signal<boolean>(false);
  selectedStatusFilter = signal<string>('todas');
  pageSize = signal<number>(10);
  multiSortMeta = signal<any[]>([]);
  actionLoadingStates = signal<Map<string, number>>(new Map());
  highlightedRowIndex = signal<number>(-1);

  // Filter options
  statusFilterOptions: StatusFilter[] = [
    { label: 'Todas', value: 'todas' },
    { label: 'Ativas', value: 'ativas' },
    { label: 'Inativas', value: 'inativas' }
  ];

  ngOnInit(): void {
    this.carregarCulturas();
  }

  /**
   * Load culturas based on current filter
   */
  carregarCulturas(): void {
    this.loading.set(true);
    this.tableLoading.set(true);
    
    const filter = this.selectedStatusFilter();
    let request;

    if (filter === 'ativas') {
      request = this.culturaService.obterAtivas();
    } else {
      request = this.culturaService.obterTodas();
    }

    request.subscribe({
      next: (culturas) => {
        if (filter === 'inativas') {
          // Filter inactive culturas when showing only inactive ones
          this.culturas.set(culturas.filter(c => !c.ativo));
        } else {
          this.culturas.set(culturas);
        }
        this.loading.set(false);
        this.tableLoading.set(false);
        
        // Show success message for data load
        this.messageService.add({
          severity: 'success',
          summary: 'Dados carregados',
          detail: `${culturas.length} cultura(s) encontrada(s)`,
          life: 2000
        });
      },
      error: (error) => {
        console.error('Erro ao carregar culturas:', error);
        this.loading.set(false);
        this.tableLoading.set(false);
        
        this.messageService.add({
          severity: 'error',
          summary: 'Erro ao carregar',
          detail: 'Não foi possível carregar as culturas. Tente novamente.',
          life: 5000
        });
      }
    });
  }

  /**
   * Handle status filter change
   */
  onStatusFilterChange(event: any): void {
    this.selectedStatusFilter.set(event.value);
    this.carregarCulturas();
  }

  /**
   * Navigate to create new cultura
   */
  novaCultura(): void {
    this.router.navigate(['/culturas/nova']);
  }

  /**
   * Navigate to edit cultura
   */
  editarCultura(cultura: CulturaDto): void {
    this.setActionLoading('edit', cultura.id, true);
    
    // Simulate loading for better UX
    setTimeout(() => {
      this.router.navigate(['/culturas', cultura.id]);
      this.setActionLoading('edit', cultura.id, false);
    }, 300);
  }

  /**
   * Confirm and delete cultura
   */
  excluirCultura(cultura: CulturaDto): void {
    this.confirmationService.confirm({
      message: `Tem certeza que deseja excluir a cultura "${cultura.nome}"?`,
      header: 'Confirmar Exclusão',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Sim',
      rejectLabel: 'Não',
      accept: () => {
        this.confirmarExclusao(cultura);
      }
    });
  }

  /**
   * Execute cultura deletion
   */
  private confirmarExclusao(cultura: CulturaDto): void {
    this.setActionLoading('delete', cultura.id, true);
    
    this.culturaService.remover(cultura.id).subscribe({
      next: () => {
        this.setActionLoading('delete', cultura.id, false);
        this.messageService.add({
          severity: 'success',
          summary: 'Sucesso',
          detail: `Cultura "${cultura.nome}" excluída com sucesso`,
          life: 3000
        });
        this.carregarCulturas(); // Reload the list
      },
      error: (error) => {
        this.setActionLoading('delete', cultura.id, false);
        console.error('Erro ao excluir cultura:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Erro na exclusão',
          detail: 'Não foi possível excluir a cultura. Tente novamente.',
          life: 5000
        });
      }
    });
  }

  /**
   * Format date for display
   */
  formatarData(data: Date): string {
    if (!data) return '-';
    return new Date(data).toLocaleDateString('pt-BR');
  }

  /**
   * Get status label for display
   */
  getStatusLabel(ativo: boolean): string {
    return ativo ? 'Ativa' : 'Inativa';
  }

  /**
   * Get status severity for PrimeNG styling
   */
  getStatusSeverity(ativo: boolean): string {
    return ativo ? 'success' : 'danger';
  }

  /**
   * Handle table sorting
   */
  onSort(event: any): void {
    this.multiSortMeta.set(event.multiSortMeta || []);
  }

  /**
   * Handle page change
   */
  onPageChange(event: any): void {
    this.pageSize.set(event.rows);
  }

  /**
   * Set action loading state
   */
  private setActionLoading(action: string, id: number, loading: boolean): void {
    const key = `${action}-${id}`;
    const currentStates = new Map(this.actionLoadingStates());
    
    if (loading) {
      currentStates.set(key, id);
    } else {
      currentStates.delete(key);
    }
    
    this.actionLoadingStates.set(currentStates);
  }

  /**
   * Check if specific action is loading
   */
  isActionLoading(action: string, id: number): boolean {
    const key = `${action}-${id}`;
    return this.actionLoadingStates().has(key);
  }

  /**
   * Check if any action is loading
   */
  hasAnyActionLoading(): boolean {
    return this.actionLoadingStates().size > 0;
  }

  /**
   * Check if row should be highlighted
   */
  isRowHighlighted(rowIndex: number): boolean {
    return this.highlightedRowIndex() === rowIndex;
  }

  /**
   * Highlight row temporarily for visual feedback
   */
  private highlightRow(rowIndex: number): void {
    this.highlightedRowIndex.set(rowIndex);
    setTimeout(() => {
      this.highlightedRowIndex.set(-1);
    }, 2000);
  }
}