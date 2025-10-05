import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, catchError, of, tap } from 'rxjs';
import { ReferenceCrudService } from '../../../../shared/services/reference-crud.service';
import { 
  CategoriaDto, 
  CriarCategoriaDto, 
  AtualizarCategoriaDto, 
  CategoriaProduto 
} from '../../../../shared/models/reference.model';

/**
 * Tree node interface for PrimeNG TreeTable integration
 */
export interface TreeNode<T> {
  data: T;
  children?: TreeNode<T>[];
  expanded?: boolean;
  leaf?: boolean;
  parent?: TreeNode<T>;
}

/**
 * Dropdown option interface for categoria pai selection
 */
export interface CategoriaDropdownOption {
  id: number;
  nome: string;
  nivel: number;
  nomeCompleto: string;
  ativo: boolean;
}

/**
 * Tipo option interface for filtering
 */
export interface TipoOption {
  value: CategoriaProduto;
  label: string;
}

/**
 * Service for Categoria CRUD operations with hierarchy management
 */
@Injectable({
  providedIn: 'root'
})
export class CategoriaService extends ReferenceCrudService<
  CategoriaDto, 
  CriarCategoriaDto, 
  AtualizarCategoriaDto
> {
  
  protected readonly entityName = 'Categoria';
  protected readonly apiEndpoint = 'categorias';
  
  protected http = inject(HttpClient);

  constructor() {
    super();
    // Configure cache for categories with longer TTL due to hierarchical nature
    this.configurarCache({
      enabled: true,
      ttlMinutes: 15,
      maxSize: 50
    });
  }

  /**
   * Get all categories with complete hierarchy
   */
  obterComHierarquia(): Observable<CategoriaDto[]> {
    const cacheKey = 'hierarchy';
    const cached = this.getFromCache<CategoriaDto[]>(cacheKey);
    
    if (cached) {
      return of(cached);
    }

    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.get<CategoriaDto[]>(`${this.baseUrl}/hierarquia`).pipe(
        map(categorias => this.buildHierarchy(categorias)),
        tap(categorias => this.setCache(cacheKey, categorias))
      ),
      'obter categorias com hierarquia',
      'categoria'
    );
  }

  /**
   * Get only root categories (categories without parent)
   */
  obterCategoriasRaiz(): Observable<CategoriaDto[]> {
    const cacheKey = 'root-categories';
    const cached = this.getFromCache<CategoriaDto[]>(cacheKey);
    
    if (cached) {
      return of(cached);
    }

    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.get<CategoriaDto[]>(`${this.baseUrl}/raiz`).pipe(
        tap(categorias => this.setCache(cacheKey, categorias))
      ),
      'obter categorias raiz',
      'categoria'
    );
  }

  /**
   * Get subcategories of a specific parent category
   */
  obterSubCategorias(categoriaPaiId: number): Observable<CategoriaDto[]> {
    const cacheKey = `subcategories-${categoriaPaiId}`;
    const cached = this.getFromCache<CategoriaDto[]>(cacheKey);
    
    if (cached) {
      return of(cached);
    }

    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.get<CategoriaDto[]>(`${this.baseUrl}/${categoriaPaiId}/subcategorias`).pipe(
        tap(categorias => this.setCache(cacheKey, categorias))
      ),
      `obter subcategorias da categoria ${categoriaPaiId}`,
      'categoria'
    );
  }

  /**
   * Get categories filtered by product type
   */
  obterPorTipo(tipo: CategoriaProduto): Observable<CategoriaDto[]> {
    const cacheKey = `by-type-${tipo}`;
    const cached = this.getFromCache<CategoriaDto[]>(cacheKey);
    
    if (cached) {
      return of(cached);
    }

    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.get<CategoriaDto[]>(`${this.baseUrl}/tipo/${tipo}`).pipe(
        tap(categorias => this.setCache(cacheKey, categorias))
      ),
      `obter categorias do tipo ${tipo}`,
      'categoria'
    );
  }

  /**
   * Get categories ordered by ordem field
   */
  obterOrdenadas(): Observable<CategoriaDto[]> {
    const cacheKey = 'ordered';
    const cached = this.getFromCache<CategoriaDto[]>(cacheKey);
    
    if (cached) {
      return of(cached);
    }

    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.get<CategoriaDto[]>(`${this.baseUrl}/ordenadas`).pipe(
        tap(categorias => this.setCache(cacheKey, categorias))
      ),
      'obter categorias ordenadas',
      'categoria'
    );
  }

  /**
   * Get category by exact name
   */
  obterPorNome(nome: string): Observable<CategoriaDto> {
    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.get<CategoriaDto>(`${this.baseUrl}/nome/${encodeURIComponent(nome)}`),
      `obter categoria por nome '${nome}'`,
      'categoria'
    );
  }

  /**
   * Check if a category name already exists (for uniqueness validation)
   */
  existeComNome(nome: string, idExcluir?: number): Observable<boolean> {
    let url = `${this.baseUrl}/existe-nome?nome=${encodeURIComponent(nome)}`;
    if (idExcluir) {
      url += `&idExcluir=${idExcluir}`;
    }
    
    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.get<{ exists: boolean }>(url).pipe(
        map(response => response.exists)
      ),
      `verificar existência do nome '${nome}'`,
      'categoria'
    );
  }

 
  /**
   * Transform flat category list to hierarchical structure
   */
  private buildHierarchy(categorias: CategoriaDto[]): CategoriaDto[] {
    const categoryMap = new Map<number, CategoriaDto>();
    const rootCategories: CategoriaDto[] = [];

    // First pass: create map and initialize subCategorias arrays
    categorias.forEach(categoria => {
      categoria.subCategorias = [];
      categoryMap.set(categoria.id, categoria);
    });

    // Second pass: build hierarchy
    categorias.forEach(categoria => {
      if (categoria.categoriaPaiId) {
        const parent = categoryMap.get(categoria.categoriaPaiId);
        if (parent) {
          parent.subCategorias.push(categoria);
        }
      } else {
        rootCategories.push(categoria);
      }
    });

    return rootCategories;
  }

  /**
   * Transform categories to TreeNode structure for PrimeNG TreeTable
   */
  transformToTreeNodes(categorias: CategoriaDto[]): TreeNode<CategoriaDto>[] {
    return categorias.map(categoria => this.createTreeNode(categoria));
  }

  /**
   * Create a single TreeNode from CategoriaDto
   */
  private createTreeNode(categoria: CategoriaDto, parent?: TreeNode<CategoriaDto>): TreeNode<CategoriaDto> {
    const node: TreeNode<CategoriaDto> = {
      data: categoria,
      expanded: false,
      leaf: !categoria.subCategorias || categoria.subCategorias.length === 0,
      parent
    };

    if (categoria.subCategorias && categoria.subCategorias.length > 0) {
      node.children = categoria.subCategorias.map(sub => this.createTreeNode(sub, node));
    }

    return node;
  }

  /**
   * Get categories formatted for dropdown selection (with hierarchy indication)
   */
  obterParaDropdown(): Observable<CategoriaDropdownOption[]> {
    return this.obterComHierarquia().pipe(
      map(categorias => this.flattenForDropdown(categorias, 0))
    );
  }

  /**
   * Flatten hierarchical categories for dropdown with level indication
   */
  private flattenForDropdown(categorias: CategoriaDto[], nivel: number): CategoriaDropdownOption[] {
    const options: CategoriaDropdownOption[] = [];

    categorias.forEach(categoria => {
      const prefix = '—'.repeat(nivel);
      const nomeCompleto = nivel > 0 ? `${prefix} ${categoria.nome}` : categoria.nome;
      
      options.push({
        id: categoria.id,
        nome: categoria.nome,
        nivel,
        nomeCompleto,
        ativo: categoria.ativo
      });

      if (categoria.subCategorias && categoria.subCategorias.length > 0) {
        options.push(...this.flattenForDropdown(categoria.subCategorias, nivel + 1));
      }
    });

    return options;
  }

  /**
   * Get available product types for filtering
   */
  obterTiposDisponiveis(): TipoOption[] {
    return [
      { value: CategoriaProduto.Sementes, label: 'Sementes' },
      { value: CategoriaProduto.Fertilizantes, label: 'Fertilizantes' },
      { value: CategoriaProduto.Defensivos, label: 'Defensivos' },
      { value: CategoriaProduto.Inoculantes, label: 'Inoculantes' },
      { value: CategoriaProduto.Adjuvantes, label: 'Adjuvantes' },
      { value: CategoriaProduto.Micronutrientes, label: 'Micronutrientes' },
      { value: CategoriaProduto.Outros, label: 'Outros' }
    ];
  }

  /**
   * Get tipo label from enum value
   */
  obterLabelTipo(tipo: CategoriaProduto): string {
    const tipoOption = this.obterTiposDisponiveis().find(t => t.value === tipo);
    return tipoOption?.label || 'Desconhecido';
  }

  /**
   * Validate that setting a parent category doesn't create circular reference
   */
  validarReferenciaCircular(categoriaId: number, categoriaPaiId: number): Observable<boolean> {
    return this.http.get<{ isValid: boolean }>(`${this.baseUrl}/${categoriaId}/validar-pai/${categoriaPaiId}`).pipe(
      map(response => response.isValid),
      catchError(error => {
        console.error(`Erro ao validar referência circular categoria ${categoriaId} -> ${categoriaPaiId}:`, error);
        // In case of error, assume it's not valid to be safe
        return of(false);
      })
    );
  }

  /**
   * Validate categoria data before submission
   */
  validarCategoria(dto: CriarCategoriaDto | AtualizarCategoriaDto, categoriaId?: number): Observable<{ isValid: boolean; errors: string[] }> {
    const validationDto = {
      ...dto,
      categoriaId: categoriaId
    };

    return this.http.post<{ isValid: boolean; errors: string[] }>(`${this.baseUrl}/validar`, validationDto).pipe(
      catchError(error => {
        console.error('Erro ao validar categoria:', error);
        return of({ isValid: false, errors: ['Erro ao validar categoria'] });
      })
    );
  }

  /**
   * Override invalidateCache to clear hierarchy-specific cache keys
   */
  protected override invalidateCache(pattern?: string): void {
    super.invalidateCache(pattern);
    
    // Additional cache invalidation is handled by the parent class
    // The parent class will clear all cache entries when called without pattern
  }

  /**
   * Override criar to invalidate hierarchy cache
   */
  override criar(dto: CriarCategoriaDto): Observable<CategoriaDto> {
    return super.criar(dto).pipe(
      tap(() => this.invalidateCache())
    );
  }

  /**
   * Override atualizar to invalidate hierarchy cache
   */
  override atualizar(id: number, dto: AtualizarCategoriaDto, rowVersion?: string): Observable<CategoriaDto> {
    return super.atualizar(id, dto, rowVersion).pipe(
      tap(() => this.invalidateCache())
    );
  }

  /**
   * Override ativar to invalidate cache
   */
  override ativar(id: number): Observable<void> {
    return super.ativar(id).pipe(
      tap(() => this.invalidateCache())
    );
  }

  /**
   * Override desativar to invalidate cache
   */
  override desativar(id: number): Observable<void> {
    return super.desativar(id).pipe(
      tap(() => this.invalidateCache())
    );
  }

  /**
   * Override remover to invalidate cache
   */
  override remover(id: number): Observable<void> {
    return super.remover(id).pipe(
      tap(() => this.invalidateCache())
    );
  }
}