namespace Agriis.Pagamentos.Aplicacao.DTOs;

/// <summary>
/// DTO para associação cultura-fornecedor-forma de pagamento
/// </summary>
public class CulturaFormaPagamentoDto
{
    /// <summary>
    /// ID da associação
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// ID do fornecedor
    /// </summary>
    public int FornecedorId { get; set; }
    
    /// <summary>
    /// ID da cultura
    /// </summary>
    public int CulturaId { get; set; }
    
    /// <summary>
    /// ID da forma de pagamento
    /// </summary>
    public int FormaPagamentoId { get; set; }
    
    /// <summary>
    /// Indica se a associação está ativa
    /// </summary>
    public bool Ativo { get; set; }
    
    /// <summary>
    /// Dados da forma de pagamento
    /// </summary>
    public FormaPagamentoDto? FormaPagamento { get; set; }
    
    /// <summary>
    /// Data de criação
    /// </summary>
    public DateTime DataCriacao { get; set; }
    
    /// <summary>
    /// Data da última atualização
    /// </summary>
    public DateTime? DataAtualizacao { get; set; }
}

/// <summary>
/// DTO para criação de associação cultura-fornecedor-forma de pagamento
/// </summary>
public class CriarCulturaFormaPagamentoDto
{
    /// <summary>
    /// ID do fornecedor
    /// </summary>
    public int FornecedorId { get; set; }
    
    /// <summary>
    /// ID da cultura
    /// </summary>
    public int CulturaId { get; set; }
    
    /// <summary>
    /// ID da forma de pagamento
    /// </summary>
    public int FormaPagamentoId { get; set; }
}

/// <summary>
/// DTO para formas de pagamento disponíveis por pedido
/// </summary>
public class FormaPagamentoPedidoDto
{
    /// <summary>
    /// ID da forma de pagamento
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Descrição da forma de pagamento
    /// </summary>
    public string Descricao { get; set; } = string.Empty;
    
    /// <summary>
    /// ID da associação cultura-forma-pagamento
    /// </summary>
    public int CulturaFormaPagamentoId { get; set; }
}