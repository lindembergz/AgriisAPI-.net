using Agriis.Combos.Dominio.Enums;

namespace Agriis.Combos.Aplicacao.DTOs;

/// <summary>
/// DTO para representar desconto por categoria em combo
/// </summary>
public class ComboCategoriaDescontoDto
{
    public int Id { get; set; }
    public int ComboId { get; set; }
    public int CategoriaId { get; set; }
    public decimal PercentualDesconto { get; set; }
    public decimal ValorDescontoFixo { get; set; }
    public decimal DescontoPorHectare { get; set; }
    public TipoDesconto TipoDesconto { get; set; }
    public decimal HectareMinimo { get; set; }
    public decimal HectareMaximo { get; set; }
    public bool Ativo { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
}

/// <summary>
/// DTO para criar desconto por categoria em combo
/// </summary>
public class CriarComboCategoriaDescontoDto
{
    public int CategoriaId { get; set; }
    public TipoDesconto TipoDesconto { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal HectareMinimo { get; set; } = 0;
    public decimal HectareMaximo { get; set; } = decimal.MaxValue;
}

/// <summary>
/// DTO para atualizar desconto por categoria em combo
/// </summary>
public class AtualizarComboCategoriaDescontoDto
{
    public TipoDesconto TipoDesconto { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal HectareMinimo { get; set; }
    public decimal HectareMaximo { get; set; }
    public bool Ativo { get; set; }
}