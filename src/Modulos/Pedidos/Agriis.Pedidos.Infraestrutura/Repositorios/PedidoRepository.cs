using Microsoft.EntityFrameworkCore;
using Agriis.Compartilhado.Infraestrutura.Persistencia;
using Agriis.Pedidos.Dominio.Entidades;
using Agriis.Pedidos.Dominio.Interfaces;
using Agriis.Pedidos.Dominio.Enums;

namespace Agriis.Pedidos.Infraestrutura.Repositorios;

/// <summary>
/// Implementação do repositório de pedidos
/// </summary>
public class PedidoRepository : RepositoryBase<Pedido, DbContext>, IPedidoRepository
{
    public PedidoRepository(DbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Pedido>> ObterPorProdutorAsync(int produtorId)
    {
        return await DbSet
            .Where(p => p.ProdutorId == produtorId)
            .OrderByDescending(p => p.DataCriacao)
            .ToListAsync();
    }

    public async Task<IEnumerable<Pedido>> ObterPorFornecedorAsync(int fornecedorId)
    {
        return await DbSet
            .Where(p => p.FornecedorId == fornecedorId)
            .OrderByDescending(p => p.DataCriacao)
            .ToListAsync();
    }

    public async Task<IEnumerable<Pedido>> ObterPorStatusAsync(StatusPedido status)
    {
        return await DbSet
            .Where(p => p.Status == status)
            .OrderByDescending(p => p.DataCriacao)
            .ToListAsync();
    }

    public async Task<IEnumerable<Pedido>> ObterPorProdutorFornecedorAsync(int produtorId, int fornecedorId)
    {
        return await DbSet
            .Where(p => p.ProdutorId == produtorId && p.FornecedorId == fornecedorId)
            .OrderByDescending(p => p.DataCriacao)
            .ToListAsync();
    }

    public async Task<IEnumerable<Pedido>> ObterProximosPrazoLimiteAsync(int diasAntes = 1)
    {
        var dataLimite = DateTime.UtcNow.AddDays(diasAntes);
        
        return await DbSet
            .Where(p => p.Status == StatusPedido.EmNegociacao && 
                       p.DataLimiteInteracao <= dataLimite &&
                       p.DataLimiteInteracao > DateTime.UtcNow)
            .OrderBy(p => p.DataLimiteInteracao)
            .ToListAsync();
    }

    public async Task<IEnumerable<Pedido>> ObterComPrazoUltrapassadoAsync()
    {
        return await DbSet
            .Where(p => p.Status == StatusPedido.EmNegociacao && 
                       p.DataLimiteInteracao < DateTime.UtcNow)
            .OrderBy(p => p.DataLimiteInteracao)
            .ToListAsync();
    }

    public async Task<Pedido?> ObterComItensAsync(int pedidoId)
    {
        return await DbSet
            .Include(p => p.Itens)
                .ThenInclude(i => i.ItensTransporte)
            .FirstOrDefaultAsync(p => p.Id == pedidoId);
    }

    public async Task<Pedido?> ObterComItensETransportesAsync(int pedidoId)
    {
        return await DbSet
            .Include(p => p.Itens)
                .ThenInclude(i => i.ItensTransporte)
            .FirstOrDefaultAsync(p => p.Id == pedidoId);
    }

    public async Task<IEnumerable<Pedido>> ObterPorPeriodoAsync(DateTime dataInicio, DateTime dataFim)
    {
        return await DbSet
            .Where(p => p.DataCriacao >= dataInicio && p.DataCriacao <= dataFim)
            .OrderByDescending(p => p.DataCriacao)
            .ToListAsync();
    }
}