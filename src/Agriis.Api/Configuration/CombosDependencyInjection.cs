using Agriis.Combos.Aplicacao.Interfaces;
using Agriis.Combos.Aplicacao.Servicos;
using Agriis.Combos.Dominio.Interfaces;
using Agriis.Combos.Infraestrutura.Repositorios;

namespace Agriis.Api.Configuration;

/// <summary>
/// Configuração de injeção de dependência para o módulo de Combos
/// </summary>
public static class CombosDependencyInjection
{
    /// <summary>
    /// Adiciona os serviços do módulo de Combos
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <returns>Coleção de serviços</returns>
    public static IServiceCollection AddCombosModule(this IServiceCollection services)
    {
        // Repositórios
        services.AddScoped<IComboRepository, ComboRepository>();
        
        // Serviços de aplicação
        services.AddScoped<IComboService, ComboService>();
        
        return services;
    }
}