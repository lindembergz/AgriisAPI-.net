using Agriis.Pedidos.Dominio.Enums;

namespace Agriis.Pedidos.Aplicacao.DTOs;

/// <summary>
/// DTO para proposta
/// </summary>
public class PropostaDto
{
    /// <summary>
    /// ID da proposta
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// ID do pedido
    /// </summary>
    public int PedidoId { get; set; }
    
    /// <summary>
    /// Data de criação da proposta
    /// </summary>
    public DateTime DataCriacao { get; set; }
    
    /// <summary>
    /// Ação realizada pelo comprador
    /// </summary>
    public AcaoCompradorPedido? AcaoComprador { get; set; }
    
    /// <summary>
    /// Observação da proposta
    /// </summary>
    public string? Observacao { get; set; }
    
    /// <summary>
    /// Informações do usuário fornecedor (quando aplicável)
    /// </summary>
    public UsuarioFornecedorPropostaDto? UsuarioFornecedor { get; set; }
    
    /// <summary>
    /// Informações do usuário produtor (quando aplicável)
    /// </summary>
    public UsuarioProdutorPropostaDto? UsuarioProdutor { get; set; }
}

/// <summary>
/// DTO para informações do usuário fornecedor na proposta
/// </summary>
public class UsuarioFornecedorPropostaDto
{
    /// <summary>
    /// ID do usuário fornecedor
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Nome do usuário fornecedor
    /// </summary>
    public string Nome { get; set; } = string.Empty;
    
    /// <summary>
    /// Cargo do usuário fornecedor
    /// </summary>
    public string? Cargo { get; set; }
    
    /// <summary>
    /// URL da foto pequena
    /// </summary>
    public string? FotoSmall { get; set; }
}

/// <summary>
/// DTO para informações do usuário produtor na proposta
/// </summary>
public class UsuarioProdutorPropostaDto
{
    /// <summary>
    /// ID do usuário produtor
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Informações do usuário
    /// </summary>
    public UsuarioPropostaDto Usuario { get; set; } = new();
}

/// <summary>
/// DTO para informações básicas do usuário na proposta
/// </summary>
public class UsuarioPropostaDto
{
    /// <summary>
    /// ID do usuário
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Nome do usuário
    /// </summary>
    public string Nome { get; set; } = string.Empty;
    
    /// <summary>
    /// URL do logo pequeno
    /// </summary>
    public string? UrlLogoSmall { get; set; }
}