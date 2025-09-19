using System.ComponentModel.DataAnnotations;

namespace Agriis.Autenticacao.Aplicacao.DTOs;

/// <summary>
/// DTO para requisição de alteração de senha
/// </summary>
public class AlterarSenhaRequestDto
{
    /// <summary>
    /// Senha atual do usuário
    /// </summary>
    [Required(ErrorMessage = "Senha atual é obrigatória")]
    public string SenhaAtual { get; set; } = string.Empty;
    
    /// <summary>
    /// Nova senha do usuário
    /// </summary>
    [Required(ErrorMessage = "Nova senha é obrigatória")]
    [MinLength(6, ErrorMessage = "Nova senha deve ter pelo menos 6 caracteres")]
    public string NovaSenha { get; set; } = string.Empty;
    
    /// <summary>
    /// Confirmação da nova senha
    /// </summary>
    [Required(ErrorMessage = "Confirmação da senha é obrigatória")]
    [Compare(nameof(NovaSenha), ErrorMessage = "Confirmação deve ser igual à nova senha")]
    public string ConfirmacaoSenha { get; set; } = string.Empty;
}