import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { Fornecedor, FornecedorForm } from '../../../shared/models/fornecedor.model';
import { environment } from '../../../../environments/environment';

/**
 * API response interface for paginated results
 */
export interface FornecedorListResponse {
  items: Fornecedor[];
  total: number;
  page: number;
  pageSize: number;
}

/**
 * Query parameters for fornecedor list
 */
export interface FornecedorQueryParams {
  page?: number;
  pageSize?: number;
  search?: string;
  tipoCliente?: string;
  uf?: string;
  ativo?: boolean;
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
  private readonly apiUrl = `${environment.apiUrl}/fornecedores`;

  /**
   * Get paginated list of fornecedores
   */

     list(params?: FornecedorQueryParams): Observable<FornecedorListResponse> {
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
   
       return this.http.get<FornecedorListResponse>(this.apiUrl, { params: httpParams })
         .pipe(
           catchError(error => this.handleError('list fornecedores', error))
         );
     }


  /**
   * Get fornecedor by ID
   */
  getById(id: number): Observable<Fornecedor> {
    return this.http.get<Fornecedor>(`${this.apiUrl}/${id}`)
      .pipe(
        catchError(error => this.handleError(`get fornecedor id=${id}`, error))
      );
  }

  /**
   * Create new fornecedor with complete structure
   */
  create(fornecedor: FornecedorForm): Observable<Fornecedor> {
    return this.http.post<Fornecedor>(`${this.apiUrl}/completo`, fornecedor)
      .pipe(
        catchError(error => this.handleError('create fornecedor', error))
      );
  }

  /**
   * Update existing fornecedor
   */
  update(id: number, fornecedor: FornecedorForm): Observable<Fornecedor> {
    return this.http.put<Fornecedor>(`${this.apiUrl}/${id}`, fornecedor)
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