using System.Text.Json;

namespace Agriis.Pedidos.Aplicacao.DTOs;

/// <summary>
/// DTO para representar um transporte de item de pedido
/// </summary>
public class PedidoItemTransporteDto
{
    public int Id { get; set; }
    public int PedidoItemId { get; set; }
    public decimal Quantidade { get; set; }
    public DateTime? DataAgendamento { get; set; }
    public decimal ValorFrete { get; set; }
    public decimal? PesoTotal { get; set; }
    public decimal? VolumeTotal { get; set; }
    public string? EnderecoOrigem { get; set; }
    public string? EnderecoDestino { get; set; }
    public JsonDocument? InformacoesTransporte { get; set; }
    public string? Observacoes { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
}

/// <summary>
/// DTO para criar um novo transporte de item
/// </summary>
public class CriarPedidoItemTransporteDto
{
    public int PedidoItemId { get; set; }
    public decimal Quantidade { get; set; }
    public decimal ValorFrete { get; set; }
    public string? EnderecoOrigem { get; set; }
    public string? EnderecoDestino { get; set; }
    public DateTime? DataAgendamento { get; set; }
}

/// <summary>
/// DTO para atualizar um transporte de item
/// </summary>
public class AtualizarPedidoItemTransporteDto
{
    public decimal? Quantidade { get; set; }
    public decimal? ValorFrete { get; set; }
    public decimal? PesoTotal { get; set; }
    public decimal? VolumeTotal { get; set; }
    public string? EnderecoOrigem { get; set; }
    public string? EnderecoDestino { get; set; }
    public DateTime? DataAgendamento { get; set; }
    public JsonDocument? InformacoesTransporte { get; set; }
    public string? Observacoes { get; set; }
}