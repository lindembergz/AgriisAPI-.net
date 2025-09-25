using System.ComponentModel.DataAnnotations;

namespace Agriis.Fornecedores.Aplicacao.DTOs;

/// <summary>
/// Request para atualizar um fornecedor com estrutura completa (frontend)
/// </summary>
public class AtualizarFornecedorCompletoRequest
{
    /// <summary>
    /// Código do fornecedor
    /// </summary>
    public string? Codigo { get; set; }
    
    /// <summary>
    /// Nome/Razão social do fornecedor
    /// </summary>
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(200, ErrorMessage = "Nome deve ter no máximo 200 caracteres")]
    public string Nome { get; set; } = string.Empty;
    
    /// <summary>
    /// CPF/CNPJ do fornecedor
    /// </summary>
    [Required(ErrorMessage = "CPF/CNPJ é obrigatório")]
    [StringLength(18, ErrorMessage = "CPF/CNPJ deve ter no máximo 18 caracteres")]
    public string CpfCnpj { get; set; } = string.Empty;
    
    /// <summary>
    /// Tipo de cliente (PF ou PJ)
    /// </summary>
    [Required(ErrorMessage = "Tipo de cliente é obrigatório")]
    public string TipoCliente { get; set; } = string.Empty;
    
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
    /// Inscrição estadual do fornecedor
    /// </summary>
    [StringLength(50, ErrorMessage = "Inscrição estadual deve ter no máximo 50 caracteres")]
    public string? InscricaoEstadual { get; set; }
    
    /// <summary>
    /// Endereço do fornecedor
    /// </summary>
    public EnderecoRequest? Endereco { get; set; }
    
    /// <summary>
    /// Lista de pontos de distribuição
    /// </summary>
    public List<PontoDistribuicaoRequest> PontosDistribuicao { get; set; } = new();
    
    /// <summary>
    /// Dados do usuário master
    /// </summary>
    [Required(ErrorMessage = "Usuário master é obrigatório")]
    public UsuarioMasterRequest UsuarioMaster { get; set; } = null!;
}