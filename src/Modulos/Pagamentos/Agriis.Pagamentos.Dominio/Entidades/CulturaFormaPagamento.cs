using Agriis.Compartilhado.Dominio.Entidades;

namespace Agriis.Pagamentos.Dominio.Entidades;

/// <summary>
/// Entidade que representa a associação entre cultura, fornecedor e forma de pagamento
/// </summary>
public class CulturaFormaPagamento : EntidadeBase
{
    /// <summary>
    /// ID do fornecedor
    /// </summary>
    public int FornecedorId { get; private set; }
    
    /// <summary>
    /// ID da cultura
    /// </summary>
    public int CulturaId { get; private set; }
    
    /// <summary>
    /// ID da forma de pagamento
    /// </summary>
    public int FormaPagamentoId { get; private set; }
    
    /// <summary>
    /// Indica se a associação está ativa
    /// </summary>
    public bool Ativo { get; private set; } = true;
    
    // Navigation Properties
    /// <summary>
    /// Forma de pagamento associada
    /// </summary>
    public virtual FormaPagamento FormaPagamento { get; private set; } = null!;
    
    /// <summary>
    /// Construtor protegido para Entity Framework
    /// </summary>
    protected CulturaFormaPagamento() { }
    
    /// <summary>
    /// Construtor para criar uma nova associação cultura-fornecedor-forma de pagamento
    /// </summary>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <param name="culturaId">ID da cultura</param>
    /// <param name="formaPagamentoId">ID da forma de pagamento</param>
    public CulturaFormaPagamento(int fornecedorId, int culturaId, int formaPagamentoId)
    {
        if (fornecedorId <= 0)
            throw new ArgumentException("ID do fornecedor deve ser maior que zero", nameof(fornecedorId));
            
        if (culturaId <= 0)
            throw new ArgumentException("ID da cultura deve ser maior que zero", nameof(culturaId));
            
        if (formaPagamentoId <= 0)
            throw new ArgumentException("ID da forma de pagamento deve ser maior que zero", nameof(formaPagamentoId));
            
        FornecedorId = fornecedorId;
        CulturaId = culturaId;
        FormaPagamentoId = formaPagamentoId;
        Ativo = true;
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