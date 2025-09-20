using Agriis.Pedidos.Aplicacao.Interfaces;
using Agriis.Pedidos.Dominio.Entidades;
using Microsoft.Extensions.Logging;

namespace Agriis.Pedidos.Aplicacao.Servicos;

/// <summary>
/// Serviço de notificações para propostas
/// </summary>
public class NotificacaoService : INotificacaoService
{
    private readonly ILogger<NotificacaoService> _logger;
    
    /// <summary>
    /// Construtor do serviço de notificações
    /// </summary>
    public NotificacaoService(ILogger<NotificacaoService> logger)
    {
        _logger = logger;
    }
    
    /// <summary>
    /// Envia notificação sobre nova proposta
    /// </summary>
    /// <param name="proposta">Proposta criada</param>
    /// <param name="pedido">Pedido relacionado</param>
    /// <returns>Task</returns>
    public async Task NotificarNovaPropostaAsync(Proposta proposta, Pedido pedido)
    {
        // TODO: Implementar notificação por email/push/SignalR
        _logger.LogInformation("Nova proposta criada para pedido {PedidoId} - Proposta {PropostaId}", 
            pedido.Id, proposta.Id);
        
        await Task.CompletedTask;
    }
    
    /// <summary>
    /// Envia notificação sobre alteração no carrinho
    /// </summary>
    /// <param name="pedido">Pedido alterado</param>
    /// <param name="descricaoAlteracao">Descrição da alteração</param>
    /// <returns>Task</returns>
    public async Task NotificarAlteracaoCarrinhoAsync(Pedido pedido, string descricaoAlteracao)
    {
        // TODO: Implementar notificação por email/push/SignalR
        _logger.LogInformation("Carrinho alterado para pedido {PedidoId}: {Descricao}", 
            pedido.Id, descricaoAlteracao);
        
        await Task.CompletedTask;
    }
}