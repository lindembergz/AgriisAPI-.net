using System.Text.Json;
using Agriis.Pedidos.Dominio.Enums;

namespace Agriis.Pedidos.Aplicacao.DTOs;

/// <summary>
/// DTO para representar um pedido
/// </summary>
public class PedidoDto
{
    public int Id { get; set; }
    public StatusPedido Status { get; set; }
    public StatusCarrinho StatusCarrinho { get; set; }
    public int QuantidadeItens { get; set; }
    public JsonDocument? Totais { get; set; }
    public bool PermiteContato { get; set; }
    public bool NegociarPedido { get; set; }
    public DateTime DataLimiteInteracao { get; set; }
    public int FornecedorId { get; set; }
    public int ProdutorId { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public List<PedidoItemDto> Itens { get; set; } = new();
}

/// <summary>
/// DTO para criar um novo pedido
/// </summary>
public class CriarPedidoDto
{
    public int FornecedorId { get; set; }
    public int ProdutorId { get; set; }
    public bool PermiteContato { get; set; }
    public bool NegociarPedido { get; set; }
    public int DiasLimiteInteracao { get; set; } = 7;
}

/// <summary>
/// DTO para atualizar um pedido
/// </summary>
public class AtualizarPedidoDto
{
    public bool? PermiteContato { get; set; }
    public bool? NegociarPedido { get; set; }
    public JsonDocument? Totais { get; set; }
}