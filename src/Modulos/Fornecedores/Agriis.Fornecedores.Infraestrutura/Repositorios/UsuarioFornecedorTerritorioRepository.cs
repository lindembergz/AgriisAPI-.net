using Microsoft.EntityFrameworkCore;
using Agriis.Compartilhado.Infraestrutura.Persistencia;
using Agriis.Fornecedores.Dominio.Entidades;
using Agriis.Fornecedores.Dominio.Interfaces;

namespace Agriis.Fornecedores.Infraestrutura.Repositorios;

/// <summary>
/// Implementação do repositório de territórios de usuários fornecedores
/// </summary>
public class UsuarioFornecedorTerritorioRepository : RepositoryBase<UsuarioFornecedorTerritorio, DbContext>, IUsuarioFornecedorTerritorioRepository
{
    public UsuarioFornecedorTerritorioRepository(DbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<UsuarioFornecedorTerritorio>> ObterPorUsuarioFornecedorAsync(int usuarioFornecedorId, bool apenasAtivos = true, CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(t => t.UsuarioFornecedor)
                .ThenInclude(uf => uf.Usuario)
            .Include(t => t.UsuarioFornecedor)
                .ThenInclude(uf => uf.Fornecedor)
            .Where(t => t.UsuarioFornecedorId == usuarioFornecedorId);

        if (apenasAtivos)
        {
            query = query.Where(t => t.Ativo);
        }

        return await query
            .OrderByDescending(t => t.TerritorioPadrao)
            .ThenBy(t => t.DataCriacao)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UsuarioFornecedorTerritorio>> ObterPorEstadoAsync(string uf, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(uf))
            return Enumerable.Empty<UsuarioFornecedorTerritorio>();

        return await DbSet
            .Include(t => t.UsuarioFornecedor)
                .ThenInclude(uf => uf.Usuario)
            .Include(t => t.UsuarioFornecedor)
                .ThenInclude(uf => uf.Fornecedor)
            .Where(t => t.Ativo && EF.Functions.JsonContains(t.Estados, $"\"{uf.ToUpper()}\""))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UsuarioFornecedorTerritorio>> ObterPorMunicipioAsync(string uf, string municipio, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(uf) || string.IsNullOrWhiteSpace(municipio))
            return Enumerable.Empty<UsuarioFornecedorTerritorio>();

        // Primeiro, obtém territórios que incluem o estado
        var territoriosEstado = await ObterPorEstadoAsync(uf, cancellationToken);

        // Filtra em memória aqueles que incluem o município específico
        var territoriosFiltrados = new List<UsuarioFornecedorTerritorio>();
        foreach (var territorio in territoriosEstado)
        {
            if (territorio.IncluiMunicipio(uf, municipio))
            {
                territoriosFiltrados.Add(territorio);
            }
        }

        return territoriosFiltrados;
    }

    public async Task<UsuarioFornecedorTerritorio?> ObterTerritorioPadraoAsync(int usuarioFornecedorId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(t => t.UsuarioFornecedor)
                .ThenInclude(uf => uf.Usuario)
            .Include(t => t.UsuarioFornecedor)
                .ThenInclude(uf => uf.Fornecedor)
            .FirstOrDefaultAsync(t => t.UsuarioFornecedorId == usuarioFornecedorId && t.TerritorioPadrao && t.Ativo, cancellationToken);
    }

    public async Task<IEnumerable<UsuarioFornecedor>> ObterUsuariosFornecedoresPorTerritorioAsync(string uf, string? municipio = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(uf))
            return Enumerable.Empty<UsuarioFornecedor>();

        var query = from t in DbSet
                    join uf_rel in Context.Set<UsuarioFornecedor>() on t.UsuarioFornecedorId equals uf_rel.Id
                    where t.Ativo && uf_rel.Ativo && EF.Functions.JsonContains(t.Estados, $"\"{uf.ToUpper()}\"")
                    select new { Territorio = t, UsuarioFornecedor = uf_rel };

        var resultados = await query
            .Include(x => x.UsuarioFornecedor.Usuario)
            .Include(x => x.UsuarioFornecedor.Fornecedor)
            .ToListAsync(cancellationToken);

        // Se município foi especificado, filtrar também por município
        if (!string.IsNullOrWhiteSpace(municipio))
        {
            var usuariosFornecedoresFiltrados = new List<UsuarioFornecedor>();
            foreach (var resultado in resultados)
            {
                if (resultado.Territorio.IncluiMunicipio(uf, municipio))
                {
                    usuariosFornecedoresFiltrados.Add(resultado.UsuarioFornecedor);
                }
            }

            return usuariosFornecedoresFiltrados.Distinct();
        }

        return resultados.Select(x => x.UsuarioFornecedor).Distinct();
    }

    public override async Task<UsuarioFornecedorTerritorio?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(t => t.UsuarioFornecedor)
                .ThenInclude(uf => uf.Usuario)
            .Include(t => t.UsuarioFornecedor)
                .ThenInclude(uf => uf.Fornecedor)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }
}