using System.ComponentModel.DataAnnotations;

namespace Agriis.Autenticacao.Aplicacao.DTOs;

/// <summary>
/// DTO para requisição de login
/// </summary>
public class LoginRequestDto
{
    /// <summary>
    /// Email do usuário
    /// </summary>
    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Senha do usuário
    /// </summary>
    [Required(ErrorMessage = "Senha é obrigatória")]
    [MinLength(6, ErrorMessage = "Senha deve ter pelo menos 6 caracteres")]
    public string Senha { get; set; } = string.Empty;
    
    /// <summary>
    /// Indica se deve lembrar do login (manter sessão)
    /// </summary>
    public bool LembrarLogin { get; set; } = false;
}