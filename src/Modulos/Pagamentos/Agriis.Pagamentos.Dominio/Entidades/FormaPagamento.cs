using Agriis.Compartilhado.Dominio.Entidades;

namespace Agriis.Pagamentos.Dominio.Entidades;

/// <summary>
/// Entidade que representa uma forma de pagamento disponível no sistema
/// </summary>
public class FormaPagamento : EntidadeBase
{
    /// <summary>
    /// Descrição da forma de pagamento
    /// </summary>
    public string Descricao { get; private set; } = string.Empty;
    
    /// <summary>
    /// Indica se a forma de pagamento está ativa
    /// </summary>
    public bool Ativo { get; private set; } = true;
    
    // Navigation Properties
    /// <summary>
    /// Relacionamentos com culturas e fornecedores
    /// </summary>
    public virtual ICollection<CulturaFormaPagamento> CulturaFormasPagamento { get; private set; } = new List<CulturaFormaPagamento>();
    
    /// <summary>
    /// Construtor protegido para Entity Framework
    /// </summary>
    protected FormaPagamento() { }
    
    /// <summary>
    /// Construtor para criar uma nova forma de pagamento
    /// </summary>
    /// <param name="descricao">Descrição da forma de pagamento</param>
    public FormaPagamento(string descricao)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("Descrição da forma de pagamento é obrigatória", nameof(descricao));
            
        Descricao = descricao.Trim();
        Ativo = true;
        CulturaFormasPagamento = new List<CulturaFormaPagamento>();
    }
    
    /// <summary>
    /// Atualiza a descrição da forma de pagamento
    /// </summary>
    /// <param name="descricao">Nova descrição</param>
    public void AtualizarDescricao(string descricao)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("Descrição da forma de pagamento é obrigatória", nameof(descricao));
            
        Descricao = descricao.Trim();
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Ativa a forma de pagamento
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
    /// Desativa a forma de pagamento
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