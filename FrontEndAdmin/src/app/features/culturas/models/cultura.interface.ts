import { FormControl } from '@angular/forms';

/**
 * Interface para representar uma Cultura completa retornada pela API
 */
export interface CulturaDto {
  id: number;
  nome: string;
  descricao?: string;
  ativo: boolean;
  dataCriacao: Date;
  dataAtualizacao?: Date;
}

/**
 * Interface para criar uma nova Cultura
 */
export interface CriarCulturaDto {
  nome: string;
  descricao?: string;
}

/**
 * Interface para atualizar uma Cultura existente
 */
export interface AtualizarCulturaDto {
  nome: string;
  descricao?: string;
  ativo: boolean;
}

/**
 * Interface para o formulário de Cultura
 */
export interface CulturaForm {
  nome: FormControl<string>;
  descricao: FormControl<string | null>;
  ativo: FormControl<boolean>;
}