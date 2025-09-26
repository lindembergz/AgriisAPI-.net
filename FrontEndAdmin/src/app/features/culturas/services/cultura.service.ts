import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { MessageService } from 'primeng/api';
import { CulturaDto, CriarCulturaDto, AtualizarCulturaDto } from '../models';
import { environment } from '../../../../environments/environment';

/**
 * Service for Cultura API communication
 * Implements CRUD operations with proper TypeScript typing and error handling
 */
@Injectable({
  providedIn: 'root'
})
export class CulturaService {
  private http = inject(HttpClient);
  private messageService = inject(MessageService);
  private readonly apiUrl = `${environment.apiUrl}/culturas`;

  /**
   * Obter todas as culturas
   */
  obterTodas(): Observable<CulturaDto[]> {
    return this.http.get<CulturaDto[]>(this.apiUrl)
      .pipe(
        catchError(error => this.handleError('obter todas as culturas', error))
      );
  }

  /**
   * Obter apenas culturas ativas
   */
  obterAtivas(): Observable<CulturaDto[]> {
    return this.http.get<CulturaDto[]>(`${this.apiUrl}/ativas`)
      .pipe(
        catchError(error => this.handleError('obter culturas ativas', error))
      );
  }

  /**
   * Obter cultura por ID
   */
  obterPorId(id: number): Observable<CulturaDto> {
    return this.http.get<CulturaDto>(`${this.apiUrl}/${id}`)
      .pipe(
        catchError(error => this.handleError(`obter cultura id=${id}`, error))
      );
  }

  /**
   * Criar nova cultura
   */
  criar(cultura: CriarCulturaDto): Observable<CulturaDto> {
    return this.http.post<CulturaDto>(this.apiUrl, cultura)
      .pipe(
        catchError(error => this.handleError('criar cultura', error))
      );
  }

  /**
   * Atualizar cultura existente
   */
  atualizar(id: number, cultura: AtualizarCulturaDto): Observable<CulturaDto> {
    return this.http.put<CulturaDto>(`${this.apiUrl}/${id}`, cultura)
      .pipe(
        catchError(error => this.handleError(`atualizar cultura id=${id}`, error))
      );
  }

  /**
   * Remover cultura
   */
  remover(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`)
      .pipe(
        catchError(error => this.handleError(`remover cultura id=${id}`, error))
      );
  }

  /**
   * Handle HTTP errors with toast messages
   */
  private handleError(operation: string, error: HttpErrorResponse): Observable<never> {
    console.error(`CulturaService: ${operation} failed:`, error);
    
    let errorMessage = 'Ocorreu um erro inesperado';
    
    // Check if error has a specific message from API
    if (error.error?.error_description) {
      errorMessage = error.error.error_description;
    } else if (error.error?.message) {
      errorMessage = error.error.message;
    } else if (error.message) {
      errorMessage = error.message;
    } else if (error.status) {
      // Handle specific HTTP status codes
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
          errorMessage = 'Cultura não encontrada';
          break;
        case 409:
          errorMessage = 'Já existe uma cultura com este nome';
          break;
        case 422:
          errorMessage = 'Dados de entrada inválidos';
          break;
        case 500:
          errorMessage = 'Erro interno do servidor';
          break;
        case 502:
        case 503:
        case 504:
          errorMessage = 'Serviço temporariamente indisponível';
          break;
        default:
          errorMessage = `Erro ${error.status}: ${error.statusText}`;
      }
    }

    // Show toast message
    this.messageService.add({
      severity: 'error',
      summary: 'Erro',
      detail: errorMessage,
      life: 5000
    });

    return throwError(() => new Error(errorMessage));
  }
}