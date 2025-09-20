namespace Agriis.Pagamentos.Aplicacao.DTOs;

/// <summary>
/// DTO para forma de pagamento
/// </summary>
public class FormaPagamentoDto
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
    /// Indica se a forma de pagamento está ativa
    /// </summary>
    public bool Ativo { get; set; }
    
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
/// DTO para criação de forma de pagamento
/// </summary>
public class CriarFormaPagamentoDto
{
    /// <summary>
    /// Descrição da forma de pagamento
    /// </summary>
    public string Descricao { get; set; } = string.Empty;
}

/// <summary>
/// DTO para atualização de forma de pagamento
/// </summary>
public class AtualizarFormaPagamentoDto
{
    /// <summary>
    /// Descrição da forma de pagamento
    /// </summary>
    public string Descricao { get; set; } = string.Empty;
    
    /// <summary>
    /// Indica se a forma de pagamento está ativa
    /// </summary>
    public bool Ativo { get; set; }
}