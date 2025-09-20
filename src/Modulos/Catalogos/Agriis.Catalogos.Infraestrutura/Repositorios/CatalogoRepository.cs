using Agriis.Catalogos.Dominio.Entidades;
using Agriis.Catalogos.Dominio.Interfaces;
using Agriis.Compartilhado.Aplicacao.Resultados;
using Agriis.Compartilhado.Dominio.Enums;
using Agriis.Compartilhado.Infraestrutura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Agriis.Catalogos.Infraestrutura.Repositorios;

public class CatalogoRepository : RepositoryBase<Catalogo, DbContext>, ICatalogoRepository
{
    private readonly DbSet<Catalogo> _dbSet;

    public CatalogoRepository(DbContext context) : base(context)
    {
        _dbSet = context.Set<Catalogo>();
    }

    public async Task<Catalogo?> ObterPorChaveUnicaAsync(int safraId, int pontoDistribuicaoId, int culturaId, int categoriaId)
    {
        return await _dbSet
            .Include(c => c.Itens)
            .FirstOrDefaultAsync(c => 
                c.SafraId == safraId && 
                c.PontoDistribuicaoId == pontoDistribuicaoId && 
                c.CulturaId == culturaId && 
                c.CategoriaId == categoriaId);
    }

    public async Task<IEnumerable<Catalogo>> ObterPorSafraAsync(int safraId)
    {
        return await _dbSet
            .Include(c => c.Itens)
            .Where(c => c.SafraId == safraId)
            .OrderBy(c => c.DataCriacao)
            .ToListAsync();
    }

    public async Task<IEnumerable<Catalogo>> ObterPorPontoDistribuicaoAsync(int pontoDistribuicaoId)
    {
        return await _dbSet
            .Include(c => c.Itens)
            .Where(c => c.PontoDistribuicaoId == pontoDistribuicaoId)
            .OrderBy(c => c.DataCriacao)
            .ToListAsync();
    }

    public async Task<IEnumerable<Catalogo>> ObterPorCulturaAsync(int culturaId)
    {
        return await _dbSet
            .Include(c => c.Itens)
            .Where(c => c.CulturaId == culturaId)
            .OrderBy(c => c.DataCriacao)
            .ToListAsync();
    }

    public async Task<IEnumerable<Catalogo>> ObterVigentesAsync(DateTime data)
    {
        return await _dbSet
            .Include(c => c.Itens)
            .Where(c => c.Ativo && 
                       c.DataInicio <= data && 
                       (c.DataFim == null || c.DataFim >= data))
            .OrderBy(c => c.DataCriacao)
            .ToListAsync();
    }

    public async Task<PagedResult<Catalogo>> ObterPaginadoAsync(int pagina, int tamanhoPagina, 
        int? safraId = null, int? pontoDistribuicaoId = null, int? culturaId = null, 
        int? categoriaId = null, Moeda? moeda = null, bool? ativo = null)
    {
        var query = _dbSet.Include(c => c.Itens).AsQueryable();

        if (safraId.HasValue)
            query = query.Where(c => c.SafraId == safraId.Value);

        if (pontoDistribuicaoId.HasValue)
            query = query.Where(c => c.PontoDistribuicaoId == pontoDistribuicaoId.Value);

        if (culturaId.HasValue)
            query = query.Where(c => c.CulturaId == culturaId.Value);

        if (categoriaId.HasValue)
            query = query.Where(c => c.CategoriaId == categoriaId.Value);

        if (moeda.HasValue)
            query = query.Where(c => c.Moeda == moeda.Value);

        if (ativo.HasValue)
            query = query.Where(c => c.Ativo == ativo.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(c => c.DataCriacao)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync();

        return new PagedResult<Catalogo>(items, totalCount, pagina, tamanhoPagina);
    }

    public async Task<bool> ExisteChaveUnicaAsync(int safraId, int pontoDistribuicaoId, int culturaId, int categoriaId, int? catalogoIdExcluir = null)
    {
        var query = _dbSet.Where(c => 
            c.SafraId == safraId && 
            c.PontoDistribuicaoId == pontoDistribuicaoId && 
            c.CulturaId == culturaId && 
            c.CategoriaId == categoriaId);

        if (catalogoIdExcluir.HasValue)
            query = query.Where(c => c.Id != catalogoIdExcluir.Value);

        return await query.AnyAsync();
    }

    public override async Task<Catalogo?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Itens)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }
}