using Agriis.Compartilhado.Aplicacao.Resultados;
using Agriis.Pedidos.Aplicacao.DTOs;

namespace Agriis.Pedidos.Aplicacao.Interfaces;

/// <summary>
/// Interface do serviço de propostas
/// </summary>
public interface IPropostaService
{
    /// <summary>
    /// Cria uma nova proposta
    /// </summary>
    /// <param name="pedidoId">ID do pedido</param>
    /// <param name="usuarioId">ID do usuário</param>
    /// <param name="clientId">ID do cliente (PRODUTOR_MOBILE ou FORNECEDOR_WEB)</param>
    /// <param name="dto">Dados da proposta</param>
    /// <returns>Resultado da operação</returns>
    Task<Result> CriarPropostaAsync(int pedidoId, int usuarioId, string clientId, CriarPropostaDto dto);
    
    /// <summary>
    /// Lista todas as propostas de um pedido
    /// </summary>
    /// <param name="pedidoId">ID do pedido</param>
    /// <param name="dto">Parâmetros de listagem</param>
    /// <returns>Lista paginada de propostas</returns>
    Task<Result<PagedResult<PropostaDto>>> ListarPropostasAsync(int pedidoId, ListarPropostasDto dto);
    
    /// <summary>
    /// Obtém a última proposta de um pedido
    /// </summary>
    /// <param name="pedidoId">ID do pedido</param>
    /// <returns>Última proposta ou null</returns>
    Task<Result<PropostaDto?>> ObterUltimaPropostaAsync(int pedidoId);
}