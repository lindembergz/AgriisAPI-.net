using System.ComponentModel.DataAnnotations;

namespace Agriis.Autenticacao.Aplicacao.DTOs;

/// <summary>
/// DTO para requisição de renovação de token
/// </summary>
public class RefreshTokenRequestDto
{
    /// <summary>
    /// Token de acesso atual (pode estar expirado)
    /// </summary>
    [Required(ErrorMessage = "Token de acesso é obrigatório")]
    public string AccessToken { get; set; } = string.Empty;
    
    /// <summary>
    /// Refresh token para renovação
    /// </summary>
    [Required(ErrorMessage = "Refresh token é obrigatório")]
    public string RefreshToken { get; set; } = string.Empty;
}