using Agriis.Safras.Dominio.Interfaces;
using Agriis.Safras.Infraestrutura.Repositorios;
using Agriis.Safras.Aplicacao.Interfaces;
using Agriis.Safras.Aplicacao.Servicos;

namespace Agriis.Api.Configuration;

/// <summary>
/// Configuração de injeção de dependência para o módulo de Safras
/// </summary>
public static class SafrasDependencyInjection
{
    /// <summary>
    /// Configura os serviços do módulo de Safras
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <returns>Coleção de serviços configurada</returns>
    public static IServiceCollection AddSafrasModule(this IServiceCollection services)
    {
        // Repositórios
        services.AddScoped<ISafraRepository, SafraRepository>();

        // Serviços de Aplicação
        services.AddScoped<ISafraService, SafraService>();

        return services;
    }
}