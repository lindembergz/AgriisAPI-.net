using Microsoft.EntityFrameworkCore;
using Agriis.Compartilhado.Infraestrutura.Persistencia;
using Agriis.Culturas.Dominio.Entidades;
using Agriis.Culturas.Dominio.Interfaces;

namespace Agriis.Culturas.Infraestrutura.Repositorios;

public class CulturaRepository : RepositoryBase<Cultura, DbContext>, ICulturaRepository
{
    public CulturaRepository(DbContext context) : base(context)
    {
    }

    public async Task<Cultura?> ObterPorNomeAsync(string nome)
    {
        return await DbSet
            .FirstOrDefaultAsync(c => c.Nome.ToLower() == nome.ToLower());
    }

    public async Task<IEnumerable<Cultura>> ObterAtivasAsync()
    {
        return await DbSet
            .Where(c => c.Ativo)
            .OrderBy(c => c.Nome)
            .ToListAsync();
    }

    public async Task<IEnumerable<Cultura>> ObterPorNomesAsync(IEnumerable<string> nomes)
    {
        var nomesLower = nomes.Select(n => n.ToLower()).ToList();
        return await DbSet
            .Where(c => nomesLower.Contains(c.Nome.ToLower()))
            .ToListAsync();
    }

    public async Task<bool> ExisteComNomeAsync(string nome, int? idExcluir = null)
    {
        var query = DbSet.Where(c => c.Nome.ToLower() == nome.ToLower());
        
        if (idExcluir.HasValue)
        {
            query = query.Where(c => c.Id != idExcluir.Value);
        }

        return await query.AnyAsync();
    }

    public override async Task<IEnumerable<Cultura>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .OrderBy(c => c.Nome)
            .ToListAsync(cancellationToken);
    }
}