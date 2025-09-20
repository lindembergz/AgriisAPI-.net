using System.Text.Json;
using Agriis.Compartilhado.Dominio.Entidades;
using Agriis.Pedidos.Dominio.Enums;

namespace Agriis.Pedidos.Dominio.Entidades;

/// <summary>
/// Entidade que representa um pedido no sistema
/// </summary>
public class Pedido : EntidadeRaizAgregada
{
    /// <summary>
    /// Status atual do pedido
    /// </summary>
    public StatusPedido Status { get; private set; }
    
    /// <summary>
    /// Status do carrinho de compras
    /// </summary>
    public StatusCarrinho StatusCarrinho { get; private set; }
    
    /// <summary>
    /// Quantidade total de itens no pedido
    /// </summary>
    public int QuantidadeItens { get; private set; }
    
    /// <summary>
    /// Totais do pedido em formato JSON (valores, descontos, etc.)
    /// </summary>
    public JsonDocument? Totais { get; private set; }
    
    /// <summary>
    /// Indica se o produtor permite contato direto
    /// </summary>
    public bool PermiteContato { get; private set; }
    
    /// <summary>
    /// Indica se o pedido pode ser negociado
    /// </summary>
    public bool NegociarPedido { get; private set; }
    
    /// <summary>
    /// Data limite para interação no pedido
    /// </summary>
    public DateTime DataLimiteInteracao { get; private set; }
    
    /// <summary>
    /// ID do fornecedor responsável pelo pedido
    /// </summary>
    public int FornecedorId { get; private set; }
    
    /// <summary>
    /// ID do produtor que fez o pedido
    /// </summary>
    public int ProdutorId { get; private set; }
    
    /// <summary>
    /// Coleção de itens do pedido
    /// </summary>
    public virtual ICollection<PedidoItem> Itens { get; private set; } = new List<PedidoItem>();
    
    /// <summary>
    /// Coleção de propostas do pedido
    /// </summary>
    public virtual ICollection<Proposta> Propostas { get; private set; } = new List<Proposta>();
    
    /// <summary>
    /// Construtor protegido para uso do Entity Framework
    /// </summary>
    protected Pedido() { }
    
    /// <summary>
    /// Construtor para criação de um novo pedido
    /// </summary>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <param name="produtorId">ID do produtor</param>
    /// <param name="permiteContato">Se permite contato direto</param>
    /// <param name="negociarPedido">Se permite negociação</param>
    /// <param name="diasLimiteInteracao">Dias limite para interação (padrão 7)</param>
    public Pedido(int fornecedorId, int produtorId, bool permiteContato, bool negociarPedido, int diasLimiteInteracao = 7)
    {
        if (fornecedorId <= 0)
            throw new ArgumentException("ID do fornecedor deve ser maior que zero", nameof(fornecedorId));
            
        if (produtorId <= 0)
            throw new ArgumentException("ID do produtor deve ser maior que zero", nameof(produtorId));
            
        if (diasLimiteInteracao <= 0)
            throw new ArgumentException("Dias limite deve ser maior que zero", nameof(diasLimiteInteracao));
        
        FornecedorId = fornecedorId;
        ProdutorId = produtorId;
        PermiteContato = permiteContato;
        NegociarPedido = negociarPedido;
        Status = StatusPedido.EmNegociacao;
        StatusCarrinho = StatusCarrinho.EmAberto;
        DataLimiteInteracao = DateTime.UtcNow.AddDays(diasLimiteInteracao);
        QuantidadeItens = 0;
    }
    
    /// <summary>
    /// Adiciona um item ao pedido
    /// </summary>
    /// <param name="item">Item a ser adicionado</param>
    public void AdicionarItem(PedidoItem item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
            
        if (Status != StatusPedido.EmNegociacao)
            throw new InvalidOperationException("Não é possível adicionar itens a um pedido que não está em negociação");
        
        Itens.Add(item);
        RecalcularQuantidadeItens();
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Remove um item do pedido
    /// </summary>
    /// <param name="itemId">ID do item a ser removido</param>
    public void RemoverItem(int itemId)
    {
        if (Status != StatusPedido.EmNegociacao)
            throw new InvalidOperationException("Não é possível remover itens de um pedido que não está em negociação");
        
        var item = Itens.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            Itens.Remove(item);
            RecalcularQuantidadeItens();
            AtualizarDataModificacao();
        }
    }
    
    /// <summary>
    /// Fecha o pedido
    /// </summary>
    public void FecharPedido()
    {
        if (Status != StatusPedido.EmNegociacao)
            throw new InvalidOperationException("Apenas pedidos em negociação podem ser fechados");
            
        if (!Itens.Any())
            throw new InvalidOperationException("Não é possível fechar um pedido sem itens");
        
        Status = StatusPedido.Fechado;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Cancela o pedido pelo comprador
    /// </summary>
    public void CancelarPorComprador()
    {
        if (Status == StatusPedido.Fechado)
            throw new InvalidOperationException("Não é possível cancelar um pedido já fechado");
        
        Status = StatusPedido.CanceladoPeloComprador;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Cancela o pedido por tempo limite
    /// </summary>
    public void CancelarPorTempoLimite()
    {
        if (Status == StatusPedido.Fechado)
            throw new InvalidOperationException("Não é possível cancelar um pedido já fechado");
        
        Status = StatusPedido.CanceladoPorTempoLimite;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Atualiza o status do pedido
    /// </summary>
    /// <param name="novoStatus">Novo status</param>
    public void AtualizarStatus(StatusPedido novoStatus)
    {
        Status = novoStatus;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Atualiza os totais do pedido
    /// </summary>
    /// <param name="totais">Dados dos totais em formato JSON</param>
    public void AtualizarTotais(JsonDocument totais)
    {
        Totais = totais;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Verifica se o pedido está dentro do prazo limite
    /// </summary>
    /// <returns>True se está dentro do prazo</returns>
    public bool EstaDentroPrazoLimite()
    {
        return DateTime.UtcNow <= DataLimiteInteracao;
    }
    
    /// <summary>
    /// Atualiza a data limite de interação
    /// </summary>
    /// <param name="novosDias">Número de dias a partir de agora</param>
    public void AtualizarPrazoLimite(int novosDias)
    {
        if (novosDias <= 0)
            throw new ArgumentException("Dias deve ser maior que zero", nameof(novosDias));
            
        DataLimiteInteracao = DateTime.UtcNow.AddDays(novosDias);
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Recalcula a quantidade de itens do pedido
    /// </summary>
    private void RecalcularQuantidadeItens()
    {
        QuantidadeItens = Itens.Count;
    }
}