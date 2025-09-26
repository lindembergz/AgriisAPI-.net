import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { CulturaService } from './cultura.service';
import { CulturaDto, CriarCulturaDto, AtualizarCulturaDto } from '../models';

/**
 * Example usage of CulturaService
 * This demonstrates how to use the service in components
 */
@Injectable({
  providedIn: 'root'
})
export class CulturaServiceExample {
  private culturaService = inject(CulturaService);

  /**
   * Example: Load all culturas
   */
  loadAllCulturas(): Observable<CulturaDto[]> {
    return this.culturaService.obterTodas();
  }

  /**
   * Example: Load only active culturas
   */
  loadActiveCulturas(): Observable<CulturaDto[]> {
    return this.culturaService.obterAtivas();
  }

  /**
   * Example: Load cultura by ID
   */
  loadCulturaById(id: number): Observable<CulturaDto> {
    return this.culturaService.obterPorId(id);
  }

  /**
   * Example: Create new cultura
   */
  createNewCultura(nome: string, descricao?: string): Observable<CulturaDto> {
    const criarDto: CriarCulturaDto = {
      nome,
      descricao
    };
    
    return this.culturaService.criar(criarDto);
  }

  /**
   * Example: Update existing cultura
   */
  updateCultura(id: number, nome: string, descricao?: string, ativo: boolean = true): Observable<CulturaDto> {
    const atualizarDto: AtualizarCulturaDto = {
      nome,
      descricao,
      ativo
    };
    
    return this.culturaService.atualizar(id, atualizarDto);
  }

  /**
   * Example: Delete cultura
   */
  deleteCultura(id: number): Observable<void> {
    return this.culturaService.remover(id);
  }

  /**
   * Example: Complete workflow - Create, Update, Delete
   */
  demonstrateWorkflow(): void {
    // Create
    this.createNewCultura('Soja', 'Cultura de soja para grãos').subscribe({
      next: (cultura) => {
        console.log('Cultura criada:', cultura);
        
        // Update
        this.updateCultura(cultura.id, 'Soja Premium', 'Cultura de soja premium', true).subscribe({
          next: (culturaAtualizada) => {
            console.log('Cultura atualizada:', culturaAtualizada);
            
            // Delete
            this.deleteCultura(culturaAtualizada.id).subscribe({
              next: () => {
                console.log('Cultura removida com sucesso');
              },
              error: (error) => {
                console.error('Erro ao remover cultura:', error);
              }
            });
          },
          error: (error) => {
            console.error('Erro ao atualizar cultura:', error);
          }
        });
      },
      error: (error) => {
        console.error('Erro ao criar cultura:', error);
      }
    });
  }

  /**
   * Example: Load culturas with error handling
   */
  loadCulturasWithErrorHandling(): void {
    this.culturaService.obterTodas().subscribe({
      next: (culturas) => {
        console.log('Culturas carregadas:', culturas);
        // Process culturas data
      },
      error: (error) => {
        console.error('Erro ao carregar culturas:', error);
        // Error is already handled by the service (toast message shown)
        // Additional error handling can be done here if needed
      }
    });
  }

  /**
   * Example: Load active culturas for dropdown
   */
  loadCulturasForDropdown(): Observable<{ label: string; value: number }[]> {
    return new Observable(observer => {
      this.culturaService.obterAtivas().subscribe({
        next: (culturas) => {
          const dropdownOptions = culturas.map(cultura => ({
            label: cultura.nome,
            value: cultura.id
          }));
          observer.next(dropdownOptions);
          observer.complete();
        },
        error: (error) => {
          observer.error(error);
        }
      });
    });
  }
}