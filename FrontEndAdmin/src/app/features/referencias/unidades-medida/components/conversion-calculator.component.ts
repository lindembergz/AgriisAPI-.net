import { Component, Input, Output, EventEmitter, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { CardModule } from 'primeng/card';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectModule } from 'primeng/select';
import { ButtonModule } from 'primeng/button';
import { DividerModule } from 'primeng/divider';
import { MessageService } from 'primeng/api';
import { UnidadeMedidaService, UnidadeDropdownOption, ConversaoResult } from '../services/unidade-medida.service';

/**
 * Conversion Calculator Component for Units of Measure
 * Allows users to convert between different units of the same type
 */
@Component({
  selector: 'app-conversion-calculator',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DialogModule,
    CardModule,
    InputNumberModule,
    SelectModule,
    ButtonModule,
    DividerModule
  ],
  templateUrl: './conversion-calculator.component.html',
  styleUrls: ['./conversion-calculator.component.scss']
})
export class ConversionCalculatorComponent implements OnInit {
  @Input() visible = false;
  @Input() unidades: UnidadeDropdownOption[] = [];
  @Output() onClose = new EventEmitter<void>();

  private service = inject(UnidadeMedidaService);
  private messageService = inject(MessageService);

  // Conversion form
  conversaoForm = {
    quantidade: 0,
    unidadeOrigemId: null as number | null,
    unidadeDestinoId: null as number | null
  };

  // State
  calculandoConversao = false;
  resultadoConversao: ConversaoResult | null = null;

  ngOnInit(): void {
    this.limparFormulario();
  }

  /**
   * Check if conversion can be calculated
   */
  podeCalcularConversao(): boolean {
    return !!(
      this.conversaoForm.quantidade > 0 &&
      this.conversaoForm.unidadeOrigemId &&
      this.conversaoForm.unidadeDestinoId &&
      this.conversaoForm.unidadeOrigemId !== this.conversaoForm.unidadeDestinoId
    );
  }

  /**
   * Calculate unit conversion
   */
  calcularConversao(): void {
    if (!this.podeCalcularConversao()) {
      return;
    }

    this.calculandoConversao = true;
    this.resultadoConversao = null;

    this.service.converter(
      this.conversaoForm.quantidade,
      this.conversaoForm.unidadeOrigemId!,
      this.conversaoForm.unidadeDestinoId!
    ).subscribe({
      next: (resultado) => {
        this.resultadoConversao = resultado;
        this.calculandoConversao = false;
      },
      error: (error) => {
        console.error('Erro ao calcular conversão:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Erro na Conversão',
          detail: error.error?.errorDescription || 'Erro ao calcular conversão entre unidades'
        });
        this.calculandoConversao = false;
      }
    });
  }

  /**
   * Get unit by ID for display purposes
   */
  getUnidadeById(id: number): UnidadeDropdownOption | undefined {
    return this.unidades.find(u => u.id === id);
  }

  /**
   * Clear conversion form
   */
  limparFormulario(): void {
    this.conversaoForm = {
      quantidade: 0,
      unidadeOrigemId: null,
      unidadeDestinoId: null
    };
    this.resultadoConversao = null;
  }
}