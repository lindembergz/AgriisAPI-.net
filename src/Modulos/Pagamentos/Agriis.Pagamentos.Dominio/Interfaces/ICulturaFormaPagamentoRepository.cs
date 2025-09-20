using Agriis.Compartilhado.Dominio.Interfaces;
using Agriis.Pagamentos.Dominio.Entidades;

namespace Agriis.Pagamentos.Dominio.Interfaces;

/// <summary>
/// Interface para repositório de associações cultura-fornecedor-forma de pagamento
/// </summary>
public interface ICulturaFormaPagamentoRepository : IRepository<CulturaFormaPagamento>
{
    /// <summary>
    /// Busca associação específica por fornecedor, cultura e forma de pagamento
    /// </summary>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <param name="culturaId">ID da cultura</param>
    /// <param name="formaPagamentoId">ID da forma de pagamento</param>
    /// <returns>Associação encontrada ou null</returns>
    Task<CulturaFormaPagamento?> ObterPorFornecedorCulturaFormaPagamentoAsync(
        int fornecedorId, 
        int culturaId, 
        int formaPagamentoId);
    
    /// <summary>
    /// Obtém todas as associações de um fornecedor
    /// </summary>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <returns>Lista de associações do fornecedor</returns>
    Task<IEnumerable<CulturaFormaPagamento>> ObterPorFornecedorAsync(int fornecedorId);
    
    /// <summary>
    /// Obtém todas as associações de uma cultura
    /// </summary>
    /// <param name="culturaId">ID da cultura</param>
    /// <returns>Lista de associações da cultura</returns>
    Task<IEnumerable<CulturaFormaPagamento>> ObterPorCulturaAsync(int culturaId);
    
    /// <summary>
    /// Obtém formas de pagamento disponíveis para um fornecedor e cultura específicos
    /// </summary>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <param name="culturaId">ID da cultura</param>
    /// <returns>Lista de formas de pagamento disponíveis</returns>
    Task<IEnumerable<FormaPagamento>> ObterFormasPagamentoPorFornecedorCulturaAsync(
        int fornecedorId, 
        int culturaId);
    
    /// <summary>
    /// Verifica se existe associação ativa entre fornecedor, cultura e forma de pagamento
    /// </summary>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <param name="culturaId">ID da cultura</param>
    /// <param name="formaPagamentoId">ID da forma de pagamento</param>
    /// <returns>True se existe associação ativa</returns>
    Task<bool> ExisteAssociacaoAtivaAsync(int fornecedorId, int culturaId, int formaPagamentoId);
}