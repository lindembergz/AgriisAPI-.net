using Agriis.Compartilhado.Dominio.Enums;
using System.Security.Claims;

namespace Agriis.Autenticacao.Dominio.Interfaces;

/// <summary>
/// Interface para serviços de token JWT
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Gera um token JWT para o usuário
    /// </summary>
    /// <param name="usuarioId">ID do usuário</param>
    /// <param name="email">Email do usuário</param>
    /// <param name="nome">Nome do usuário</param>
    /// <param name="roles">Roles do usuário</param>
    /// <returns>Token JWT</returns>
    string GerarToken(int usuarioId, string email, string nome, IEnumerable<Roles> roles);
    
    /// <summary>
    /// Gera um refresh token único
    /// </summary>
    /// <returns>Refresh token</returns>
    string GerarRefreshToken();
    
    /// <summary>
    /// Valida um token JWT
    /// </summary>
    /// <param name="token">Token a ser validado</param>
    /// <returns>ClaimsPrincipal se válido, null se inválido</returns>
    ClaimsPrincipal? ValidarToken(string token);
    
    /// <summary>
    /// Obtém o ID do usuário a partir de um token
    /// </summary>
    /// <param name="token">Token JWT</param>
    /// <returns>ID do usuário ou null se inválido</returns>
    int? ObterUsuarioIdDoToken(string token);
    
    /// <summary>
    /// Obtém as claims de um token
    /// </summary>
    /// <param name="token">Token JWT</param>
    /// <returns>Claims do token</returns>
    IEnumerable<Claim> ObterClaimsDoToken(string token);
    
    /// <summary>
    /// Verifica se um token expirou
    /// </summary>
    /// <param name="token">Token JWT</param>
    /// <returns>True se expirou</returns>
    bool TokenExpirou(string token);
}