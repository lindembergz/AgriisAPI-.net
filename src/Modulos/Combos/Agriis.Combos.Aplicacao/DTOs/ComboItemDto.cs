namespace Agriis.Combos.Aplicacao.DTOs;

/// <summary>
/// DTO para representar um item de combo
/// </summary>
public class ComboItemDto
{
    public int Id { get; set; }
    public int ComboId { get; set; }
    public int ProdutoId { get; set; }
    public decimal Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal PercentualDesconto { get; set; }
    public bool ProdutoObrigatorio { get; set; }
    public int Ordem { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
}

/// <summary>
/// DTO para criar um item de combo
/// </summary>
public class CriarComboItemDto
{
    public int ProdutoId { get; set; }
    public decimal Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal PercentualDesconto { get; set; } = 0;
    public bool ProdutoObrigatorio { get; set; } = false;
    public int Ordem { get; set; } = 0;
}

/// <summary>
/// DTO para atualizar um item de combo
/// </summary>
public class AtualizarComboItemDto
{
    public decimal Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal PercentualDesconto { get; set; }
    public bool ProdutoObrigatorio { get; set; }
    public int Ordem { get; set; }
}