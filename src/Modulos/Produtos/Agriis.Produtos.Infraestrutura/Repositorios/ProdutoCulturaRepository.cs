using Agriis.Compartilhado.Infraestrutura.Persistencia;
using Agriis.Produtos.Dominio.Entidades;
using Agriis.Produtos.Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Agriis.Produtos.Infraestrutura.Repositorios;

/// <summary>
/// Implementação do repositório de relacionamento produto-cultura
/// </summary>
public class ProdutoCulturaRepository : RepositoryBase<ProdutoCultura, DbContext>, IProdutoCulturaRepository
{
    public ProdutoCulturaRepository(DbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ProdutoCultura>> ObterPorProdutoAsync(int produtoId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(pc => pc.Produto)
            .Where(pc => pc.ProdutoId == produtoId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ProdutoCultura>> ObterPorCulturaAsync(int culturaId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(pc => pc.Produto)
            .Where(pc => pc.CulturaId == culturaId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ProdutoCultura>> ObterAtivosPorProdutoAsync(int produtoId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(pc => pc.Produto)
            .Where(pc => pc.ProdutoId == produtoId && pc.Ativo)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ProdutoCultura>> ObterAtivosPorCulturaAsync(int culturaId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(pc => pc.Produto)
            .Where(pc => pc.CulturaId == culturaId && pc.Ativo)
            .ToListAsync(cancellationToken);
    }

    public async Task<ProdutoCultura?> ObterPorProdutoECulturaAsync(int produtoId, int culturaId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(pc => pc.Produto)
            .FirstOrDefaultAsync(pc => pc.ProdutoId == produtoId && pc.CulturaId == culturaId, cancellationToken);
    }

    public async Task<bool> ExisteRelacionamentoAsync(int produtoId, int culturaId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(pc => pc.ProdutoId == produtoId && pc.CulturaId == culturaId, cancellationToken);
    }

    public async Task RemoverPorProdutoAsync(int produtoId, CancellationToken cancellationToken = default)
    {
        var relacionamentos = await DbSet
            .Where(pc => pc.ProdutoId == produtoId)
            .ToListAsync(cancellationToken);

        if (relacionamentos.Any())
        {
            DbSet.RemoveRange(relacionamentos);
        }
    }

    public async Task RemoverPorCulturaAsync(int culturaId, CancellationToken cancellationToken = default)
    {
        var relacionamentos = await DbSet
            .Where(pc => pc.CulturaId == culturaId)
            .ToListAsync(cancellationToken);

        if (relacionamentos.Any())
        {
            DbSet.RemoveRange(relacionamentos);
        }
    }
}