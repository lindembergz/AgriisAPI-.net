using Agriis.Pedidos.Dominio.Entidades;

namespace Agriis.Pedidos.Aplicacao.Interfaces;

/// <summary>
/// Interface do serviço de notificações
/// </summary>
public interface INotificacaoService
{
    /// <summary>
    /// Envia notificação sobre nova proposta
    /// </summary>
    /// <param name="proposta">Proposta criada</param>
    /// <param name="pedido">Pedido relacionado</param>
    /// <returns>Task</returns>
    Task NotificarNovaPropostaAsync(Proposta proposta, Pedido pedido);
    
    /// <summary>
    /// Envia notificação sobre alteração no carrinho
    /// </summary>
    /// <param name="pedido">Pedido alterado</param>
    /// <param name="descricaoAlteracao">Descrição da alteração</param>
    /// <returns>Task</returns>
    Task NotificarAlteracaoCarrinhoAsync(Pedido pedido, string descricaoAlteracao);
}