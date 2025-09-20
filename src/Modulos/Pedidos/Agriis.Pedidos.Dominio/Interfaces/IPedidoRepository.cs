using Agriis.Compartilhado.Dominio.Interfaces;
using Agriis.Pedidos.Dominio.Entidades;
using Agriis.Pedidos.Dominio.Enums;

namespace Agriis.Pedidos.Dominio.Interfaces;

/// <summary>
/// Interface para repositório de pedidos
/// </summary>
public interface IPedidoRepository : IRepository<Pedido>
{
    /// <summary>
    /// Obtém pedidos por produtor
    /// </summary>
    /// <param name="produtorId">ID do produtor</param>
    /// <returns>Lista de pedidos do produtor</returns>
    Task<IEnumerable<Pedido>> ObterPorProdutorAsync(int produtorId);
    
    /// <summary>
    /// Obtém pedidos por fornecedor
    /// </summary>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <returns>Lista de pedidos do fornecedor</returns>
    Task<IEnumerable<Pedido>> ObterPorFornecedorAsync(int fornecedorId);
    
    /// <summary>
    /// Obtém pedidos por status
    /// </summary>
    /// <param name="status">Status do pedido</param>
    /// <returns>Lista de pedidos com o status especificado</returns>
    Task<IEnumerable<Pedido>> ObterPorStatusAsync(StatusPedido status);
    
    /// <summary>
    /// Obtém pedidos por produtor e fornecedor
    /// </summary>
    /// <param name="produtorId">ID do produtor</param>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <returns>Lista de pedidos entre o produtor e fornecedor</returns>
    Task<IEnumerable<Pedido>> ObterPorProdutorFornecedorAsync(int produtorId, int fornecedorId);
    
    /// <summary>
    /// Obtém pedidos que estão próximos do prazo limite
    /// </summary>
    /// <param name="diasAntes">Quantos dias antes do prazo limite</param>
    /// <returns>Lista de pedidos próximos do prazo</returns>
    Task<IEnumerable<Pedido>> ObterProximosPrazoLimiteAsync(int diasAntes = 1);
    
    /// <summary>
    /// Obtém pedidos que ultrapassaram o prazo limite
    /// </summary>
    /// <returns>Lista de pedidos com prazo ultrapassado</returns>
    Task<IEnumerable<Pedido>> ObterComPrazoUltrapassadoAsync();
    
    /// <summary>
    /// Obtém um pedido com todos os seus itens e transportes
    /// </summary>
    /// <param name="pedidoId">ID do pedido</param>
    /// <returns>Pedido completo com itens e transportes</returns>
    Task<Pedido?> ObterComItensAsync(int pedidoId);
    
    /// <summary>
    /// Obtém um pedido com todos os seus itens e transportes
    /// </summary>
    /// <param name="pedidoId">ID do pedido</param>
    /// <returns>Pedido completo com itens e transportes</returns>
    Task<Pedido?> ObterComItensETransportesAsync(int pedidoId);
    
    /// <summary>
    /// Obtém pedidos por período
    /// </summary>
    /// <param name="dataInicio">Data de início</param>
    /// <param name="dataFim">Data de fim</param>
    /// <returns>Lista de pedidos no período</returns>
    Task<IEnumerable<Pedido>> ObterPorPeriodoAsync(DateTime dataInicio, DateTime dataFim);
}