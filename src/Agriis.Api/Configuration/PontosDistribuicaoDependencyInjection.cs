using Agriis.PontosDistribuicao.Dominio.Interfaces;
using Agriis.PontosDistribuicao.Infraestrutura.Repositorios;
using Agriis.PontosDistribuicao.Aplicacao.Servicos;

namespace Agriis.Api.Configuration;

/// <summary>
/// Configuração de injeção de dependência para o módulo de Pontos de Distribuição
/// </summary>
public static class PontosDistribuicaoDependencyInjection
{
    /// <summary>
    /// Configura os serviços do módulo de Pontos de Distribuição
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <returns>Coleção de serviços configurada</returns>
    public static IServiceCollection AddPontosDistribuicaoModule(this IServiceCollection services)
    {
        // Repositórios
        services.AddScoped<IPontoDistribuicaoRepository, PontoDistribuicaoRepository>();

        // Serviços de aplicação
        services.AddScoped<PontoDistribuicaoService>();

        return services;
    }
}