using Microsoft.EntityFrameworkCore;
using Agriis.Compartilhado.Infraestrutura.Persistencia;
using Agriis.Safras.Dominio.Entidades;
using Agriis.Safras.Dominio.Interfaces;

namespace Agriis.Safras.Infraestrutura.Repositorios;

/// <summary>
/// Implementação do repositório de Safras
/// </summary>
public class SafraRepository : RepositoryBase<Safra, DbContext>, ISafraRepository
{
    public SafraRepository(DbContext context) : base(context)
    {
    }

    public async Task<Safra?> ObterSafraAtualAsync()
    {
        var agora = DateTime.Now;
        return await DbSet
            .Where(s => agora >= s.PlantioInicial && agora <= s.PlantioFinal && s.PlantioNome == "S1")
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Safra>> ObterPorAnoColheitaAsync(int anoColheita)
    {
        return await DbSet
            .Where(s => s.AnoColheita == anoColheita)
            .OrderBy(s => s.PlantioInicial)
            .ToListAsync();
    }

    public async Task<IEnumerable<Safra>> ObterTodasOrdenadasAsync()
    {
        return await DbSet
            .OrderBy(s => s.PlantioInicial)
            .ToListAsync();
    }

    public async Task<bool> ExisteConflitoPeriodoAsync(DateTime plantioInicial, DateTime plantioFinal, string plantioNome, int? idExcluir = null)
    {
        var query = DbSet.Where(s => 
            s.PlantioNome == plantioNome &&
            ((plantioInicial >= s.PlantioInicial && plantioInicial <= s.PlantioFinal) ||
             (plantioFinal >= s.PlantioInicial && plantioFinal <= s.PlantioFinal) ||
             (plantioInicial <= s.PlantioInicial && plantioFinal >= s.PlantioFinal)));

        if (idExcluir.HasValue)
        {
            query = query.Where(s => s.Id != idExcluir.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<IEnumerable<Safra>> ObterSafrasAtivasAsync()
    {
        var agora = DateTime.Now;
        return await DbSet
            .Where(s => agora >= s.PlantioInicial && agora <= s.PlantioFinal)
            .OrderBy(s => s.PlantioInicial)
            .ToListAsync();
    }

    public override async Task<IEnumerable<Safra>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        return await ObterTodasOrdenadasAsync();
    }
}