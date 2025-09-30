import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ReferenceCrudService } from '../../../../shared/services/reference-crud.service';
import { UfDto, CriarUfDto, AtualizarUfDto, PaisDto, MunicipioDto } from '../../../../shared/models/reference.model';

/**
 * Service for managing UFs (Unidades Federativas) with País relationships
 */
@Injectable({
  providedIn: 'root'
})
export class UfService extends ReferenceCrudService<UfDto, CriarUfDto, AtualizarUfDto> {
  
  protected readonly entityName = 'UF';
  protected readonly apiEndpoint = 'referencias/ufs';

  /**
   * Get UFs by País ID
   */
  obterPorPais(paisId: number): Observable<UfDto[]> {
    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.get<UfDto[]>(`${this.baseUrl}/pais/${paisId}`),
      `obter UFs por país id=${paisId}`,
      'UF'
    );
  }

  /**
   * Get UFs with País information included
   */
  obterComPais(): Observable<UfDto[]> {
    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.get<UfDto[]>(`${this.baseUrl}?include=pais`),
      'obter UFs com país',
      'UF'
    );
  }

  /**
   * Override obterTodos to include país information by default
   */
  override obterTodos(): Observable<UfDto[]> {
    return this.obterComPais();
  }

  /**
   * Get active UFs by País ID for dropdowns
   */
  obterAtivosPorPais(paisId: number): Observable<UfDto[]> {
    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.get<UfDto[]>(`${this.baseUrl}/pais/${paisId}`),
      `obter UFs ativas por país id=${paisId}`,
      'UF'
    );
  }

  /**
   * Check if UF has Municípios before deletion
   */
  verificarDependenciasMunicipio(ufId: number): Observable<boolean> {
    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.get<{ hasMunicipios: boolean }>(`${this.baseUrl}/${ufId}/tem-municipios`).pipe(
        map(response => response.hasMunicipios)
      ),
      `verificar dependências município UF id=${ufId}`,
      'UF'
    );
  }

  /**
   * Get Municípios count for UF
   */
  obterContagemMunicipios(ufId: number): Observable<number> {
    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.get<{ count: number }>(`${this.baseUrl}/${ufId}/municipios/count`).pipe(
        map(response => response.count)
      ),
      `obter contagem municípios UF id=${ufId}`,
      'UF'
    );
  }

  /**
   * Get Municípios by UF ID
   */
  obterMunicipios(ufId: number): Observable<MunicipioDto[]> {
    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.get<MunicipioDto[]>(`${this.baseUrl}/${ufId}/municipios`),
      `obter municípios UF id=${ufId}`,
      'UF'
    );
  }

  /**
   * Validate UF code uniqueness within País
   */
  validarCodigoUnico(codigo: string, paisId: number, ufId?: number): Observable<boolean> {
    let params = new HttpParams()
      .set('codigo', codigo)
      .set('paisId', paisId.toString());
    
    if (ufId) {
      params = params.set('excludeId', ufId.toString());
    }

    return this.errorHandlingService.wrapWithErrorHandling(
      this.http.get<{ isUnique: boolean }>(`${this.baseUrl}/validar-codigo`, { params }).pipe(
        map(response => response.isUnique)
      ),
      'validar código UF único',
      'UF'
    );
  }
}