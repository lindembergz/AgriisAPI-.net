import { BaseEntity, BaseForm } from './base.model';

/**
 * Fornecedor entity interface - matching API structure
 */
export interface Fornecedor extends BaseEntity {
  nome: string;
  cnpj: string;
  cnpjFormatado: string;
  cpfCnpj?: string; // Alternative property name
  inscricaoEstadual?: string;
  tipoCliente?: string; // For backward compatibility
  
  // Endereço fields (direct in fornecedor, not nested)
  logradouro?: string;
  ufId?: number;
  ufNome?: string;
  ufCodigo?: string;
  municipioId?: number;
  municipioNome?: string;
  cep?: string;
  complemento?: string;
  latitude?: number;
  longitude?: number;
  endereco?: any; // For backward compatibility
  
  // Contact info
  telefone?: string;
  email?: string;
  logoUrl?: string;
  
  // Business info
  moedaPadrao: number; // 0 = Real, 1 = Dólar
  moedaPadraoNome: string;
  pedidoMinimo?: number;
  tokenLincros?: string;
  
  // Related entities for backward compatibility
  pontosDistribuicao?: any[];
  usuarioMaster?: {
    nome: string;
    email: string;
    senha?: string;
    telefone?: string;
  };
  
  ativo: boolean;
  dadosAdicionais?: any;
  
  // Timestamps
  dataCriacao: Date;
  dataAtualizacao?: Date;
  
  // Related entities
  usuarios: UsuarioFornecedor[];
}

/**
 * Usuario Fornecedor DTO
 */
export interface UsuarioFornecedor {
  id: number;
  nome: string;
  email: string;
  ativo: boolean;
  // Add other user fields as needed
}

/**
 * Fornecedor form interface for reactive forms - matching NEW API structure
 */
export interface FornecedorForm extends BaseForm {
  codigo?: string;
  nome: string;
  cpfCnpj: string; // Matches CriarFornecedorCompletoRequest.CpfCnpj
  tipoCliente: string; // Matches CriarFornecedorCompletoRequest.TipoCliente
  telefone?: string;
  email?: string;
  inscricaoEstadual?: string;
  
  // Endereco structure matching EnderecoRequest
  endereco?: {
    logradouro: string;
    numero: string;
    complemento?: string;
    bairro: string;
    cidade: string;
    uf: string;
    cep: string;
    latitude?: number;
    longitude?: number;
  };
  
  // Pontos de distribuição matching PontoDistribuicaoRequest[]
  pontosDistribuicao?: Array<{
    nome: string;
    latitude?: number;
    longitude?: number;
    endereco: {
      logradouro: string;
      numero: string;
      complemento?: string;
      bairro: string;
      cidade: string;
      uf: string;
      cep: string;
      latitude?: number;
      longitude?: number;
    };
  }>;
  
  // Usuario master matching UsuarioMasterRequest
  usuarioMaster?: {
    nome: string;
    email: string;
    senha: string;
    telefone?: string;
  };
}

/**
 * Create Fornecedor DTO - matching NEW API CriarFornecedorCompletoRequest
 */
export interface CriarFornecedorDto {
  codigo?: string;
  nome: string;
  cpfCnpj: string;
  tipoCliente: string;
  telefone?: string;
  email?: string;
  inscricaoEstadual?: string;
  endereco?: {
    logradouro: string;
    numero: string;
    complemento?: string;
    bairro: string;
    cidade: string;
    uf: string;
    cep: string;
    latitude?: number;
    longitude?: number;
  };
  pontosDistribuicao?: Array<{
    nome: string;
    latitude?: number;
    longitude?: number;
    endereco: {
      logradouro: string;
      numero: string;
      complemento?: string;
      bairro: string;
      cidade: string;
      uf: string;
      cep: string;
      latitude?: number;
      longitude?: number;
    };
  }>;
  usuarioMaster: {
    nome: string;
    email: string;
    senha: string;
    telefone?: string;
  };
}

/**
 * Update Fornecedor DTO - matching NEW API AtualizarFornecedorRequest
 */
export interface AtualizarFornecedorDto {
  nome: string;
  inscricaoEstadual?: string;
  logradouro?: string;
  ufId?: number;
  municipioId?: number;
  cep?: string;
  complemento?: string;
  latitude?: number;
  longitude?: number;
  telefone?: string;
  email?: string;
  moedaPadrao: number;
  pedidoMinimo?: number;
  tokenLincros?: string;
}

/**
 * Fornecedor query parameters - matching NEW API FiltrosFornecedorDto
 */
export interface FornecedorQueryParams {
  filtro?: string;
  pagina: number;
  tamanhoPagina: number;
}

/**
 * Fornecedor filters DTO - matching API
 */
export interface FiltrosFornecedorDto {
  filtro?: string;
  pagina: number;
  tamanhoPagina: number;
}

/**
 * Fornecedor list response - matching API
 */
export interface FornecedorListResponse {
  items: Fornecedor[];
  total_items: number;
  total: number; // Alternative property name
  page: number;
  page_size: number;
  total_pages: number;
}