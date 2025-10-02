import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { ReferenceCrudService } from '../../../../shared/services/reference-crud.service';
import { 
  AtividadeAgropecuariaDto, 
  CriarAtividadeAgropecuariaDto, 
  AtualizarAtividadeAgropecuariaDto,
  TipoAtividadeAgropecuaria 
} from '../../../../shared/models/reference.model';

/**
 * Service for managing Atividades Agropecuárias (Agricultural Activities)
 * Extends ReferenceCrudService for standard CRUD operations with caching and error handling
 */
@Injectable({
  providedIn: 'root'
})
export class AtividadeAgropecuariaService extends ReferenceCrudService<
  AtividadeAgropecuariaDto,
  CriarAtividadeAgropecuariaDto,
  AtualizarAtividadeAgropecuariaDto
> {
  
  protected readonly entityName = 'Atividade Agropecuária';
  protected readonly apiEndpoint = 'atividades-agropecuarias';

  /**
   * Get atividades by tipo (type filtering)
   */
  obterPorTipo(tipo: TipoAtividadeAgropecuaria): Observable<AtividadeAgropecuariaDto[]> {
    return this.http.get<AtividadeAgropecuariaDto[]>(`${this.baseUrl}/tipo/${tipo}`)
      .pipe(
        catchError(error => {
          console.error('Erro ao obter atividades por tipo:', error);
          return of([]);
        })
      );
  }

  /**
   * Get atividades grouped by tipo for display
   */
  obterAgrupadasPorTipo(): Observable<{ [key in TipoAtividadeAgropecuaria]: AtividadeAgropecuariaDto[] }> {
    return this.obterAtivos().pipe(
      map(atividades => {
        const grouped = {} as { [key in TipoAtividadeAgropecuaria]: AtividadeAgropecuariaDto[] };
        
        // Initialize groups
        grouped[TipoAtividadeAgropecuaria.Agricultura] = [];
        grouped[TipoAtividadeAgropecuaria.Pecuaria] = [];
        grouped[TipoAtividadeAgropecuaria.Mista] = [];
        
        // Group activities by type
        atividades.forEach(atividade => {
          if (grouped[atividade.tipo]) {
            grouped[atividade.tipo].push(atividade);
          }
        });
        
        return grouped;
      })
    );
  }

  /**
   * Get atividades by codigo pattern (for search/autocomplete)
   */
  buscarPorCodigo(codigo: string): Observable<AtividadeAgropecuariaDto[]> {
    return this.buscar({ termo: codigo }).pipe(
      map(response => response.items)
    );
  }

  /**
   * Get atividades by descricao pattern (for search/autocomplete)
   */
  buscarPorDescricao(descricao: string, tipo?: TipoAtividadeAgropecuaria): Observable<AtividadeAgropecuariaDto[]> {
    const params: any = { termo: descricao };
    if (tipo !== undefined) {
      params.tipo = tipo;
    }
    
    return this.buscar(params).pipe(
      map(response => response.items)
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
        catchError(error => {
          console.error('Erro ao verificar código único:', error);
          return of(false);
        })
      );
  }

  /**
   * Check if descricao is unique (excluding current item if editing)
   */
  verificarDescricaoUnica(descricao: string, idExcluir?: number): Observable<boolean> {
    const params: any = { descricao };
    if (idExcluir) {
      params.idExcluir = idExcluir;
    }
    
    return this.http.get<{ isUnique: boolean }>(`${this.baseUrl}/verificar-descricao`, { params })
      .pipe(
        map(response => response.isUnique),
        catchError(error => {
          console.error('Erro ao verificar descrição única:', error);
          return of(false);
        })
      );
  }

  /**
   * Get atividades for dropdown usage (only active, minimal data)
   */
  obterParaDropdown(tipo?: TipoAtividadeAgropecuaria): Observable<{ id: number; codigo: string; descricao: string; tipo: TipoAtividadeAgropecuaria }[]> {
    const params: any = {};
    if (tipo !== undefined) {
      params.tipo = tipo.toString();
    }
    
    return this.http.get<{ id: number; codigo: string; descricao: string; tipo: TipoAtividadeAgropecuaria }[]>(`${this.baseUrl}/dropdown`, { params })
      .pipe(
        catchError(error => {
          console.error('Erro ao obter atividades para dropdown:', error);
          return of([]);
        })
      );
  }

  /**
   * Get tipo description for display
   */
  getTipoDescricao(tipo: TipoAtividadeAgropecuaria): string {
    switch (tipo) {
      case TipoAtividadeAgropecuaria.Agricultura:
        return 'Agricultura';
      case TipoAtividadeAgropecuaria.Pecuaria:
        return 'Pecuária';
      case TipoAtividadeAgropecuaria.Mista:
        return 'Mista';
      default:
        return 'Desconhecido';
    }
  }

  /**
   * Get all tipo options for dropdown
   */
  getTipoOptions(): { value: TipoAtividadeAgropecuaria; label: string }[] {
    return [
      { value: TipoAtividadeAgropecuaria.Agricultura, label: 'Agricultura' },
      { value: TipoAtividadeAgropecuaria.Pecuaria, label: 'Pecuária' },
      { value: TipoAtividadeAgropecuaria.Mista, label: 'Mista' }
    ];
  }

  /**
   * Configure cache for atividades agropecuárias
   */
  constructor() {
    super();
    
    // Atividades don't change frequently, so we can cache for a moderate time
    this.configurarCache({
      enabled: true,
      ttlMinutes: 15, // 15 minutes cache
      maxSize: 100
    });
  }
}