import { BaseEntity, BaseForm } from './base.model';
import { Endereco, EnderecoForm } from './endereco.model';
import { Propriedade, PropriedadeForm } from './propriedade.model';
import { Usuario, UsuarioForm } from './user.model';

/**
 * Tipo Cliente enum
 */
export enum TipoCliente {
  PF = 'PF',
  PJ = 'PJ'
}

/**
 * Produtor entity interface
 */
export interface Produtor extends BaseEntity {
  codigo: string;
  nome: string;
  cpfCnpj: string;
  tipoCliente: TipoCliente;
  telefone1?: string;
  telefone2?: string;
  telefone3?: string;
  email?: string;
  enderecos: Endereco[];
  propriedades: Propriedade[];
  usuarioMaster?: Usuario;
}

/**
 * Produtor form interface for reactive forms
 */
export interface ProdutorForm extends BaseForm {
  codigo: string;
  nome: string;
  cpfCnpj: string;
  tipoCliente: TipoCliente;
  telefone1?: string;
  telefone2?: string;
  telefone3?: string;
  email?: string;
  enderecos: EnderecoForm[];
  propriedades: PropriedadeForm[];
  usuarioMaster?: UsuarioForm;
}