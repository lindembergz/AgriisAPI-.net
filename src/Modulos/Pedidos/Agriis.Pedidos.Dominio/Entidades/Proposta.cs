using Agriis.Compartilhado.Dominio.Entidades;
using Agriis.Pedidos.Dominio.Enums;

namespace Agriis.Pedidos.Dominio.Entidades;

/// <summary>
/// Entidade que representa uma proposta de negociação em um pedido
/// </summary>
public class Proposta : EntidadeBase
{
    /// <summary>
    /// ID do pedido ao qual a proposta pertence
    /// </summary>
    public int PedidoId { get; private set; }
    
    /// <summary>
    /// Ação realizada pelo comprador (produtor)
    /// </summary>
    public AcaoCompradorPedido? AcaoComprador { get; private set; }
    
    /// <summary>
    /// Observação da proposta (geralmente do fornecedor)
    /// </summary>
    public string? Observacao { get; private set; }
    
    /// <summary>
    /// ID do usuário produtor que criou a proposta (quando ação do comprador)
    /// </summary>
    public int? UsuarioProdutorId { get; private set; }
    
    /// <summary>
    /// ID do usuário fornecedor que criou a proposta (quando observação do fornecedor)
    /// </summary>
    public int? UsuarioFornecedorId { get; private set; }
    
    /// <summary>
    /// Navegação para o pedido
    /// </summary>
    public virtual Pedido Pedido { get; private set; } = null!;
    
    /// <summary>
    /// Construtor protegido para uso do Entity Framework
    /// </summary>
    protected Proposta() { }
    
    /// <summary>
    /// Construtor para proposta de ação do comprador (produtor)
    /// </summary>
    /// <param name="pedidoId">ID do pedido</param>
    /// <param name="acaoComprador">Ação realizada pelo comprador</param>
    /// <param name="usuarioProdutorId">ID do usuário produtor</param>
    /// <param name="observacao">Observação opcional</param>
    public Proposta(int pedidoId, AcaoCompradorPedido acaoComprador, int usuarioProdutorId, string? observacao = null)
    {
        if (pedidoId <= 0)
            throw new ArgumentException("ID do pedido deve ser maior que zero", nameof(pedidoId));
            
        if (usuarioProdutorId <= 0)
            throw new ArgumentException("ID do usuário produtor deve ser maior que zero", nameof(usuarioProdutorId));
        
        PedidoId = pedidoId;
        AcaoComprador = acaoComprador;
        UsuarioProdutorId = usuarioProdutorId;
        Observacao = observacao;
    }
    
    /// <summary>
    /// Construtor para proposta de observação do fornecedor
    /// </summary>
    /// <param name="pedidoId">ID do pedido</param>
    /// <param name="observacao">Observação do fornecedor</param>
    /// <param name="usuarioFornecedorId">ID do usuário fornecedor</param>
    public Proposta(int pedidoId, string observacao, int usuarioFornecedorId)
    {
        if (pedidoId <= 0)
            throw new ArgumentException("ID do pedido deve ser maior que zero", nameof(pedidoId));
            
        if (string.IsNullOrWhiteSpace(observacao))
            throw new ArgumentException("Observação não pode ser vazia", nameof(observacao));
            
        if (usuarioFornecedorId <= 0)
            throw new ArgumentException("ID do usuário fornecedor deve ser maior que zero", nameof(usuarioFornecedorId));
        
        PedidoId = pedidoId;
        Observacao = observacao;
        UsuarioFornecedorId = usuarioFornecedorId;
    }
    
    /// <summary>
    /// Verifica se a proposta é de um produtor (comprador)
    /// </summary>
    /// <returns>True se é proposta de produtor</returns>
    public bool EhPropostaProdutor()
    {
        return UsuarioProdutorId.HasValue;
    }
    
    /// <summary>
    /// Verifica se a proposta é de um fornecedor
    /// </summary>
    /// <returns>True se é proposta de fornecedor</returns>
    public bool EhPropostaFornecedor()
    {
        return UsuarioFornecedorId.HasValue;
    }
}