using Agriis.Pedidos.Aplicacao.Interfaces;
using Agriis.Pedidos.Aplicacao.Servicos;
using Agriis.Pedidos.Aplicacao.Validadores;
using Agriis.Pedidos.Aplicacao.DTOs;
using Agriis.Pedidos.Dominio.Interfaces;
using Agriis.Pedidos.Dominio.Servicos;
using Agriis.Pedidos.Infraestrutura.Repositorios;
using FluentValidation;

namespace Agriis.Api.Configuration;

/// <summary>
/// Configuração de injeção de dependência para o módulo de Pedidos
/// </summary>
public static class PedidosDependencyInjection
{
    /// <summary>
    /// Adiciona os serviços do módulo de Pedidos
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <returns>Coleção de serviços</returns>
    public static IServiceCollection AddPedidosModule(this IServiceCollection services)
    {
        // Repositórios
        services.AddScoped<IPedidoRepository, PedidoRepository>();
        services.AddScoped<IPedidoItemRepository, PedidoItemRepository>();
        services.AddScoped<IPedidoItemTransporteRepository, PedidoItemTransporteRepository>();
        services.AddScoped<IPropostaRepository, PropostaRepository>();
        
        // Serviços de domínio
        services.AddScoped<CarrinhoComprasService>();
        services.AddScoped<FreteCalculoService>();
        services.AddScoped<TransporteAgendamentoService>();
        
        // Serviços de aplicação
        services.AddScoped<IPedidoService, PedidoService>();
        services.AddScoped<IPropostaService, PropostaService>();
        services.AddScoped<INotificacaoService, NotificacaoService>();
        services.AddScoped<ITransporteService, TransporteService>();
        
        // Serviços em background
        services.AddHostedService<PrazoLimiteBackgroundService>();
        
        // Validadores
        services.AddScoped<IValidator<CalcularFreteDto>, CalcularFreteDtoValidator>();
        services.AddScoped<IValidator<CalcularFreteConsolidadoDto>, CalcularFreteConsolidadoDtoValidator>();
        services.AddScoped<IValidator<AgendarTransporteDto>, AgendarTransporteDtoValidator>();
        services.AddScoped<IValidator<ReagendarTransporteDto>, ReagendarTransporteDtoValidator>();
        services.AddScoped<IValidator<AtualizarValorFreteDto>, AtualizarValorFreteDtoValidator>();
        services.AddScoped<IValidator<ValidarAgendamentosDto>, ValidarAgendamentosDtoValidator>();
        
        return services;
    }
}