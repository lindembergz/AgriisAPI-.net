using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Agriis.Pedidos.Aplicacao.Interfaces;

namespace Agriis.Pedidos.Aplicacao.Servicos;

/// <summary>
/// Serviço em background para gerenciar prazos limite de pedidos
/// </summary>
public class PrazoLimiteBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PrazoLimiteBackgroundService> _logger;
    private readonly TimeSpan _intervaloExecucao = TimeSpan.FromHours(1); // Executa a cada hora

    public PrazoLimiteBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<PrazoLimiteBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Serviço de prazo limite iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessarPrazosLimiteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar prazos limite de pedidos");
            }

            await Task.Delay(_intervaloExecucao, stoppingToken);
        }

        _logger.LogInformation("Serviço de prazo limite finalizado");
    }

    private async Task ProcessarPrazosLimiteAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var pedidoService = scope.ServiceProvider.GetRequiredService<IPedidoService>();

        try
        {
            // Cancelar pedidos com prazo ultrapassado
            var pedidosCancelados = await pedidoService.CancelarPedidosComPrazoUltrapassadoAsync();
            
            if (pedidosCancelados > 0)
            {
                _logger.LogInformation("Cancelados {Count} pedidos por prazo ultrapassado", pedidosCancelados);
            }

            // Notificar sobre pedidos próximos do prazo (1 dia antes)
            var pedidosProximos = await pedidoService.ObterProximosPrazoLimiteAsync(1);
            var countProximos = pedidosProximos.Count();
            
            if (countProximos > 0)
            {
                _logger.LogInformation("Encontrados {Count} pedidos próximos do prazo limite", countProximos);
                
                // TODO: Implementar notificações para produtores e fornecedores
                // sobre pedidos próximos do prazo limite
                foreach (var pedido in pedidosProximos)
                {
                    _logger.LogDebug("Pedido {PedidoId} próximo do prazo limite: {DataLimite}", 
                        pedido.Id, pedido.DataLimiteInteracao);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar prazos limite");
            throw;
        }
    }
}