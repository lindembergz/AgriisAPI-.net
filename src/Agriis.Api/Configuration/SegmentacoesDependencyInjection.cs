using Agriis.Segmentacoes.Aplicacao.Interfaces;
using Agriis.Segmentacoes.Aplicacao.Servicos;
using Agriis.Segmentacoes.Dominio.Interfaces;
using Agriis.Segmentacoes.Dominio.Servicos;
using Agriis.Segmentacoes.Infraestrutura.Repositorios;

namespace Agriis.Api.Configuration;

/// <summary>
/// Configuração de injeção de dependência para o módulo de Segmentações
/// </summary>
public static class SegmentacoesDependencyInjection
{
    /// <summary>
    /// Adiciona os serviços do módulo de Segmentações
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <returns>Coleção de serviços</returns>
    public static IServiceCollection AddSegmentacoesModule(this IServiceCollection services)
    {
        // Repositórios
        services.AddScoped<ISegmentacaoRepository, SegmentacaoRepository>();
        services.AddScoped<IGrupoRepository, GrupoRepository>();
        services.AddScoped<IGrupoSegmentacaoRepository, GrupoSegmentacaoRepository>();
        
        // Serviços de domínio
        services.AddScoped<CalculoDescontoSegmentadoService>();
        
        // Serviços de aplicação
        services.AddScoped<ISegmentacaoService, SegmentacaoService>();
        services.AddScoped<ICalculoDescontoService, CalculoDescontoService>();
        
        return services;
    }
}