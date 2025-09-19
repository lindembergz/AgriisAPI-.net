using Agriis.Fornecedores.Aplicacao.Interfaces;
using Agriis.Fornecedores.Aplicacao.Servicos;
using Agriis.Fornecedores.Dominio.Interfaces;
using Agriis.Fornecedores.Dominio.Servicos;
using Agriis.Fornecedores.Infraestrutura.Repositorios;

namespace Agriis.Api.Configuration;

/// <summary>
/// Configuração de injeção de dependência para o módulo de Fornecedores
/// </summary>
public static class FornecedoresDependencyInjection
{
    /// <summary>
    /// Registra os serviços do módulo de Fornecedores
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <returns>Coleção de serviços configurada</returns>
    public static IServiceCollection AddFornecedoresModule(this IServiceCollection services)
    {
        // Repositórios
        services.AddScoped<IFornecedorRepository, FornecedorRepository>();
        services.AddScoped<IUsuarioFornecedorRepository, UsuarioFornecedorRepository>();
        services.AddScoped<IUsuarioFornecedorTerritorioRepository, UsuarioFornecedorTerritorioRepository>();
        
        // Serviços de domínio
        services.AddScoped<FornecedorDomainService>();
        
        // Serviços de aplicação
        services.AddScoped<IFornecedorService, FornecedorService>();
        
        return services;
    }
}