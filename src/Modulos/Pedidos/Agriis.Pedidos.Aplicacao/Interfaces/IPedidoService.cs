using Agriis.Pedidos.Aplicacao.DTOs;
using Agriis.Pedidos.Dominio.Enums;

namespace Agriis.Pedidos.Aplicacao.Interfaces;

/// <summary>
/// Interface para serviços de pedidos
/// </summary>
public interface IPedidoService
{
    /// <summary>
    /// Obtém um pedido por ID
    /// </summary>
    /// <param name="id">ID do pedido</param>
    /// <returns>DTO do pedido</returns>
    Task<PedidoDto?> ObterPorIdAsync(int id);
    
    /// <summary>
    /// Obtém um pedido com todos os seus itens
    /// </summary>
    /// <param name="id">ID do pedido</param>
    /// <returns>DTO do pedido com itens</returns>
    Task<PedidoDto?> ObterComItensAsync(int id);
    
    /// <summary>
    /// Obtém pedidos por produtor
    /// </summary>
    /// <param name="produtorId">ID do produtor</param>
    /// <returns>Lista de DTOs de pedidos</returns>
    Task<IEnumerable<PedidoDto>> ObterPorProdutorAsync(int produtorId);
    
    /// <summary>
    /// Obtém pedidos por fornecedor
    /// </summary>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <returns>Lista de DTOs de pedidos</returns>
    Task<IEnumerable<PedidoDto>> ObterPorFornecedorAsync(int fornecedorId);
    
    /// <summary>
    /// Obtém pedidos por status
    /// </summary>
    /// <param name="status">Status do pedido</param>
    /// <returns>Lista de DTOs de pedidos</returns>
    Task<IEnumerable<PedidoDto>> ObterPorStatusAsync(StatusPedido status);
    
    /// <summary>
    /// Cria um novo pedido
    /// </summary>
    /// <param name="dto">DTO para criação do pedido</param>
    /// <returns>DTO do pedido criado</returns>
    Task<PedidoDto> CriarAsync(CriarPedidoDto dto);
    
    /// <summary>
    /// Atualiza um pedido
    /// </summary>
    /// <param name="id">ID do pedido</param>
    /// <param name="dto">DTO com dados para atualização</param>
    /// <returns>DTO do pedido atualizado</returns>
    Task<PedidoDto?> AtualizarAsync(int id, AtualizarPedidoDto dto);
    
    /// <summary>
    /// Fecha um pedido
    /// </summary>
    /// <param name="id">ID do pedido</param>
    /// <returns>DTO do pedido fechado</returns>
    Task<PedidoDto?> FecharPedidoAsync(int id);
    
    /// <summary>
    /// Cancela um pedido pelo comprador
    /// </summary>
    /// <param name="id">ID do pedido</param>
    /// <returns>DTO do pedido cancelado</returns>
    Task<PedidoDto?> CancelarPorCompradorAsync(int id);
    
    /// <summary>
    /// Obtém pedidos próximos do prazo limite
    /// </summary>
    /// <param name="diasAntes">Quantos dias antes do prazo</param>
    /// <returns>Lista de DTOs de pedidos</returns>
    Task<IEnumerable<PedidoDto>> ObterProximosPrazoLimiteAsync(int diasAntes = 1);
    
    /// <summary>
    /// Obtém pedidos com prazo ultrapassado
    /// </summary>
    /// <returns>Lista de DTOs de pedidos</returns>
    Task<IEnumerable<PedidoDto>> ObterComPrazoUltrapassadoAsync();
    
    /// <summary>
    /// Cancela pedidos com prazo ultrapassado
    /// </summary>
    /// <returns>Quantidade de pedidos cancelados</returns>
    Task<int> CancelarPedidosComPrazoUltrapassadoAsync();
}