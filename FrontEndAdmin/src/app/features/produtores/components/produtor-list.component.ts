import { Component, inject, signal, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { ToolbarModule } from 'primeng/toolbar';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TagModule } from 'primeng/tag';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmationService } from 'primeng/api';

import { Produtor, TipoCliente } from '../../../shared/models/produtor.model';
import { ProdutorService, ProdutorQueryParams } from '../services/produtor.service';
import { NotificationService } from '../../../core/services/notification.service';

/**
 * Interface for table column configuration
 */
interface TableColumn {
  field: string;
  header: string;
  sortable?: boolean;
  width?: string;
}

/**
 * Interface for filter options
 */
interface FilterOption {
  label: string;
  value: string;
}

@Component({
  selector: 'app-produtor-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TableModule,
    ButtonModule,
    ToolbarModule,
    InputTextModule,
    SelectModule,
    ProgressSpinnerModule,
    TagModule,
    ConfirmDialogModule,
    TooltipModule
  ],
  providers: [ConfirmationService],
  templateUrl: './produtor-list.component.html',
  styleUrls: ['./produtor-list.component.scss']
})
export class ProdutorListComponent implements OnInit {
  private router = inject(Router);
  private produtorService = inject(ProdutorService);
  private notificationService = inject(NotificationService);
  private confirmationService = inject(ConfirmationService);

  // Signals for reactive state management
  produtores = signal<Produtor[]>([]);
  isLoading = signal<boolean>(false);
  totalRecords = signal<number>(0);

  // Filter properties
  searchTerm = '';
  selectedTipoCliente: string | null = null;
  selectedUf: string | null = null;

  // Pagination
  pageSize = 10;
  currentPage = 0;

  // Search debounce timer
  private searchTimer?: any;

  // Table configuration
  columns: TableColumn[] = [
    { field: 'codigo', header: 'Código', sortable: true, width: '120px' },
    { field: 'nome', header: 'Nome', sortable: true },
    { field: 'cpfCnpj', header: 'CPF/CNPJ', sortable: true, width: '150px' },
    { field: 'tipoCliente', header: 'Tipo Cliente', sortable: true, width: '120px' },
    { field: 'localidade', header: 'Localidade', sortable: false, width: '150px' },
    { field: 'uf', header: 'UF', sortable: true, width: '80px' }
  ];

  // Filter options
  tipoClienteOptions: FilterOption[] = [
    { label: 'Pessoa Física', value: TipoCliente.PF },
    { label: 'Pessoa Jurídica', value: TipoCliente.PJ }
  ];

  ufOptions: FilterOption[] = [
    { label: 'AC', value: 'AC' }, { label: 'AL', value: 'AL' }, { label: 'AP', value: 'AP' },
    { label: 'AM', value: 'AM' }, { label: 'BA', value: 'BA' }, { label: 'CE', value: 'CE' },
    { label: 'DF', value: 'DF' }, { label: 'ES', value: 'ES' }, { label: 'GO', value: 'GO' },
    { label: 'MA', value: 'MA' }, { label: 'MT', value: 'MT' }, { label: 'MS', value: 'MS' },
    { label: 'MG', value: 'MG' }, { label: 'PA', value: 'PA' }, { label: 'PB', value: 'PB' },
    { label: 'PR', value: 'PR' }, { label: 'PE', value: 'PE' }, { label: 'PI', value: 'PI' },
    { label: 'RJ', value: 'RJ' }, { label: 'RN', value: 'RN' }, { label: 'RS', value: 'RS' },
    { label: 'RO', value: 'RO' }, { label: 'RR', value: 'RR' }, { label: 'SC', value: 'SC' },
    { label: 'SP', value: 'SP' }, { label: 'SE', value: 'SE' }, { label: 'TO', value: 'TO' }
  ];

  ngOnInit(): void {
    this.loadInitialData();
  }

  /**
   * Load initial data
   */
  private loadInitialData(): void {
    this.loadProdutores({
      first: 0,
      rows: this.pageSize
    });
  }

  /**
   * Load produtores with lazy loading
   */
  loadProdutores(event: any): void {
    this.isLoading.set(true);
    
    const params: ProdutorQueryParams = {
      page: Math.floor(event.first / event.rows) + 1,
      pageSize: event.rows
    };

    // Add filters
    if (this.searchTerm) {
      params.search = this.searchTerm;
    }
    if (this.selectedTipoCliente) {
      params.tipoCliente = this.selectedTipoCliente;
    }
    if (this.selectedUf) {
      params.uf = this.selectedUf;
    }

    this.produtorService.list(params).subscribe({
      next: (response) => {
console.log(response.items)
        console.log(response)
        this.produtores.set(response.items);
        this.totalRecords.set(response.total);
        this.isLoading.set(false);
      },
      error: (error) => {
        this.isLoading.set(false);
        this.notificationService.showLoadError();
        console.error('Error loading produtores:', error);
      }
    });
  }

  /**
   * Handle search input change with debounce
   */
  onSearchChange(event: any): void {
    if (this.searchTimer) {
      clearTimeout(this.searchTimer);
    }

    this.searchTimer = setTimeout(() => {
      this.searchTerm = event.target.value;
      this.onFilterChange();
    }, 500);
  }

  /**
   * Handle filter changes
   */
  onFilterChange(): void {
    this.loadProdutores({
      first: 0,
      rows: this.pageSize
    });
  }

  /**
   * Clear all filters
   */
  clearFilters(): void {
    this.searchTerm = '';
    this.selectedTipoCliente = null;
    this.selectedUf = null;
    this.onFilterChange();
  }

  /**
   * Navigate to new produtor form
   */
  navigateToNew(): void {
    this.router.navigate(['/produtores/novo']);
  }

  /**
   * Navigate to edit produtor form
   */
  navigateToEdit(id: number): void {
    this.router.navigate(['/produtores/editar', id]);
  }

  /**
   * Confirm delete produtor
   */
  confirmDelete(produtor: Produtor): void {
    this.confirmationService.confirm({
      message: `Tem certeza que deseja excluir o produtor "${produtor.nome}"?`,
      header: 'Confirmar Exclusão',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Sim',
      rejectLabel: 'Não',
      accept: () => {
        this.deleteProdutor(produtor.id);
      }
    });
  }

  /**
   * Delete produtor
   */
  private deleteProdutor(id: number): void {
    this.produtorService.delete(id).subscribe({
      next: () => {
        this.notificationService.showDeleteSuccess();
        this.loadProdutores({
          first: this.currentPage * this.pageSize,
          rows: this.pageSize
        });
      },
      error: (error) => {
        this.notificationService.showDeleteError();
        console.error('Error deleting produtor:', error);
      }
    });
  }

  /**
   * Format CPF/CNPJ for display
   */
  formatCpfCnpj(cpfCnpj: string): string {
    if (!cpfCnpj) return '';
    
    const numbers = cpfCnpj.replace(/\D/g, '');
    
    if (numbers.length === 11) {
      // CPF format: 000.000.000-00
      return numbers.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
    } else if (numbers.length === 14) {
      // CNPJ format: 00.000.000/0000-00
      return numbers.replace(/(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})/, '$1.$2.$3/$4-$5');
    }
    
    return cpfCnpj;
  }

  /**
   * Get tipo cliente label
   */
  getTipoClienteLabel(tipoCliente: TipoCliente): string {
    return tipoCliente === TipoCliente.PF ? 'Pessoa Física' : 'Pessoa Jurídica';
  }

  /**
   * Get tipo cliente severity for tag styling
   */
  getTipoClienteSeverity(tipoCliente: TipoCliente): 'success' | 'info' {
    return tipoCliente === TipoCliente.PF ? 'success' : 'info';
  }

  /**
   * Get localidade from first address
   */
  getLocalidade(produtor: Produtor): string {
    if (produtor.enderecos && produtor.enderecos.length > 0) {
      return produtor.enderecos[0].cidade;
    }
    return '-';
  }

  /**
   * Get UF from first address
   */
  getUf(produtor: Produtor): string {
    if (produtor.enderecos && produtor.enderecos.length > 0) {
      return produtor.enderecos[0].uf;
    }
    return '-';
  }
}