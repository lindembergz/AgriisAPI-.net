import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ConfirmationService, MessageService } from 'primeng/api';
import { SafraService } from '../../services/safra.service';
import { SafraDto, SafraAtualDto } from '../../models/safra.interface';

@Component({
  selector: 'app-safras-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TableModule,
    ButtonModule,
    SelectModule,
    ConfirmDialogModule,
    ToastModule,
    ProgressSpinnerModule
  ],
  providers: [ConfirmationService, MessageService],
  templateUrl: './safras-list.component.html',
  styleUrl: './safras-list.component.scss'
})
export class SafrasListComponent implements OnInit {
  private safraService = inject(SafraService);
  private router = inject(Router);
  private confirmationService = inject(ConfirmationService);
  private messageService = inject(MessageService);

  // Signals para gerenciar estado
  safras = signal<SafraDto[]>([]);
  safraAtual = signal<SafraAtualDto | null>(null);
  loading = signal(false);
  tableLoading = signal(false);
  anosFiltro = signal<{ label: string; value: number | null }[]>([]);
  anoSelecionado = signal<number | null>(null);
  pageSize = signal<number>(10);
  multiSortMeta = signal<any[]>([]);
  actionLoadingStates = signal<Map<string, number>>(new Map());
  highlightedRowIndex = signal<number>(-1);

  ngOnInit(): void {
    this.carregarSafras();
    this.carregarSafraAtual();
    this.gerarFiltroAnos();
  }

  private carregarSafras(): void {
    this.loading.set(true);
    this.tableLoading.set(true);
    
    const observable = this.anoSelecionado() 
      ? this.safraService.obterPorAnoColheita(this.anoSelecionado()!)
      : this.safraService.obterTodas();

    observable.subscribe({
      next: (safras) => {
        this.safras.set(safras);
        this.loading.set(false);
        this.tableLoading.set(false);
        
        // Show success message for data load
        this.messageService.add({
          severity: 'success',
          summary: 'Dados carregados',
          detail: `${safras.length} safra(s) encontrada(s)`,
          life: 2000
        });
      },
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: 'Erro ao carregar',
          detail: 'Não foi possível carregar as safras. Tente novamente.',
          life: 5000
        });
        this.loading.set(false);
        this.tableLoading.set(false);
      }
    });
  }

  private carregarSafraAtual(): void {
    this.safraService.obterAtual().subscribe({
      next: (safraAtual) => {
        this.safraAtual.set(safraAtual);
      },
      error: (error) => {
        // Não exibir erro se não houver safra atual
        console.warn('Nenhuma safra atual encontrada');
      }
    });
  }

  private gerarFiltroAnos(): void {
    const anoAtual = new Date().getFullYear();
    const anos: { label: string; value: number | null }[] = [
      { label: 'Todos os anos', value: null }
    ];

    // Gerar anos de 5 anos atrás até 2 anos no futuro
    for (let ano = anoAtual - 5; ano <= anoAtual + 2; ano++) {
      anos.push({ label: ano.toString(), value: ano });
    }

    this.anosFiltro.set(anos);
  }

  onFiltroAnoChange(ano: number | null): void {
    this.anoSelecionado.set(ano);
    this.carregarSafras();
  }

  novaSafra(): void {
    this.router.navigate(['/safras/nova']);
  }

  editarSafra(safra: SafraDto): void {
    this.setActionLoading('edit', safra.id, true);
    
    // Simulate loading for better UX
    setTimeout(() => {
      this.router.navigate(['/safras', safra.id]);
      this.setActionLoading('edit', safra.id, false);
    }, 300);
  }

  excluirSafra(safra: SafraDto): void {
    this.confirmationService.confirm({
      message: `Tem certeza que deseja excluir a safra "${safra.safraFormatada}"?`,
      header: 'Confirmar Exclusão',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Sim',
      rejectLabel: 'Não',
      accept: () => {
        this.setActionLoading('delete', safra.id, true);
        
        this.safraService.remover(safra.id).subscribe({
          next: () => {
            this.setActionLoading('delete', safra.id, false);
            this.messageService.add({
              severity: 'success',
              summary: 'Sucesso',
              detail: `Safra "${safra.safraFormatada}" excluída com sucesso`,
              life: 3000
            });
            this.carregarSafras();
          },
          error: () => {
            this.setActionLoading('delete', safra.id, false);
            this.messageService.add({
              severity: 'error',
              summary: 'Erro na exclusão',
              detail: 'Não foi possível excluir a safra. Tente novamente.',
              life: 5000
            });
          }
        });
      }
    });
  }

  isSafraAtual(safra: SafraDto): boolean {
    return this.safraAtual()?.id === safra.id;
  }

  formatarData(data: Date): string {
    return new Date(data).toLocaleDateString('pt-BR');
  }

  getStatusSafra(safra: SafraDto): string {
    return this.isSafraAtual(safra) ? 'Atual' : 'Inativa';
  }

  getStatusClass(safra: SafraDto): string {
    return this.isSafraAtual(safra) ? 'status-atual' : 'status-inativa';
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