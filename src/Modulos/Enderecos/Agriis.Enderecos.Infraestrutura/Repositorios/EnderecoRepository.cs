using Agriis.Compartilhado.Infraestrutura.Persistencia;
using Agriis.Enderecos.Dominio.Entidades;
using Agriis.Enderecos.Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Agriis.Enderecos.Infraestrutura.Repositorios;

/// <summary>
/// Implementação do repositório de Endereços
/// </summary>
public class EnderecoRepository : RepositoryBase<Endereco, DbContext>, IEnderecoRepository
{
    public EnderecoRepository(DbContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtém endereços por CEP
    /// </summary>
    public async Task<IEnumerable<Endereco>> ObterPorCepAsync(string cep)
    {
        var cepLimpo = LimparCep(cep);
        
        return await DbSet
            .Include(e => e.Municipio)
            .Include(e => e.Estado)
            .Where(e => e.Cep == cepLimpo)
            .OrderBy(e => e.Logradouro)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém endereços por município
    /// </summary>
    public async Task<IEnumerable<Endereco>> ObterPorMunicipioAsync(int municipioId)
    {
        return await DbSet
            .Include(e => e.Municipio)
            .Include(e => e.Estado)
            .Where(e => e.MunicipioId == municipioId)
            .OrderBy(e => e.Logradouro)
            .ThenBy(e => e.Numero)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém endereços por estado
    /// </summary>
    public async Task<IEnumerable<Endereco>> ObterPorEstadoAsync(int estadoId)
    {
        return await DbSet
            .Include(e => e.Municipio)
            .Include(e => e.Estado)
            .Where(e => e.EstadoId == estadoId)
            .OrderBy(e => e.Municipio.Nome)
            .ThenBy(e => e.Logradouro)
            .ToListAsync();
    }

    /// <summary>
    /// Busca endereços por logradouro
    /// </summary>
    public async Task<IEnumerable<Endereco>> BuscarPorLogradouroAsync(string logradouro, int? municipioId = null)
    {
        var query = DbSet
            .Include(e => e.Municipio)
            .Include(e => e.Estado)
            .Where(e => EF.Functions.ILike(e.Logradouro, $"%{logradouro}%"));

        if (municipioId.HasValue)
        {
            query = query.Where(e => e.MunicipioId == municipioId.Value);
        }

        return await query
            .OrderBy(e => e.Logradouro)
            .ThenBy(e => e.Numero)
            .ToListAsync();
    }

    /// <summary>
    /// Busca endereços por bairro
    /// </summary>
    public async Task<IEnumerable<Endereco>> BuscarPorBairroAsync(string bairro, int? municipioId = null)
    {
        var query = DbSet
            .Include(e => e.Municipio)
            .Include(e => e.Estado)
            .Where(e => EF.Functions.ILike(e.Bairro, $"%{bairro}%"));

        if (municipioId.HasValue)
        {
            query = query.Where(e => e.MunicipioId == municipioId.Value);
        }

        return await query
            .OrderBy(e => e.Bairro)
            .ThenBy(e => e.Logradouro)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém endereços próximos a uma localização
    /// </summary>
    public async Task<IEnumerable<Endereco>> ObterProximosAsync(double latitude, double longitude, double raioKm, int limite = 10)
    {
        var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
        var pontoReferencia = geometryFactory.CreatePoint(new Coordinate(longitude, latitude));
        var raioMetros = raioKm * 1000;

        return await DbSet
            .Include(e => e.Municipio)
            .Include(e => e.Estado)
            .Where(e => e.Localizacao != null && e.Localizacao.Distance(pontoReferencia) <= raioMetros)
            .OrderBy(e => e.Localizacao!.Distance(pontoReferencia))
            .Take(limite)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém endereços que possuem localização específica definida
    /// </summary>
    public async Task<IEnumerable<Endereco>> ObterComLocalizacaoAsync(int? municipioId = null)
    {
        var query = DbSet
            .Include(e => e.Municipio)
            .Include(e => e.Estado)
            .Where(e => e.Localizacao != null);

        if (municipioId.HasValue)
        {
            query = query.Where(e => e.MunicipioId == municipioId.Value);
        }

        return await query
            .OrderBy(e => e.Logradouro)
            .ThenBy(e => e.Numero)
            .ToListAsync();
    }

    /// <summary>
    /// Calcula a distância entre dois endereços
    /// </summary>
    public async Task<double?> CalcularDistanciaAsync(int enderecoOrigemId, int enderecoDestinoId)
    {
        var enderecos = await DbSet
            .Where(e => e.Id == enderecoOrigemId || e.Id == enderecoDestinoId)
            .Where(e => e.Localizacao != null)
            .ToListAsync();

        if (enderecos.Count != 2)
            return null;

        var origem = enderecos.First(e => e.Id == enderecoOrigemId);
        var destino = enderecos.First(e => e.Id == enderecoDestinoId);

        return origem.CalcularDistanciaKm(destino);
    }

    /// <summary>
    /// Verifica se existe um endereço com os dados especificados
    /// </summary>
    public async Task<bool> ExisteEnderecoAsync(string cep, string logradouro, string? numero, int municipioId)
    {
        var cepLimpo = LimparCep(cep);
        
        return await DbSet
            .AnyAsync(e => e.Cep == cepLimpo && 
                          e.Logradouro == logradouro && 
                          e.Numero == numero && 
                          e.MunicipioId == municipioId);
    }

    /// <summary>
    /// Sobrescreve o método base para incluir relacionamentos
    /// </summary>
    public override async Task<Endereco?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.Municipio)
            .Include(e => e.Estado)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    /// <summary>
    /// Sobrescreve o método base para incluir relacionamentos e ordenação
    /// </summary>
    public override async Task<IEnumerable<Endereco>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.Municipio)
            .Include(e => e.Estado)
            .OrderBy(e => e.Estado.Nome)
            .ThenBy(e => e.Municipio.Nome)
            .ThenBy(e => e.Logradouro)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Remove formatação do CEP
    /// </summary>
    private static string LimparCep(string cep)
    {
        return cep.Replace("-", "").Replace(".", "").Replace(" ", "");
    }
}