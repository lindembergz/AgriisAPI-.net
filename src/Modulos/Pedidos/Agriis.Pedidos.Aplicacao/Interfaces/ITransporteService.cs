using Agriis.Compartilhado.Aplicacao.Resultados;
using Agriis.Pedidos.Aplicacao.DTOs;

namespace Agriis.Pedidos.Aplicacao.Interfaces;

/// <summary>
/// Interface para serviços de transporte
/// </summary>
public interface ITransporteService
{
    /// <summary>
    /// Calcula o frete para um item de pedido
    /// </summary>
    /// <param name="dto">Dados para cálculo do frete</param>
    /// <returns>Resultado do cálculo de frete</returns>
    Task<Result<CalculoFreteDto>> CalcularFreteAsync(CalcularFreteDto dto);

    /// <summary>
    /// Calcula o frete consolidado para múltiplos itens
    /// </summary>
    /// <param name="dto">Dados para cálculo consolidado</param>
    /// <returns>Resultado do cálculo consolidado</returns>
    Task<Result<CalculoFreteConsolidadoDto>> CalcularFreteConsolidadoAsync(CalcularFreteConsolidadoDto dto);

    /// <summary>
    /// Agenda um transporte para um item de pedido
    /// </summary>
    /// <param name="dto">Dados do agendamento</param>
    /// <returns>Transporte agendado</returns>
    Task<Result<PedidoItemTransporteDto>> AgendarTransporteAsync(AgendarTransporteDto dto);

    /// <summary>
    /// Reagenda um transporte existente
    /// </summary>
    /// <param name="transporteId">ID do transporte</param>
    /// <param name="dto">Dados do reagendamento</param>
    /// <returns>Transporte reagendado</returns>
    Task<Result<PedidoItemTransporteDto>> ReagendarTransporteAsync(int transporteId, ReagendarTransporteDto dto);

    /// <summary>
    /// Atualiza o valor do frete de um transporte
    /// </summary>
    /// <param name="transporteId">ID do transporte</param>
    /// <param name="dto">Dados da atualização</param>
    /// <returns>Transporte atualizado</returns>
    Task<Result<PedidoItemTransporteDto>> AtualizarValorFreteAsync(int transporteId, AtualizarValorFreteDto dto);

    /// <summary>
    /// Lista os transportes de um pedido
    /// </summary>
    /// <param name="pedidoId">ID do pedido</param>
    /// <returns>Lista de transportes</returns>
    Task<Result<IEnumerable<PedidoItemTransporteDto>>> ListarTransportesPedidoAsync(int pedidoId);

    /// <summary>
    /// Obtém um transporte por ID
    /// </summary>
    /// <param name="transporteId">ID do transporte</param>
    /// <returns>Dados do transporte</returns>
    Task<Result<PedidoItemTransporteDto>> ObterTransportePorIdAsync(int transporteId);

    /// <summary>
    /// Valida múltiplos agendamentos
    /// </summary>
    /// <param name="dto">Dados dos agendamentos</param>
    /// <returns>Resultado da validação</returns>
    Task<Result<ValidacaoAgendamentoDto>> ValidarMultiplosAgendamentosAsync(ValidarAgendamentosDto dto);

    /// <summary>
    /// Obtém o resumo de transporte de um pedido
    /// </summary>
    /// <param name="pedidoId">ID do pedido</param>
    /// <returns>Resumo do transporte</returns>
    Task<Result<ResumoTransportePedidoDto>> ObterResumoTransporteAsync(int pedidoId);

    /// <summary>
    /// Cancela um transporte agendado
    /// </summary>
    /// <param name="transporteId">ID do transporte</param>
    /// <param name="motivo">Motivo do cancelamento</param>
    /// <returns>Resultado da operação</returns>
    Task<Result<bool>> CancelarTransporteAsync(int transporteId, string? motivo = null);
}