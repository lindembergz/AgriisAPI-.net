import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, combineLatest, map, switchMap } from 'rxjs';
import { ReferenceCrudService } from '../../../shared/services/reference-crud.service';
import { 
  ProdutoDto, 
  CriarProdutoDto, 
  AtualizarProdutoDto, 
  ProdutoQueryParams,
  ProdutoListResponse
} from '../../../shared/models/produto.model';
import { 
  UnidadeMedidaDto, 
  EmbalagemDto,
  CategoriaDto, 
  AtividadeAgropecuariaDto 
} from '../../../shared/models/reference.model';

/**
 * Dropdown option interface for reference entities
 */
export interface DropdownOption {
  id: number;
  nome: string;
  codigo?: string;
  ativo?: boolean;
}

/**
 * Reference data for produto form
 */
export interface ProdutoReferenceData {
  unidadesMedida: UnidadeMedidaDto[];
  embalagens: EmbalagemDto[];
  categorias: CategoriaDto[];
  atividadesAgropecuarias: AtividadeAgropecuariaDto[];
}

/**
 * Produto display DTO with reference names
 */
export interface ProdutoDisplayDto extends ProdutoDto {
  categoriaNome: string;
  unidadeMedidaNome: string;
  unidadeMedidaSimbolo: string;
  embalagemNome?: string;
  atividadeAgropecuariaNome?: string;
  atividadeAgropecuariaCodigo?: string;
  preco?: number; // For display purposes
}

/**
 * Service for Produto CRUD operations with reference data management
 */
@Injectable({
  providedIn: 'root'
})
export class ProdutoService extends ReferenceCrudService<ProdutoDto, CriarProdutoDto, AtualizarProdutoDto> {
  
  protected readonly entityName = 'Produto';
  protected readonly apiEndpoint = 'produtos';
  
  protected http = inject(HttpClient);

  /**
   * Get all reference data needed for produto form
   */
  obterDadosReferencia(): Observable<ProdutoReferenceData> {
    return this.errorHandlingService.wrapWithErrorHandling(
      combineLatest({
        unidadesMedida: this.http.get<UnidadeMedidaDto[]>(`${this.baseUrl.replace('produtos', 'unidades-medida')}/ativos`),
        embalagens: this.http.get<EmbalagemDto[]>(`${this.baseUrl.replace('produtos', 'embalagens')}/ativos`),
        categorias: this.http.get<CategoriaDto[]>(`${this.baseUrl.replace('produtos', 'Categorias')}/ativas`),
        atividadesAgropecuarias: this.http.get<AtividadeAgropecuariaDto[]>(`${this.baseUrl.replace('produtos', 'atividades-agropecuarias')}/ativos`)
      }),
      'obter dados de referência',
      'Produto'
    );
  }

  /**
   * Get unidades de medida for dropdown
   */
  obterUnidadesMedida(): Observable<DropdownOption[]> {
    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.get<UnidadeMedidaDto[]>(`${this.baseUrl.replace('produtos', 'unidades-medida')}/ativos`).pipe(
        map(unidades => unidades.map(u => ({
          id: u.id,
          nome: u.nome,
          codigo: u.simbolo,
          ativo: u.ativo
        })))
      ),
      'obter unidades de medida',
      'Produto'
    );
  }

  /**
   * Get embalagens filtered by unidade de medida
   */
  obterEmbalagensPorUnidade(unidadeMedidaId?: number): Observable<DropdownOption[]> {
    let url = `${this.baseUrl.replace('produtos', 'embalagens')}/ativos`;
    
    if (unidadeMedidaId) {
      url = `${this.baseUrl.replace('produtos', 'embalagens')}/unidade/${unidadeMedidaId}`;
    }
    
    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.get<EmbalagemDto[]>(url).pipe(
        map(embalagens => embalagens.map(e => ({
          id: e.id,
          nome: e.nome,
          ativo: e.ativo
        })))
      ),
      'obter embalagens',
      'Produto'
    );
  }

  /**
   * Get categorias for hierarchical selector
   */
  obterCategorias(): Observable<CategoriaDto[]> {
    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.get<CategoriaDto[]>(`${this.baseUrl.replace('produtos', 'Categorias')}/ativas`),
      'obter categorias',
      'Produto'
    );
  }

  /**
   * Get atividades agropecuarias for dropdown
   */
  obterAtividadesAgropecuarias(): Observable<DropdownOption[]> {
    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.get<AtividadeAgropecuariaDto[]>(`${this.baseUrl.replace('produtos', 'atividades-agropecuarias')}/ativos`).pipe(
        map(atividades => atividades.map(a => ({
          id: a.id,
          nome: a.descricao,
          codigo: a.codigo,
          ativo: a.ativo
        })))
      ),
      'obter atividades agropecuárias',
      'Produto'
    );
  }

  /**
   * Search produtos with filters
   */
  buscarProdutos(params: ProdutoQueryParams): Observable<ProdutoListResponse> {
    const searchParams = new URLSearchParams();
    
    if (params.termo) searchParams.set('termo', params.termo);
    if (params.categoriaId) searchParams.set('categoriaId', params.categoriaId.toString());
    if (params.unidadeMedidaId) searchParams.set('unidadeMedidaId', params.unidadeMedidaId.toString());
    if (params.atividadeAgropecuariaId) searchParams.set('atividadeAgropecuariaId', params.atividadeAgropecuariaId.toString());
    if (params.ativo !== undefined) searchParams.set('ativo', params.ativo.toString());
    if (params.pagina) searchParams.set('pagina', params.pagina.toString());
    if (params.tamanhoPagina) searchParams.set('tamanhoPagina', params.tamanhoPagina.toString());
    if (params.ordenacao) searchParams.set('ordenacao', params.ordenacao);

    return this.http.get<ProdutoListResponse>(`${this.baseUrl}/buscar?${searchParams.toString()}`);
  }

  /**
   * Get produtos with all reference data included
   */
  obterTodosComReferencias(): Observable<ProdutoDto[]> {
    return this.http.get<ProdutoDto[]>(`${this.baseUrl}?includeReferences=true`);
  }

  /**
   * Get produto by ID with all reference data included
   */
  obterPorIdComReferencias(id: number): Observable<ProdutoDto> {
    return this.http.get<ProdutoDto>(`${this.baseUrl}/${id}?includeReferences=true`);
  }

  /**
   * Validate if codigo is unique
   */
  validarCodigoUnico(codigo: string, idExcluir?: number): Observable<boolean> {
    let url = `${this.baseUrl}/validar-codigo?codigo=${encodeURIComponent(codigo)}`;
    if (idExcluir) {
      url += `&idExcluir=${idExcluir}`;
    }
    return this.http.get<{ isUnique: boolean }>(url).pipe(
      map(response => response.isUnique)
    );
  }

  /**
   * Validate if nome is unique
   */
  validarNomeUnico(nome: string, idExcluir?: number): Observable<boolean> {
    let url = `${this.baseUrl}/validar-nome?nome=${encodeURIComponent(nome)}`;
    if (idExcluir) {
      url += `&idExcluir=${idExcluir}`;
    }
    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.get<{ isUnique: boolean }>(url).pipe(
        map(response => response.isUnique)
      ),
      'validar nome único',
      'Produto'
    );
  }

  /**
   * Validate reference entity existence
   */
  validarReferencias(dto: CriarProdutoDto | AtualizarProdutoDto): Observable<{ isValid: boolean; errors: string[] }> {
    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.post<{ isValid: boolean; errors: string[] }>(`${this.baseUrl}/validar-referencias`, dto),
      'validar referências',
      'Produto'
    );
  }

  /**
   * Get fornecedores for dropdown
   */
  obterFornecedores(): Observable<DropdownOption[]> {
    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.get<any[]>(`${this.baseUrl.replace('produtos', 'fornecedores')}/ativos`).pipe(
        map(fornecedores => fornecedores.map(f => ({
          id: f.id,
          nome: f.nome,
          ativo: f.ativo
        })))
      ),
      'obter fornecedores',
      'Produto'
    );
  }

  /**
   * Get culturas for multi-select
   */
  obterCulturas(): Observable<DropdownOption[]> {
    return this.errorHandlingService.wrapWithErrorHandling(

      
      this.http.get<any[]>(`${this.baseUrl.replace('produtos', 'Culturas')}/ativas`).pipe(
        map(culturas => culturas.map(c => ({
          id: c.id,
          nome: c.nome,
          ativo: c.ativo
        })))
      ),
      'obter culturas',
      'Produto'
    );
  }

  /**
   * Get produtos pais for dropdown (produtos que podem ter filhos)
   */
  obterProdutosPais(): Observable<DropdownOption[]> {
    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.get<any[]>(`${this.baseUrl}/fabricantes`).pipe(
        map(produtos => produtos.map(p => ({
          id: p.id,
          nome: p.nome,
          codigo: p.codigo,
          ativo: p.status === 0 // StatusProduto.Ativo
        })))
      ),
      'obter produtos pais',
      'Produto'
    );
  }

  /**
   * Get produto with full reference data for display
   */
  obterComReferenciasCompletas(id: number): Observable<ProdutoDto> {
    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.get<ProdutoDto>(`${this.baseUrl}/${id}?includeReferences=true&includeInactive=false`),
      `obter produto com referências id=${id}`,
      'Produto'
    );
  }

  /**
   * Override criar to include reference validation
   */
  override criar(dto: CriarProdutoDto): Observable<ProdutoDto> {
    return this.errorHandlingService.wrapWithErrorHandling(
      this.validarReferencias(dto).pipe(
        switchMap(validation => {
          if (!validation.isValid) {
            throw new Error(`Referências inválidas: ${validation.errors.join(', ')}`);
          }
          return this.http.post<ProdutoDto>(this.baseUrl, dto);
        })
      ),
      'criar produto',
      'Produto'
    );
  }

  /**
   * Override atualizar to include reference validation
   */
  override atualizar(id: number, dto: AtualizarProdutoDto, rowVersion?: string): Observable<ProdutoDto> {
    return this.errorHandlingService.wrapWithErrorHandling(
      this.validarReferencias(dto).pipe(
        switchMap(validation => {
          if (!validation.isValid) {
            throw new Error(`Referências inválidas: ${validation.errors.join(', ')}`);
          }
          return this.http.put<ProdutoDto>(`${this.baseUrl}/${id}`, dto);
        })
      ),
      'atualizar produto',
      'Produto'
    );
  }

  /**
   * Transform ProdutoDto to ProdutoDisplayDto with reference names
   */
  transformToDisplayDto(produto: ProdutoDto): ProdutoDisplayDto {
    return {
      ...produto,
      categoriaNome: produto.categoriaNome || produto.categoria?.nome || 'N/A',
      unidadeMedidaNome: produto.unidadeMedidaNome || produto.unidadeMedida?.nome || 'N/A',
      unidadeMedidaSimbolo: produto.unidadeMedidaSimbolo || produto.unidadeMedida?.simbolo || 'N/A',
      embalagemNome: produto.embalagemNome || produto.embalagem?.nome,
      atividadeAgropecuariaNome: produto.atividadeAgropecuariaNome || produto.atividadeAgropecuaria?.descricao,
      atividadeAgropecuariaCodigo: produto.atividadeAgropecuariaCodigo || produto.atividadeAgropecuaria?.codigo
    };
  }

  /**
   * Get all produtos with display-friendly data
   */
  obterTodosParaExibicao(): Observable<ProdutoDisplayDto[]> {
    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.get<ProdutoDto[]>(`${this.baseUrl}`).pipe(
        map(produtos => produtos.map(produto => this.transformToDisplayDto(produto)))
      ),
      'obter produtos para exibição',
      'Produto'
    );
  }

  /**
   * Get active produtos with display-friendly data
   */
  obterAtivosParaExibicao(): Observable<ProdutoDisplayDto[]> {
    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.get<ProdutoDto[]>(`${this.baseUrl}/ativos?includeReferences=true`).pipe(
        map(produtos => produtos.map(produto => this.transformToDisplayDto(produto)))
      ),
      'obter produtos ativos para exibição',
      'Produto'
    );
  }
}