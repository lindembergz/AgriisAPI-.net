import { FormArray, FormControl, FormGroup } from '@angular/forms';
import { TipoCliente, TipoCultura } from './index';

/**
 * Reactive form interfaces for type safety
 */

/**
 * Login form controls
 */
export interface LoginFormControls {
  username: FormControl<string>;
  password: FormControl<string>;
  rememberMe: FormControl<boolean>;
}

/**
 * Endereco form controls
 */
export interface EnderecoFormControls {
  logradouro: FormControl<string>;
  numero: FormControl<string>;
  complemento: FormControl<string>;
  bairro: FormControl<string>;
  cidade: FormControl<string>;
  uf: FormControl<string>;
  cep: FormControl<string>;
  latitude: FormControl<number | null>;
  longitude: FormControl<number | null>;
}

/**
 * Usuario form controls
 */
export interface UsuarioFormControls {
  nome: FormControl<string | null>;
  email: FormControl<string | null>;
  senha: FormControl<string | null>;
  telefone: FormControl<string | null>;
}

/**
 * Cultura form controls
 */
export interface CulturaFormControls {
  tipo: FormControl<TipoCultura>;
  anoSafra: FormControl<number>;
  areaCultivada: FormControl<number>;
}

/**
 * Propriedade form controls
 */
export interface PropriedadeFormControls {
  nome: FormControl<string>;
  area: FormControl<number>;
  latitude: FormControl<number | null>;
  longitude: FormControl<number | null>;
  culturas: FormArray<FormGroup<CulturaFormControls>>;
}

/**
 * Produtor dados gerais form controls
 */
export interface ProdutorDadosGeraisFormControls {
  nome: FormControl<string>;
  cpfCnpj: FormControl<string>;
  tipoCliente: FormControl<TipoCliente>;
  telefone1: FormControl<string>;
  telefone2: FormControl<string>;
  telefone3: FormControl<string>;
  email: FormControl<string>;
}

/**
 * Produtor complete form controls
 */
export interface ProdutorFormControls {
  dadosGerais: FormGroup<ProdutorDadosGeraisFormControls>;
  enderecos: FormArray<FormGroup<EnderecoFormControls>>;
  propriedades: FormArray<FormGroup<PropriedadeFormControls>>;
  usuarioMaster: FormGroup<UsuarioFormControls>;
}

/**
 * Ponto Distribuicao form controls
 */
export interface PontoDistribuicaoFormControls {
  nome: FormControl<string>;
  endereco: FormGroup<EnderecoFormControls>;
  latitude: FormControl<number | null>;
  longitude: FormControl<number | null>;
}

/**
 * Fornecedor dados gerais form controls
 */
export interface FornecedorDadosGeraisFormControls {
  nome: FormControl<string>;
  cpfCnpj: FormControl<string>;
  tipoCliente: FormControl<TipoCliente>;
  telefone: FormControl<string>;
  email: FormControl<string>;
}

/**
 * Fornecedor complete form controls
 */
export interface FornecedorFormControls {
  dadosGerais: FormGroup<FornecedorDadosGeraisFormControls>;
  inscricaoEstadual: FormControl<string>;
  endereco: FormGroup<EnderecoFormControls>;
  pontosDistribuicao: FormArray<FormGroup<PontoDistribuicaoFormControls>>;
  usuarioMaster: FormGroup<UsuarioFormControls>;
}