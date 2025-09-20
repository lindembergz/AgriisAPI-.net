using Agriis.Compartilhado.Aplicacao.Resultados;
using Agriis.Pagamentos.Aplicacao.DTOs;

namespace Agriis.Pagamentos.Aplicacao.Interfaces;

/// <summary>
/// Interface para serviços de forma de pagamento
/// </summary>
public interface IFormaPagamentoService
{
    /// <summary>
    /// Obtém todas as formas de pagamento ativas
    /// </summary>
    /// <returns>Lista de formas de pagamento ativas</returns>
    Task<Result<IEnumerable<FormaPagamentoDto>>> ObterAtivasAsync();
    
    /// <summary>
    /// Obtém forma de pagamento por ID
    /// </summary>
    /// <param name="id">ID da forma de pagamento</param>
    /// <returns>Forma de pagamento encontrada</returns>
    Task<Result<FormaPagamentoDto>> ObterPorIdAsync(int id);
    
    /// <summary>
    /// Obtém formas de pagamento disponíveis para um pedido
    /// </summary>
    /// <param name="pedidoId">ID do pedido</param>
    /// <returns>Lista de formas de pagamento disponíveis</returns>
    Task<Result<IEnumerable<FormaPagamentoPedidoDto>>> ObterPorPedidoAsync(int pedidoId);
    
    /// <summary>
    /// Cria uma nova forma de pagamento
    /// </summary>
    /// <param name="dto">Dados da forma de pagamento</param>
    /// <returns>Forma de pagamento criada</returns>
    Task<Result<FormaPagamentoDto>> CriarAsync(CriarFormaPagamentoDto dto);
    
    /// <summary>
    /// Atualiza uma forma de pagamento
    /// </summary>
    /// <param name="id">ID da forma de pagamento</param>
    /// <param name="dto">Dados atualizados</param>
    /// <returns>Forma de pagamento atualizada</returns>
    Task<Result<FormaPagamentoDto>> AtualizarAsync(int id, AtualizarFormaPagamentoDto dto);
    
    /// <summary>
    /// Remove uma forma de pagamento
    /// </summary>
    /// <param name="id">ID da forma de pagamento</param>
    /// <returns>Resultado da operação</returns>
    Task<Result> RemoverAsync(int id);
}