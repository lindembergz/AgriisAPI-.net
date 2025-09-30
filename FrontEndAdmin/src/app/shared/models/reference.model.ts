import { BaseEntity } from './base.model';

/**
 * Base reference entity interface
 */
export interface BaseReferenceEntity extends BaseEntity {
  nome: string;
  ativo: boolean;
}

/**
 * Moeda interfaces
 */
export interface MoedaDto extends BaseReferenceEntity {
  codigo: string;
  simbolo: string;
}

export interface CriarMoedaDto {
  codigo: string;
  nome: string;
  simbolo: string;
}

export interface AtualizarMoedaDto {
  nome: string;
  simbolo: string;
  ativo: boolean;
}

/**
 * País interfaces
 */
export interface PaisDto extends BaseReferenceEntity {
  codigo: string;
}

export interface CriarPaisDto {
  codigo: string;
  nome: string;
}

export interface AtualizarPaisDto {
  nome: string;
  ativo: boolean;
}

/**
 * UF interfaces
 */
export interface UfDto extends BaseReferenceEntity {
  codigo: string;
  paisId: number;
  pais?: PaisDto;
}

export interface CriarUfDto {
  codigo: string;
  nome: string;
  paisId: number;
}

export interface AtualizarUfDto {
  nome: string;
  ativo: boolean;
}

/**
 * Município interfaces
 */
export interface MunicipioDto extends BaseReferenceEntity {
  codigoIbge: string;
  ufId: number;
  ufNome?: string;
  ufCodigo?: string;
  uf?: UfDto;
}

export interface CriarMunicipioDto {
  nome: string;
  codigoIbge: number;
  ufId: number;
}

export interface AtualizarMunicipioDto {
  nome: string;
  ativo: boolean;
}

/**
 * Atividade Agropecuária interfaces
 */
export enum TipoAtividadeAgropecuaria {
  Agricultura = 1,
  Pecuaria = 2,
  Mista = 3
}

export interface AtividadeAgropecuariaDto extends BaseEntity {
  codigo: string;
  descricao: string;
  tipo: TipoAtividadeAgropecuaria;
  tipoDescricao: string;
  rowVersion: Uint8Array;
}

export interface CriarAtividadeAgropecuariaDto {
  codigo: string;
  descricao: string;
  tipo: TipoAtividadeAgropecuaria;
}

export interface AtualizarAtividadeAgropecuariaDto {
  descricao: string;
  tipo: TipoAtividadeAgropecuaria;
  ativo: boolean;
}

/**
 * Unidade de Medida interfaces
 */
export enum TipoUnidadeMedida {
  Peso = 1,
  Volume = 2,
  Area = 3,
  Unidade = 4
}

export interface UnidadeMedidaDto extends BaseReferenceEntity {
  simbolo: string;
  tipo: TipoUnidadeMedida;
  fatorConversao?: number;
}

export interface CriarUnidadeMedidaDto {
  simbolo: string;
  nome: string;
  tipo: TipoUnidadeMedida;
  fatorConversao?: number;
}

export interface AtualizarUnidadeMedidaDto {
  nome: string;
  tipo: TipoUnidadeMedida;
  fatorConversao?: number;
  ativo: boolean;
}

/**
 * Embalagem interfaces
 */
export interface EmbalagemDto extends BaseReferenceEntity {
  descricao?: string;
  unidadeMedidaId: number;
  unidadeMedida?: UnidadeMedidaDto;
}

export interface CriarEmbalagemDto {
  nome: string;
  descricao?: string;
  unidadeMedidaId: number;
}

export interface AtualizarEmbalagemDto {
  nome: string;
  descricao?: string;
  unidadeMedidaId: number;
  ativo: boolean;
}

/**
 * Geographic selector data interfaces
 */
export interface GeographicData {
  pais?: PaisDto;
  uf?: UfDto;
  municipio?: MunicipioDto;
}

/**
 * Cascading dropdown options
 */
export interface CascadingDropdownOption {
  id: number;
  nome: string;
  codigo?: string;
}

/**
 * Categoria interfaces
 */
export enum CategoriaProduto {
  Sementes = 1,
  Fertilizantes = 2,
  Defensivos = 3,
  Inoculantes = 4,
  Adjuvantes = 5,
  Micronutrientes = 6,
  Outros = 99
}

export interface CategoriaDto extends BaseReferenceEntity {
  descricao?: string;
  tipo: CategoriaProduto;
  categoriaPaiId?: number;
  categoriaPaiNome?: string;
  ordem: number;
  subCategorias: CategoriaDto[];
  quantidadeProdutos: number;
}

export interface CriarCategoriaDto {
  nome: string;
  descricao?: string;
  tipo: CategoriaProduto;
  categoriaPaiId?: number;
  ordem?: number;
}

export interface AtualizarCategoriaDto {
  nome: string;
  descricao?: string;
  tipo: CategoriaProduto;
  categoriaPaiId?: number;
  ordem: number;
  ativo: boolean;
}

/**
 * Reference entity type union
 */
export type ReferenceEntityDto = 
  | MoedaDto 
  | PaisDto 
  | UfDto 
  | MunicipioDto 
  | AtividadeAgropecuariaDto 
  | UnidadeMedidaDto 
  | EmbalagemDto
  | CategoriaDto;

/**
 * Reference entity create DTO union
 */
export type ReferenceCreateDto = 
  | CriarMoedaDto 
  | CriarPaisDto 
  | CriarUfDto 
  | CriarMunicipioDto 
  | CriarAtividadeAgropecuariaDto 
  | CriarUnidadeMedidaDto 
  | CriarEmbalagemDto
  | CriarCategoriaDto;

/**
 * Reference entity update DTO union
 */
export type ReferenceUpdateDto = 
  | AtualizarMoedaDto 
  | AtualizarPaisDto 
  | AtualizarUfDto 
  | AtualizarMunicipioDto 
  | AtualizarAtividadeAgropecuariaDto 
  | AtualizarUnidadeMedidaDto 
  | AtualizarEmbalagemDto
  | AtualizarCategoriaDto;