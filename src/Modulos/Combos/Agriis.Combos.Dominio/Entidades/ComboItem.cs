using Agriis.Compartilhado.Dominio.Entidades;

namespace Agriis.Combos.Dominio.Entidades;

/// <summary>
/// Representa um item de produto dentro de um combo
/// </summary>
public class ComboItem : EntidadeBase
{
    public int ComboId { get; private set; }
    public int ProdutoId { get; private set; }
    public decimal Quantidade { get; private set; }
    public decimal PrecoUnitario { get; private set; }
    public decimal PercentualDesconto { get; private set; }
    public bool ProdutoObrigatorio { get; private set; }
    public int Ordem { get; private set; }

    // Navigation Properties
    public virtual Combo Combo { get; private set; } = null!;

    protected ComboItem() { } // EF Constructor

    public ComboItem(
        int comboId,
        int produtoId,
        decimal quantidade,
        decimal precoUnitario,
        decimal percentualDesconto = 0,
        bool produtoObrigatorio = false,
        int ordem = 0)
    {
        ValidarParametros(quantidade, precoUnitario, percentualDesconto);

        ComboId = comboId;
        ProdutoId = produtoId;
        Quantidade = quantidade;
        PrecoUnitario = precoUnitario;
        PercentualDesconto = percentualDesconto;
        ProdutoObrigatorio = produtoObrigatorio;
        Ordem = ordem;
    }

    public void AtualizarQuantidade(decimal novaQuantidade)
    {
        if (novaQuantidade <= 0)
            throw new ArgumentException("Quantidade deve ser maior que zero", nameof(novaQuantidade));

        Quantidade = novaQuantidade;
        AtualizarDataModificacao();
    }

    public void AtualizarPreco(decimal novoPreco)
    {
        if (novoPreco < 0)
            throw new ArgumentException("Preço não pode ser negativo", nameof(novoPreco));

        PrecoUnitario = novoPreco;
        AtualizarDataModificacao();
    }

    public void AtualizarDesconto(decimal novoDesconto)
    {
        if (novoDesconto < 0 || novoDesconto > 100)
            throw new ArgumentException("Desconto deve estar entre 0 e 100", nameof(novoDesconto));

        PercentualDesconto = novoDesconto;
        AtualizarDataModificacao();
    }

    public void DefinirComoObrigatorio(bool obrigatorio)
    {
        ProdutoObrigatorio = obrigatorio;
        AtualizarDataModificacao();
    }

    public void AtualizarOrdem(int novaOrdem)
    {
        if (novaOrdem < 0)
            throw new ArgumentException("Ordem deve ser maior ou igual a zero", nameof(novaOrdem));

        Ordem = novaOrdem;
        AtualizarDataModificacao();
    }

    public decimal CalcularValorComDesconto()
    {
        var valorTotal = Quantidade * PrecoUnitario;
        var desconto = valorTotal * (PercentualDesconto / 100);
        return valorTotal - desconto;
    }

    private static void ValidarParametros(decimal quantidade, decimal precoUnitario, decimal percentualDesconto)
    {
        if (quantidade <= 0)
            throw new ArgumentException("Quantidade deve ser maior que zero", nameof(quantidade));

        if (precoUnitario < 0)
            throw new ArgumentException("Preço unitário não pode ser negativo", nameof(precoUnitario));

        if (percentualDesconto < 0 || percentualDesconto > 100)
            throw new ArgumentException("Percentual de desconto deve estar entre 0 e 100", nameof(percentualDesconto));
    }
}