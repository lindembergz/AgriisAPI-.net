using Microsoft.EntityFrameworkCore;
using Agriis.Compartilhado.Aplicacao.Resultados;
using Agriis.Compartilhado.Infraestrutura.Persistencia;
using Agriis.Produtores.Dominio.Entidades;
using Agriis.Produtores.Dominio.Enums;
using Agriis.Produtores.Dominio.Interfaces;

namespace Agriis.Produtores.Infraestrutura.Repositorios;

/// <summary>
/// Implementação do repositório de produtores
/// </summary>
public class ProdutorRepository : RepositoryBase<Produtor, DbContext>, IProdutorRepository
{
    public ProdutorRepository(DbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<Produtor?> ObterPorCpfAsync(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return null;

        var cpfLimpo = cpf.Replace(".", "").Replace("-", "");
        
        return await Context.Set<Produtor>()
            .Include(p => p.UsuariosProdutores)
            .ThenInclude(up => up.Usuario)
            .FirstOrDefaultAsync(p => p.Cpf != null && p.Cpf.Valor == cpfLimpo);
    }

    /// <inheritdoc />
    public async Task<Produtor?> ObterPorCnpjAsync(string cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            return null;

        var cnpjLimpo = cnpj.Replace(".", "").Replace("/", "").Replace("-", "");
        
        return await Context.Set<Produtor>()
            .Include(p => p.UsuariosProdutores)
            .ThenInclude(up => up.Usuario)
            .FirstOrDefaultAsync(p => p.Cnpj != null && p.Cnpj.Valor == cnpjLimpo);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Produtor>> ObterPorFornecedorAsync(int fornecedorId)
    {
        // Esta implementação seria mais complexa, envolvendo consultas de território
        // Por enquanto, retorna todos os produtores autorizados
        return await Context.Set<Produtor>()
            .Include(p => p.UsuariosProdutores)
            .ThenInclude(up => up.Usuario)
            .Where(p => p.Status == StatusProdutor.AutorizadoAutomaticamente || 
                       p.Status == StatusProdutor.AutorizadoManualmente)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<PagedResult<Produtor>> ObterPaginadoAsync(
        int pagina, 
        int tamanhoPagina, 
        string? filtro = null,
        StatusProdutor? status = null,
        int? culturaId = null)
    {
        var query = Context.Set<Produtor>()
            .Include(p => p.UsuariosProdutores)
            .ThenInclude(up => up.Usuario)
            .AsQueryable();

        // Aplicar filtros
        if (!string.IsNullOrWhiteSpace(filtro))
        {
            var filtroLimpo = filtro.Trim().ToLower();
            query = query.Where(p => 
                p.Nome.ToLower().Contains(filtroLimpo) ||
                (p.Cpf != null && p.Cpf.Valor.Contains(filtroLimpo)) ||
                (p.Cnpj != null && p.Cnpj.Valor.Contains(filtroLimpo)));
        }

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        if (culturaId.HasValue)
        {
            query = query.Where(p => p.Culturas.Contains(culturaId.Value));
        }

        // Ordenação
        query = query.OrderByDescending(p => p.DataCriacao);

        // Paginação
        var totalItems = await query.CountAsync();
        var items = await query
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync();

        return new PagedResult<Produtor>(items, pagina, tamanhoPagina, totalItems);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Produtor>> ObterPorStatusAsync(StatusProdutor status)
    {
        return await Context.Set<Produtor>()
            .Include(p => p.UsuariosProdutores)
            .ThenInclude(up => up.Usuario)
            .Where(p => p.Status == status)
            .OrderByDescending(p => p.DataCriacao)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Produtor>> ObterPorCulturaAsync(int culturaId)
    {
        return await Context.Set<Produtor>()
            .Include(p => p.UsuariosProdutores)
            .ThenInclude(up => up.Usuario)
            .Where(p => p.Culturas.Contains(culturaId))
            .OrderBy(p => p.Nome)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Produtor>> ObterPorFaixaAreaAsync(decimal areaMinima, decimal areaMaxima)
    {
        return await Context.Set<Produtor>()
            .Include(p => p.UsuariosProdutores)
            .ThenInclude(up => up.Usuario)
            .Where(p => p.AreaPlantio.Valor >= areaMinima && p.AreaPlantio.Valor <= areaMaxima)
            .OrderByDescending(p => p.AreaPlantio.Valor)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<bool> ExistePorCpfAsync(string cpf, int? produtorIdExcluir = null)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return false;

        var cpfLimpo = cpf.Replace(".", "").Replace("-", "");
        
        var query = Context.Set<Produtor>()
            .Where(p => p.Cpf != null && p.Cpf.Valor == cpfLimpo);

        if (produtorIdExcluir.HasValue)
        {
            query = query.Where(p => p.Id != produtorIdExcluir.Value);
        }

        return await query.AnyAsync();
    }

    /// <inheritdoc />
    public async Task<bool> ExistePorCnpjAsync(string cnpj, int? produtorIdExcluir = null)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            return false;

        var cnpjLimpo = cnpj.Replace(".", "").Replace("/", "").Replace("-", "");
        
        var query = Context.Set<Produtor>()
            .Where(p => p.Cnpj != null && p.Cnpj.Valor == cnpjLimpo);

        if (produtorIdExcluir.HasValue)
        {
            query = query.Where(p => p.Id != produtorIdExcluir.Value);
        }

        return await query.AnyAsync();
    }

    /// <inheritdoc />
    public async Task<ProdutorEstatisticas> ObterEstatisticasAsync()
    {
        var produtores = await Context.Set<Produtor>().ToListAsync();

        var totalProdutores = produtores.Count;
        var produtoresAutorizados = produtores.Count(p => 
            p.Status == StatusProdutor.AutorizadoAutomaticamente || 
            p.Status == StatusProdutor.AutorizadoManualmente);
        var produtoresPendentes = produtores.Count(p => 
            p.Status == StatusProdutor.PendenteValidacaoAutomatica || 
            p.Status == StatusProdutor.PendenteValidacaoManual ||
            p.Status == StatusProdutor.PendenteCnpj);
        var produtoresNegados = produtores.Count(p => p.Status == StatusProdutor.Negado);

        var areaTotalPlantio = produtores.Sum(p => p.AreaPlantio.Valor);
        var areaMediaPlantio = totalProdutores > 0 ? areaTotalPlantio / totalProdutores : 0;

        return new ProdutorEstatisticas
        {
            TotalProdutores = totalProdutores,
            ProdutoresAutorizados = produtoresAutorizados,
            ProdutoresPendentes = produtoresPendentes,
            ProdutoresNegados = produtoresNegados,
            AreaTotalPlantio = areaTotalPlantio,
            AreaMediaPlantio = areaMediaPlantio
        };
    }

    /// <inheritdoc />
    public override async Task<Produtor?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Produtor>()
            .Include(p => p.UsuariosProdutores)
            .ThenInclude(up => up.Usuario)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public override async Task<IEnumerable<Produtor>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Set<Produtor>()
            .Include(p => p.UsuariosProdutores)
            .ThenInclude(up => up.Usuario)
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }
}