import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

/**
 * Service for API-based validations
 * Uses backend endpoints to validate data before submission
 */
@Injectable({
  providedIn: 'root'
})
export class ApiValidationService {
  private http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  /**
   * Validate if produto codigo is unique
   */
  validateProdutoCodigoUnico(codigo: string, idExcluir?: number): Observable<boolean> {
    let params = new HttpParams().set('codigo', codigo);
    if (idExcluir) {
      params = params.set('idExcluir', idExcluir.toString());
    }

    return this.http.get<{ isUnique: boolean }>(`${this.baseUrl}/produtos/validar-codigo`, { params })
      .pipe(
        map(response => response.isUnique),
        catchError(error => {
          console.error('Erro ao validar código do produto:', error);
          return of(true); // Assume válido em caso de erro para não bloquear o usuário
        })
      );
  }

  /**
   * Validate if produto nome is unique
   */
  validateProdutoNomeUnico(nome: string, idExcluir?: number): Observable<boolean> {
    let params = new HttpParams().set('nome', nome);
    if (idExcluir) {
      params = params.set('idExcluir', idExcluir.toString());
    }

    return this.http.get<{ isUnique: boolean }>(`${this.baseUrl}/produtos/validar-nome`, { params })
      .pipe(
        map(response => response.isUnique),
        catchError(error => {
          console.error('Erro ao validar nome do produto:', error);
          return of(true);
        })
      );
  }

  /**
   * Validate produto references (unidade medida, embalagem, etc.)
   */
  validateProdutoReferencias(dto: any): Observable<{ isValid: boolean; errors: string[] }> {
    return this.http.post<{ isValid: boolean; errors: string[] }>(`${this.baseUrl}/produtos/validar-referencias`, dto)
      .pipe(
        catchError(error => {
          console.error('Erro ao validar referências do produto:', error);
          return of({ isValid: false, errors: ['Erro ao validar referências'] });
        })
      );
  }

  /**
   * Validate if fornecedor CNPJ is available
   */
  validateFornecedorCnpjDisponivel(cnpj: string, fornecedorIdExcluir?: number): Observable<boolean> {
    let params = new HttpParams();
    if (fornecedorIdExcluir) {
      params = params.set('fornecedorIdExcluir', fornecedorIdExcluir.toString());
    }

    return this.http.get<{ disponivel: boolean }>(`${this.baseUrl}/fornecedores/cnpj/${cnpj}/disponivel`, { params })
      .pipe(
        map(response => response.disponivel),
        catchError(error => {
          console.error('Erro ao validar CNPJ do fornecedor:', error);
          return of(true);
        })
      );
  }

  /**
   * Validate if produtor CPF is available
   */
  validateProdutorCpfDisponivel(cpf: string, produtorIdExcluir?: number): Observable<boolean> {
    let params = new HttpParams();
    if (produtorIdExcluir) {
      params = params.set('produtorIdExcluir', produtorIdExcluir.toString());
    }

    return this.http.get<{ disponivel: boolean }>(`${this.baseUrl}/produtores/cpf/${cpf}/disponivel`, { params })
      .pipe(
        map(response => response.disponivel),
        catchError(error => {
          console.error('Erro ao validar CPF do produtor:', error);
          return of(true);
        })
      );
  }

  /**
   * Validate if produtor CNPJ is available
   */
  validateProdutorCnpjDisponivel(cnpj: string, produtorIdExcluir?: number): Observable<boolean> {
    let params = new HttpParams();
    if (produtorIdExcluir) {
      params = params.set('produtorIdExcluir', produtorIdExcluir.toString());
    }

    return this.http.get<{ disponivel: boolean }>(`${this.baseUrl}/produtores/cnpj/${cnpj}/disponivel`, { params })
      .pipe(
        map(response => response.disponivel),
        catchError(error => {
          console.error('Erro ao validar CNPJ do produtor:', error);
          return of(true);
        })
      );
  }

  /**
   * Validate geographic relationship (UF-Município)
   */
  validateGeographicRelationship(ufId: number, municipioId: number): Observable<boolean> {
    return this.http.get<any>(`${this.baseUrl}/referencias/municipios/${municipioId}`)
      .pipe(
        map(municipio => municipio.ufId === ufId),
        catchError(error => {
          console.error('Erro ao validar relacionamento geográfico:', error);
          return of(false);
        })
      );
  }

  /**
   * Validate if reference entity exists and is active
   */
  validateReferenceEntity(entityType: string, id: number): Observable<boolean> {
    return this.http.get<any>(`${this.baseUrl}/referencias/${entityType}/${id}`)
      .pipe(
        map(entity => entity && entity.ativo === true),
        catchError(error => {
          console.error(`Erro ao validar entidade de referência ${entityType}:`, error);
          return of(false);
        })
      );
  }

  /**
   * Batch validate multiple reference entities
   */
  validateMultipleReferences(validations: { entityType: string; id: number }[]): Observable<{ [key: string]: boolean }> {
    const requests = validations.map(validation => 
      this.validateReferenceEntity(validation.entityType, validation.id).pipe(
        map(isValid => ({ [`${validation.entityType}_${validation.id}`]: isValid }))
      )
    );

    return new Observable(observer => {
      Promise.all(requests.map(req => req.toPromise())).then(results => {
        const combined = results.reduce((acc, result) => ({ ...acc, ...result }), {});
        observer.next(combined);
        observer.complete();
      }).catch(error => {
        console.error('Erro na validação em lote:', error);
        observer.error(error);
      });
    });
  }
}