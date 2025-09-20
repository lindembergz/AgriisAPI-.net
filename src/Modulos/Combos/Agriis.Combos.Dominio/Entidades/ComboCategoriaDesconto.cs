using Agriis.Combos.Dominio.Enums;
using Agriis.Compartilhado.Dominio.Entidades;

namespace Agriis.Combos.Dominio.Entidades;

/// <summary>
/// Representa descontos específicos por categoria de produto dentro de um combo
/// </summary>
public class ComboCategoriaDesconto : EntidadeBase
{
    public int ComboId { get; private set; }
    public int CategoriaId { get; private set; }
    public decimal PercentualDesconto { get; private set; }
    public decimal ValorDescontoFixo { get; private set; }
    public decimal DescontoPorHectare { get; private set; }
    public TipoDesconto TipoDesconto { get; private set; }
    public decimal HectareMinimo { get; private set; }
    public decimal HectareMaximo { get; private set; }
    public bool Ativo { get; private set; }

    // Navigation Properties
    public virtual Combo Combo { get; private set; } = null!;

    protected ComboCategoriaDesconto() { } // EF Constructor

    public ComboCategoriaDesconto(
        int comboId,
        int categoriaId,
        TipoDesconto tipoDesconto,
        decimal hectareMinimo = 0,
        decimal hectareMaximo = decimal.MaxValue)
    {
        ValidarParametros(hectareMinimo, hectareMaximo);

        ComboId = comboId;
        CategoriaId = categoriaId;
        TipoDesconto = tipoDesconto;
        HectareMinimo = hectareMinimo;
        HectareMaximo = hectareMaximo;
        Ativo = true;
        PercentualDesconto = 0;
        ValorDescontoFixo = 0;
        DescontoPorHectare = 0;
    }

    public void DefinirDescontoPercentual(decimal percentual)
    {
        if (percentual < 0 || percentual > 100)
            throw new ArgumentException("Percentual deve estar entre 0 e 100", nameof(percentual));

        TipoDesconto = TipoDesconto.Percentual;
        PercentualDesconto = percentual;
        ValorDescontoFixo = 0;
        DescontoPorHectare = 0;
        AtualizarDataModificacao();
    }

    public void DefinirDescontoFixo(decimal valorFixo)
    {
        if (valorFixo < 0)
            throw new ArgumentException("Valor fixo não pode ser negativo", nameof(valorFixo));

        TipoDesconto = TipoDesconto.ValorFixo;
        ValorDescontoFixo = valorFixo;
        PercentualDesconto = 0;
        DescontoPorHectare = 0;
        AtualizarDataModificacao();
    }

    public void DefinirDescontoPorHectare(decimal valorPorHectare)
    {
        if (valorPorHectare < 0)
            throw new ArgumentException("Valor por hectare não pode ser negativo", nameof(valorPorHectare));

        TipoDesconto = TipoDesconto.PorHectare;
        DescontoPorHectare = valorPorHectare;
        PercentualDesconto = 0;
        ValorDescontoFixo = 0;
        AtualizarDataModificacao();
    }

    public void AtualizarFaixaHectare(decimal hectareMinimo, decimal hectareMaximo)
    {
        ValidarParametros(hectareMinimo, hectareMaximo);

        HectareMinimo = hectareMinimo;
        HectareMaximo = hectareMaximo;
        AtualizarDataModificacao();
    }

    public void Ativar()
    {
        Ativo = true;
        AtualizarDataModificacao();
    }

    public void Desativar()
    {
        Ativo = false;
        AtualizarDataModificacao();
    }

    public decimal CalcularDesconto(decimal valorBase, decimal hectareProdutor)
    {
        if (!Ativo || !ValidarFaixaHectare(hectareProdutor))
            return 0;

        return TipoDesconto switch
        {
            TipoDesconto.Percentual => valorBase * (PercentualDesconto / 100),
            TipoDesconto.ValorFixo => ValorDescontoFixo,
            TipoDesconto.PorHectare => DescontoPorHectare * hectareProdutor,
            _ => 0
        };
    }

    public bool ValidarFaixaHectare(decimal hectareProdutor)
    {
        return hectareProdutor >= HectareMinimo && hectareProdutor <= HectareMaximo;
    }

    private static void ValidarParametros(decimal hectareMinimo, decimal hectareMaximo)
    {
        if (hectareMinimo < 0)
            throw new ArgumentException("Hectare mínimo deve ser maior ou igual a zero", nameof(hectareMinimo));

        if (hectareMaximo <= hectareMinimo)
            throw new ArgumentException("Hectare máximo deve ser maior que o mínimo", nameof(hectareMaximo));
    }
}