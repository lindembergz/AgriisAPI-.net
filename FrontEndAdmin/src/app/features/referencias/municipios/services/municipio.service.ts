import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
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
  protected readonly apiEndpoint = 'enderecos/municipios'; // Usando controller temporário de compatibilidade

  /**
   * Get Municípios by UF ID
   */
  obterPorUf(ufId: number): Observable<MunicipioDto[]> {
    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.get<MunicipioDto[]>(`${this.baseUrl}/uf/${ufId}`),
      `obter municípios por UF id=${ufId}`,
      'Município'
    );
  }

  /**
   * Get Municípios with UF information included
   */
  obterComUf(): Observable<MunicipioDto[]> {
    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.get<MunicipioDto[]>(`${this.baseUrl}?include=uf`),
      'obter municípios com UF',
      'Município'
    );
  }

  /**
   * Get active Municípios by UF ID for dropdowns
   */
  obterAtivosPorUf(ufId: number): Observable<MunicipioDto[]> {
    return this.errorHandlingService.wrapWithErrorHandling(
      this.obterPorUf(ufId).pipe(
        map(municipios => municipios.filter(m => m.ativo))
      ),
      `obter municípios ativos por UF id=${ufId}`,
      'Município'
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

    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.get<MunicipioDto[]>(`${this.baseUrl}/buscar`, { params }),
      `buscar municípios por nome=${nome}`,
      'Município'
    );
  }

  /**
   * Get active Municípios by UF ID for dropdowns (optimized)
   */
  obterDropdownPorUf(ufId: number): Observable<{ id: number; nome: string; codigoIbge: number }[]> {
    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.get<{ id: number; nome: string; codigoIbge: number }[]>(`${this.baseUrl}/uf/${ufId}/dropdown`),
      `obter dropdown municípios por UF id=${ufId}`,
      'Município'
    );
  }

  /**
   * Validate IBGE code uniqueness
   */
  validarCodigoIbgeUnico(codigoIbge: number, municipioId?: number): Observable<boolean> {
    let params = new HttpParams();
    
    if (municipioId) {
      params = params.set('idExcluir', municipioId.toString());
    }

    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.get<{ existe: boolean }>(`${this.baseUrl}/existe-codigo-ibge/${codigoIbge}`, { params }).pipe(
        map(response => !response.existe) // Return true if unique (not exists)
      ),
      'validar código IBGE único',
      'Município'
    );
  }

  /**
   * Validate município name uniqueness within UF
   */
  validarNomeUnico(nome: string, ufId: number, municipioId?: number): Observable<boolean> {

      let params = new HttpParams().set('nome', nome)
      
      if (municipioId) {
        params = params.set('idExcluir', municipioId.toString());
      }
      const url = `${this.baseUrl}/buscar?nome=${nome}`;

      return this.errorHandlingService.wrapWithErrorHandling(
        this.http.get<any[]>(url, { params }).pipe(
          map(municipios => municipios.length === 0) // Retorna true se o nome não existe (é único)
        ),
        'validar nome município único',
        'Município'
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

    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.get<{
        items: MunicipioDto[];
        totalItems: number;
        totalPages: number;
        currentPage: number;
      }>(`${this.baseUrl}/paginado`, { params }),
      'obter municípios paginados',
      'Município'
    );
  }
}