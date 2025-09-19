using Agriis.Compartilhado.Infraestrutura.Persistencia;
using Agriis.Propriedades.Dominio.Entidades;
using Agriis.Propriedades.Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Agriis.Propriedades.Infraestrutura.Repositorios;

public class PropriedadeRepository : RepositoryBase<Propriedade, DbContext>, IPropriedadeRepository
{
    public PropriedadeRepository(DbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Propriedade>> ObterPorProdutorAsync(int produtorId)
    {
        return await DbSet
            .Where(p => p.ProdutorId == produtorId)
            .Include(p => p.Talhoes)
            .Include(p => p.PropriedadeCulturas)
            .OrderBy(p => p.Nome)
            .ToListAsync();
    }

    public async Task<IEnumerable<Propriedade>> ObterPorCulturaAsync(int culturaId)
    {
        return await DbSet
            .Where(p => p.PropriedadeCulturas.Any(pc => pc.CulturaId == culturaId))
            .Include(p => p.Talhoes)
            .Include(p => p.PropriedadeCulturas)
            .OrderBy(p => p.Nome)
            .ToListAsync();
    }

    public async Task<IEnumerable<Propriedade>> ObterPorRegiao(Point centro, double raioKm)
    {
        // Converter raio de km para metros
        var raioMetros = raioKm * 1000;

        return await DbSet
            .Where(p => p.Talhoes.Any(t => t.Localizacao != null && t.Localizacao.IsWithinDistance(centro, raioMetros)))
            .Include(p => p.Talhoes)
            .Include(p => p.PropriedadeCulturas)
            .OrderBy(p => p.Nome)
            .ToListAsync();
    }

    public async Task<Propriedade?> ObterComTalhoesAsync(int propriedadeId)
    {
        return await DbSet
            .Include(p => p.Talhoes)
            .FirstOrDefaultAsync(p => p.Id == propriedadeId);
    }

    public async Task<Propriedade?> ObterComCulturasAsync(int propriedadeId)
    {
        return await DbSet
            .Include(p => p.PropriedadeCulturas)
            .FirstOrDefaultAsync(p => p.Id == propriedadeId);
    }

    public async Task<Propriedade?> ObterCompletaAsync(int propriedadeId)
    {
        return await DbSet
            .Include(p => p.Talhoes)
            .Include(p => p.PropriedadeCulturas)
            .FirstOrDefaultAsync(p => p.Id == propriedadeId);
    }

    public async Task<decimal> CalcularAreaTotalPorProdutorAsync(int produtorId)
    {
        var areaTotal = await DbSet
            .Where(p => p.ProdutorId == produtorId)
            .SumAsync(p => p.AreaTotal.Valor);

        return areaTotal;
    }

    public async Task<decimal> CalcularAreaTotalPorCulturaAsync(int culturaId)
    {
        var areaTotal = await Context.Set<PropriedadeCultura>()
            .Where(pc => pc.CulturaId == culturaId)
            .SumAsync(pc => pc.Area.Valor);

        return areaTotal;
    }

    public async Task<IEnumerable<Propriedade>> ObterPropriedadesProximasAsync(Point localizacao, double raioKm)
    {
        // Converter raio de km para metros
        var raioMetros = raioKm * 1000;

        return await DbSet
            .Where(p => p.Talhoes.Any(t => t.Localizacao != null && t.Localizacao.IsWithinDistance(localizacao, raioMetros)))
            .Include(p => p.Talhoes.Where(t => t.Localizacao != null && t.Localizacao.IsWithinDistance(localizacao, raioMetros)))
            .Include(p => p.PropriedadeCulturas)
            .OrderBy(p => p.Talhoes
                .Where(t => t.Localizacao != null)
                .Min(t => t.Localizacao!.Distance(localizacao)))
            .ToListAsync();
    }
}