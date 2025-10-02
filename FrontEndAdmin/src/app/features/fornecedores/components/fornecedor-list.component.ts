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
import { UfDto, MunicipioDto } from '../../../shared/models/reference.model';
import { UfService } from '../../referencias/ufs/services/uf.service';
import { MunicipioService } from '../../referencias/municipios/services/municipio.service';

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

  templateUrl: './fornecedor-list.component.html',
  styleUrls: ['./fornecedor-list.component.scss']
})
export class FornecedorListComponent implements OnInit {
  private router = inject(Router);
  private fornecedorService = inject(FornecedorService);
  private notificationService = inject(NotificationService);
  private confirmationService = inject(ConfirmationService);
  public userFeedback = inject(UserFeedbackService);
  private ufService = inject(UfService);
  private municipioService = inject(MunicipioService);

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
  selectedMunicipio: string | null = null;
  
  // Geographic data signals
  availableUfs = signal<UfDto[]>([]);
  availableMunicipios = signal<MunicipioDto[]>([]);
  ufsLoading = signal<boolean>(false);
  municipiosLoading = signal<boolean>(false);

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
    this.loadUfsForFilter();
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
   * Load UFs for filter dropdown
   */
  private loadUfsForFilter(): void {
    this.ufsLoading.set(true);
    
    this.ufService.obterAtivos().subscribe({
      next: (ufs) => {
        this.availableUfs.set(ufs);
        this.ufsLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading UFs for filter:', error);
        this.ufsLoading.set(false);
      }
    });
  }

  /**
   * Load Municípios for filter dropdown based on selected UF
   */
  private loadMunicipiosForFilter(ufId: number): void {
    this.municipiosLoading.set(true);
    
    this.municipioService.obterAtivosPorUf(ufId).subscribe({
      next: (municipios) => {
        this.availableMunicipios.set(municipios);
        this.municipiosLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading Municípios for filter:', error);
        this.municipiosLoading.set(false);
      }
    });
  }

  /**
   * Load fornecedores with lazy loading and enhanced feedback
   */

     loadFornecedores(event: any): void {
       this.isLoading.set(true);
       
       const params: FornecedorQueryParams = {
         pagina: Math.floor(event.first / event.rows) + 1,
         tamanhoPagina: event.rows
       };
   
       // Add search filter (combine all filters into single filtro parameter)
       const filtros: string[] = [];
       if (this.searchTerm) {
         filtros.push(this.searchTerm);
       }
       if (this.selectedTipoCliente) {
         filtros.push(`tipo:${this.selectedTipoCliente}`);
       }
       if (this.selectedUf) {
         filtros.push(`uf:${this.selectedUf}`);
       }
       if (this.selectedMunicipio) {
         filtros.push(`municipio:${this.selectedMunicipio}`);
       }
       
       if (filtros.length > 0) {
         params.filtro = filtros.join(' ');
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
   * Handle UF filter change
   */
  onUfFilterChange(ufId: string | null): void {
    this.selectedUf = ufId;
    this.selectedMunicipio = null; // Reset município when UF changes
    this.availableMunicipios.set([]); // Clear município options
    
    if (ufId) {
      this.loadMunicipiosForFilter(parseInt(ufId));
    }
    
    this.onFilterChange();
  }

  /**
   * Handle Município filter change
   */
  onMunicipioFilterChange(municipioId: string | null): void {
    this.selectedMunicipio = municipioId;
    this.onFilterChange();
  }

  /**
   * Clear all filters
   */
  clearFilters(): void {
    this.searchTerm = '';
    this.selectedTipoCliente = null;
    this.selectedUf = null;
    this.selectedMunicipio = null;
    this.availableMunicipios.set([]);
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
   * Get localidade from address with complete geographic information
   */
  getLocalidade(fornecedor: Fornecedor): string {
    if (fornecedor.endereco) {
      const endereco = fornecedor.endereco as any;
      // Use enriched geographic data if available, fallback to original fields
      const municipioNome = endereco.municipioNome || endereco.cidade;
      return municipioNome || '-';
    }
    return '-';
  }

  /**
   * Get UF from address with complete geographic information
   */
  getUf(fornecedor: Fornecedor): string {
    if (fornecedor.endereco) {
      const endereco = fornecedor.endereco as any;
      // Use enriched geographic data if available, fallback to original fields
      const ufCodigo = endereco.ufCodigo || endereco.uf;
      return ufCodigo || '-';
    }
    return '-';
  }

  /**
   * Get complete geographic information for display
   */
  getCompleteGeographicInfo(fornecedor: Fornecedor): string {
    if (fornecedor.endereco) {
      const endereco = fornecedor.endereco as any;
      const municipio = endereco.municipioNome || endereco.cidade;
      const uf = endereco.ufCodigo || endereco.uf;
      
      if (municipio && uf) {
        return `${municipio} - ${uf}`;
      } else if (municipio) {
        return municipio;
      } else if (uf) {
        return uf;
      }
    }
    return '-';
  }

  /**
   * Get UF display name for filter dropdown
   */
  getUfDisplayName(ufId: string): string {
    const uf = this.availableUfs().find(u => u.id.toString() === ufId);
    return uf ? `${uf.nome} (${uf.uf})` : '';
  }
}