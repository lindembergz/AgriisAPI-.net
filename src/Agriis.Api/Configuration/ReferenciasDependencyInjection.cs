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
        // Repositórios
        services.AddScoped<IMoedaRepository, MoedaRepository>();
        services.AddScoped<IPaisRepository, PaisRepository>();
        services.AddScoped<IUfRepository, UfRepository>();
        services.AddScoped<IMunicipioRepository, MunicipioRepository>();
        services.AddScoped<IAtividadeAgropecuariaRepository, AtividadeAgropecuariaRepository>();
        services.AddScoped<IUnidadeMedidaRepository, UnidadeMedidaRepository>();
        services.AddScoped<IEmbalagemRepository, EmbalagemRepository>();
        
        // Serviços de aplicação
        services.AddScoped<IMoedaService, MoedaService>();
        services.AddScoped<IPaisService, PaisService>();
        services.AddScoped<IUfService, UfService>();
        services.AddScoped<IMunicipioService, MunicipioService>();
        services.AddScoped<IAtividadeAgropecuariaService, AtividadeAgropecuariaService>();
        services.AddScoped<IUnidadeMedidaService, UnidadeMedidaService>();
        services.AddScoped<IEmbalagemService, EmbalagemService>();
        
        return services;
    }
}