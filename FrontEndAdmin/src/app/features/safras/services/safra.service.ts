import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { MessageService } from 'primeng/api';
import { SafraDto, CriarSafraDto, AtualizarSafraDto, SafraAtualDto } from '../models';
import { environment } from '../../../../environments/environment';

/**
 * Service for Safra API communication
 * Implements CRUD operations with proper TypeScript typing and error handling
 */
@Injectable({
  providedIn: 'root'
})
export class SafraService {
  private http = inject(HttpClient);
  private messageService = inject(MessageService);
  private readonly apiUrl = `${environment.apiUrl}/safras`;

  /**
   * Obter todas as safras
   */
  obterTodas(): Observable<SafraDto[]> {
    return this.http.get<SafraDto[]>(this.apiUrl)
      .pipe(
        catchError(error => this.handleError('obter todas as safras', error))
      );
  }

  /**
   * Obter safra atual
   */
  obterAtual(): Observable<SafraAtualDto> {
    return this.http.get<SafraAtualDto>(`${this.apiUrl}/atual`)
      .pipe(
        catchError(error => this.handleError('obter safra atual', error))
      );
  }

  /**
   * Obter safra por ID
   */
  obterPorId(id: number): Observable<SafraDto> {
    return this.http.get<SafraDto>(`${this.apiUrl}/${id}`)
      .pipe(
        catchError(error => this.handleError(`obter safra id=${id}`, error))
      );
  }

  /**
   * Obter safras por ano de colheita
   */
  obterPorAnoColheita(ano: number): Observable<SafraDto[]> {
    return this.http.get<SafraDto[]>(`${this.apiUrl}/ano-colheita/${ano}`)
      .pipe(
        catchError(error => this.handleError(`obter safras do ano ${ano}`, error))
      );
  }

  /**
   * Criar nova safra
   */
  criar(safra: CriarSafraDto): Observable<SafraDto> {
    return this.http.post<SafraDto>(this.apiUrl, safra)
      .pipe(
        catchError(error => this.handleError('criar safra', error))
      );
  }

  /**
   * Atualizar safra existente
   */
  atualizar(id: number, safra: AtualizarSafraDto): Observable<SafraDto> {
    return this.http.put<SafraDto>(`${this.apiUrl}/${id}`, safra)
      .pipe(
        catchError(error => this.handleError(`atualizar safra id=${id}`, error))
      );
  }

  /**
   * Remover safra
   */
  remover(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`)
      .pipe(
        catchError(error => this.handleError(`remover safra id=${id}`, error))
      );
  }

  /**
   * Handle HTTP errors with toast messages
   */
  private handleError(operation: string, error: HttpErrorResponse): Observable<never> {
    console.error(`SafraService: ${operation} failed:`, error);
    
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
          errorMessage = 'Safra não encontrada';
          break;
        case 409:
          errorMessage = 'Já existe uma safra com estas datas';
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