using System.Text.Json;
using Agriis.Compartilhado.Dominio.Entidades;

namespace Agriis.Pedidos.Dominio.Entidades;

/// <summary>
/// Entidade que representa um item de pedido
/// </summary>
public class PedidoItem : EntidadeBase
{
    /// <summary>
    /// ID do pedido ao qual este item pertence
    /// </summary>
    public int PedidoId { get; private set; }
    
    /// <summary>
    /// ID do produto
    /// </summary>
    public int ProdutoId { get; private set; }
    
    /// <summary>
    /// Quantidade do produto solicitada
    /// </summary>
    public decimal Quantidade { get; private set; }
    
    /// <summary>
    /// Preço unitário do produto
    /// </summary>
    public decimal PrecoUnitario { get; private set; }
    
    /// <summary>
    /// Valor total do item (quantidade * preço unitário)
    /// </summary>
    public decimal ValorTotal { get; private set; }
    
    /// <summary>
    /// Percentual de desconto aplicado
    /// </summary>
    public decimal PercentualDesconto { get; private set; }
    
    /// <summary>
    /// Valor do desconto aplicado
    /// </summary>
    public decimal ValorDesconto { get; private set; }
    
    /// <summary>
    /// Valor final após desconto
    /// </summary>
    public decimal ValorFinal { get; private set; }
    
    /// <summary>
    /// Dados adicionais do item em formato JSON
    /// </summary>
    public JsonDocument? DadosAdicionais { get; private set; }
    
    /// <summary>
    /// Observações sobre o item
    /// </summary>
    public string? Observacoes { get; private set; }
    
    /// <summary>
    /// Referência de navegação para o pedido
    /// </summary>
    public virtual Pedido Pedido { get; private set; } = null!;
    
    /// <summary>
    /// Referência de navegação para o produto
    /// </summary>
    public virtual Produtos.Dominio.Entidades.Produto? Produto { get; private set; }
    
    /// <summary>
    /// Coleção de itens de transporte relacionados
    /// </summary>
    public virtual ICollection<PedidoItemTransporte> ItensTransporte { get; private set; } = new List<PedidoItemTransporte>();
    
    /// <summary>
    /// Alias para ItensTransporte para compatibilidade com serviços de domínio
    /// </summary>
    public virtual ICollection<PedidoItemTransporte> Transportes => ItensTransporte;
    
    /// <summary>
    /// Construtor protegido para uso do Entity Framework
    /// </summary>
    protected PedidoItem() { }
    
    /// <summary>
    /// Construtor para criação de um novo item de pedido
    /// </summary>
    /// <param name="pedidoId">ID do pedido</param>
    /// <param name="produtoId">ID do produto</param>
    /// <param name="quantidade">Quantidade solicitada</param>
    /// <param name="precoUnitario">Preço unitário</param>
    /// <param name="percentualDesconto">Percentual de desconto (opcional)</param>
    /// <param name="observacoes">Observações (opcional)</param>
    public PedidoItem(int pedidoId, int produtoId, decimal quantidade, decimal precoUnitario, 
                     decimal percentualDesconto = 0, string? observacoes = null)
    {
        if (pedidoId <= 0)
            throw new ArgumentException("ID do pedido deve ser maior que zero", nameof(pedidoId));
            
        if (produtoId <= 0)
            throw new ArgumentException("ID do produto deve ser maior que zero", nameof(produtoId));
            
        if (quantidade <= 0)
            throw new ArgumentException("Quantidade deve ser maior que zero", nameof(quantidade));
            
        if (precoUnitario < 0)
            throw new ArgumentException("Preço unitário não pode ser negativo", nameof(precoUnitario));
            
        if (percentualDesconto < 0 || percentualDesconto > 100)
            throw new ArgumentException("Percentual de desconto deve estar entre 0 e 100", nameof(percentualDesconto));
        
        PedidoId = pedidoId;
        ProdutoId = produtoId;
        Quantidade = quantidade;
        PrecoUnitario = precoUnitario;
        PercentualDesconto = percentualDesconto;
        Observacoes = observacoes;
        
        CalcularValores();
    }
    
    /// <summary>
    /// Atualiza a quantidade do item
    /// </summary>
    /// <param name="novaQuantidade">Nova quantidade</param>
    public void AtualizarQuantidade(decimal novaQuantidade)
    {
        if (novaQuantidade <= 0)
            throw new ArgumentException("Quantidade deve ser maior que zero", nameof(novaQuantidade));
        
        Quantidade = novaQuantidade;
        CalcularValores();
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Atualiza o preço unitário do item
    /// </summary>
    /// <param name="novoPreco">Novo preço unitário</param>
    public void AtualizarPrecoUnitario(decimal novoPreco)
    {
        if (novoPreco < 0)
            throw new ArgumentException("Preço unitário não pode ser negativo", nameof(novoPreco));
        
        PrecoUnitario = novoPreco;
        CalcularValores();
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Atualiza o percentual de desconto
    /// </summary>
    /// <param name="novoPercentual">Novo percentual de desconto</param>
    public void AtualizarDesconto(decimal novoPercentual)
    {
        if (novoPercentual < 0 || novoPercentual > 100)
            throw new ArgumentException("Percentual de desconto deve estar entre 0 e 100", nameof(novoPercentual));
        
        PercentualDesconto = novoPercentual;
        CalcularValores();
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Atualiza as observações do item
    /// </summary>
    /// <param name="novasObservacoes">Novas observações</param>
    public void AtualizarObservacoes(string? novasObservacoes)
    {
        Observacoes = novasObservacoes;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Atualiza os dados adicionais do item
    /// </summary>
    /// <param name="dadosAdicionais">Dados adicionais em formato JSON</param>
    public void AtualizarDadosAdicionais(JsonDocument? dadosAdicionais)
    {
        DadosAdicionais = dadosAdicionais;
        AtualizarDataModificacao();
    }
    
    /// <summary>
    /// Calcula os valores do item (total, desconto, final)
    /// </summary>
    private void CalcularValores()
    {
        ValorTotal = Quantidade * PrecoUnitario;
        ValorDesconto = ValorTotal * (PercentualDesconto / 100);
        ValorFinal = ValorTotal - ValorDesconto;
    }
}