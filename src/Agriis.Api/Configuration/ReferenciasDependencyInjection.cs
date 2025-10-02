using Agriis.Referencias.Aplicacao.Interfaces;
using Agriis.Referencias.Aplicacao.Servicos;
using Agriis.Referencias.Dominio.Interfaces;
using Agriis.Referencias.Infraestrutura.Repositorios;

namespace Agriis.Api.Configuration;

/// <summary>
/// Configuração de injeção de dependência para o módulo de Referencias
/// </summary>
public static class ReferenciasDependencyInjection
{
    /// <summary>
    /// Adiciona os serviços do módulo de Referencias
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <returns>Coleção de serviços</returns>
    public static IServiceCollection AddReferenciasModule(this IServiceCollection services)
    {
        return services;
    }
}