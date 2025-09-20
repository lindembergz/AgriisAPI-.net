using Microsoft.EntityFrameworkCore;
using Agriis.Compartilhado.Infraestrutura.Persistencia;
using Agriis.Pedidos.Dominio.Entidades;
using Agriis.Pedidos.Dominio.Interfaces;

namespace Agriis.Pedidos.Infraestrutura.Repositorios;

/// <summary>
/// Implementação do repositório de itens de pedido
/// </summary>
public class PedidoItemRepository : RepositoryBase<PedidoItem, DbContext>, IPedidoItemRepository
{
    public PedidoItemRepository(DbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<PedidoItem>> ObterPorPedidoAsync(int pedidoId)
    {
        return await DbSet
            .Where(pi => pi.PedidoId == pedidoId)
            .OrderBy(pi => pi.DataCriacao)
            .ToListAsync();
    }

    public async Task<IEnumerable<PedidoItem>> ObterPorProdutoAsync(int produtoId)
    {
        return await DbSet
            .Where(pi => pi.ProdutoId == produtoId)
            .OrderByDescending(pi => pi.DataCriacao)
            .ToListAsync();
    }

    public async Task<PedidoItem?> ObterComTransportesAsync(int itemId)
    {
        return await DbSet
            .Include(pi => pi.ItensTransporte)
            .FirstOrDefaultAsync(pi => pi.Id == itemId);
    }

    public async Task<IEnumerable<PedidoItem>> ObterPorFaixaValorAsync(decimal valorMinimo, decimal valorMaximo)
    {
        return await DbSet
            .Where(pi => pi.ValorFinal >= valorMinimo && pi.ValorFinal <= valorMaximo)
            .OrderByDescending(pi => pi.ValorFinal)
            .ToListAsync();
    }
}