import { BaseEntity, BaseForm } from './base.model';
import { UnidadeMedidaDto, EmbalagemDto, AtividadeAgropecuariaDto } from './reference.model';

/**
 * Categoria interfaces (assuming hierarchical structure)
 */
export interface CategoriaDto extends BaseEntity {
  nome: string;
  descricao?: string;
  parentId?: number;
  parent?: CategoriaDto;
  children?: CategoriaDto[];
  ativo: boolean;
}

/**
 * Produto entity interface
 */
export interface ProdutoDto extends BaseEntity {
  codigo: string;
  nome: string;
  descricao?: string;
  preco?: number;
  
  // Reference relationships
  unidadeMedidaId: number;
  unidadeMedida?: UnidadeMedidaDto;
  
  embalagemId?: number;
  embalagem?: EmbalagemDto;
  
  categoriaId: number;
  categoria?: CategoriaDto;
  
  atividadeAgropecuariaId?: number;
  atividadeAgropecuaria?: AtividadeAgropecuariaDto;
  
  ativo: boolean;
}

/**
 * Produto form interface for reactive forms
 */
export interface ProdutoForm extends BaseForm {
  codigo: string;
  nome: string;
  descricao?: string;
  preco?: number;
  unidadeMedidaId: number;
  embalagemId?: number;
  categoriaId: number;
  atividadeAgropecuariaId?: number;
  ativo: boolean;
}

/**
 * Create Produto DTO
 */
export interface CriarProdutoDto {
  codigo: string;
  nome: string;
  descricao?: string;
  preco?: number;
  unidadeMedidaId: number;
  embalagemId?: number;
  categoriaId: number;
  atividadeAgropecuariaId?: number;
}

/**
 * Update Produto DTO
 */
export interface AtualizarProdutoDto {
  nome: string;
  descricao?: string;
  preco?: number;
  unidadeMedidaId: number;
  embalagemId?: number;
  categoriaId: number;
  atividadeAgropecuariaId?: number;
  ativo: boolean;
}

/**
 * Produto list query parameters
 */
export interface ProdutoQueryParams {
  termo?: string;
  categoriaId?: number;
  unidadeMedidaId?: number;
  atividadeAgropecuariaId?: number;
  ativo?: boolean;
  pagina?: number;
  tamanhoPagina?: number;
  ordenacao?: string;
}

/**
 * Produto list response
 */
export interface ProdutoListResponse {
  items: ProdutoDto[];
  total: number;
  pagina: number;
  tamanhoPagina: number;
}