using Agriis.Compartilhado.Dominio.Interfaces;
using Agriis.Pedidos.Dominio.Entidades;

namespace Agriis.Pedidos.Dominio.Interfaces;

/// <summary>
/// Interface para repositório de itens de pedido
/// </summary>
public interface IPedidoItemRepository : IRepository<PedidoItem>
{
    /// <summary>
    /// Obtém itens por pedido
    /// </summary>
    /// <param name="pedidoId">ID do pedido</param>
    /// <returns>Lista de itens do pedido</returns>
    Task<IEnumerable<PedidoItem>> ObterPorPedidoAsync(int pedidoId);
    
    /// <summary>
    /// Obtém itens por produto
    /// </summary>
    /// <param name="produtoId">ID do produto</param>
    /// <returns>Lista de itens do produto</returns>
    Task<IEnumerable<PedidoItem>> ObterPorProdutoAsync(int produtoId);
    
    /// <summary>
    /// Obtém um item com seus transportes
    /// </summary>
    /// <param name="itemId">ID do item</param>
    /// <returns>Item com transportes</returns>
    Task<PedidoItem?> ObterComTransportesAsync(int itemId);
    
    /// <summary>
    /// Obtém itens por faixa de valor
    /// </summary>
    /// <param name="valorMinimo">Valor mínimo</param>
    /// <param name="valorMaximo">Valor máximo</param>
    /// <returns>Lista de itens na faixa de valor</returns>
    Task<IEnumerable<PedidoItem>> ObterPorFaixaValorAsync(decimal valorMinimo, decimal valorMaximo);
}