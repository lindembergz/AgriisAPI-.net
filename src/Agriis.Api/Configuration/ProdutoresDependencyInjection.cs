using Agriis.Produtores.Aplicacao.Interfaces;
using Agriis.Produtores.Aplicacao.Servicos;
using Agriis.Produtores.Dominio.Interfaces;
using Agriis.Produtores.Dominio.Servicos;
using Agriis.Produtores.Infraestrutura.Repositorios;
using Agriis.Produtores.Infraestrutura.Servicos;

namespace Agriis.Api.Configuration;

/// <summary>
/// Configuração de injeção de dependência para o módulo de Produtores
/// </summary>
public static class ProdutoresDependencyInjection
{
    /// <summary>
    /// Adiciona os serviços do módulo de Produtores
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <returns>Coleção de serviços</returns>
    public static IServiceCollection AddProdutoresModule(this IServiceCollection services)
    {
        // Repositórios
        services.AddScoped<IProdutorRepository, ProdutorRepository>();
        services.AddScoped<IUsuarioProdutorRepository, UsuarioProdutorRepository>();
        
        // Serviços de domínio
        services.AddScoped<ProdutorDomainService>();
        
        // Serviços de aplicação
        services.AddScoped<IProdutorService, ProdutorService>();
        
        // Serviços de infraestrutura
        services.AddScoped<ISerproService, SerproService>();
        
        //HttpClient para SERPRO
        services.AddHttpClient<ISerproService, SerproService>(client =>
        {
            client.BaseAddress = new Uri("https://gateway.apiserpro.serpro.gov.br/");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .SetHandlerLifetime(TimeSpan.FromMinutes(10)); // Reduz a frequência de cleanup
       
        
        return services;
    }
}