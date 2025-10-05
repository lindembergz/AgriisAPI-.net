import { BaseEntity, BaseForm } from './base.model';

/**
 * Constantes para ramos de atividade disponíveis - sincronizado com backend RamosAtividadeConstants
 */
export const RAMOS_ATIVIDADE_DISPONIVEIS = [
  'Industria',
  'Distribuição',
  'Cooperativa',
  'Outro'
] as const;

/**
 * Opções para endereço de correspondência
 */
export const ENDERECO_CORRESPONDENCIA_OPTIONS = [
  { value: 'MesmoFaturamento', label: 'Mesmo endereço do faturamento' },
  { value: 'DiferenteFaturamento', label: 'Diferente do faturamento' }
] as const;

/**
 * Fornecedor entity interface - matching API structure
 */
export interface Fornecedor extends BaseEntity {
  nome: string;
  nomeFantasia?: string; // NOVO CAMPO
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
  bairro?: string;
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

  // NOVOS CAMPOS
  ramosAtividade: string[]; // Lista de ramos de atividade
  enderecoCorrespondencia: string; // 'MesmoFaturamento' ou 'DiferenteFaturamento'
}

/**
 * Usuario Fornecedor DTO - matching API UsuarioFornecedorDto
 */
export interface UsuarioFornecedor {
  id: number;
  usuarioId: number;
  usuarioNome: string;
  usuarioEmail: string;
  fornecedorId: number;
  fornecedorNome: string;
  role: number;
  roleNome: string;
  ativo: boolean;
  dataInicio: string;
  dataFim?: string;
  dataCriacao: string;
  dataAtualizacao?: string;
  territorios: any[];
}

/**
 * Fornecedor form interface for reactive forms - matching NEW API structure
 */
export interface FornecedorForm extends BaseForm {
  codigo?: string;
  nome: string;
  nomeFantasia?: string; // NOVO CAMPO
  cpfCnpj: string; // Matches CriarFornecedorCompletoRequest.CpfCnpj
  tipoCliente: string; // Matches CriarFornecedorCompletoRequest.TipoCliente
  telefone?: string;
  email?: string;
  inscricaoEstadual?: string;
  ramosAtividade?: string[]; // NOVO CAMPO
  enderecoCorrespondencia?: string; // NOVO CAMPO

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
    ufId?: number; // CAMPO ADICIONADO
    municipioId?: number; // CAMPO ADICIONADO
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
      ufId?: number; // CAMPO ADICIONADO
      municipioId?: number; // CAMPO ADICIONADO
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
  nomeFantasia?: string; // NOVO CAMPO
  inscricaoEstadual?: string;
  logradouro?: string;
  ufId?: number;
  municipioId?: number;
  bairro?: string; // CAMPO FALTANTE
  cep?: string;
  complemento?: string;
  latitude?: number;
  longitude?: number;
  telefone?: string;
  email?: string;
  moedaPadrao: number;
  pedidoMinimo?: number;
  tokenLincros?: string;
  ramosAtividade?: string[]; // NOVO CAMPO
  enderecoCorrespondencia?: string; // NOVO CAMPO
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
  ramosAtividade?: string[]; // NOVO CAMPO para filtro
  enderecoCorrespondencia?: string; // NOVO CAMPO para filtro
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