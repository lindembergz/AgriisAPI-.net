import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { ReferenceCrudService } from '../../../../shared/services/reference-crud.service';
import { MunicipioDto, CriarMunicipioDto, AtualizarMunicipioDto, UfDto } from '../../../../shared/models/reference.model';

/**
 * Service for managing Municípios with UF relationships and filtering
 */
@Injectable({
  providedIn: 'root'
})
export class MunicipioService extends ReferenceCrudService<MunicipioDto, CriarMunicipioDto, AtualizarMunicipioDto> {
  
  protected readonly entityName = 'Município';
  protected readonly apiEndpoint = 'api/referencias/municipios';

  /**
   * Get Municípios by UF ID
   */
  obterPorUf(ufId: number): Observable<MunicipioDto[]> {
    return this.http.get<MunicipioDto[]>(`${this.baseUrl}/uf/${ufId}`).pipe(
      catchError(error => this.handleError(`obter municípios por UF id=${ufId}`, error))
    );
  }

  /**
   * Get Municípios with UF information included
   */
  obterComUf(): Observable<MunicipioDto[]> {
    return this.http.get<MunicipioDto[]>(`${this.baseUrl}?include=uf`).pipe(
      catchError(error => this.handleError('obter municípios com UF', error))
    );
  }

  /**
   * Get active Municípios by UF ID for dropdowns
   */
  obterAtivosPorUf(ufId: number): Observable<MunicipioDto[]> {
    return this.obterPorUf(ufId).pipe(
      map(municipios => municipios.filter(m => m.ativo)),
      catchError(error => this.handleError(`obter municípios ativos por UF id=${ufId}`, error))
    );
  }

  /**
   * Search Municípios by name with optional UF filtering
   */
  buscarPorNome(nome: string, ufId?: number): Observable<MunicipioDto[]> {
    let params = new HttpParams().set('nome', nome);
    
    if (ufId) {
      params = params.set('ufId', ufId.toString());
    }

    return this.http.get<MunicipioDto[]>(`${this.baseUrl}/buscar`, { params }).pipe(
      catchError(error => this.handleError(`buscar municípios por nome=${nome}`, error))
    );
  }

  /**
   * Get active Municípios by UF ID for dropdowns (optimized)
   */
  obterDropdownPorUf(ufId: number): Observable<{ id: number; nome: string; codigoIbge: string }[]> {
    return this.http.get<{ id: number; nome: string; codigoIbge: string }[]>(`${this.baseUrl}/uf/${ufId}/dropdown`).pipe(
      catchError(error => this.handleError(`obter dropdown municípios por UF id=${ufId}`, error))
    );
  }

  /**
   * Validate IBGE code uniqueness
   */
  validarCodigoIbgeUnico(codigoIbge: string, municipioId?: number): Observable<boolean> {
    let params = new HttpParams();
    
    if (municipioId) {
      params = params.set('idExcluir', municipioId.toString());
    }

    return this.http.get<{ existe: boolean }>(`${this.baseUrl}/existe-codigo-ibge/${codigoIbge}`, { params }).pipe(
      map(response => !response.existe), // Return true if unique (not exists)
      catchError(error => this.handleError('validar código IBGE único', error))
    );
  }

  /**
   * Validate município name uniqueness within UF
   */
  validarNomeUnico(nome: string, ufId: number, municipioId?: number): Observable<boolean> {
    let params = new HttpParams();
    
    if (municipioId) {
      params = params.set('idExcluir', municipioId.toString());
    }

    return this.http.get<{ existe: boolean }>(`${this.baseUrl}/existe-nome/${nome}/uf/${ufId}`, { params }).pipe(
      map(response => !response.existe), // Return true if unique (not exists)
      catchError(error => this.handleError('validar nome município único', error))
    );
  }

  /**
   * Get Municípios with pagination and UF filtering
   */
  obterComPaginacao(page: number = 1, size: number = 20, ufId?: number, search?: string): Observable<{
    items: MunicipioDto[];
    totalItems: number;
    totalPages: number;
    currentPage: number;
  }> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('size', size.toString());
    
    if (ufId) {
      params = params.set('ufId', ufId.toString());
    }
    
    if (search) {
      params = params.set('search', search);
    }

    return this.http.get<{
      items: MunicipioDto[];
      totalItems: number;
      totalPages: number;
      currentPage: number;
    }>(`${this.baseUrl}/paginado`, { params }).pipe(
      catchError(error => this.handleError('obter municípios paginados', error))
    );
  }
}