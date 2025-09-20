namespace Agriis.Segmentacoes.Aplicacao.DTOs;

/// <summary>
/// DTO para grupo de segmentação
/// </summary>
public class GrupoDto
{
    /// <summary>
    /// ID do grupo
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Nome do grupo
    /// </summary>
    public string Nome { get; set; } = string.Empty;
    
    /// <summary>
    /// Descrição do grupo
    /// </summary>
    public string? Descricao { get; set; }
    
    /// <summary>
    /// Área mínima em hectares
    /// </summary>
    public decimal AreaMinima { get; set; }
    
    /// <summary>
    /// Área máxima em hectares
    /// </summary>
    public decimal? AreaMaxima { get; set; }
    
    /// <summary>
    /// Indica se o grupo está ativo
    /// </summary>
    public bool Ativo { get; set; }
    
    /// <summary>
    /// ID da segmentação proprietária
    /// </summary>
    public int SegmentacaoId { get; set; }
    
    /// <summary>
    /// Data de criação
    /// </summary>
    public DateTime DataCriacao { get; set; }
    
    /// <summary>
    /// Data de atualização
    /// </summary>
    public DateTime? DataAtualizacao { get; set; }
    
    /// <summary>
    /// Descontos por categoria
    /// </summary>
    public List<GrupoSegmentacaoDto> Descontos { get; set; } = new();
}

/// <summary>
/// DTO para criação de grupo
/// </summary>
public class CriarGrupoDto
{
    /// <summary>
    /// Nome do grupo
    /// </summary>
    public string Nome { get; set; } = string.Empty;
    
    /// <summary>
    /// Descrição do grupo
    /// </summary>
    public string? Descricao { get; set; }
    
    /// <summary>
    /// Área mínima em hectares
    /// </summary>
    public decimal AreaMinima { get; set; }
    
    /// <summary>
    /// Área máxima em hectares
    /// </summary>
    public decimal? AreaMaxima { get; set; }
    
    /// <summary>
    /// ID da segmentação proprietária
    /// </summary>
    public int SegmentacaoId { get; set; }
}

/// <summary>
/// DTO para atualização de grupo
/// </summary>
public class AtualizarGrupoDto
{
    /// <summary>
    /// Nome do grupo
    /// </summary>
    public string Nome { get; set; } = string.Empty;
    
    /// <summary>
    /// Descrição do grupo
    /// </summary>
    public string? Descricao { get; set; }
    
    /// <summary>
    /// Área mínima em hectares
    /// </summary>
    public decimal AreaMinima { get; set; }
    
    /// <summary>
    /// Área máxima em hectares
    /// </summary>
    public decimal? AreaMaxima { get; set; }
    
    /// <summary>
    /// Indica se o grupo está ativo
    /// </summary>
    public bool Ativo { get; set; }
}