using System.ComponentModel.DataAnnotations;

namespace Agriis.Produtores.Aplicacao.DTOs;

/// <summary>
/// Request para criar um novo produtor com estrutura completa (frontend)
/// </summary>
public class CriarProdutorCompletoRequest
{
    /// <summary>
    /// Código do produtor (gerado automaticamente)
    /// </summary>
    public string? Codigo { get; set; }
    
    /// <summary>
    /// Nome completo do produtor
    /// </summary>
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(200, ErrorMessage = "Nome deve ter no máximo 200 caracteres")]
    public string Nome { get; set; } = string.Empty;
    
    /// <summary>
    /// CPF/CNPJ do produtor
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
    /// Inscrição estadual do produtor
    /// </summary>
    [StringLength(50, ErrorMessage = "Inscrição estadual deve ter no máximo 50 caracteres")]
    public string? InscricaoEstadual { get; set; }
    
    /// <summary>
    /// Tipo de atividade desenvolvida pelo produtor
    /// </summary>
    [StringLength(100, ErrorMessage = "Tipo de atividade deve ter no máximo 100 caracteres")]
    public string? TipoAtividade { get; set; }
    
    /// <summary>
    /// Telefone principal do produtor
    /// </summary>
    [StringLength(20, ErrorMessage = "Telefone 1 deve ter no máximo 20 caracteres")]
    public string? Telefone1 { get; set; }
    
    /// <summary>
    /// Telefone secundário do produtor
    /// </summary>
    [StringLength(20, ErrorMessage = "Telefone 2 deve ter no máximo 20 caracteres")]
    public string? Telefone2 { get; set; }
    
    /// <summary>
    /// Telefone terciário do produtor
    /// </summary>
    [StringLength(20, ErrorMessage = "Telefone 3 deve ter no máximo 20 caracteres")]
    public string? Telefone3 { get; set; }
    
    /// <summary>
    /// Email do produtor
    /// </summary>
    [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
    [StringLength(100, ErrorMessage = "Email deve ter no máximo 100 caracteres")]
    public string? Email { get; set; }
    
    /// <summary>
    /// Área total de plantio em hectares
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Área de plantio deve ser maior ou igual a zero")]
    public decimal AreaPlantio { get; set; }
    
    /// <summary>
    /// Lista de IDs das culturas associadas ao produtor
    /// </summary>
    public List<int> Culturas { get; set; } = new();
    
    /// <summary>
    /// Dados do usuário master
    /// </summary>
    [Required(ErrorMessage = "Usuário master é obrigatório")]
    public UsuarioMasterProdutorRequest UsuarioMaster { get; set; } = null!;
}

/// <summary>
/// Request para usuário master do produtor
/// </summary>
public class UsuarioMasterProdutorRequest
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
    
    /// <summary>
    /// CPF do usuário (opcional, pode ser diferente do CPF do produtor)
    /// </summary>
    [StringLength(14, ErrorMessage = "CPF deve ter no máximo 14 caracteres")]
    public string? Cpf { get; set; }
}