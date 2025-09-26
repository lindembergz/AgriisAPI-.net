import { FormControl } from '@angular/forms';

/**
 * Interface para representar uma Safra completa retornada pela API
 */
export interface SafraDto {
  id: number;
  plantioInicial: Date;
  plantioFinal: Date;
  plantioNome: string;
  descricao: string;
  anoColheita: number;
  safraFormatada: string;
  atual: boolean;
  dataCriacao: Date;
  dataAtualizacao?: Date;
}

/**
 * Interface para criar uma nova Safra
 */
export interface CriarSafraDto {
  plantioInicial: Date;
  plantioFinal: Date;
  plantioNome: string;
  descricao: string;
}

/**
 * Interface para atualizar uma Safra existente
 */
export interface AtualizarSafraDto {
  plantioInicial: Date;
  plantioFinal: Date;
  plantioNome: string;
  descricao: string;
}

/**
 * Interface para representar a Safra atual retornada pela API
 */
export interface SafraAtualDto {
  id: number;
  descricao: string;
  safra: string;
}

/**
 * Interface para o formulário de Safra
 */
export interface SafraForm {
  plantioInicial: FormControl<Date>;
  plantioFinal: FormControl<Date>;
  plantioNome: FormControl<string>;
  descricao: FormControl<string>;
}