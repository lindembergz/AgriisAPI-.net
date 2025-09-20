using System.Text.Json;

namespace Agriis.Pedidos.Aplicacao.DTOs;

/// <summary>
/// DTO para representar um item de pedido
/// </summary>
public class PedidoItemDto
{
    public int Id { get; set; }
    public int PedidoId { get; set; }
    public int ProdutoId { get; set; }
    public decimal Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal PercentualDesconto { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorFinal { get; set; }
    public JsonDocument? DadosAdicionais { get; set; }
    public string? Observacoes { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public List<PedidoItemTransporteDto> ItensTransporte { get; set; } = new();
}

/// <summary>
/// DTO para criar um novo item de pedido
/// </summary>
public class CriarPedidoItemDto
{
    public int PedidoId { get; set; }
    public int ProdutoId { get; set; }
    public decimal Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal PercentualDesconto { get; set; } = 0;
    public string? Observacoes { get; set; }
}

/// <summary>
/// DTO para atualizar um item de pedido
/// </summary>
public class AtualizarPedidoItemDto
{
    public decimal? Quantidade { get; set; }
    public decimal? PrecoUnitario { get; set; }
    public decimal? PercentualDesconto { get; set; }
    public string? Observacoes { get; set; }
    public JsonDocument? DadosAdicionais { get; set; }
}