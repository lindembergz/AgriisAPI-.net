import { BaseEntity, BaseForm } from './base.model';

/**
 * Usuario entity interface
 */
export interface Usuario extends BaseEntity {
  nome: string;
  email: string;
  senha: string;
  telefone?: string;
}

/**
 * Usuario form interface for reactive forms
 */
export interface UsuarioForm extends BaseForm {
  nome: string;
  email: string;
  senha: string;
  telefone?: string;
}

/**
 * Legacy User interface for backward compatibility
 * @deprecated Use Usuario instead
 */
export interface User {
  nome?: string;
  email: string;
  senha?: string;
  name?: string;
  avatar?: string;
}
