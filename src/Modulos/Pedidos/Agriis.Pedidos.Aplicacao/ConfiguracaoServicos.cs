using Microsoft.Extensions.DependencyInjection;
using Agriis.Pedidos.Aplicacao.Interfaces;
using Agriis.Pedidos.Aplicacao.Servicos;
using Agriis.Pedidos.Dominio.Servicos;

namespace Agriis.Pedidos.Aplicacao;

/// <summary>
/// Configuração de serviços do módulo de pedidos
/// </summary>
public static class ConfiguracaoServicos
{
    /// <summary>
    /// Adiciona os serviços do módulo de pedidos
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <returns>Coleção de serviços</returns>
    public static IServiceCollection AdicionarServicosPedidos(this IServiceCollection services)
    {
        // Serviços de aplicação
        services.AddScoped<IPedidoService, PedidoService>();
        services.AddScoped<IPropostaService, PropostaService>();
        
        // Serviços de domínio
        services.AddScoped<CarrinhoComprasService>();
        
        // Serviços em background
        services.AddHostedService<PrazoLimiteBackgroundService>();
        
        return services;
    }
}