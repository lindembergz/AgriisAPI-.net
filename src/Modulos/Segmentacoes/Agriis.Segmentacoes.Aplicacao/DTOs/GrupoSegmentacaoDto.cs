namespace Agriis.Segmentacoes.Aplicacao.DTOs;

/// <summary>
/// DTO para grupo de segmentação (desconto por categoria)
/// </summary>
public class GrupoSegmentacaoDto
{
    /// <summary>
    /// ID do desconto
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// ID do grupo
    /// </summary>
    public int GrupoId { get; set; }
    
    /// <summary>
    /// ID da categoria
    /// </summary>
    public int CategoriaId { get; set; }
    
    /// <summary>
    /// Percentual de desconto
    /// </summary>
    public decimal PercentualDesconto { get; set; }
    
    /// <summary>
    /// Indica se o desconto está ativo
    /// </summary>
    public bool Ativo { get; set; }
    
    /// <summary>
    /// Observações sobre o desconto
    /// </summary>
    public string? Observacoes { get; set; }
    
    /// <summary>
    /// Data de criação
    /// </summary>
    public DateTime DataCriacao { get; set; }
    
    /// <summary>
    /// Data de atualização
    /// </summary>
    public DateTime? DataAtualizacao { get; set; }
}

/// <summary>
/// DTO para criação de desconto por categoria
/// </summary>
public class CriarGrupoSegmentacaoDto
{
    /// <summary>
    /// ID do grupo
    /// </summary>
    public int GrupoId { get; set; }
    
    /// <summary>
    /// ID da categoria
    /// </summary>
    public int CategoriaId { get; set; }
    
    /// <summary>
    /// Percentual de desconto
    /// </summary>
    public decimal PercentualDesconto { get; set; }
    
    /// <summary>
    /// Observações sobre o desconto
    /// </summary>
    public string? Observacoes { get; set; }
}

/// <summary>
/// DTO para atualização de desconto por categoria
/// </summary>
public class AtualizarGrupoSegmentacaoDto
{
    /// <summary>
    /// Percentual de desconto
    /// </summary>
    public decimal PercentualDesconto { get; set; }
    
    /// <summary>
    /// Observações sobre o desconto
    /// </summary>
    public string? Observacoes { get; set; }
    
    /// <summary>
    /// Indica se o desconto está ativo
    /// </summary>
    public bool Ativo { get; set; }
}

/// <summary>
/// DTO para resultado de cálculo de desconto segmentado
/// </summary>
public class ResultadoDescontoSegmentadoDto
{
    /// <summary>
    /// Percentual de desconto aplicado
    /// </summary>
    public decimal PercentualDesconto { get; set; }
    
    /// <summary>
    /// Valor do desconto em moeda
    /// </summary>
    public decimal ValorDesconto { get; set; }
    
    /// <summary>
    /// Valor final após aplicar o desconto
    /// </summary>
    public decimal ValorFinal { get; set; }
    
    /// <summary>
    /// Nome da segmentação aplicada
    /// </summary>
    public string? SegmentacaoAplicada { get; set; }
    
    /// <summary>
    /// Nome do grupo aplicado
    /// </summary>
    public string? GrupoAplicado { get; set; }
    
    /// <summary>
    /// Observações sobre o cálculo
    /// </summary>
    public string? Observacoes { get; set; }
}