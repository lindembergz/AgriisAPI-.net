using Agriis.Enderecos.Aplicacao.DTOs;

namespace Agriis.PontosDistribuicao.Aplicacao.DTOs;

/// <summary>
/// DTO para ponto de distribuição
/// </summary>
public class PontoDistribuicaoDto
{
    /// <summary>
    /// ID do ponto de distribuição
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Nome do ponto de distribuição
    /// </summary>
    public string Nome { get; set; } = string.Empty;
    
    /// <summary>
    /// Descrição do ponto de distribuição
    /// </summary>
    public string? Descricao { get; set; }
    
    /// <summary>
    /// Indica se o ponto está ativo
    /// </summary>
    public bool Ativo { get; set; }
    
    /// <summary>
    /// ID do fornecedor proprietário do ponto
    /// </summary>
    public int FornecedorId { get; set; }
    
    /// <summary>
    /// Endereço do ponto de distribuição
    /// </summary>
    public EnderecoDto? Endereco { get; set; }
    
    /// <summary>
    /// Lista de IDs dos estados atendidos
    /// </summary>
    public List<int> EstadosAtendidos { get; set; } = new();
    
    /// <summary>
    /// Lista de IDs dos municípios atendidos
    /// </summary>
    public List<int> MunicipiosAtendidos { get; set; } = new();
    
    /// <summary>
    /// Raio de cobertura em quilômetros
    /// </summary>
    public double? RaioCobertura { get; set; }
    
    /// <summary>
    /// Capacidade máxima de armazenamento
    /// </summary>
    public decimal? CapacidadeMaxima { get; set; }
    
    /// <summary>
    /// Unidade da capacidade
    /// </summary>
    public string? UnidadeCapacidade { get; set; }
    
    /// <summary>
    /// Horário de funcionamento por dia da semana
    /// </summary>
    public Dictionary<string, string> HorarioFuncionamento { get; set; } = new();
    
    /// <summary>
    /// Observações adicionais
    /// </summary>
    public string? Observacoes { get; set; }
    
    /// <summary>
    /// Data de criação
    /// </summary>
    public DateTime DataCriacao { get; set; }
    
    /// <summary>
    /// Data da última atualização
    /// </summary>
    public DateTime? DataAtualizacao { get; set; }
    
    /// <summary>
    /// Distância em quilômetros (preenchida em consultas por localização)
    /// </summary>
    public double? DistanciaKm { get; set; }
}

/// <summary>
/// DTO para criação de ponto de distribuição
/// </summary>
public class CriarPontoDistribuicaoDto
{
    /// <summary>
    /// Nome do ponto de distribuição
    /// </summary>
    public string Nome { get; set; } = string.Empty;
    
    /// <summary>
    /// Descrição do ponto de distribuição
    /// </summary>
    public string? Descricao { get; set; }
    
    /// <summary>
    /// ID do fornecedor proprietário do ponto
    /// </summary>
    public int FornecedorId { get; set; }
    
    /// <summary>
    /// ID do endereço do ponto de distribuição
    /// </summary>
    public int EnderecoId { get; set; }
    
    /// <summary>
    /// Lista de IDs dos estados atendidos
    /// </summary>
    public List<int> EstadosAtendidos { get; set; } = new();
    
    /// <summary>
    /// Lista de IDs dos municípios atendidos
    /// </summary>
    public List<int> MunicipiosAtendidos { get; set; } = new();
    
    /// <summary>
    /// Raio de cobertura em quilômetros
    /// </summary>
    public double? RaioCobertura { get; set; }
    
    /// <summary>
    /// Capacidade máxima de armazenamento
    /// </summary>
    public decimal? CapacidadeMaxima { get; set; }
    
    /// <summary>
    /// Unidade da capacidade
    /// </summary>
    public string? UnidadeCapacidade { get; set; }
    
    /// <summary>
    /// Horário de funcionamento por dia da semana
    /// </summary>
    public Dictionary<string, string> HorarioFuncionamento { get; set; } = new();
    
    /// <summary>
    /// Observações adicionais
    /// </summary>
    public string? Observacoes { get; set; }
}

/// <summary>
/// DTO para atualização de ponto de distribuição
/// </summary>
public class AtualizarPontoDistribuicaoDto
{
    /// <summary>
    /// Nome do ponto de distribuição
    /// </summary>
    public string Nome { get; set; } = string.Empty;
    
    /// <summary>
    /// Descrição do ponto de distribuição
    /// </summary>
    public string? Descricao { get; set; }
    
    /// <summary>
    /// Lista de IDs dos estados atendidos
    /// </summary>
    public List<int> EstadosAtendidos { get; set; } = new();
    
    /// <summary>
    /// Lista de IDs dos municípios atendidos
    /// </summary>
    public List<int> MunicipiosAtendidos { get; set; } = new();
    
    /// <summary>
    /// Raio de cobertura em quilômetros
    /// </summary>
    public double? RaioCobertura { get; set; }
    
    /// <summary>
    /// Capacidade máxima de armazenamento
    /// </summary>
    public decimal? CapacidadeMaxima { get; set; }
    
    /// <summary>
    /// Unidade da capacidade
    /// </summary>
    public string? UnidadeCapacidade { get; set; }
    
    /// <summary>
    /// Horário de funcionamento por dia da semana
    /// </summary>
    public Dictionary<string, string> HorarioFuncionamento { get; set; } = new();
    
    /// <summary>
    /// Observações adicionais
    /// </summary>
    public string? Observacoes { get; set; }
}

/// <summary>
/// DTO para consulta de pontos por localização
/// </summary>
public class ConsultaPontosPorLocalizacaoDto
{
    /// <summary>
    /// ID do estado (opcional)
    /// </summary>
    public int? EstadoId { get; set; }
    
    /// <summary>
    /// ID do município (opcional)
    /// </summary>
    public int? MunicipioId { get; set; }
    
    /// <summary>
    /// Latitude para busca por proximidade (opcional)
    /// </summary>
    public double? Latitude { get; set; }
    
    /// <summary>
    /// Longitude para busca por proximidade (opcional)
    /// </summary>
    public double? Longitude { get; set; }
    
    /// <summary>
    /// Raio de busca em quilômetros (padrão: 50km)
    /// </summary>
    public double RaioKm { get; set; } = 50;
    
    /// <summary>
    /// ID do fornecedor para filtrar (opcional)
    /// </summary>
    public int? FornecedorId { get; set; }
    
    /// <summary>
    /// Se deve retornar apenas pontos ativos (padrão: true)
    /// </summary>
    public bool ApenasAtivos { get; set; } = true;
}

/// <summary>
/// DTO para estatísticas de pontos de distribuição
/// </summary>
public class EstatisticasPontosDistribuicaoDto
{
    /// <summary>
    /// Total de pontos
    /// </summary>
    public int Total { get; set; }
    
    /// <summary>
    /// Pontos ativos
    /// </summary>
    public int Ativos { get; set; }
    
    /// <summary>
    /// Pontos inativos
    /// </summary>
    public int Inativos { get; set; }
    
    /// <summary>
    /// Percentual de pontos ativos
    /// </summary>
    public double PercentualAtivos => Total > 0 ? (double)Ativos / Total * 100 : 0;
}