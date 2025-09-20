using Microsoft.EntityFrameworkCore;
using Agriis.Compartilhado.Infraestrutura.Persistencia;
using Agriis.Pedidos.Dominio.Entidades;
using Agriis.Pedidos.Dominio.Interfaces;

namespace Agriis.Pedidos.Infraestrutura.Repositorios;

/// <summary>
/// Implementação do repositório de transportes de itens de pedido
/// </summary>
public class PedidoItemTransporteRepository : RepositoryBase<PedidoItemTransporte, DbContext>, IPedidoItemTransporteRepository
{
    public PedidoItemTransporteRepository(DbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<PedidoItemTransporte>> ObterPorPedidoItemAsync(int pedidoItemId)
    {
        return await DbSet
            .Where(pit => pit.PedidoItemId == pedidoItemId)
            .OrderBy(pit => pit.DataAgendamento ?? DateTime.MaxValue)
            .ToListAsync();
    }

    public async Task<IEnumerable<PedidoItemTransporte>> ObterPorDataAgendamentoAsync(DateTime data)
    {
        var dataInicio = data.Date;
        var dataFim = dataInicio.AddDays(1);
        
        return await DbSet
            .Where(pit => pit.DataAgendamento >= dataInicio && pit.DataAgendamento < dataFim)
            .OrderBy(pit => pit.DataAgendamento)
            .ToListAsync();
    }

    public async Task<IEnumerable<PedidoItemTransporte>> ObterPorPeriodoAgendamentoAsync(DateTime dataInicio, DateTime dataFim)
    {
        return await DbSet
            .Where(pit => pit.DataAgendamento >= dataInicio && pit.DataAgendamento <= dataFim)
            .OrderBy(pit => pit.DataAgendamento)
            .ToListAsync();
    }

    public async Task<IEnumerable<PedidoItemTransporte>> ObterSemAgendamentoAsync()
    {
        return await DbSet
            .Where(pit => pit.DataAgendamento == null)
            .OrderByDescending(pit => pit.DataCriacao)
            .ToListAsync();
    }

    public async Task<IEnumerable<PedidoItemTransporte>> ObterPorFaixaValorFreteAsync(decimal valorMinimo, decimal valorMaximo)
    {
        return await DbSet
            .Where(pit => pit.ValorFrete >= valorMinimo && pit.ValorFrete <= valorMaximo)
            .OrderByDescending(pit => pit.ValorFrete)
            .ToListAsync();
    }
}