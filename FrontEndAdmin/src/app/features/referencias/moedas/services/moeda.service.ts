import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { ReferenceCrudService } from '../../../../shared/services/reference-crud.service';
import { MoedaDto, CriarMoedaDto, AtualizarMoedaDto } from '../../../../shared/models/reference.model';

/**
 * Service for managing Moedas (Currencies)
 * Extends ReferenceCrudService for standard CRUD operations with caching and error handling
 */
@Injectable({
  providedIn: 'root'
})
export class MoedaService extends ReferenceCrudService<
  MoedaDto,
  CriarMoedaDto,
  AtualizarMoedaDto
> {
  
  protected readonly entityName = 'Moeda';
  protected readonly apiEndpoint = 'api/referencias/moedas';

  /**
   * Get moedas by codigo pattern (for search/autocomplete)
   */
  buscarPorCodigo(codigo: string): Observable<MoedaDto[]> {
    return this.buscar({ termo: codigo }).pipe(
      map((response: any) => response.items || [])
    );
  }

  /**
   * Check if codigo is unique (excluding current item if editing)
   */
  verificarCodigoUnico(codigo: string, idExcluir?: number): Observable<boolean> {
    const params: any = { codigo };
    if (idExcluir) {
      params.idExcluir = idExcluir;
    }
    
    return this.http.get<{ isUnique: boolean }>(`${this.baseUrl}/verificar-codigo`, { params })
      .pipe(
        map(response => response.isUnique),
        catchError(() => of(false))
      );
  }

  /**
   * Check if nome is unique (excluding current item if editing)
   */
  verificarNomeUnico(nome: string, idExcluir?: number): Observable<boolean> {
    const params: any = { nome };
    if (idExcluir) {
      params.idExcluir = idExcluir;
    }
    
    return this.http.get<{ isUnique: boolean }>(`${this.baseUrl}/verificar-nome`, { params })
      .pipe(
        map(response => response.isUnique),
        catchError(() => of(false))
      );
  }

  /**
   * Get moedas for dropdown usage (only active, minimal data)
   */
  obterParaDropdown(): Observable<{ id: number; codigo: string; nome: string; simbolo: string }[]> {
    return this.http.get<{ id: number; codigo: string; nome: string; simbolo: string }[]>(`${this.baseUrl}/dropdown`)
      .pipe(
        // shareReplay(1),
        // catchError(error => this.handleError('obter moedas para dropdown', error))
      );
  }

  /**
   * Configure cache for moedas (they change infrequently)
   */
  constructor() {
    super();
    
    // Moedas don't change frequently, so we can cache for longer
    this.configurarCache({
      enabled: true,
      ttlMinutes: 30, // 30 minutes cache
      maxSize: 50
    });
  }
}