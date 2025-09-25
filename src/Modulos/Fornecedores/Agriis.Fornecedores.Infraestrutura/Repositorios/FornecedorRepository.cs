using Microsoft.EntityFrameworkCore;
using Agriis.Compartilhado.Infraestrutura.Persistencia;
using Agriis.Fornecedores.Dominio.Entidades;
using Agriis.Fornecedores.Dominio.Interfaces;
using Agriis.Compartilhado.Dominio.ObjetosValor;
using Agriis.Compartilhado.Aplicacao.Resultados;

namespace Agriis.Fornecedores.Infraestrutura.Repositorios;

/// <summary>
/// Implementação do repositório de fornecedores
/// </summary>
public class FornecedorRepository : RepositoryBase<Fornecedor, DbContext>, IFornecedorRepository
{
    public FornecedorRepository(DbContext context) : base(context)
    {
    }


    public async Task<PagedResult<Fornecedor>> ObterPaginadoAsync(
    int pagina,
    int tamanhoPagina,
    string? filtro = null)
    {
        var query = Context.Set<Fornecedor>()
            .Include(p => p.UsuariosFornecedores)
            .ThenInclude(up => up.Usuario)
            .AsQueryable();

        // Aplicar filtros
        if (!string.IsNullOrWhiteSpace(filtro))
        {
            var filtroLimpo = filtro.Trim().ToLower();
            query = query.Where(p =>
                p.Nome.ToLower().Contains(filtroLimpo));
            // Nota: Filtros por CPF/CNPJ foram removidos para evitar problemas de tradução LINQ
            // Estes filtros podem ser implementados em memória após a consulta se necessário
        }


        // Ordenação
        query = query.OrderByDescending(p => p.DataCriacao);

        // Paginação
        var totalItems = await query.CountAsync();
        var items = await query
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync();

        return new PagedResult<Fornecedor>(items, pagina, tamanhoPagina, totalItems);
    }


    public async Task<Fornecedor?> ObterPorCnpjAsync(string cnpj, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            return null;

        var cnpjObj = new Cnpj(cnpj);

        return await DbSet
            .FirstOrDefaultAsync(f => f.Cnpj == cnpjObj, cancellationToken);
    }

    public async Task<IEnumerable<Fornecedor>> ObterAtivosAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(f => f.Ativo)
            .OrderBy(f => f.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Fornecedor>> ObterPorTerritorioAsync(string uf, string? municipio = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(uf))
            return Enumerable.Empty<Fornecedor>();

        var query = from f in DbSet
                    join uf_rel in Context.Set<UsuarioFornecedor>() on f.Id equals uf_rel.FornecedorId
                    join t in Context.Set<UsuarioFornecedorTerritorio>() on uf_rel.Id equals t.UsuarioFornecedorId
                    where f.Ativo && uf_rel.Ativo && t.Ativo
                    select new { Fornecedor = f, Territorio = t };

        // Filtrar por estado usando JSON
        query = query.Where(x => EF.Functions.JsonContains(x.Territorio.Estados, $"\"{uf.ToUpper()}\""));

        // Se município foi especificado, filtrar também por município
        if (!string.IsNullOrWhiteSpace(municipio))
        {
            // Esta é uma consulta mais complexa que requer verificação no JSON de municípios
            // Por simplicidade, vamos fazer a filtragem em memória após obter os resultados
            var resultados = await query.Select(x => x.Fornecedor).Distinct().ToListAsync(cancellationToken);
            
            var fornecedoresFiltrados = new List<Fornecedor>();
            foreach (var fornecedor in resultados)
            {
                var territorios = await Context.Set<UsuarioFornecedorTerritorio>()
                    .Where(t => t.UsuarioFornecedor.FornecedorId == fornecedor.Id && t.Ativo)
                    .ToListAsync(cancellationToken);

                if (territorios.Any(t => t.IncluiMunicipio(uf, municipio)))
                {
                    fornecedoresFiltrados.Add(fornecedor);
                }
            }

            return fornecedoresFiltrados.Distinct();
        }

        return await query.Select(x => x.Fornecedor).Distinct().ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Fornecedor>> ObterComFiltrosAsync(
        string? nome = null,
        string? cnpj = null,
        bool? ativo = null,
        int? moedaPadrao = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(nome))
        {
            query = query.Where(f => f.Nome.Contains(nome));
        }

        if (!string.IsNullOrWhiteSpace(cnpj))
        {
            var cnpjObj = new Cnpj(cnpj);
            query = query.Where(f => f.Cnpj == cnpjObj);
        }

        if (ativo.HasValue)
        {
            query = query.Where(f => f.Ativo == ativo.Value);
        }

        if (moedaPadrao.HasValue)
        {
            query = query.Where(f => (int)f.MoedaPadrao == moedaPadrao.Value);
        }

        return await query
            .OrderBy(f => f.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExisteCnpjAsync(string cnpj, int? fornecedorIdExcluir = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            return false;

        var cnpjObj = new Cnpj(cnpj);

        var query = DbSet.Where(f => f.Cnpj == cnpjObj);

        if (fornecedorIdExcluir.HasValue)
        {
            query = query.Where(f => f.Id != fornecedorIdExcluir.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public override async Task<Fornecedor?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(f => f.UsuariosFornecedores)
                .ThenInclude(uf => uf.Usuario)
            .Include(f => f.UsuariosFornecedores)
                .ThenInclude(uf => uf.Territorios)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public override async Task<IEnumerable<Fornecedor>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(f => f.UsuariosFornecedores)
                .ThenInclude(uf => uf.Usuario)
            .OrderBy(f => f.Nome)
            .ToListAsync(cancellationToken);
    }
}