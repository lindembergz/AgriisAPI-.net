using Agriis.Compartilhado.Dominio.Entidades;

namespace Agriis.Produtos.Dominio.Entidades;

/// <summary>
/// Entidade de relacionamento entre Produto e Cultura
/// </summary>
public class ProdutoCultura : EntidadeBase
{
    /// <summary>
    /// ID do produto
    /// </summary>
    public int ProdutoId { get; private set; }
    
    /// <summary>
    /// ID da cultura
    /// </summary>
    public int CulturaId { get; private set; }
    
    /// <summary>
    /// Indica se a associação está ativa
    /// </summary>
    public bool Ativo { get; private set; }
    
    /// <summary>
    /// Observações sobre a compatibilidade
    /// </summary>
    public string? Observacoes { get; private set; }

    // Navigation Properties
    public virtual Produto Produto { get; private set; } = null!;

    protected ProdutoCultura() { } // EF Constructor

    public ProdutoCultura(int produtoId, int culturaId, string? observacoes = null)
    {
        ProdutoId = produtoId;
        CulturaId = culturaId;
        Observacoes = observacoes;
        Ativo = true;
    }

    /// <summary>
    /// Atualiza as observações
    /// </summary>
    public void AtualizarObservacoes(string? observacoes)
    {
        Observacoes = observacoes;
        AtualizarDataModificacao();
    }

    /// <summary>
    /// Ativa a associação
    /// </summary>
    public void Ativar()
    {
        if (!Ativo)
        {
            Ativo = true;
            AtualizarDataModificacao();
        }
    }

    /// <summary>
    /// Desativa a associação
    /// </summary>
    public void Desativar()
    {
        if (Ativo)
        {
            Ativo = false;
            AtualizarDataModificacao();
        }
    }
}