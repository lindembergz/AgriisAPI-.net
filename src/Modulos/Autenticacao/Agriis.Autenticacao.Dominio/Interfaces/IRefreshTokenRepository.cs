using Agriis.Autenticacao.Dominio.Entidades;
using Agriis.Compartilhado.Dominio.Interfaces;

namespace Agriis.Autenticacao.Dominio.Interfaces;

/// <summary>
/// Interface para repositório de refresh tokens
/// </summary>
public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    /// <summary>
    /// Obtém um refresh token pelo valor do token
    /// </summary>
    /// <param name="token">Token a ser buscado</param>
    /// <returns>Refresh token encontrado ou null</returns>
    Task<RefreshToken?> ObterPorTokenAsync(string token);
    
    /// <summary>
    /// Obtém todos os refresh tokens válidos de um usuário
    /// </summary>
    /// <param name="usuarioId">ID do usuário</param>
    /// <returns>Lista de refresh tokens válidos</returns>
    Task<IEnumerable<RefreshToken>> ObterTokensValidosPorUsuarioAsync(int usuarioId);
    
    /// <summary>
    /// Revoga todos os refresh tokens de um usuário
    /// </summary>
    /// <param name="usuarioId">ID do usuário</param>
    /// <returns>Task</returns>
    Task RevogarTodosTokensUsuarioAsync(int usuarioId);
    
    /// <summary>
    /// Remove refresh tokens expirados
    /// </summary>
    /// <returns>Número de tokens removidos</returns>
    Task<int> RemoverTokensExpiradosAsync();
    
    /// <summary>
    /// Verifica se existe um refresh token válido para o usuário
    /// </summary>
    /// <param name="usuarioId">ID do usuário</param>
    /// <returns>True se existe token válido</returns>
    Task<bool> ExisteTokenValidoParaUsuarioAsync(int usuarioId);
}