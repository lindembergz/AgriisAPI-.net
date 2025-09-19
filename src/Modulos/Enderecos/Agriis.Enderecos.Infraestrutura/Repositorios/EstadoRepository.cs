using Agriis.Compartilhado.Infraestrutura.Persistencia;
using Agriis.Enderecos.Dominio.Entidades;
using Agriis.Enderecos.Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Agriis.Enderecos.Infraestrutura.Repositorios;

/// <summary>
/// Implementação do repositório de Estados
/// </summary>
public class EstadoRepository : RepositoryBase<Estado, DbContext>, IEstadoRepository
{
    public EstadoRepository(DbContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtém um estado pela sigla (UF)
    /// </summary>
    public async Task<Estado?> ObterPorUfAsync(string uf)
    {
        return await DbSet
            .FirstOrDefaultAsync(e => e.Uf == uf.ToUpperInvariant());
    }

    /// <summary>
    /// Obtém um estado pelo código IBGE
    /// </summary>
    public async Task<Estado?> ObterPorCodigoIbgeAsync(int codigoIbge)
    {
        return await DbSet
            .FirstOrDefaultAsync(e => e.CodigoIbge == codigoIbge);
    }

    /// <summary>
    /// Obtém estados por região
    /// </summary>
    public async Task<IEnumerable<Estado>> ObterPorRegiaoAsync(string regiao)
    {
        return await DbSet
            .Where(e => e.Regiao == regiao)
            .OrderBy(e => e.Nome)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém todos os estados com seus municípios
    /// </summary>
    public async Task<IEnumerable<Estado>> ObterTodosComMunicipiosAsync()
    {
        return await DbSet
            .Include(e => e.Municipios)
            .OrderBy(e => e.Nome)
            .ToListAsync();
    }

    /// <summary>
    /// Verifica se existe um estado com a UF especificada
    /// </summary>
    public async Task<bool> ExistePorUfAsync(string uf)
    {
        return await DbSet
            .AnyAsync(e => e.Uf == uf.ToUpperInvariant());
    }

    /// <summary>
    /// Verifica se existe um estado com o código IBGE especificado
    /// </summary>
    public async Task<bool> ExistePorCodigoIbgeAsync(int codigoIbge)
    {
        return await DbSet
            .AnyAsync(e => e.CodigoIbge == codigoIbge);
    }

    /// <summary>
    /// Sobrescreve o método base para incluir ordenação por nome
    /// </summary>
    public override async Task<IEnumerable<Estado>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .OrderBy(e => e.Nome)
            .ToListAsync(cancellationToken);
    }
}