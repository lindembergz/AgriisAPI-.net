import { BaseEntity, BaseForm } from './base.model';
import { Cultura, CulturaForm } from './cultura.model';

/**
 * Propriedade entity interface
 */
export interface Propriedade extends BaseEntity {
  nome: string;
  area: number;
  latitude?: number;
  longitude?: number;
  produtorId: number;
  culturas: Cultura[];
}

/**
 * Propriedade form interface for reactive forms
 */
export interface PropriedadeForm extends BaseForm {
  nome: string;
  area: number;
  latitude?: number;
  longitude?: number;
  produtorId?: number;
  culturas: CulturaForm[];
}