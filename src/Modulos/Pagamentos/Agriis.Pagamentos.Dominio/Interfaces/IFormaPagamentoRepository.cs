using Agriis.Compartilhado.Dominio.Interfaces;
using Agriis.Pagamentos.Dominio.Entidades;

namespace Agriis.Pagamentos.Dominio.Interfaces;

/// <summary>
/// Interface para repositório de formas de pagamento
/// </summary>
public interface IFormaPagamentoRepository : IRepository<FormaPagamento>
{
    /// <summary>
    /// Obtém todas as formas de pagamento ativas ordenadas por descrição
    /// </summary>
    /// <returns>Lista de formas de pagamento ativas</returns>
    Task<IEnumerable<FormaPagamento>> ObterAtivasAsync();
    
    /// <summary>
    /// Obtém formas de pagamento por pedido ID
    /// </summary>
    /// <param name="pedidoId">ID do pedido</param>
    /// <returns>Lista de formas de pagamento disponíveis para o pedido</returns>
    Task<IEnumerable<FormaPagamento>> ObterPorPedidoIdAsync(int pedidoId);
    
    /// <summary>
    /// Verifica se uma forma de pagamento existe e está ativa
    /// </summary>
    /// <param name="id">ID da forma de pagamento</param>
    /// <returns>True se existe e está ativa</returns>
    Task<bool> ExisteAtivaAsync(int id);
}