using Agriis.Compartilhado.Dominio.Enums;

namespace Agriis.Autenticacao.Aplicacao.DTOs;

/// <summary>
/// DTO para resposta de login
/// </summary>
public class LoginResponseDto
{
    /// <summary>
    /// Token JWT de acesso
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;
    
    /// <summary>
    /// Refresh token para renovação
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
    
    /// <summary>
    /// Tipo do token (sempre "Bearer")
    /// </summary>
    public string TokenType { get; set; } = "Bearer";
    
    /// <summary>
    /// Tempo de expiração do token em segundos
    /// </summary>
    public int ExpiresIn { get; set; }
    
    /// <summary>
    /// Dados do usuário logado
    /// </summary>
    public UsuarioLogadoDto Usuario { get; set; } = new();
}

/// <summary>
/// DTO com dados do usuário logado
/// </summary>
public class UsuarioLogadoDto
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
    /// Email do usuário
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Roles do usuário
    /// </summary>
    public IEnumerable<string> Roles { get; set; } = new List<string>();
    
    /// <summary>
    /// URL da logo do usuário
    /// </summary>
    public string? LogoUrl { get; set; }
    
    /// <summary>
    /// Data do último login (UTC com timezone)
    /// </summary>
    public DateTimeOffset? UltimoLogin { get; set; }
}