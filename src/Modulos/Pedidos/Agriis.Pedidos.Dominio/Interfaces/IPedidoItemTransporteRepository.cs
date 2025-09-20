using Agriis.Compartilhado.Dominio.Interfaces;
using Agriis.Pedidos.Dominio.Entidades;

namespace Agriis.Pedidos.Dominio.Interfaces;

/// <summary>
/// Interface para repositório de transportes de itens de pedido
/// </summary>
public interface IPedidoItemTransporteRepository : IRepository<PedidoItemTransporte>
{
    /// <summary>
    /// Obtém transportes por item de pedido
    /// </summary>
    /// <param name="pedidoItemId">ID do item de pedido</param>
    /// <returns>Lista de transportes do item</returns>
    Task<IEnumerable<PedidoItemTransporte>> ObterPorPedidoItemAsync(int pedidoItemId);
    
    /// <summary>
    /// Obtém transportes agendados para uma data específica
    /// </summary>
    /// <param name="data">Data do agendamento</param>
    /// <returns>Lista de transportes agendados</returns>
    Task<IEnumerable<PedidoItemTransporte>> ObterPorDataAgendamentoAsync(DateTime data);
    
    /// <summary>
    /// Obtém transportes por período de agendamento
    /// </summary>
    /// <param name="dataInicio">Data de início</param>
    /// <param name="dataFim">Data de fim</param>
    /// <returns>Lista de transportes no período</returns>
    Task<IEnumerable<PedidoItemTransporte>> ObterPorPeriodoAgendamentoAsync(DateTime dataInicio, DateTime dataFim);
    
    /// <summary>
    /// Obtém transportes sem agendamento
    /// </summary>
    /// <returns>Lista de transportes sem data agendada</returns>
    Task<IEnumerable<PedidoItemTransporte>> ObterSemAgendamentoAsync();
    
    /// <summary>
    /// Obtém transportes por faixa de valor de frete
    /// </summary>
    /// <param name="valorMinimo">Valor mínimo do frete</param>
    /// <param name="valorMaximo">Valor máximo do frete</param>
    /// <returns>Lista de transportes na faixa de valor</returns>
    Task<IEnumerable<PedidoItemTransporte>> ObterPorFaixaValorFreteAsync(decimal valorMinimo, decimal valorMaximo);
}