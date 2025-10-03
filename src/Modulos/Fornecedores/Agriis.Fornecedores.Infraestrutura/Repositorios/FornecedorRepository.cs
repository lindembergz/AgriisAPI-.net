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
        try
        {
            Console.WriteLine($"🔍 DEBUG - Iniciando consulta paginada. Página: {pagina}, Tamanho: {tamanhoPagina}, Filtro: '{filtro}'");

            // Primeira consulta: buscar os fornecedores com paginação
            var queryBase = Context.Set<Fornecedor>().AsQueryable();

            // Aplicar filtros
            if (!string.IsNullOrWhiteSpace(filtro))
            {
                var filtroLimpo = filtro.Trim().ToLower();
                queryBase = queryBase.Where(p =>
                    p.Nome.ToLower().Contains(filtroLimpo));
                Console.WriteLine($"🔍 DEBUG - Filtro aplicado: '{filtroLimpo}'");
            }

            // Ordenação
            queryBase = queryBase.OrderByDescending(p => p.DataCriacao);

            // Contar total
            Console.WriteLine($"🔍 DEBUG - Contando total de registros...");
            var totalItems = await queryBase.CountAsync();
            Console.WriteLine($"🔍 DEBUG - Total de registros encontrados: {totalItems}");

            if (totalItems == 0)
            {
                Console.WriteLine($"🔍 DEBUG - Nenhum fornecedor encontrado, retornando resultado vazio");
                return new PagedResult<Fornecedor>(new List<Fornecedor>(), pagina, tamanhoPagina, 0);
            }

            // Buscar IDs dos fornecedores da página atual
            Console.WriteLine($"🔍 DEBUG - Buscando IDs da página {pagina}...");
            var fornecedorIds = await queryBase
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .Select(f => f.Id)
                .ToListAsync();

            Console.WriteLine($"🔍 DEBUG - IDs encontrados: [{string.Join(", ", fornecedorIds)}]");

            // Segunda consulta: buscar os fornecedores completos com todos os relacionamentos
            Console.WriteLine($"🔍 DEBUG - Buscando fornecedores completos...");
            var fornecedores = await Context.Set<Fornecedor>()
                .Where(f => fornecedorIds.Contains(f.Id))
                .Include(f => f.Estado)
                .Include(f => f.Municipio)
                .Include(f => f.UsuariosFornecedores)
                    .ThenInclude(uf => uf.Usuario)
                .OrderByDescending(f => f.DataCriacao)
                .ToListAsync();

            Console.WriteLine($"🔍 DEBUG - Fornecedores carregados: {fornecedores.Count}");

            return new PagedResult<Fornecedor>(fornecedores, pagina, tamanhoPagina, totalItems);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERRO - Falha na consulta paginada: {ex.Message}");
            Console.WriteLine($"❌ ERRO - Stack trace: {ex.StackTrace}");
            throw;
        }
    }


    public async Task<Fornecedor?> ObterPorCnpjAsync(string cnpj, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            return null;

        var cnpjObj = new Cnpj(cnpj);

        return await DbSet
            .Include(f => f.Estado)
            .Include(f => f.Municipio)
            .FirstOrDefaultAsync(f => f.Cnpj == cnpjObj, cancellationToken);
    }

    public async Task<IEnumerable<Fornecedor>> ObterAtivosAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(f => f.Estado)
            .Include(f => f.Municipio)
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
            query = query.Where(f => (int)f.Moeda == moedaPadrao.Value);
        }

        return await query
            .Include(f => f.Estado)
            .Include(f => f.Municipio)
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
            .Include(f => f.Estado)
            .Include(f => f.Municipio)
            .Include(f => f.UsuariosFornecedores)
                .ThenInclude(uf => uf.Usuario)
            .Include(f => f.UsuariosFornecedores)
                .ThenInclude(uf => uf.Territorios)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public override async Task<IEnumerable<Fornecedor>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(f => f.Estado)
            .Include(f => f.Municipio)
            .Include(f => f.UsuariosFornecedores)
                .ThenInclude(uf => uf.Usuario)
            .OrderBy(f => f.Nome)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtém dados geográficos para enriquecer os fornecedores
    /// </summary>
    public async Task<Dictionary<string, object>> ObterDadosGeograficosAsync(IEnumerable<int> ufIds, IEnumerable<int> municipioIds, CancellationToken cancellationToken = default)
    {
        var resultado = new Dictionary<string, object>
        {
            ["estados"] = new Dictionary<int, object>(),
            ["municipios"] = new Dictionary<int, object>()
        };

        // Buscar Estados
        if (ufIds.Any())
        {
            var estados = await Context.Set<Agriis.Enderecos.Dominio.Entidades.Estado>()
                .Where(e => ufIds.Contains(e.Id))
                .Select(e => new { e.Id, e.Nome, e.Uf })
                .ToListAsync(cancellationToken);

            var estadosDict = (Dictionary<int, object>)resultado["estados"];
            foreach (var estado in estados)
            {
                estadosDict[estado.Id] = new { estado.Nome, estado.Uf };
            }
        }

        // Buscar Municípios
        if (municipioIds.Any())
        {
            var municipios = await Context.Set<Agriis.Enderecos.Dominio.Entidades.Municipio>()
                .Where(m => municipioIds.Contains(m.Id))
                .Select(m => new { m.Id, m.Nome })
                .ToListAsync(cancellationToken);

            var municipiosDict = (Dictionary<int, object>)resultado["municipios"];
            foreach (var municipio in municipios)
            {
                municipiosDict[municipio.Id] = new { municipio.Nome };
            }
        }

        return resultado;
    }
}