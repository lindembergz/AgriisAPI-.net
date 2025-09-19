using Agriis.Enderecos.Dominio.Interfaces;
using Agriis.Enderecos.Infraestrutura.Repositorios;

namespace Agriis.Api.Configuration;

/// <summary>
/// Configuração de injeção de dependência para o módulo de Endereços
/// </summary>
public static class EnderecosDependencyInjection
{
    /// <summary>
    /// Configura os serviços do módulo de Endereços
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <returns>Coleção de serviços configurada</returns>
    public static IServiceCollection AddEnderecosModule(this IServiceCollection services)
    {
        // Repositórios
        services.AddScoped<IEstadoRepository, EstadoRepository>();
        services.AddScoped<IMunicipioRepository, MunicipioRepository>();
        services.AddScoped<IEnderecoRepository, EnderecoRepository>();

        return services;
    }
}