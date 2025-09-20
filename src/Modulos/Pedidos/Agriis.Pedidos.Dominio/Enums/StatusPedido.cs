namespace Agriis.Pedidos.Dominio.Enums;

/// <summary>
/// Status possíveis para um pedido
/// </summary>
public enum StatusPedido
{
    /// <summary>
    /// Pedido em negociação entre produtor e fornecedor
    /// </summary>
    EmNegociacao = 1,
    
    /// <summary>
    /// Pedido fechado e confirmado
    /// </summary>
    Fechado = 2,
    
    /// <summary>
    /// Pedido cancelado por tempo limite de interação
    /// </summary>
    CanceladoPorTempoLimite = 3,
    
    /// <summary>
    /// Pedido cancelado pelo comprador (produtor)
    /// </summary>
    CanceladoPeloComprador = 4
}