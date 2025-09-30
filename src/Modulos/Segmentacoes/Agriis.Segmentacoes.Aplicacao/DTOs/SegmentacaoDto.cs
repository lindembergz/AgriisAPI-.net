using System.Text.Json;

namespace Agriis.Segmentacoes.Aplicacao.DTOs;

/// <summary>
/// DTO para segmentação
/// </summary>
public class SegmentacaoDto
{
    /// <summary>
    /// ID da segmentação
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Nome da segmentação
    /// </summary>
    public string Nome { get; set; } = string.Empty;
    
    /// <summary>
    /// Descrição da segmentação
    /// </summary>
    public string? Descricao { get; set; }
    
    /// <summary>
    /// Indica se a segmentação está ativa
    /// </summary>
    public bool Ativo { get; set; }
    
    /// <summary>
    /// ID do fornecedor proprietário
    /// </summary>
    public int FornecedorId { get; set; }
    
    /// <summary>
    /// Configuração territorial em formato JSON
    /// </summary>
    public JsonDocument? ConfiguracaoTerritorial { get; set; }
    
    /// <summary>
    /// Indica se é a segmentação padrão
    /// </summary>
    public bool EhPadrao { get; set; }
    
    /// <summary>
    /// Data de criação (UTC com timezone)
    /// </summary>
    public DateTimeOffset DataCriacao { get; set; }
    
    /// <summary>
    /// Data de atualização (UTC com timezone)
    /// </summary>
    public DateTimeOffset? DataAtualizacao { get; set; }
    
    /// <summary>
    /// Grupos associados
    /// </summary>
    public List<GrupoDto> Grupos { get; set; } = new();
}

/// <summary>
/// DTO para criação de segmentação
/// </summary>
public class CriarSegmentacaoDto
{
    /// <summary>
    /// Nome da segmentação
    /// </summary>
    public string Nome { get; set; } = string.Empty;
    
    /// <summary>
    /// Descrição da segmentação
    /// </summary>
    public string? Descricao { get; set; }
    
    /// <summary>
    /// ID do fornecedor proprietário
    /// </summary>
    public int FornecedorId { get; set; }
    
    /// <summary>
    /// Configuração territorial em formato JSON
    /// </summary>
    public JsonDocument? ConfiguracaoTerritorial { get; set; }
    
    /// <summary>
    /// Indica se é a segmentação padrão
    /// </summary>
    public bool EhPadrao { get; set; }
}

/// <summary>
/// DTO para atualização de segmentação
/// </summary>
public class AtualizarSegmentacaoDto
{
    /// <summary>
    /// Nome da segmentação
    /// </summary>
    public string Nome { get; set; } = string.Empty;
    
    /// <summary>
    /// Descrição da segmentação
    /// </summary>
    public string? Descricao { get; set; }
    
    /// <summary>
    /// Configuração territorial em formato JSON
    /// </summary>
    public JsonDocument? ConfiguracaoTerritorial { get; set; }
    
    /// <summary>
    /// Indica se é a segmentação padrão
    /// </summary>
    public bool EhPadrao { get; set; }
    
    /// <summary>
    /// Indica se a segmentação está ativa
    /// </summary>
    public bool Ativo { get; set; }
}