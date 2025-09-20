namespace Agriis.Pedidos.Dominio.Enums;

/// <summary>
/// Ações que o comprador (produtor) pode realizar em uma proposta
/// </summary>
public enum AcaoCompradorPedido
{
    /// <summary>
    /// Iniciou a negociação
    /// </summary>
    Iniciou = 0,
    
    /// <summary>
    /// Aceitou a proposta/pedido
    /// </summary>
    Aceitou = 1,
    
    /// <summary>
    /// Alterou o carrinho de compras
    /// </summary>
    AlterouCarrinho = 2,
    
    /// <summary>
    /// Cancelou o pedido
    /// </summary>
    Cancelou = 3
}