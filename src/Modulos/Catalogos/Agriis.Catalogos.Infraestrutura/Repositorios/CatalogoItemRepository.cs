using Agriis.Catalogos.Dominio.Entidades;
using Agriis.Catalogos.Dominio.Interfaces;
using Agriis.Compartilhado.Aplicacao.Resultados;
using Agriis.Compartilhado.Infraestrutura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Agriis.Catalogos.Infraestrutura.Repositorios;

public class CatalogoItemRepository : RepositoryBase<CatalogoItem, DbContext>, ICatalogoItemRepository
{
    private readonly DbSet<CatalogoItem> _dbSet;

    public CatalogoItemRepository(DbContext context) : base(context)
    {
        _dbSet = context.Set<CatalogoItem>();
    }

    public async Task<IEnumerable<CatalogoItem>> ObterPorCatalogoAsync(int catalogoId)
    {
        return await _dbSet
            .Where(ci => ci.CatalogoId == catalogoId)
            .OrderBy(ci => ci.DataCriacao)
            .ToListAsync();
    }

    public async Task<CatalogoItem?> ObterPorCatalogoEProdutoAsync(int catalogoId, int produtoId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(ci => ci.CatalogoId == catalogoId && ci.ProdutoId == produtoId);
    }

    public async Task<IEnumerable<CatalogoItem>> ObterPorProdutoAsync(int produtoId)
    {
        return await _dbSet
            .Include(ci => ci.Catalogo)
            .Where(ci => ci.ProdutoId == produtoId)
            .OrderBy(ci => ci.DataCriacao)
            .ToListAsync();
    }

    public async Task<PagedResult<CatalogoItem>> ObterPaginadoAsync(int pagina, int tamanhoPagina, 
        int? catalogoId = null, int? produtoId = null, bool? ativo = null)
    {
        var query = _dbSet.AsQueryable();

        if (catalogoId.HasValue)
            query = query.Where(ci => ci.CatalogoId == catalogoId.Value);

        if (produtoId.HasValue)
            query = query.Where(ci => ci.ProdutoId == produtoId.Value);

        if (ativo.HasValue)
            query = query.Where(ci => ci.Ativo == ativo.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(ci => ci.DataCriacao)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync();

        return new PagedResult<CatalogoItem>(items, totalCount, pagina, tamanhoPagina);
    }
}