using Agriis.Compartilhado.Infraestrutura.Persistencia;
using Agriis.Propriedades.Dominio.Entidades;
using Agriis.Propriedades.Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Agriis.Propriedades.Infraestrutura.Repositorios;

public class PropriedadeCulturaRepository : RepositoryBase<PropriedadeCultura, DbContext>, IPropriedadeCulturaRepository
{
    public PropriedadeCulturaRepository(DbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<PropriedadeCultura>> ObterPorPropriedadeAsync(int propriedadeId)
    {
        return await DbSet
            .Where(pc => pc.PropriedadeId == propriedadeId)
            .Include(pc => pc.Propriedade)
            .OrderBy(pc => pc.CulturaId)
            .ToListAsync();
    }

    public async Task<IEnumerable<PropriedadeCultura>> ObterPorCulturaAsync(int culturaId)
    {
        return await DbSet
            .Where(pc => pc.CulturaId == culturaId)
            .Include(pc => pc.Propriedade)
            .OrderBy(pc => pc.Propriedade.Nome)
            .ToListAsync();
    }

    public async Task<IEnumerable<PropriedadeCultura>> ObterPorSafraAsync(int safraId)
    {
        return await DbSet
            .Where(pc => pc.SafraId == safraId)
            .Include(pc => pc.Propriedade)
            .OrderBy(pc => pc.Propriedade.Nome)
            .ThenBy(pc => pc.CulturaId)
            .ToListAsync();
    }

    public async Task<PropriedadeCultura?> ObterPorPropriedadeECulturaAsync(int propriedadeId, int culturaId)
    {
        return await DbSet
            .Include(pc => pc.Propriedade)
            .FirstOrDefaultAsync(pc => pc.PropriedadeId == propriedadeId && pc.CulturaId == culturaId);
    }

    public async Task<decimal> CalcularAreaTotalPorCulturaAsync(int culturaId)
    {
        var areaTotal = await DbSet
            .Where(pc => pc.CulturaId == culturaId)
            .SumAsync(pc => pc.Area.Valor);

        return areaTotal;
    }

    public async Task<IEnumerable<PropriedadeCultura>> ObterEmPeriodoPlantioAsync()
    {
        var agora = DateTime.UtcNow;

        return await DbSet
            .Where(pc => pc.DataPlantio != null && 
                        pc.DataPlantio <= agora && 
                        (pc.DataColheitaPrevista == null || pc.DataColheitaPrevista >= agora))
            .Include(pc => pc.Propriedade)
            .OrderBy(pc => pc.DataPlantio)
            .ToListAsync();
    }
}