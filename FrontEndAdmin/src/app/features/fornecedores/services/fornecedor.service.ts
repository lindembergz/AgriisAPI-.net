import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, throwError, forkJoin } from 'rxjs';
import { catchError, map, switchMap } from 'rxjs/operators';
import {
  Fornecedor,
  FornecedorForm,
  CriarFornecedorDto,
  AtualizarFornecedorDto,
  FiltrosFornecedorDto,
  FornecedorListResponse
} from '../../../shared/models/fornecedor.model';
import { UfDto, MunicipioDto } from '../../../shared/models/reference.model';
import { UfService } from '../../referencias/ufs/services/uf.service';
import { MunicipioService } from '../../referencias/municipios/services/municipio.service';
import { environment } from '../../../../environments/environment';

/**
 * Query parameters for fornecedor list - matching API
 */
export interface FornecedorQueryParams {
  filtro?: string;
  pagina?: number;
  tamanhoPagina?: number;
}

/**
 * Service for Fornecedor API communication
 * Implements CRUD operations with proper TypeScript typing and error handling
 */
@Injectable({
  providedIn: 'root'
})
export class FornecedorService {
  private http = inject(HttpClient);
  private ufService = inject(UfService);
  private municipioService = inject(MunicipioService);
  private readonly apiUrl = `${environment.apiUrl}/fornecedores`;

  /**
   * Get paginated list of fornecedores - matching API structure
   */
  list(params?: FornecedorQueryParams): Observable<FornecedorListResponse> {
    const filtros: FiltrosFornecedorDto = {
      filtro: params?.filtro,
      pagina: params?.pagina || 1,
      tamanhoPagina: params?.tamanhoPagina || 20
    };

    let httpParams = new HttpParams();
    if (filtros.filtro) httpParams = httpParams.set('Filtro', filtros.filtro);
    httpParams = httpParams.set('Pagina', filtros.pagina.toString());
    httpParams = httpParams.set('TamanhoPagina', filtros.tamanhoPagina.toString());

    return this.http.get<FornecedorListResponse>(this.apiUrl, { params: httpParams })
      .pipe(
        catchError(error => this.handleError('list fornecedores', error))
      );
  }


  /**
   * Get fornecedor by ID - matching API structure
   */
  getById(id: number): Observable<Fornecedor> {
    return this.http.get<Fornecedor>(`${this.apiUrl}/${id}`)
      .pipe(
        catchError(error => this.handleError(`get fornecedor id=${id}`, error))
      );
  }

  /**
   * Create new fornecedor - matching NEW API structure
   */
  create(fornecedor: FornecedorForm): Observable<Fornecedor> {
    const dto: CriarFornecedorDto = {
      codigo: fornecedor.codigo,
      nome: fornecedor.nome,
      cpfCnpj: fornecedor.cpfCnpj,
      tipoCliente: fornecedor.tipoCliente,
      telefone: fornecedor.telefone,
      email: fornecedor.email,
      inscricaoEstadual: fornecedor.inscricaoEstadual,
      endereco: fornecedor.endereco,
      pontosDistribuicao: fornecedor.pontosDistribuicao || [],
      usuarioMaster: fornecedor.usuarioMaster!
    };

    return this.http.post<Fornecedor>(`${this.apiUrl}/completo`, dto)
      .pipe(
        catchError(error => this.handleError('create fornecedor', error))
      );
  }

  /**
   * Update existing fornecedor - matching NEW API structure
   */
  update(id: number, fornecedor: FornecedorForm): Observable<Fornecedor> {
    const dto: AtualizarFornecedorDto = {
      nome: fornecedor.nome,
      inscricaoEstadual: fornecedor.inscricaoEstadual,
      logradouro: fornecedor.endereco?.logradouro,
      ufId: undefined, // Will be derived from endereco
      municipioId: undefined, // Will be derived from endereco
      cep: fornecedor.endereco?.cep,
      complemento: fornecedor.endereco?.complemento,
      latitude: fornecedor.endereco?.latitude,
      longitude: fornecedor.endereco?.longitude,
      telefone: fornecedor.telefone,
      email: fornecedor.email,
      moedaPadrao: 0, // Default value
      pedidoMinimo: undefined,
      tokenLincros: undefined
    };

    return this.http.put<Fornecedor>(`${this.apiUrl}/${id}`, dto)
      .pipe(
        catchError(error => this.handleError(`update fornecedor id=${id}`, error))
      );
  }

  /**
   * Delete fornecedor (soft delete - set ativo = false)
   */
  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`)
      .pipe(
        catchError(error => this.handleError(`delete fornecedor id=${id}`, error))
      );
  }

  /**
   * Search fornecedores by name or CPF/CNPJ
   */
  search(term: string): Observable<Fornecedor[]> {
    const params = new HttpParams().set('search', term);

    return this.http.get<FornecedorListResponse>(`${this.apiUrl}/search`, { params })
      .pipe(
        map(response => response.items),
        catchError(error => this.handleError(`search fornecedores term=${term}`, error))
      );
  }

  /**
   * Get fornecedores by UF (state)
   */
  getByUf(uf: string): Observable<Fornecedor[]> {
    const params = new HttpParams().set('uf', uf);

    return this.http.get<FornecedorListResponse>(this.apiUrl, { params })
      .pipe(
        map(response => response.items),
        catchError(error => this.handleError(`get fornecedores by UF=${uf}`, error))
      );
  }

  /**
   * Validate CNPJ availability - using API endpoint
   */
  validateCnpjDisponivel(cnpj: string, fornecedorIdExcluir?: number): Observable<{ disponivel: boolean }> {
    let params = new HttpParams().set('cnpj', cnpj);
    if (fornecedorIdExcluir) {
      params = params.set('fornecedorIdExcluir', fornecedorIdExcluir.toString());
    }

    return this.http.get<{ disponivel: boolean }>(`${this.apiUrl}/cnpj/${cnpj}/disponivel`, { params })
      .pipe(
        catchError(error => this.handleError(`validate CNPJ=${cnpj}`, error))
      );
  }

  /**
   * Get fornecedor by CNPJ - using API endpoint
   */
  getByCnpj(cnpj: string): Observable<Fornecedor> {
    return this.http.get<Fornecedor>(`${this.apiUrl}/cnpj/${cnpj}`)
      .pipe(
        catchError(error => this.handleError(`get fornecedor by CNPJ=${cnpj}`, error))
      );
  }

  /**
   * Validate UF-Município relationship
   */
  validateGeographicRelationship(ufId: number, municipioId: number): Observable<boolean> {
    return this.municipioService.obterPorId(municipioId).pipe(
      map(municipio => municipio.ufId === ufId),
      catchError(error => {
        console.error('Error validating geographic relationship:', error);
        return throwError(() => new Error('Erro ao validar relacionamento geográfico'));
      })
    );
  }

  /**
   * Get UFs for dropdown
   */
  getUfsForDropdown(): Observable<UfDto[]> {
    return this.ufService.obterAtivos().pipe(
      catchError(error => this.handleError('get UFs for dropdown', error))
    );
  }

  /**
   * Get Municípios by UF for dropdown
   */
  getMunicipiosByUf(ufId: number): Observable<MunicipioDto[]> {
    return this.municipioService.obterAtivosPorUf(ufId).pipe(
      catchError(error => this.handleError(`get municípios by UF id=${ufId}`, error))
    );
  }

  /**
   * Enrich single fornecedor with geographic data
   */
  private enrichWithGeographicData(fornecedor: Fornecedor): Observable<Fornecedor> {
    if (!fornecedor.endereco) {
      return new Observable(observer => {
        observer.next(fornecedor);
        observer.complete();
      });
    }

    const endereco = fornecedor.endereco as any;
    const ufId = endereco.ufId;
    const municipioId = endereco.municipioId;

    if (!ufId || !municipioId) {
      return new Observable(observer => {
        observer.next(fornecedor);
        observer.complete();
      });
    }

    return (forkJoin({
      uf: this.ufService.obterPorId(ufId) as Observable<any>,
      municipio: this.municipioService.obterPorId(municipioId) as Observable<any>
    }) as Observable<any>).pipe(
      map(({ uf, municipio }) => {
        const enrichedFornecedor = { ...fornecedor };
        if (enrichedFornecedor.endereco) {
          enrichedFornecedor.endereco = {
            ...enrichedFornecedor.endereco,
            ufNome: uf.nome,
            ufCodigo: uf.codigo,
            municipioNome: municipio.nome,
            municipioCodigoIbge: municipio.codigoIbge
          };
        }
        return enrichedFornecedor;
      }),
      catchError(error => {
        console.warn('Error enriching fornecedor with geographic data:', error);
        // Return original fornecedor if geographic data loading fails
        return new Observable(observer => {
          observer.next(fornecedor);
          observer.complete();
        });
      })
    ) as Observable<Fornecedor>;
  }

  /**
   * Enrich list response with geographic data
   */
  private enrichListWithGeographicData(response: FornecedorListResponse): Observable<FornecedorListResponse> {
    if (!response.items || response.items.length === 0) {
      return new Observable(observer => {
        observer.next(response);
        observer.complete();
      });
    }

    // Get unique UF and Município IDs from all fornecedores
    const ufIds = new Set<number>();
    const municipioIds = new Set<number>();

    response.items.forEach(fornecedor => {
      if (fornecedor.endereco) {
        const endereco = fornecedor.endereco as any;
        if (endereco.ufId) ufIds.add(endereco.ufId);
        if (endereco.municipioId) municipioIds.add(endereco.municipioId);
      }
    });

    if (ufIds.size === 0 && municipioIds.size === 0) {
      return new Observable(observer => {
        observer.next(response);
        observer.complete();
      });
    }

    // Load all UFs and Municípios in parallel
    const ufRequests = Array.from(ufIds).map(id => this.ufService.obterPorId(id));
    const municipioRequests = Array.from(municipioIds).map(id => this.municipioService.obterPorId(id));

    return (forkJoin({
      ufs: ufRequests.length > 0 ? forkJoin(ufRequests) as Observable<any[]> : new Observable(observer => { observer.next([]); observer.complete(); }),
      municipios: municipioRequests.length > 0 ? forkJoin(municipioRequests) as Observable<any[]> : new Observable(observer => { observer.next([]); observer.complete(); })
    }) as Observable<any>).pipe(
      map(({ ufs, municipios }) => {
        // Create lookup maps
        const ufMap = new Map((ufs as any[]).map((uf: any) => [uf.id, uf]));
        const municipioMap = new Map((municipios as any[]).map((municipio: any) => [municipio.id, municipio]));

        // Enrich each fornecedor
        const enrichedItems = response.items.map(fornecedor => {
          if (!fornecedor.endereco) return fornecedor;

          const endereco = fornecedor.endereco as any;
          const uf = endereco.ufId ? ufMap.get(endereco.ufId) as any : null;
          const municipio = endereco.municipioId ? municipioMap.get(endereco.municipioId) as any : null;

          return {
            ...fornecedor,
            endereco: {
              ...fornecedor.endereco,
              ufNome: uf?.nome || endereco.uf || '',
              ufCodigo: uf?.codigo || endereco.uf || '',
              municipioNome: municipio?.nome || endereco.cidade || '',
              municipioCodigoIbge: municipio?.codigoIbge || ''
            }
          };
        });

        return {
          ...response,
          items: enrichedItems
        };
      }),
      catchError(error => {
        console.warn('Error enriching fornecedor list with geographic data:', error);
        // Return original response if geographic data loading fails
        return new Observable(observer => {
          observer.next(response);
          observer.complete();
        });
      })
    ) as Observable<FornecedorListResponse>;
  }

  /**
   * Validate fornecedor geographic data before save
   */
  private validateFornecedorGeographicData(fornecedor: FornecedorForm): Observable<FornecedorForm> {
    if (!fornecedor.endereco) {
      return new Observable(observer => {
        observer.next(fornecedor);
        observer.complete();
      });
    }

    const endereco = fornecedor.endereco as any;
    const ufId = endereco.ufId;
    const municipioId = endereco.municipioId;

    // If no geographic IDs provided, return as is
    if (!ufId || !municipioId) {
      return new Observable(observer => {
        observer.next(fornecedor);
        observer.complete();
      });
    }

    // Validate that município belongs to the selected UF
    return this.validateGeographicRelationship(ufId, municipioId).pipe(
      map(isValid => {
        if (!isValid) {
          throw new Error('O município selecionado não pertence à UF selecionada');
        }
        return fornecedor;
      }),
      catchError(error => {
        console.error('Geographic validation failed:', error);
        return throwError(() => new Error('Erro na validação geográfica: ' + error.message));
      })
    );
  }

  /**
   * Handle HTTP errors
   */
  private handleError(operation: string, error: any): Observable<never> {
    console.error(`FornecedorService: ${operation} failed:`, error);

    let errorMessage = 'Ocorreu um erro inesperado';

    if (error.error?.message) {
      errorMessage = error.error.message;
    } else if (error.message) {
      errorMessage = error.message;
    } else if (error.status) {
      switch (error.status) {
        case 400:
          errorMessage = 'Dados inválidos fornecidos';
          break;
        case 401:
          errorMessage = 'Não autorizado. Faça login novamente';
          break;
        case 403:
          errorMessage = 'Acesso negado';
          break;
        case 404:
          errorMessage = 'Fornecedor não encontrado';
          break;
        case 409:
          errorMessage = 'CPF/CNPJ já cadastrado';
          break;
        case 500:
          errorMessage = 'Erro interno do servidor';
          break;
        default:
          errorMessage = `Erro ${error.status}: ${error.statusText}`;
      }
    }

    return throwError(() => new Error(errorMessage));
  }
}