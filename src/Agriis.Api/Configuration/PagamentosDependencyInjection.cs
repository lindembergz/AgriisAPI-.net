using Agriis.Pagamentos.Aplicacao.Interfaces;
using Agriis.Pagamentos.Aplicacao.Servicos;
using Agriis.Pagamentos.Dominio.Interfaces;
using Agriis.Pagamentos.Infraestrutura.Repositorios;

namespace Agriis.Api.Configuration;

/// <summary>
/// Configuração de injeção de dependência para o módulo de Pagamentos
/// </summary>
public static class PagamentosDependencyInjection
{
    /// <summary>
    /// Registra os serviços do módulo de Pagamentos
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <returns>Coleção de serviços configurada</returns>
    public static IServiceCollection AddPagamentosModule(this IServiceCollection services)
    {
        // Repositórios
        services.AddScoped<IFormaPagamentoRepository, FormaPagamentoRepository>();
        services.AddScoped<ICulturaFormaPagamentoRepository, CulturaFormaPagamentoRepository>();

        // Serviços de Aplicação
        services.AddScoped<IFormaPagamentoService, FormaPagamentoService>();
        services.AddScoped<ICulturaFormaPagamentoService, CulturaFormaPagamentoService>();

        return services;
    }
}