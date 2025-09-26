using System.ComponentModel.DataAnnotations;
using Agriis.Fornecedores.Aplicacao.Validadores;

namespace Agriis.Fornecedores.Aplicacao.DTOs;

/// <summary>
/// Request para atualizar um fornecedor
/// </summary>
public class AtualizarFornecedorRequest
{
    /// <summary>
    /// Nome/Razão social do fornecedor
    /// </summary>
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(200, ErrorMessage = "Nome deve ter no máximo 200 caracteres")]
    public string Nome { get; set; } = string.Empty;
    
    /// <summary>
    /// Inscrição estadual do fornecedor
    /// </summary>
    [StringLength(20, ErrorMessage = "Inscrição estadual deve ter no máximo 20 caracteres")]
    public string? InscricaoEstadual { get; set; }
    
    /// <summary>
    /// Logradouro do fornecedor
    /// </summary>
    [StringLength(500, ErrorMessage = "Logradouro deve ter no máximo 500 caracteres")]
    public string? Logradouro { get; set; }
    
    /// <summary>
    /// ID da UF do fornecedor
    /// </summary>
    public int? UfId { get; set; }
    
    /// <summary>
    /// ID do município do fornecedor
    /// </summary>
    [ValidarMunicipioUf(nameof(UfId))]
    public int? MunicipioId { get; set; }
    
    /// <summary>
    /// CEP do fornecedor
    /// </summary>
    [StringLength(10, ErrorMessage = "CEP deve ter no máximo 10 caracteres")]
    public string? Cep { get; set; }
    
    /// <summary>
    /// Complemento do endereço
    /// </summary>
    [StringLength(200, ErrorMessage = "Complemento deve ter no máximo 200 caracteres")]
    public string? Complemento { get; set; }
    
    /// <summary>
    /// Latitude da localização
    /// </summary>
    public decimal? Latitude { get; set; }
    
    /// <summary>
    /// Longitude da localização
    /// </summary>
    public decimal? Longitude { get; set; }
    
    /// <summary>
    /// Telefone de contato do fornecedor
    /// </summary>
    [StringLength(20, ErrorMessage = "Telefone deve ter no máximo 20 caracteres")]
    public string? Telefone { get; set; }
    
    /// <summary>
    /// Email de contato do fornecedor
    /// </summary>
    [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
    [StringLength(100, ErrorMessage = "Email deve ter no máximo 100 caracteres")]
    public string? Email { get; set; }
    
    /// <summary>
    /// Moeda padrão do fornecedor (0 = Real, 1 = Dólar)
    /// </summary>
    [Range(0, 1, ErrorMessage = "Moeda padrão deve ser 0 (Real) ou 1 (Dólar)")]
    public int MoedaPadrao { get; set; } = 0;
    
    /// <summary>
    /// Valor mínimo de pedido
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Pedido mínimo deve ser maior ou igual a zero")]
    public decimal? PedidoMinimo { get; set; }
    
    /// <summary>
    /// Token para integração Lincros
    /// </summary>
    [StringLength(100, ErrorMessage = "Token Lincros deve ter no máximo 100 caracteres")]
    public string? TokenLincros { get; set; }
}