import { Component, inject, signal, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
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

import { Fornecedor } from '../../../shared/models/fornecedor.model';
import { TipoCliente } from '../../../shared/models/produtor.model';
import { FornecedorService, FornecedorQueryParams } from '../services/fornecedor.service';
import { NotificationService } from '../../../core/services/notification.service';
import { UserFeedbackService } from '../../../shared/services/user-feedback.service';
import { LoadingOperations } from '../../../shared/utils/loading-state.util';
import { LoadingOverlayComponent } from '../../../shared/components/loading-overlay.component';
import { LoadingButtonComponent } from '../../../shared/components/loading-button.component';
import { LoadingStateDirective } from '../../../shared/directives/loading-state.directive';

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
  selector: 'app-fornecedor-list',
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
    TooltipModule,
    LoadingOverlayComponent,
    LoadingButtonComponent,
    LoadingStateDirective,
    RouterLink
  ],
  providers: [ConfirmationService],
  template: `
    <div class="card">
      <!-- Toolbar -->
      <p-toolbar styleClass="mb-4">
        <ng-template pTemplate="left">
          <h2 class="m-0">Fornecedores</h2>
        </ng-template>
        <ng-template pTemplate="right">
          <p-button 
            label="Novo" 
            icon="pi pi-plus" 
            styleClass="p-button-success mr-2"
            routerLink="novo"
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
        [value]="fornecedores()" 
        [columns]="columns"
        [loading]="isLoading()"
        [paginator]="true"
        [rows]="pageSize"
        [totalRecords]="totalRecords()"
        [lazy]="true"
        (onLazyLoad)="loadFornecedores($event)"
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
        
        <ng-template pTemplate="body" let-fornecedor>
          <tr>
            <td>{{ fornecedor.id }}</td>
            <td>{{ fornecedor.nome }}</td>
            <td>{{fornecedor.cnpjFormatado }}</td>
            <td>
              <p-tag 
                [value]="getTipoClienteLabel(fornecedor.tipoCliente)"
                [severity]="getTipoClienteSeverity(fornecedor.tipoCliente)"
              ></p-tag>
            </td>
            <td>{{ getLocalidade(fornecedor) }}</td>
            <td>{{ getUf(fornecedor) }}</td>
            <td>
              <div class="flex gap-2">
                <p-button 
                  icon="pi pi-pencil" 
                  styleClass="p-button-rounded p-button-text p-button-info"
                  pTooltip="Editar"
                  (onClick)="navigateToEdit(fornecedor.id)"
                ></p-button>
                <p-button 
                  icon="pi pi-trash" 
                  styleClass="p-button-rounded p-button-text p-button-danger"
                  pTooltip="Excluir"
                  (onClick)="confirmDelete(fornecedor)"
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
                  {{ searchTerm || selectedTipoCliente || selectedUf ? 'Nenhum fornecedor encontrado com os filtros aplicados' : 'Nenhum fornecedor cadastrado' }}
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
                <span class="ml-3">Carregando fornecedores...</span>
              </div>
            </td>
          </tr>
        </ng-template>
      </p-table>
    </div>

    <p-confirmDialog></p-confirmDialog>
  `

  ,
  styleUrls: ['./fornecedor-list.component.scss']
})
export class FornecedorListComponent implements OnInit {
  private router = inject(Router);
  private fornecedorService = inject(FornecedorService);
  private notificationService = inject(NotificationService);
  private confirmationService = inject(ConfirmationService);
  public userFeedback = inject(UserFeedbackService);

  // Signals for reactive state management
  fornecedores = signal<Fornecedor[]>([]);
  isLoading = signal<boolean>(false);
  totalRecords = signal<number>(0);
  
  // Enhanced loading states
  isListLoading = this.userFeedback.isOperationLoading(LoadingOperations.LIST_LOAD);
  isDeleting = this.userFeedback.isOperationLoading(LoadingOperations.DELETE);

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
    this.loadFornecedores({
      first: 0,
      rows: this.pageSize
    });
  }

  /**
   * Load fornecedores with lazy loading and enhanced feedback
   */

     loadFornecedores(event: any): void {
       this.isLoading.set(true);
       
       const params: FornecedorQueryParams = {
         page: Math.floor(event.first / event.rows) + 1,
         pageSize: event.rows
       };
   
       // Add filters
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
   
   this.fornecedorService.list(params).subscribe({
      next: (response) => {
     console.log(response.items)
        console.log(response)
        this.fornecedores.set(response.items);
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
  **/
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
    this.loadFornecedores({
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
   * Navigate to edit fornecedor form
   */
  navigateToEdit(id: number): void {
    this.router.navigate(['/fornecedores/editar', id]);
  }

  /**
   * Confirm delete fornecedor
   */
  confirmDelete(fornecedor: Fornecedor): void {
    this.confirmationService.confirm({
      message: `Tem certeza que deseja excluir o fornecedor "${fornecedor.nome}"?`,
      header: 'Confirmar Exclusão',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Sim',
      rejectLabel: 'Não',
      accept: () => {
        this.deleteFornecedor(fornecedor.id);
      }
    });
  }

  /**
   * Delete fornecedor with enhanced feedback
   */
  private async deleteFornecedor(id: number): Promise<void> {
    try {
      await this.userFeedback.executeOperation({
        operation: LoadingOperations.DELETE,
        asyncFn: () => this.fornecedorService.delete(id).toPromise(),
        startMessage: 'Excluindo fornecedor',
        successMessage: 'Fornecedor excluído com sucesso',
        errorMessage: 'Erro ao excluir fornecedor',
        showNotifications: true,
        timeout: 10000
      });

      // Reload the list after successful deletion
      await this.loadFornecedores({
        first: this.currentPage * this.pageSize,
        rows: this.pageSize
      });
    } catch (error) {
      console.error('Error deleting fornecedor:', error);
    }
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
   * Get localidade from address
   */
  getLocalidade(fornecedor: Fornecedor): string {
    if (fornecedor.endereco?.cidade) {
      return fornecedor.endereco.cidade;
    }
    return '-';
  }

  /**
   * Get UF from address
   */
  getUf(fornecedor: Fornecedor): string {
    if (fornecedor.endereco?.uf) {
      return fornecedor.endereco.uf;
    }
    return '-';
  }
}