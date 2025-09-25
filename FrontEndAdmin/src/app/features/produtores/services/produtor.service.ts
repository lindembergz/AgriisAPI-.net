import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { Produtor, ProdutorForm } from '../../../shared/models/produtor.model';
import { environment } from '../../../../environments/environment';

/**
 * API response interface for paginated results
 */
export interface ProdutorListResponse {
  items: Produtor[];
  total: number;
  page: number;
  pageSize: number;
}

/**
 * Query parameters for produtor list
 */
export interface ProdutorQueryParams {
  page?: number;
  pageSize?: number;
  search?: string;
  tipoCliente?: string;
  uf?: string;
  ativo?: boolean;
}

/**
 * Service for Produtor API communication
 * Implements CRUD operations with proper TypeScript typing and error handling
 */
@Injectable({
  providedIn: 'root'
})
export class ProdutorService {
  private http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/produtores`;

  /**
   * Get paginated list of produtores
   */
  list(params?: ProdutorQueryParams): Observable<ProdutorListResponse> {
    let httpParams = new HttpParams();
    
    if (params) {
      if (params.page !== undefined) {
        httpParams = httpParams.set('page', params.page.toString());
      }
      if (params.pageSize !== undefined) {
        httpParams = httpParams.set('pageSize', params.pageSize.toString());
      }
      if (params.search) {
        httpParams = httpParams.set('search', params.search);
      }
      if (params.tipoCliente) {
        httpParams = httpParams.set('tipoCliente', params.tipoCliente);
      }
      if (params.uf) {
        httpParams = httpParams.set('uf', params.uf);
      }
      if (params.ativo !== undefined) {
        httpParams = httpParams.set('ativo', params.ativo.toString());
      }
    }

    return this.http.get<ProdutorListResponse>(this.apiUrl, { params: httpParams })
      .pipe(
        catchError(error => this.handleError('list produtores', error))
      );
  }

  /**
   * Get produtor by ID
   */
  getById(id: number): Observable<Produtor> {
    return this.http.get<Produtor>(`${this.apiUrl}/${id}`)
      .pipe(
        catchError(error => this.handleError(`get produtor id=${id}`, error))
      );
  }

  /**
   * Create new produtor
   */
  create(produtor: ProdutorForm): Observable<Produtor> {
    return this.http.post<Produtor>(this.apiUrl, produtor)
      .pipe(
        catchError(error => this.handleError('create produtor', error))
      );
  }

  /**
   * Create new produtor with complete structure (user master and relationships)
   */
  createComplete(produtor: ProdutorForm): Observable<Produtor> {
    return this.http.post<Produtor>(`${this.apiUrl}/completo`, produtor)
      .pipe(
        catchError(error => this.handleError('create complete produtor', error))
      );
  }

  /**
   * Update existing produtor
   */
  update(id: number, produtor: ProdutorForm): Observable<Produtor> {
    return this.http.put<Produtor>(`${this.apiUrl}/${id}`, produtor)
      .pipe(
        catchError(error => this.handleError(`update produtor id=${id}`, error))
      );
  }

  /**
   * Delete produtor (soft delete - set ativo = false)
   */
  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`)
      .pipe(
        catchError(error => this.handleError(`delete produtor id=${id}`, error))
      );
  }

  /**
   * Search produtores by name or CPF/CNPJ
   */
  search(term: string): Observable<Produtor[]> {
    const params = new HttpParams().set('search', term);
    
    return this.http.get<ProdutorListResponse>(`${this.apiUrl}/search`, { params })
      .pipe(
        map(response => response.items),
        catchError(error => this.handleError(`search produtores term=${term}`, error))
      );
  }

  /**
   * Get produtores by UF (state)
   */
  getByUf(uf: string): Observable<Produtor[]> {
    const params = new HttpParams().set('uf', uf);
    
    return this.http.get<ProdutorListResponse>(this.apiUrl, { params })
      .pipe(
        map(response => response.items),
        catchError(error => this.handleError(`get produtores by UF=${uf}`, error))
      );
  }

  /**
   * Validate CPF/CNPJ
   */
  validateDocument(cpfCnpj: string): Observable<{ valid: boolean; message?: string }> {
    const params = new HttpParams().set('document', cpfCnpj);
    
    return this.http.get<{ valid: boolean; message?: string }>(`${this.apiUrl}/validate-document`, { params })
      .pipe(
        catchError(error => this.handleError(`validate document=${cpfCnpj}`, error))
      );
  }

  /**
   * Handle HTTP errors
   */
  private handleError(operation: string, error: any): Observable<never> {
    console.error(`ProdutorService: ${operation} failed:`, error);
    
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
          errorMessage = 'Produtor não encontrado';
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