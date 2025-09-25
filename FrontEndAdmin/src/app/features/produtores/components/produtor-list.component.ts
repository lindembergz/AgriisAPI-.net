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
  template: `
    <div class="card">
      <!-- Toolbar -->
      <p-toolbar styleClass="mb-4">
        <ng-template pTemplate="left">
          <h2 class="m-0">Produtores</h2>
        </ng-template>
        <ng-template pTemplate="right">
          <p-button 
            label="Novo" 
            icon="pi pi-plus" 
            styleClass="p-button-success mr-2"
            (onClick)="navigateToNew()"
          ></p-button>
        </ng-template>
      </p-toolbar>

      <!-- Filters -->
      <div class="grid mb-3">
        <div class="col-12 md:col-4">
          <span class="p-input-icon-left w-full">
            <i class="pi pi-search"></i>
            <input 
              pInputText 
              type="text" 
              placeholder="Buscar por nome ou CPF/CNPJ"
              class="w-full"
              [(ngModel)]="searchTerm"
              (input)="onSearchChange($event)"
            />
          </span>
        </div>
      </div>

      <!-- Data Table -->
      <p-table 
        [value]="produtores()" 
        [columns]="columns"
        [loading]="isLoading()"
        [paginator]="true"
        [rows]="pageSize"
        [totalRecords]="totalRecords()"
        [lazy]="true"
        (onLazyLoad)="loadProdutores($event)"
        [showCurrentPageReport]="true"
        currentPageReportTemplate="Mostrando {first} a {last} de {totalRecords} registros"
        [rowsPerPageOptions]="[10, 25, 50]"
        styleClass="p-datatable-gridlines"
        [tableStyle]="{ 'min-width': '50rem' }"
        [globalFilterFields]="['nome', 'cpfCnpj', 'codigo']"
      >
        <ng-template pTemplate="header">
          <tr>
            <th *ngFor="let col of columns" [pSortableColumn]="col.sortable ? col.field : undefined" [style.width]="col.width">
              {{ col.header }}
              <p-sortIcon *ngIf="col.sortable" [field]="col.field"></p-sortIcon>
            </th>
            <th style="width: 120px">Ações</th>
          </tr>
        </ng-template>
        
        <ng-template pTemplate="body" let-produtor>
          <tr>
            <td>{{ produtor.id }}</td>
            <td>{{ produtor.nome }}</td>
            <td>{{produtor.cpfFormatado }}</td>
            <td>
              <p-tag 
                [value]="getTipoClienteLabel(produtor.tipoCliente)"
                [severity]="getTipoClienteSeverity(produtor.tipoCliente)"
              ></p-tag>
            </td>
            <td>{{ getLocalidade(produtor) }}</td>
            <td>{{ getUf(produtor) }}</td>
            <td>
              <div class="flex gap-2">
                <p-button 
                  icon="pi pi-pencil" 
                  styleClass="p-button-rounded p-button-text p-button-info"
                  pTooltip="Editar"
                  (onClick)="navigateToEdit(produtor.id)"
                ></p-button>
                <p-button 
                  icon="pi pi-trash" 
                  styleClass="p-button-rounded p-button-text p-button-danger"
                  pTooltip="Excluir"
                  (onClick)="confirmDelete(produtor)"
                ></p-button>
              </div>
            </td>
          </tr>
        </ng-template>

        <ng-template pTemplate="emptymessage">
          <tr>
            <td [attr.colspan]="columns.length + 1" class="text-center">
              <div class="flex flex-column align-items-center justify-content-center py-5">
                <i class="pi pi-users" style="font-size: 3rem; color: var(--text-color-secondary);"></i>
                <p class="mt-3 mb-0" style="color: var(--text-color-secondary);">
                  {{ searchTerm || selectedTipoCliente || selectedUf ? 'Nenhum produtor encontrado com os filtros aplicados' : 'Nenhum produtor cadastrado' }}
                </p>
              </div>
            </td>
          </tr>
        </ng-template>

        <ng-template pTemplate="loadingbody">
          <tr>
            <td [attr.colspan]="columns.length + 1" class="text-center">
              <div class="flex align-items-center justify-content-center py-5">
                <p-progressSpinner [style]="{ width: '50px', height: '50px' }"></p-progressSpinner>
                <span class="ml-3">Carregando produtores...</span>
              </div>
            </td>
          </tr>
        </ng-template>
      </p-table>
    </div>

    <p-confirmDialog></p-confirmDialog>
  `,
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