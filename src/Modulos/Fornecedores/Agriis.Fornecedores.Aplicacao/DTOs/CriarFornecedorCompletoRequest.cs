using System.ComponentModel.DataAnnotations;

namespace Agriis.Fornecedores.Aplicacao.DTOs;

/// <summary>
/// Request para criar um novo fornecedor com estrutura completa (frontend)
/// </summary>
public class CriarFornecedorCompletoRequest
{
    /// <summary>
    /// Código do fornecedor (gerado automaticamente)
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

/// <summary>
/// Request para endereço
/// </summary>
public class EnderecoRequest
{
    [Required(ErrorMessage = "Logradouro é obrigatório")]
    public string Logradouro { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Número é obrigatório")]
    public string Numero { get; set; } = string.Empty;
    
    public string? Complemento { get; set; }
    
    [Required(ErrorMessage = "Bairro é obrigatório")]
    public string Bairro { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Cidade é obrigatória")]
    public string Cidade { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "UF é obrigatória")]
    public string Uf { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "CEP é obrigatório")]
    public string Cep { get; set; } = string.Empty;
    
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}

/// <summary>
/// Request para ponto de distribuição
/// </summary>
public class PontoDistribuicaoRequest
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    public string Nome { get; set; } = string.Empty;
    
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    
    [Required(ErrorMessage = "Endereço é obrigatório")]
    public EnderecoRequest Endereco { get; set; } = null!;
}

/// <summary>
/// Request para usuário master
/// </summary>
public class UsuarioMasterRequest
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
    public string Nome { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
    [StringLength(100, ErrorMessage = "Email deve ter no máximo 100 caracteres")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Senha é obrigatória")]
    [StringLength(50, MinimumLength = 6, ErrorMessage = "Senha deve ter entre 6 e 50 caracteres")]
    public string Senha { get; set; } = string.Empty;
    
    [StringLength(20, ErrorMessage = "Telefone deve ter no máximo 20 caracteres")]
    public string? Telefone { get; set; }
}