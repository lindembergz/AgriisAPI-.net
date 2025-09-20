using Agriis.Compartilhado.Dominio.Entidades;

namespace Agriis.Combos.Dominio.Entidades;

/// <summary>
/// Representa um local de recebimento específico para um combo com preços diferenciados
/// </summary>
public class ComboLocalRecebimento : EntidadeBase
{
    public int ComboId { get; private set; }
    public int PontoDistribuicaoId { get; private set; }
    public decimal PrecoAdicional { get; private set; }
    public decimal PercentualDesconto { get; private set; }
    public bool LocalPadrao { get; private set; }
    public string? Observacoes { get; private set; }

    // Navigation Properties
    public virtual Combo Combo { get; private set; } = null!;

    protected ComboLocalRecebimento() { } // EF Constructor

    public ComboLocalRecebimento(
        int comboId,
        int pontoDistribuicaoId,
        decimal precoAdicional = 0,
        decimal percentualDesconto = 0,
        bool localPadrao = false,
        string? observacoes = null)
    {
        ValidarParametros(precoAdicional, percentualDesconto);

        ComboId = comboId;
        PontoDistribuicaoId = pontoDistribuicaoId;
        PrecoAdicional = precoAdicional;
        PercentualDesconto = percentualDesconto;
        LocalPadrao = localPadrao;
        Observacoes = observacoes;
    }

    public void AtualizarPrecoAdicional(decimal novoPreco)
    {
        if (novoPreco < 0)
            throw new ArgumentException("Preço adicional não pode ser negativo", nameof(novoPreco));

        PrecoAdicional = novoPreco;
        AtualizarDataModificacao();
    }

    public void AtualizarDesconto(decimal novoDesconto)
    {
        if (novoDesconto < 0 || novoDesconto > 100)
            throw new ArgumentException("Desconto deve estar entre 0 e 100", nameof(novoDesconto));

        PercentualDesconto = novoDesconto;
        AtualizarDataModificacao();
    }

    public void DefinirComoPadrao(bool padrao)
    {
        LocalPadrao = padrao;
        AtualizarDataModificacao();
    }

    public void AtualizarObservacoes(string? observacoes)
    {
        Observacoes = observacoes;
        AtualizarDataModificacao();
    }

    public decimal CalcularPrecoFinal(decimal precoBase)
    {
        var precoComAdicional = precoBase + PrecoAdicional;
        var desconto = precoComAdicional * (PercentualDesconto / 100);
        return precoComAdicional - desconto;
    }

    private static void ValidarParametros(decimal precoAdicional, decimal percentualDesconto)
    {
        if (precoAdicional < 0)
            throw new ArgumentException("Preço adicional não pode ser negativo", nameof(precoAdicional));

        if (percentualDesconto < 0 || percentualDesconto > 100)
            throw new ArgumentException("Percentual de desconto deve estar entre 0 e 100", nameof(percentualDesconto));
    }
}