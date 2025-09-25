import { BaseEntity, BaseForm } from './base.model';
import { Endereco, EnderecoForm } from './endereco.model';

/**
 * PontoDistribuicao entity interface
 */
export interface PontoDistribuicao extends BaseEntity {
  nome: string;
  endereco: Endereco;
  latitude?: number;
  longitude?: number;
  fornecedorId: number;
}

/**
 * PontoDistribuicao form interface for reactive forms
 */
export interface PontoDistribuicaoForm extends BaseForm {
  nome: string;
  endereco: EnderecoForm;
  latitude?: number;
  longitude?: number;
  fornecedorId?: number;
}