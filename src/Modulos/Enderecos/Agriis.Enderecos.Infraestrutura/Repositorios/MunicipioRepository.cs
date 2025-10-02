using Agriis.Compartilhado.Infraestrutura.Persistencia;
using Agriis.Enderecos.Dominio.Entidades;
using Agriis.Enderecos.Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Agriis.Enderecos.Infraestrutura.Repositorios;

/// <summary>
/// Implementação do repositório de Municípios
/// </summary>
public class MunicipioRepository : RepositoryBase<Municipio, DbContext>, IMunicipioRepository
{
    public MunicipioRepository(DbContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtém um município pelo código IBGE
    /// </summary>
    public async Task<Municipio?> ObterPorCodigoIbgeAsync(int codigoIbge)
    {
        return await DbSet
            .Include(m => m.Estado)
            .FirstOrDefaultAsync(m => m.CodigoIbge == codigoIbge);
    }

    /// <summary>
    /// Obtém municípios por estado
    /// </summary>
    public async Task<IEnumerable<Municipio>> ObterPorEstadoAsync(int estadoId)
    {
        return await DbSet
            .Where(m => m.EstadoId == estadoId)
            .OrderBy(m => m.Nome)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém municípios por UF
    /// </summary>
    public async Task<IEnumerable<Municipio>> ObterPorUfAsync(string uf)
    {
        return await DbSet
            .Include(m => m.Estado)
            .Where(m => m.Estado.Uf == uf.ToUpperInvariant())
            .OrderBy(m => m.Nome)
            .ToListAsync();
    }

    /// <summary>
    /// Busca municípios por nome (busca parcial)
    /// </summary>
    public async Task<IEnumerable<Municipio>> BuscarPorNomeAsync(string nome, int? estadoId = null)
    {
        var query = DbSet
            .Include(m => m.Estado)
            .Where(m => EF.Functions.ILike(m.Nome, $"%{nome}%"));

        if (estadoId.HasValue)
        {
            query = query.Where(m => m.EstadoId == estadoId.Value);
        }

        return await query
            .OrderBy(m => m.Nome)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém municípios próximos a uma localização
    /// </summary>
    public async Task<IEnumerable<Municipio>> ObterProximosAsync(double latitude, double longitude, double raioKm, int limite = 10)
    {
        var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
        var pontoReferencia = geometryFactory.CreatePoint(new Coordinate(longitude, latitude));
        var raioMetros = raioKm * 1000;

        return await DbSet
            .Include(m => m.Estado)
            .Where(m => m.Localizacao != null && m.Localizacao.Distance(pontoReferencia) <= raioMetros)
            .OrderBy(m => m.Localizacao!.Distance(pontoReferencia))
            .Take(limite)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém municípios que possuem localização definida
    /// </summary>
    public async Task<IEnumerable<Municipio>> ObterComLocalizacaoAsync(int? estadoId = null)
    {
        var query = DbSet
            .Include(m => m.Estado)
            .Where(m => m.Localizacao != null);

        if (estadoId.HasValue)
        {
            query = query.Where(m => m.EstadoId == estadoId.Value);
        }

        return await query
            .OrderBy(m => m.Nome)
            .ToListAsync();
    }

    /// <summary>
    /// Calcula a distância entre dois municípios
    /// </summary>
    public async Task<double?> CalcularDistanciaAsync(int municipioOrigemId, int municipioDestinoId)
    {
        var municipios = await DbSet
            .Where(m => m.Id == municipioOrigemId || m.Id == municipioDestinoId)
            .Where(m => m.Localizacao != null)
            .ToListAsync();

        if (municipios.Count != 2)
            return null;

        var origem = municipios.First(m => m.Id == municipioOrigemId);
        var destino = municipios.First(m => m.Id == municipioDestinoId);

        return origem.CalcularDistanciaKm(destino);
    }

    /// <summary>
    /// Verifica se existe um município com o código IBGE especificado
    /// </summary>
    public async Task<bool> ExistePorCodigoIbgeAsync(int codigoIbge)
    {
        return await DbSet
            .AnyAsync(m => m.CodigoIbge == codigoIbge);
    }

    /// <summary>
    /// Sobrescreve o método base para incluir o estado
    /// </summary>
    public override async Task<Municipio?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(m => m.Estado)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    /// <summary>
    /// Verifica se existe um município com o código IBGE especificado, excluindo um ID específico
    /// </summary>
    public async Task<bool> ExisteCodigoIbgeAsync(int codigoIbge, int? excludeId = null)
    {
        var query = DbSet.Where(m => m.CodigoIbge == codigoIbge);
        
        if (excludeId.HasValue)
        {
            query = query.Where(m => m.Id != excludeId.Value);
        }
        
        return await query.AnyAsync();
    }

    /// <summary>
    /// Obtém municípios com paginação
    /// </summary>
    public async Task<(IEnumerable<Municipio> Items, int TotalCount)> ObterPaginadoAsync(int page, int size, int? ufId = null, string? search = null)
    {
        var query = DbSet.Include(m => m.Estado).AsQueryable();

        // Filtrar por UF se especificado
        if (ufId.HasValue)
        {
            query = query.Where(m => m.EstadoId == ufId.Value);
        }

        // Filtrar por termo de busca se especificado
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(m => EF.Functions.ILike(m.Nome, $"%{search}%"));
        }

        // Contar total de itens
        var totalCount = await query.CountAsync();

        // Aplicar paginação
        var items = await query
            .OrderBy(m => m.Nome)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        return (items, totalCount);
    }

    /// <summary>
    /// Sobrescreve o método base para incluir ordenação por nome
    /// </summary>
    public override async Task<IEnumerable<Municipio>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(m => m.Estado)
            .OrderBy(m => m.Nome)
            .ToListAsync(cancellationToken);
    }
}