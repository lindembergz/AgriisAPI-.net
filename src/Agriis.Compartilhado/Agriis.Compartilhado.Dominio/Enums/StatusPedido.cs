namespace Agriis.Compartilhado.Dominio.Enums;

/// <summary>
/// Status do pedido no sistema
/// </summary>
public enum StatusPedido
{
    /// <summary>
    /// Pedido em processo de negociação
    /// </summary>
    EmNegociacao = 1,
    
    /// <summary>
    /// Pedido fechado/finalizado
    /// </summary>
    Fechado = 2,
    
    /// <summary>
    /// Cancelado por atingir tempo limite
    /// </summary>
    CanceladoPorTempoLimite = 3,
    
    /// <summary>
    /// Cancelado pelo comprador
    /// </summary>
    CanceladoPeloComprador = 4
}

/// <summary>
/// Status do carrinho de compras
/// </summary>
public enum StatusCarrinho
{
    /// <summary>
    /// Carrinho em aberto para alterações
    /// </summary>
    EmAberto = 0
}

/// <summary>
/// Ações do comprador no pedido
/// </summary>
public enum AcaoCompradorPedido
{
    /// <summary>
    /// Iniciou a negociação
    /// </summary>
    Iniciou = 0,
    
    /// <summary>
    /// Aceitou a proposta
    /// </summary>
    Aceitou = 1,
    
    /// <summary>
    /// Alterou o carrinho
    /// </summary>
    AlterouCarrinho = 2,
    
    /// <summary>
    /// Cancelou o pedido
    /// </summary>
    Cancelou = 3
}