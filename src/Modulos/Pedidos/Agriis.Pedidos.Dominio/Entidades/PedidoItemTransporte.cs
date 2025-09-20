using System.Text.Json;
using Agriis.Compartilhado.Dominio.Entidades;

namespace Agriis.Pedidos.Dominio.Entidades;

/// <summary>
/// Entidade que representa informações de transporte para um item de pedido
/// </summary>
public class PedidoItemTransporte : EntidadeBase
{
    /// <summary>
    /// ID do item de pedido ao qual este transporte pertence
    /// </summary>
    public int PedidoItemId { get; private set; }
    
    /// <summary>
    /// Quantidade a ser transportada
    /// </summary>
    public decimal Quantidade { get; private set; }
    
    /// <summary>
    /// Data agendada para o transporte
    /// </summary>
    public DateTime? DataAgendamento { get; private set; }
    
    /// <summary>
    /// Valor do frete para este transporte
    /// </summary>
    public decimal ValorFrete { get; private set; }
    
    /// <summary>
    /// Peso total da carga
    /// </summary>
    public decimal? PesoTotal { get; private set; }
    
    /// <summary>
    /// Volume total da carga (cubagem)
    /// </summary>
    public decimal? VolumeTotal { get; private set; }
    
    /// <summary>
    /// Endereço de origem do transporte
    /// </summary>
    public string? EnderecoOrigem { get; private set; }
    
    /// <summary>
    /// Endereço de destino do transporte
    /// </summary>
    public string? EnderecoDestino { get; private set; }
    
    /// <summary>
    /// Informações adicionais do transporte em formato JSON
    /// </summary>
    public JsonDocument? InformacoesTransporte { get; private set; }
    
    /// <summary>
    /// Observações sobre o transporte
    /// </summary>
    public string? Observacoes { get; private set; }
    
    /// <summary>
    /// Referência de navegação para o item de pedido
    /// </summary>
    public virtual PedidoItem PedidoItem { get; private set; } = null!;
    
    /// <summary>
    /// Construtor protegido para uso do Entity Framework
    /// </summary>
    protected PedidoItemTransporte() { }
    
    /// <summary>
    /// Construtor para criação de um novo transporte de item
    /// </summary>
    /// <param name="pedidoItemId">ID do item de pedido</param>
    /// <param name="quantidade">Quantidade a ser transportada</param>
    /// <param name="valorFrete">Valor do frete</param>
    /// <param name="enderecoOrigem">Endereço de origem (opcional)</param>
    /// <param name="enderecoDestino">Endereço de destino (opcional)</param>
    public PedidoItemTransporte(int pedidoItemId, decimal quantidade, decimal valorFrete, 
                               string? enderecoOrigem = null, string? enderecoDestino = null)
    {
        if (pedidoItemId <= 0)
            throw new ArgumentException("ID do item de pedido deve ser maior que zero", nameof(pedidoItemId));
            
        if (quantidade <= 0)
            throw new ArgumentException("Quantidade deve ser maior que zero", nameof(quantidade));
            
        if (valorFrete < 0)
            throw new ArgumentException("Valor do frete não pode ser negativo", nameof(valorFrete));
        
        PedidoItemId = pedidoItemId;
        Quantidade = quantidade;
        ValorFrete = valorFrete;
        EnderecoOrigem = enderecoOrigem;
        EnderecoDestino = enderecoDestino;
    }
    
    /// <summary>
    /// Agenda uma data para o transporte
    /// </summary>
    /// <param name="dataAgendamento">Data do agendamento</param>
    public void AgendarTransporte(DateTime dataAgendamento)
    {
        if (dataAgendamento <= DateTime.UtcNow)
            throw new ArgumentException("Data de agendamento deve ser futura", nameof(dataAgendamento));
        
        DataAgendamento = dataAgendamento;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Atualiza as informações de peso e volume
    /// </summary>
    /// <param name="pesoTotal">Peso total da carga</param>
    /// <param name="volumeTotal">Volume total da carga</param>
    public void AtualizarPesoVolume(decimal? pesoTotal, decimal? volumeTotal)
    {
        if (pesoTotal.HasValue && pesoTotal.Value < 0)
            throw new ArgumentException("Peso total não pode ser negativo", nameof(pesoTotal));
            
        if (volumeTotal.HasValue && volumeTotal.Value < 0)
            throw new ArgumentException("Volume total não pode ser negativo", nameof(volumeTotal));
        
        PesoTotal = pesoTotal;
        VolumeTotal = volumeTotal;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Atualiza o valor do frete
    /// </summary>
    /// <param name="novoValorFrete">Novo valor do frete</param>
    public void AtualizarValorFrete(decimal novoValorFrete)
    {
        if (novoValorFrete < 0)
            throw new ArgumentException("Valor do frete não pode ser negativo", nameof(novoValorFrete));
        
        ValorFrete = novoValorFrete;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Atualiza os endereços de origem e destino
    /// </summary>
    /// <param name="enderecoOrigem">Endereço de origem</param>
    /// <param name="enderecoDestino">Endereço de destino</param>
    public void AtualizarEnderecos(string? enderecoOrigem, string? enderecoDestino)
    {
        EnderecoOrigem = enderecoOrigem;
        EnderecoDestino = enderecoDestino;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Atualiza as informações adicionais do transporte
    /// </summary>
    /// <param name="informacoesTransporte">Informações em formato JSON</param>
    public void AtualizarInformacoesTransporte(JsonDocument? informacoesTransporte)
    {
        InformacoesTransporte = informacoesTransporte;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Atualiza as observações do transporte
    /// </summary>
    /// <param name="observacoes">Novas observações</param>
    public void AtualizarObservacoes(string? observacoes)
    {
        Observacoes = observacoes;
        AtualizarDataModificacao();
    }
}