using Agriis.Propriedades.Aplicacao.Interfaces;
using Agriis.Propriedades.Aplicacao.Servicos;
using Agriis.Propriedades.Dominio.Interfaces;
using Agriis.Propriedades.Dominio.Servicos;
using Agriis.Propriedades.Infraestrutura.Repositorios;

namespace Agriis.Api.Configuration;

public static class PropriedadesDependencyInjection
{
    public static IServiceCollection AddPropriedadesModule(this IServiceCollection services)
    {
        // Domain Services
        services.AddScoped<PropriedadeDomainService>();

        // Application Services
        services.AddScoped<IPropriedadeService, PropriedadeService>();
        services.AddScoped<ITalhaoService, TalhaoService>();
        services.AddScoped<IPropriedadeCulturaService, PropriedadeCulturaService>();

        // Repositories
        services.AddScoped<IPropriedadeRepository, PropriedadeRepository>();
        services.AddScoped<ITalhaoRepository, TalhaoRepository>();
        services.AddScoped<IPropriedadeCulturaRepository, PropriedadeCulturaRepository>();

        return services;
    }
}