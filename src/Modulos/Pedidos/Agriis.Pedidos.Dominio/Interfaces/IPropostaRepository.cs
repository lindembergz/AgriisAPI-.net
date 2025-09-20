using Agriis.Compartilhado.Aplicacao.Resultados;
using Agriis.Compartilhado.Dominio.Interfaces;
using Agriis.Pedidos.Dominio.Entidades;

namespace Agriis.Pedidos.Dominio.Interfaces;

/// <summary>
/// Interface do repositório de propostas
/// </summary>
public interface IPropostaRepository : IRepository<Proposta>
{
    /// <summary>
    /// Obtém a última proposta de um pedido
    /// </summary>
    /// <param name="pedidoId">ID do pedido</param>
    /// <returns>Última proposta ou null se não existir</returns>
    Task<Proposta?> ObterUltimaPorPedidoAsync(int pedidoId);
    
    /// <summary>
    /// Lista todas as propostas de um pedido com paginação
    /// </summary>
    /// <param name="pedidoId">ID do pedido</param>
    /// <param name="pagina">Número da página</param>
    /// <param name="tamanhoPagina">Tamanho da página</param>
    /// <param name="ordenacao">Campo de ordenação</param>
    /// <returns>Lista paginada de propostas</returns>
    Task<PagedResult<Proposta>> ListarPorPedidoAsync(int pedidoId, int pagina, int tamanhoPagina, string? ordenacao = null);
    
    /// <summary>
    /// Verifica se existe alguma proposta para o pedido
    /// </summary>
    /// <param name="pedidoId">ID do pedido</param>
    /// <returns>True se existe proposta</returns>
    Task<bool> ExistePropostaPorPedidoAsync(int pedidoId);
}