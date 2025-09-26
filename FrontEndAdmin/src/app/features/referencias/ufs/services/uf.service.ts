import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
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
  protected readonly apiEndpoint = 'api/referencias/ufs';

  /**
   * Get UFs by País ID
   */
  obterPorPais(paisId: number): Observable<UfDto[]> {
    return this.http.get<UfDto[]>(`${this.baseUrl}/pais/${paisId}`).pipe(
      catchError(error => this.handleError(`obter UFs por país id=${paisId}`, error))
    );
  }

  /**
   * Get UFs with País information included
   */
  obterComPais(): Observable<UfDto[]> {
    return this.http.get<UfDto[]>(`${this.baseUrl}?include=pais`).pipe(
      catchError(error => this.handleError('obter UFs com país', error))
    );
  }

  /**
   * Get active UFs by País ID for dropdowns
   */
  obterAtivosPorPais(paisId: number): Observable<UfDto[]> {
    return this.http.get<UfDto[]>(`${this.baseUrl}/ativos/pais/${paisId}`).pipe(
      catchError(error => this.handleError(`obter UFs ativas por país id=${paisId}`, error))
    );
  }

  /**
   * Check if UF has Municípios before deletion
   */
  verificarDependenciasMunicipio(ufId: number): Observable<boolean> {
    return this.http.get<{ hasMunicipios: boolean }>(`${this.baseUrl}/${ufId}/tem-municipios`).pipe(
      map(response => response.hasMunicipios),
      catchError(error => this.handleError(`verificar dependências município UF id=${ufId}`, error))
    );
  }

  /**
   * Get Municípios count for UF
   */
  obterContagemMunicipios(ufId: number): Observable<number> {
    return this.http.get<{ count: number }>(`${this.baseUrl}/${ufId}/municipios/count`).pipe(
      map(response => response.count),
      catchError(error => this.handleError(`obter contagem municípios UF id=${ufId}`, error))
    );
  }

  /**
   * Get Municípios by UF ID
   */
  obterMunicipios(ufId: number): Observable<MunicipioDto[]> {
    return this.http.get<MunicipioDto[]>(`${this.baseUrl}/${ufId}/municipios`).pipe(
      catchError(error => this.handleError(`obter municípios UF id=${ufId}`, error))
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

    return this.http.get<{ isUnique: boolean }>(`${this.baseUrl}/validar-codigo`, { params }).pipe(
      map(response => response.isUnique),
      catchError(error => this.handleError('validar código UF único', error))
    );
  }
}