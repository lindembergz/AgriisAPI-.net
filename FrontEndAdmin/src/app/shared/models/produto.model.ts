import { BaseEntity, BaseForm } from './base.model';
import { UnidadeMedidaDto, EmbalagemDto, AtividadeAgropecuariaDto, CategoriaDto } from './reference.model';

/**
 * Enums matching API
 */
export enum TipoProduto {
  Semente = 0,
  Defensivo = 1,
  Fertilizante = 2,
  Inoculante = 3,
  Adjuvante = 4,
  Equipamento = 5,
  Servico = 6,
  Outro = 7
}

export enum StatusProduto {
  Ativo = 0,
  Inativo = 1,
  Descontinuado = 2
}

export enum TipoCalculoPeso {
  PesoNominal = 0,
  PesoCubado = 1,
  MaiorPeso = 2
}



/**
 * Dimensões do produto - matching API structure
 */
export interface DimensoesProdutoDto {
  altura: number;
  largura: number;
  comprimento: number;
  pesoNominal: number;
  pesoEmbalagem: number;
  pms?: number;
  quantidadeMinima: number;
  embalagem: string;
  faixaDensidadeInicial?: number;
  faixaDensidadeFinal?: number;
  volume: number;
  pesoCubado?: number;
  pesoParaFrete: number;
}

/**
 * Dimensões para criação - matching API structure
 */
export interface CriarDimensoesProdutoDto {
  altura: number;
  largura: number;
  comprimento: number;
  pesoNominal: number;
  pesoEmbalagem: number;
  pms?: number;
  quantidadeMinima: number;
  embalagem: string;
  faixaDensidadeInicial?: number;
  faixaDensidadeFinal?: number;
}

/**
 * Dimensões para atualização - matching API structure
 */
export interface AtualizarDimensoesProdutoDto {
  altura: number;
  largura: number;
  comprimento: number;
  pesoNominal: number;
  pesoEmbalagem: number;
  pms?: number;
  quantidadeMinima: number;
  embalagem: string;
  faixaDensidadeInicial?: number;
  faixaDensidadeFinal?: number;
}

/**
 * Produto entity interface - matching API structure
 */
export interface ProdutoDto extends BaseEntity {
  nome: string;
  descricao?: string;
  codigo: string;
  marca?: string;
  tipo: TipoProduto;
  status: StatusProduto;
  
  // Reference relationships
  unidadeMedidaId: number;
  unidadeMedidaNome?: string;
  unidadeMedidaSimbolo?: string;
  unidadeMedida?: UnidadeMedidaDto;
  
  embalagemId?: number;
  embalagemNome?: string;
  embalagem?: EmbalagemDto;
  
  atividadeAgropecuariaId?: number;
  atividadeAgropecuariaNome?: string;
  atividadeAgropecuariaCodigo?: string;
  atividadeAgropecuaria?: AtividadeAgropecuariaDto;
  
  tipoCalculoPeso: TipoCalculoPeso;
  produtoRestrito: boolean;
  observacoesRestricao?: string;
  
  categoriaId: number;
  categoriaNome?: string;
  categoria?: CategoriaDto;
  
  fornecedorId: number;
  fornecedorNome?: string;
  
  produtoPaiId?: number;
  produtoPaiNome?: string;
  
  // Dimensões obrigatórias
  dimensoes: DimensoesProdutoDto;
  
  // Culturas relacionadas
  culturasIds: number[];
  culturasNomes: string[];
  
  // Timestamps
  dataCriacao: Date;
  dataAtualizacao?: Date;
  
  ativo: boolean;
}

/**
 * Produto form interface for reactive forms - matching API requirements
 */
export interface ProdutoForm extends BaseForm {
  codigo: string;
  nome: string;
  descricao?: string;
  marca?: string;
  tipo: TipoProduto;
  unidadeMedidaId: number;
  embalagemId?: number;
  atividadeAgropecuariaId?: number;
  tipoCalculoPeso: TipoCalculoPeso;
  produtoRestrito: boolean;
  observacoesRestricao?: string;
  categoriaId: number;
  fornecedorId: number;
  produtoPaiId?: number;
  dimensoes: CriarDimensoesProdutoDto;
  culturasIds: number[];
  ativo: boolean;
}

/**
 * Create Produto DTO - matching API structure
 */
export interface CriarProdutoDto {
  nome: string;
  descricao?: string;
  codigo: string;
  marca?: string;
  tipo: TipoProduto;
  unidadeMedidaId: number;
  embalagemId?: number;
  atividadeAgropecuariaId?: number;
  tipoCalculoPeso: TipoCalculoPeso;
  produtoRestrito: boolean;
  observacoesRestricao?: string;
  categoriaId: number;
  fornecedorId: number;
  produtoPaiId?: number;
  dimensoes: CriarDimensoesProdutoDto;
  culturasIds: number[];
}

/**
 * Update Produto DTO - matching API structure
 */
export interface AtualizarProdutoDto {
  nome: string;
  descricao?: string;
  codigo: string;
  marca?: string;
  unidadeMedidaId: number;
  embalagemId?: number;
  atividadeAgropecuariaId?: number;
  tipoCalculoPeso: TipoCalculoPeso;
  produtoRestrito: boolean;
  observacoesRestricao?: string;
  categoriaId: number;
  dimensoes: AtualizarDimensoesProdutoDto;
  culturasIds: number[];
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