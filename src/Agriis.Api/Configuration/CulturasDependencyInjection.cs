using Agriis.Culturas.Dominio.Interfaces;
using Agriis.Culturas.Infraestrutura.Repositorios;
using Agriis.Culturas.Aplicacao.Interfaces;
using Agriis.Culturas.Aplicacao.Servicos;

namespace Agriis.Api.Configuration;

/// <summary>
/// Configuração de injeção de dependência para o módulo de Culturas
/// </summary>
public static class CulturasDependencyInjection
{
    /// <summary>
    /// Configura os serviços do módulo de Culturas
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <returns>Coleção de serviços configurada</returns>
    public static IServiceCollection AddCulturasModule(this IServiceCollection services)
    {
        // Repositórios
        services.AddScoped<ICulturaRepository, CulturaRepository>();

        // Serviços de Aplicação
        services.AddScoped<ICulturaService, CulturaService>();

        return services;
    }
}