import { Component, inject, signal, OnInit, computed } from '@angular/core';
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

import { ProdutoDisplayDto } from '../services/produto.service';
import { ProdutoService, DropdownOption } from '../services/produto.service';
import { NotificationService } from '../../../core/services/notification.service';
import { TableColumn } from '../../../shared/interfaces/component-template.interface';

/**
 * Interface for filter options
 */
interface FilterOption {
    id: number;
    nome: string;
}

@Component({
    selector: 'app-produto-list',
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
    templateUrl: './produto-list.component.html',
    styleUrls: ['./produto-list.component.scss']
})
export class ProdutoListComponent implements OnInit {
    private router = inject(Router);
    private produtoService = inject(ProdutoService);
    private notificationService = inject(NotificationService);
    private confirmationService = inject(ConfirmationService);

    // Signals for reactive state management
    produtos = signal<ProdutoDisplayDto[]>([]);
    isLoading = signal<boolean>(false);
    totalRecords = signal<number>(0);

    // Filter properties
    searchTerm = '';
    selectedCategoria: number | null = null;
    selectedUnidadeMedida: number | null = null;
    selectedAtividadeAgropecuaria: number | null = null;
    selectedStatusFilter = 'ativas';

    // Filter options
    categoriasParaFiltro = signal<FilterOption[]>([]);
    unidadesParaFiltro = signal<FilterOption[]>([]);
    atividadesParaFiltro = signal<FilterOption[]>([]);

    // All items for filtering
    private todosItens: ProdutoDisplayDto[] = [];

    // Search debounce timer
    private searchTimer?: any;

    // Pagination
    pageSize = 10;
    currentPage = 0;

    // Table configuration
    displayColumns = computed<TableColumn[]>(() => [
        { field: 'codigo', header: 'Código', sortable: true, width: '120px', type: 'text' },
        { field: 'nome', header: 'Nome', sortable: true, type: 'text' },
        { field: 'categoriaNome', header: 'Categoria', sortable: true, hideOnMobile: true, type: 'text' },
        { field: 'unidadeMedidaSimbolo', header: 'Unidade', sortable: true, width: '100px', hideOnMobile: true, type: 'text' },
        { field: 'atividadeAgropecuariaNome', header: 'Atividade', sortable: true, width: '120px', hideOnTablet: true, type: 'text' },
    ]);

    // Status filter options
    statusFilterOptions = [
        { label: 'Ativos', value: 'ativas' },
        { label: 'Inativos', value: 'inativas' },
        { label: 'Todos', value: 'todos' }
    ];

    ngOnInit(): void {
        this.loadInitialData();
    }

    /**
     * Load initial data
     */
    private loadInitialData(): void {
        this.loadProdutos();
    }

    /**
     * Load produtos with filters
     */
    loadProdutos(): void {
        this.isLoading.set(true);

        let request;
        if (this.selectedStatusFilter === 'ativas') {
            request = this.produtoService.obterAtivosParaExibicao();
        } else {
            request = this.produtoService.obterTodosParaExibicao();
        }

        request.subscribe({
            next: (items) => {
                let filteredItems = items;

                if (this.selectedStatusFilter === 'inativas') {
                    filteredItems = items.filter(item => !item.ativo);
                }

                // Store all items for filtering
                this.todosItens = filteredItems;
                this.aplicarFiltros();
                this.carregarOpcoesParaFiltros(filteredItems);
                this.isLoading.set(false);
            },
            error: (error) => {
                this.isLoading.set(false);
                this.notificationService.showLoadError();
                console.error('Error loading produtos:', error);
            }
        });
    }

    /**
     * Load filter options from loaded items
     */
    private carregarOpcoesParaFiltros(items: ProdutoDisplayDto[]): void {
        // Extract unique categorias
        const categorias = Array.from(
            new Map(
                items
                    .filter(item => item.categoriaId && item.categoriaNome)
                    .map(item => [item.categoriaId, { id: item.categoriaId!, nome: item.categoriaNome! }])
            ).values()
        );
        this.categoriasParaFiltro.set(categorias);

        // Extract unique unidades de medida
        const unidades = Array.from(
            new Map(
                items
                    .filter(item => item.unidadeMedidaId && item.unidadeMedidaNome)
                    .map(item => [item.unidadeMedidaId, {
                        id: item.unidadeMedidaId!,
                        nome: `${item.unidadeMedidaSimbolo} - ${item.unidadeMedidaNome}`
                    }])
            ).values()
        );
        this.unidadesParaFiltro.set(unidades);

        // Extract unique atividades agropecuárias
        const atividades = Array.from(
            new Map(
                items
                    .filter(item => item.atividadeAgropecuariaId && item.atividadeAgropecuariaNome)
                    .map(item => [item.atividadeAgropecuariaId, {
                        id: item.atividadeAgropecuariaId!,
                        nome: item.atividadeAgropecuariaCodigo
                            ? `${item.atividadeAgropecuariaCodigo} - ${item.atividadeAgropecuariaNome}`
                            : item.atividadeAgropecuariaNome!
                    }])
            ).values()
        );
        this.atividadesParaFiltro.set(atividades);
    }

    /**
     * Apply all active filters
     */
    aplicarFiltros(): void {
        let itemsFiltrados = [...this.todosItens];

        // Apply categoria filter
        if (this.selectedCategoria) {
            itemsFiltrados = itemsFiltrados.filter(item => item.categoriaId === this.selectedCategoria);
        }

        // Apply unidade de medida filter
        if (this.selectedUnidadeMedida) {
            itemsFiltrados = itemsFiltrados.filter(item => item.unidadeMedidaId === this.selectedUnidadeMedida);
        }

        // Apply atividade agropecuária filter
        if (this.selectedAtividadeAgropecuaria) {
            itemsFiltrados = itemsFiltrados.filter(item => item.atividadeAgropecuariaId === this.selectedAtividadeAgropecuaria);
        }

        // Apply text search filter
        if (this.searchTerm.trim()) {
            const searchTermLower = this.searchTerm.toLowerCase().trim();
            itemsFiltrados = itemsFiltrados.filter(item =>
                item.codigo.toLowerCase().includes(searchTermLower) ||
                item.nome.toLowerCase().includes(searchTermLower) ||
                (item.categoriaNome && item.categoriaNome.toLowerCase().includes(searchTermLower)) ||
                (item.unidadeMedidaNome && item.unidadeMedidaNome.toLowerCase().includes(searchTermLower)) ||
                (item.atividadeAgropecuariaNome && item.atividadeAgropecuariaNome.toLowerCase().includes(searchTermLower))
            );
        }

        this.produtos.set(itemsFiltrados);
        this.totalRecords.set(itemsFiltrados.length);
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
            this.aplicarFiltros();
        }, 300);
    }

    /**
     * Handle filter changes
     */
    onFilterChange(): void {
        this.aplicarFiltros();
    }

    /**
     * Handle status filter change
     */
    onStatusFilterChange(): void {
        this.loadProdutos();
    }

    /**
     * Clear all filters
     */
    clearFilters(): void {
        this.searchTerm = '';
        this.selectedCategoria = null;
        this.selectedUnidadeMedida = null;
        this.selectedAtividadeAgropecuaria = null;
        this.aplicarFiltros();
    }

    /**
     * Check if any filters are active
     */
    readonly temFiltrosAtivos = computed(() => !!(
        this.selectedCategoria ||
        this.selectedUnidadeMedida ||
        this.selectedAtividadeAgropecuaria ||
        this.searchTerm.trim()
    ));

    /**
     * Navigate to new produto form
     */
    navigateToNew(): void {
        this.router.navigate(['/produtos/novo']);
    }

    /**
     * Navigate to edit produto form
     */
    navigateToEdit(id: number): void {
        this.router.navigate(['/produtos', id]);
    }

    /**
     * Confirm delete produto
     */
    confirmDelete(produto: ProdutoDisplayDto): void {
        this.confirmationService.confirm({
            message: `Tem certeza que deseja excluir o produto "${produto.nome}"?`,
            header: 'Confirmar Exclusão',
            icon: 'pi pi-exclamation-triangle',
            acceptLabel: 'Sim',
            rejectLabel: 'Não',
            accept: () => {
                this.deleteProduto(produto.id);
            }
        });
    }

    /**
     * Delete produto
     */
    private deleteProduto(id: number): void {
        this.produtoService.remover(id).subscribe({
            next: () => {
                this.notificationService.showDeleteSuccess();
                this.loadProdutos();
            },
            error: (error) => {
                this.notificationService.showDeleteError();
                console.error('Error deleting produto:', error);
            }
        });
    }

    /**
     * Format price for display
     */
    formatarPreco(preco?: number): string {
        if (!preco || preco === 0) {
            return 'Não informado';
        }
        return new Intl.NumberFormat('pt-BR', {
            style: 'currency',
            currency: 'BRL'
        }).format(preco);
    }

    /**
     * Get CSS class for price display
     */
    getPrecoClass(preco?: number): string {
        if (!preco || preco === 0) {
            return 'price-display price-zero';
        }
        return 'price-display';
    }
}