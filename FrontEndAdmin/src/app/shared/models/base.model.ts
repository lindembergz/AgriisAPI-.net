/**
 * Base entity interface with common audit fields
 */
export interface BaseEntity {
  id: number;
  dataCriacao: Date;
  dataAtualizacao?: Date;
  ativo: boolean;
}

/**
 * Base form interface for reactive forms
 */
export interface BaseForm {
  id?: number;
  ativo?: boolean;
}