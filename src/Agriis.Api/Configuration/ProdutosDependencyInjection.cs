using Agriis.Produtos.Aplicacao.Interfaces;
using Agriis.Produtos.Aplicacao.Servicos;
using Agriis.Produtos.Dominio.Interfaces;
using Agriis.Produtos.Infraestrutura.Repositorios;

namespace Agriis.Api.Configuration;

/// <summary>
/// Configuração de injeção de dependência para o módulo de Produtos
/// </summary>
public static class ProdutosDependencyInjection
{
    /// <summary>
    /// Adiciona os serviços do módulo de Produtos
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <returns>Coleção de serviços</returns>
    public static IServiceCollection AddProdutosModule(this IServiceCollection services)
    {
        // Repositórios
        services.AddScoped<IProdutoRepository, ProdutoRepository>();
        services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        services.AddScoped<IProdutoCulturaRepository, ProdutoCulturaRepository>();
        
        // Serviços de aplicação
        services.AddScoped<IProdutoService, ProdutoService>();
        services.AddScoped<ICategoriaService, CategoriaService>();
        
        return services;
    }
}