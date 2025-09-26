import { BaseEntity, BaseForm } from './base.model';

/**
 * Endereco entity interface
 */
export interface Endereco extends BaseEntity {
  logradouro: string;
  numero: string;
  complemento?: string;
  bairro: string;
  cidade: string;
  uf: string;
  cep: string;
  latitude?: number;
  longitude?: number;
  ufId?: number;
  municipioId?: number;
  ufNome?: string;
  ufCodigo?: string;
  municipioNome?: string;
  municipioCodigoIbge?: string;
}

/**
 * Endereco form interface for reactive forms
 */
export interface EnderecoForm extends BaseForm {
  logradouro: string;
  numero: string;
  complemento?: string;
  bairro: string;
  cidade: string;
  uf: string;
  cep: string;
  latitude?: number;
  longitude?: number;
  ufId?: number;
  municipioId?: number;
  ufNome?: string;
  ufCodigo?: string;
  municipioNome?: string;
  municipioCodigoIbge?: string;
}