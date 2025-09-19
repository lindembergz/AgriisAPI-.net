using Agriis.Compartilhado.Infraestrutura.Persistencia;
using Agriis.Propriedades.Dominio.Entidades;
using Agriis.Propriedades.Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Agriis.Propriedades.Infraestrutura.Repositorios;

public class TalhaoRepository : RepositoryBase<Talhao, DbContext>, ITalhaoRepository
{
    public TalhaoRepository(DbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Talhao>> ObterPorPropriedadeAsync(int propriedadeId)
    {
        return await DbSet
            .Where(t => t.PropriedadeId == propriedadeId)
            .Include(t => t.Propriedade)
            .OrderBy(t => t.Nome)
            .ToListAsync();
    }

    public async Task<IEnumerable<Talhao>> ObterPorRegiao(Point centro, double raioKm)
    {
        // Converter raio de km para metros
        var raioMetros = raioKm * 1000;

        return await DbSet
            .Where(t => t.Localizacao != null && t.Localizacao.IsWithinDistance(centro, raioMetros))
            .Include(t => t.Propriedade)
            .OrderBy(t => t.Localizacao!.Distance(centro))
            .ToListAsync();
    }

    public async Task<decimal> CalcularAreaTotalPorPropriedadeAsync(int propriedadeId)
    {
        var areaTotal = await DbSet
            .Where(t => t.PropriedadeId == propriedadeId)
            .SumAsync(t => t.Area.Valor);

        return areaTotal;
    }

    public async Task<IEnumerable<Talhao>> ObterTalhoesProximosAsync(Point localizacao, double raioKm)
    {
        // Converter raio de km para metros
        var raioMetros = raioKm * 1000;

        return await DbSet
            .Where(t => t.Localizacao != null && t.Localizacao.IsWithinDistance(localizacao, raioMetros))
            .Include(t => t.Propriedade)
            .OrderBy(t => t.Localizacao!.Distance(localizacao))
            .ToListAsync();
    }
}