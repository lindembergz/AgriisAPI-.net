namespace Agriis.Combos.Aplicacao.DTOs;

/// <summary>
/// DTO para representar um local de recebimento de combo
/// </summary>
public class ComboLocalRecebimentoDto
{
    public int Id { get; set; }
    public int ComboId { get; set; }
    public int PontoDistribuicaoId { get; set; }
    public decimal PrecoAdicional { get; set; }
    public decimal PercentualDesconto { get; set; }
    public bool LocalPadrao { get; set; }
    public string? Observacoes { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
}

/// <summary>
/// DTO para criar um local de recebimento de combo
/// </summary>
public class CriarComboLocalRecebimentoDto
{
    public int PontoDistribuicaoId { get; set; }
    public decimal PrecoAdicional { get; set; } = 0;
    public decimal PercentualDesconto { get; set; } = 0;
    public bool LocalPadrao { get; set; } = false;
    public string? Observacoes { get; set; }
}

/// <summary>
/// DTO para atualizar um local de recebimento de combo
/// </summary>
public class AtualizarComboLocalRecebimentoDto
{
    public decimal PrecoAdicional { get; set; }
    public decimal PercentualDesconto { get; set; }
    public bool LocalPadrao { get; set; }
    public string? Observacoes { get; set; }
}