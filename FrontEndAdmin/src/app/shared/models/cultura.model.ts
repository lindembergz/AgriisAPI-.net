import { BaseEntity, BaseForm } from './base.model';

/**
 * Cultura types enum
 */
export enum TipoCultura {
  SOJA = 'SOJA',
  MILHO = 'MILHO',
  ALGODAO = 'ALGODAO',
  OUTROS = 'OUTROS'
}

/**
 * Cultura entity interface
 */
export interface Cultura extends BaseEntity {
  tipo: TipoCultura;
  anoSafra: number;
  areaCultivada: number;
  propriedadeId: number;
}

/**
 * Cultura form interface for reactive forms
 */
export interface CulturaForm extends BaseForm {
  tipo: TipoCultura;
  anoSafra: number;
  areaCultivada: number;
  propriedadeId?: number;
}