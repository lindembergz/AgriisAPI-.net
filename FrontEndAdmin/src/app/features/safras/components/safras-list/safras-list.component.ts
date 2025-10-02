import { Component, OnInit, inject, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
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
  styleUrl: './safras-list.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SafrasListComponent implements OnInit {
  private safraService = inject(SafraService);
  private router = inject(Router);
  private confirmationService = inject(ConfirmationService);
  private messageService = inject(MessageService);
  private cdr = inject(ChangeDetectorRef);

  // Estado do componente
  safras: SafraDto[] = [];
  safraAtual: SafraAtualDto | null = null;
  loading = false;
  tableLoading = false;
  anosFiltro: { label: string; value: number | null }[] = [];
  anoSelecionado: number | null = null;
  pageSize = 10;
  multiSortMeta: any[] = [];
  actionLoadingStates: Map<string, number> = new Map();
  highlightedRowIndex = -1;

  ngOnInit(): void {
    this.carregarSafras();
    this.carregarSafraAtual();
    this.gerarFiltroAnos();
  }

  private carregarSafras(): void {
    this.loading = true;
    this.tableLoading = true;
    
    const observable = this.anoSelecionado 
      ? this.safraService.obterPorAnoColheita(this.anoSelecionado!)
      : this.safraService.obterTodas();

    observable.subscribe({
      next: (safras) => {
        this.safras = safras;
        this.loading = false;
        this.tableLoading = false;
        
        this.messageService.add({
          severity: 'success',
          summary: 'Dados carregados',
          detail: `${safras.length} safra(s) encontrada(s)`,
          life: 2000
        });
        this.cdr.markForCheck();
      },
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: 'Erro ao carregar',
          detail: 'Não foi possível carregar as safras. Tente novamente.',
          life: 5000
        });
        this.loading = false;
        this.tableLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  private carregarSafraAtual(): void {
    this.safraService.obterAtual().subscribe({
      next: (safraAtual) => {
        this.safraAtual = safraAtual;
        this.cdr.markForCheck();
      },
      error: (error) => {
        console.warn('Nenhuma safra atual encontrada');
      }
    });
  }

  private gerarFiltroAnos(): void {
    const anoAtual = new Date().getFullYear();
    const anos: { label: string; value: number | null }[] = [
      { label: 'Todos os anos', value: null }
    ];

    for (let ano = anoAtual - 5; ano <= anoAtual + 2; ano++) {
      anos.push({ label: ano.toString(), value: ano });
    }

    this.anosFiltro = anos;
  }

  onFiltroAnoChange(ano: number | null): void {
    this.anoSelecionado = ano;
    this.carregarSafras();
  }

  novaSafra(): void {
    this.router.navigate(['/safras/nova']);
  }

  editarSafra(safra: SafraDto): void {
    this.setActionLoading('edit', safra.id, true);
    
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
    return this.safraAtual?.id === safra.id;
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

  onSort(event: any): void {
    this.multiSortMeta = event.multiSortMeta || [];
  }

  onPageChange(event: any): void {
    this.pageSize = event.rows;
  }

  private setActionLoading(action: string, id: number, loading: boolean): void {
    const key = `${action}-${id}`;
    
    if (loading) {
      this.actionLoadingStates.set(key, id);
    } else {
      this.actionLoadingStates.delete(key);
    }
    this.cdr.markForCheck();
  }

  isActionLoading(action: string, id: number): boolean {
    const key = `${action}-${id}`;
    return this.actionLoadingStates.has(key);
  }

  hasAnyActionLoading(): boolean {
    return this.actionLoadingStates.size > 0;
  }

  isRowHighlighted(rowIndex: number): boolean {
    return this.highlightedRowIndex === rowIndex;
  }

  private highlightRow(rowIndex: number): void {
    this.highlightedRowIndex = rowIndex;
    this.cdr.markForCheck();
    setTimeout(() => {
      this.highlightedRowIndex = -1;
      this.cdr.markForCheck();
    }, 2000);
  }
}