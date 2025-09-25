import { BaseEntity, BaseForm } from './base.model';
import { Endereco, EnderecoForm } from './endereco.model';
import { PontoDistribuicao, PontoDistribuicaoForm } from './ponto-distribuicao.model';
import { Usuario, UsuarioForm } from './user.model';
import { TipoCliente } from './produtor.model';

/**
 * Fornecedor entity interface
 */
export interface Fornecedor extends BaseEntity {
  codigo: string;
  nome: string;
  cpfCnpj: string;
  tipoCliente: TipoCliente;
  inscricaoEstadual?: string;
  telefone?: string;
  email?: string;
  endereco?: Endereco;
  pontosDistribuicao: PontoDistribuicao[];
  usuarioMaster?: Usuario;
}

/**
 * Fornecedor form interface for reactive forms
 */
export interface FornecedorForm extends BaseForm {
  codigo: string;
  nome: string;
  cpfCnpj: string;
  tipoCliente: TipoCliente;
  inscricaoEstadual?: string;
  telefone?: string;
  email?: string;
  endereco?: EnderecoForm;
  pontosDistribuicao: PontoDistribuicaoForm[];
  usuarioMaster?: UsuarioForm;
}