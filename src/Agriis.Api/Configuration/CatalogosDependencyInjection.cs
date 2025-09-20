using Agriis.Catalogos.Dominio.Interfaces;
using Agriis.Catalogos.Infraestrutura.Repositorios;
using Agriis.Catalogos.Aplicacao.Interfaces;
using Agriis.Catalogos.Aplicacao.Servicos;

namespace Agriis.Api.Configuration;

/// <summary>
/// Configuração de injeção de dependência para o módulo de Catálogos
/// </summary>
public static class CatalogosDependencyInjection
{
    /// <summary>
    /// Configura os serviços do módulo de Catálogos
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <returns>Coleção de serviços configurada</returns>
    public static IServiceCollection AddCatalogosModule(this IServiceCollection services)
    {
        // Repositórios
        services.AddScoped<ICatalogoRepository, CatalogoRepository>();
        services.AddScoped<ICatalogoItemRepository, CatalogoItemRepository>();

        // Serviços de Aplicação
        services.AddScoped<ICatalogoService, CatalogoService>();

        return services;
    }
}