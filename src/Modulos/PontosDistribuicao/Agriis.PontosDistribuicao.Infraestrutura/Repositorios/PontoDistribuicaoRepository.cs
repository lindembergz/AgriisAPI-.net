using Microsoft.EntityFrameworkCore;
using Agriis.Compartilhado.Infraestrutura.Persistencia;
using Agriis.PontosDistribuicao.Dominio.Entidades;
using Agriis.PontosDistribuicao.Dominio.Interfaces;
using Agriis.Enderecos.Dominio.Entidades;
using NetTopologySuite.Geometries;

namespace Agriis.PontosDistribuicao.Infraestrutura.Repositorios;

/// <summary>
/// Implementação do repositório de pontos de distribuição
/// </summary>
public class PontoDistribuicaoRepository : RepositoryBase<PontoDistribuicao, DbContext>, IPontoDistribuicaoRepository
{
    public PontoDistribuicaoRepository(DbContext context) : base(context)
    {
    }
    
    /// <summary>
    /// Obtém pontos de distribuição por fornecedor
    /// </summary>
    public async Task<IEnumerable<PontoDistribuicao>> ObterPorFornecedorAsync(int fornecedorId, bool apenasAtivos = true)
    {
        var query = Context.Set<PontoDistribuicao>()
                          .Include(p => p.Endereco)
                          .ThenInclude(e => e.Municipio)
                          .ThenInclude(m => m.Estado)
                          .Where(p => p.FornecedorId == fornecedorId);
        
        if (apenasAtivos)
        {
            query = query.Where(p => p.Ativo);
        }
        
        return await query.OrderBy(p => p.Nome).ToListAsync();
    }
    
    /// <summary>
    /// Obtém pontos de distribuição que atendem um estado específico
    /// </summary>
    public async Task<IEnumerable<PontoDistribuicao>> ObterPorEstadoAsync(int estadoId, bool apenasAtivos = true)
    {
        var query = Context.Set<PontoDistribuicao>()
                          .Include(p => p.Endereco)
                          .ThenInclude(e => e.Municipio)
                          .ThenInclude(m => m.Estado)
                          .Where(p => p.CoberturaTerritorios != null &&
                                     EF.Functions.JsonContains(p.CoberturaTerritorios, $"{{\"estados\": [{estadoId}]}}"));
        
        if (apenasAtivos)
        {
            query = query.Where(p => p.Ativo);
        }
        
        return await query.OrderBy(p => p.Nome).ToListAsync();
    }
    
    /// <summary>
    /// Obtém pontos de distribuição que atendem um município específico
    /// </summary>
    public async Task<IEnumerable<PontoDistribuicao>> ObterPorMunicipioAsync(int municipioId, bool apenasAtivos = true)
    {
        var query = Context.Set<PontoDistribuicao>()
                          .Include(p => p.Endereco)
                          .ThenInclude(e => e.Municipio)
                          .ThenInclude(m => m.Estado)
                          .Where(p => p.CoberturaTerritorios != null &&
                                     EF.Functions.JsonContains(p.CoberturaTerritorios, $"{{\"municipios\": [{municipioId}]}}"));
        
        if (apenasAtivos)
        {
            query = query.Where(p => p.Ativo);
        }
        
        return await query.OrderBy(p => p.Nome).ToListAsync();
    }
    
    /// <summary>
    /// Obtém pontos de distribuição próximos a uma localização
    /// </summary>
    public async Task<IEnumerable<PontoDistribuicao>> ObterProximosAsync(double latitude, double longitude, 
                                                                        double raioKm, bool apenasAtivos = true)
    {
        // Criar ponto de referência
        var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
        var pontoReferencia = geometryFactory.CreatePoint(new Coordinate(longitude, latitude));
        
        var query = Context.Set<PontoDistribuicao>()
                          .Include(p => p.Endereco)
                          .ThenInclude(e => e.Municipio)
                          .ThenInclude(m => m.Estado)
                          .Where(p => p.Endereco.Localizacao != null &&
                                     p.Endereco.Localizacao.Distance(pontoReferencia) <= raioKm * 1000); // Converter km para metros
        
        if (apenasAtivos)
        {
            query = query.Where(p => p.Ativo);
        }
        
        // Ordenar por distância
        return await query.OrderBy(p => p.Endereco.Localizacao!.Distance(pontoReferencia))
                         .ToListAsync();
    }
    
    /// <summary>
    /// Obtém pontos de distribuição próximos a um endereço
    /// </summary>
    public async Task<IEnumerable<PontoDistribuicao>> ObterProximosAsync(Endereco endereco, double raioKm, 
                                                                        bool apenasAtivos = true)
    {
        if (endereco.Localizacao == null)
        {
            return new List<PontoDistribuicao>();
        }
        
        return await ObterProximosAsync(endereco.Latitude!.Value, endereco.Longitude!.Value, raioKm, apenasAtivos);
    }
    
    /// <summary>
    /// Obtém pontos de distribuição próximos a um município
    /// </summary>
    public async Task<IEnumerable<PontoDistribuicao>> ObterProximosAsync(Municipio municipio, double raioKm, 
                                                                        bool apenasAtivos = true)
    {
        if (municipio.Localizacao == null)
        {
            return new List<PontoDistribuicao>();
        }
        
        return await ObterProximosAsync(municipio.Latitude!.Value, municipio.Longitude!.Value, raioKm, apenasAtivos);
    }
    
    /// <summary>
    /// Obtém pontos de distribuição que atendem uma localização específica
    /// </summary>
    public async Task<IEnumerable<PontoDistribuicao>> ObterQueAtendemLocalizacaoAsync(int estadoId, int municipioId, 
                                                                                     Endereco? endereco = null, 
                                                                                     bool apenasAtivos = true)
    {
        var query = Context.Set<PontoDistribuicao>()
                          .Include(p => p.Endereco)
                          .ThenInclude(e => e.Municipio)
                          .ThenInclude(m => m.Estado)
                          .Where(p => p.CoberturaTerritorios != null);
        
        if (apenasAtivos)
        {
            query = query.Where(p => p.Ativo);
        }
        
        // Filtrar por cobertura territorial (estado ou município)
        query = query.Where(p => 
            EF.Functions.JsonContains(p.CoberturaTerritorios, $"{{\"estados\": [{estadoId}]}}") ||
            EF.Functions.JsonContains(p.CoberturaTerritorios, $"{{\"municipios\": [{municipioId}]}}"));
        
        var pontos = await query.ToListAsync();
        
        // Se tem endereço específico, filtrar também por raio de cobertura
        if (endereco != null)
        {
            pontos = pontos.Where(p => p.EstaDentroRaioCobertura(endereco)).ToList();
        }
        
        return pontos.OrderBy(p => p.Nome);
    }
    
    /// <summary>
    /// Calcula distâncias de pontos de distribuição para uma localização
    /// </summary>
    public async Task<Dictionary<int, double>> CalcularDistanciasAsync(IEnumerable<int> pontosIds, 
                                                                      double latitude, double longitude)
    {
        var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
        var pontoReferencia = geometryFactory.CreatePoint(new Coordinate(longitude, latitude));
        
        var pontos = await Context.Set<PontoDistribuicao>()
                                 .Include(p => p.Endereco)
                                 .Where(p => pontosIds.Contains(p.Id) && p.Endereco.Localizacao != null)
                                 .Select(p => new { p.Id, p.Endereco.Localizacao })
                                 .ToListAsync();
        
        var distancias = new Dictionary<int, double>();
        
        foreach (var ponto in pontos)
        {
            if (ponto.Localizacao != null)
            {
                var distanciaMetros = ponto.Localizacao.Distance(pontoReferencia);
                distancias[ponto.Id] = distanciaMetros / 1000.0; // Converter para km
            }
        }
        
        return distancias;
    }
    
    /// <summary>
    /// Verifica se existe ponto de distribuição com o mesmo nome para o fornecedor
    /// </summary>
    public async Task<bool> ExisteComMesmoNomeAsync(int fornecedorId, string nome, int? pontoId = null)
    {
        var query = Context.Set<PontoDistribuicao>()
                          .Where(p => p.FornecedorId == fornecedorId && 
                                     p.Nome.ToLower() == nome.ToLower());
        
        if (pontoId.HasValue)
        {
            query = query.Where(p => p.Id != pontoId.Value);
        }
        
        return await query.AnyAsync();
    }
    
    /// <summary>
    /// Obtém estatísticas de pontos de distribuição por fornecedor
    /// </summary>
    public async Task<(int Total, int Ativos, int Inativos)> ObterEstatisticasPorFornecedorAsync(int fornecedorId)
    {
        var pontos = await Context.Set<PontoDistribuicao>()
                                 .Where(p => p.FornecedorId == fornecedorId)
                                 .GroupBy(p => p.Ativo)
                                 .Select(g => new { Ativo = g.Key, Quantidade = g.Count() })
                                 .ToListAsync();
        
        var total = pontos.Sum(p => p.Quantidade);
        var ativos = pontos.Where(p => p.Ativo).Sum(p => p.Quantidade);
        var inativos = pontos.Where(p => !p.Ativo).Sum(p => p.Quantidade);
        
        return (total, ativos, inativos);
    }
    
    /// <summary>
    /// Override do método base para incluir relacionamentos
    /// </summary>
    public override async Task<PontoDistribuicao?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Context.Set<PontoDistribuicao>()
                           .Include(p => p.Endereco)
                           .ThenInclude(e => e.Municipio)
                           .ThenInclude(m => m.Estado)
                           .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }
    
    /// <summary>
    /// Override do método base para incluir relacionamentos
    /// </summary>
    public override async Task<IEnumerable<PontoDistribuicao>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Set<PontoDistribuicao>()
                           .Include(p => p.Endereco)
                           .ThenInclude(e => e.Municipio)
                           .ThenInclude(m => m.Estado)
                           .OrderBy(p => p.Nome)
                           .ToListAsync(cancellationToken);
    }
}