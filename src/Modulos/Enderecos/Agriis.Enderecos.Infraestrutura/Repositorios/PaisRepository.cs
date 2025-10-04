using Agriis.Compartilhado.Infraestrutura.Persistencia;
using Agriis.Enderecos.Dominio.Entidades;
using Agriis.Enderecos.Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Agriis.Enderecos.Infraestrutura.Repositorios;

/// <summary>
/// Implementação do repositório de Países
/// </summary>
public class PaisRepository : RepositoryBase<Pais, DbContext>, IPaisRepository
{
    public PaisRepository(DbContext context) : base(context)
    {
    }

    public async Task<Pais?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Pais>()
            .FirstOrDefaultAsync(p => p.Codigo == codigo, cancellationToken);
    }

    public async Task<bool> ExisteCodigoAsync(string codigo, int? idExcluir = null, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Pais>().Where(p => p.Codigo == codigo);
        
        if (idExcluir.HasValue)
            query = query.Where(p => p.Id != idExcluir.Value);
            
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> ExisteNomeAsync(string nome, int? idExcluir = null, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Pais>().Where(p => p.Nome == nome);
        
        if (idExcluir.HasValue)
            query = query.Where(p => p.Id != idExcluir.Value);
            
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<IEnumerable<Pais>> ObterAtivosAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Set<Pais>()
            .Where(p => p.Ativo)
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Pais>> BuscarPorNomeAsync(string nome, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Pais>()
            .Where(p => p.Nome.Contains(nome) && p.Ativo)
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Pais>> ObterTodosComEstadosAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Set<Pais>()
            .Include(p => p.Estados)
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Pais>> ObterAtivosComEstadosAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Set<Pais>()
            .Include(p => p.Estados)
            .Where(p => p.Ativo)
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Pais>> ObterTodosAsync(int pagina = 1, int tamanhoPagina = 50, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Pais>()
            .Include(p => p.Estados)
            .OrderBy(p => p.Nome)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistePorCodigoAsync(string codigo, int? idExcluir = null, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Pais>().Where(p => p.Codigo == codigo);
        
        if (idExcluir.HasValue)
            query = query.Where(p => p.Id != idExcluir.Value);
            
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> ExistePorNomeAsync(string nome, int? idExcluir = null, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Pais>().Where(p => p.Nome == nome);
        
        if (idExcluir.HasValue)
            query = query.Where(p => p.Id != idExcluir.Value);
            
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> PossuiEstadosAsync(int paisId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Estado>()
            .AnyAsync(e => e.PaisId == paisId, cancellationToken);
    }
}