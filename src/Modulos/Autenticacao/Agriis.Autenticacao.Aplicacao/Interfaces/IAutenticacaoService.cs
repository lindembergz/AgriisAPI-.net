using Agriis.Autenticacao.Aplicacao.DTOs;
using Agriis.Compartilhado.Aplicacao.Resultados;

namespace Agriis.Autenticacao.Aplicacao.Interfaces;

/// <summary>
/// Interface para serviços de autenticação
/// </summary>
public interface IAutenticacaoService
{
    /// <summary>
    /// Realiza o login do usuário
    /// </summary>
    /// <param name="request">Dados de login</param>
    /// <param name="enderecoIp">Endereço IP do cliente</param>
    /// <param name="userAgent">User Agent do cliente</param>
    /// <returns>Resultado do login com tokens</returns>
    Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto request, string? enderecoIp = null, string? userAgent = null);
    
    /// <summary>
    /// Renova o token de acesso usando refresh token
    /// </summary>
    /// <param name="request">Dados para renovação</param>
    /// <param name="enderecoIp">Endereço IP do cliente</param>
    /// <param name="userAgent">User Agent do cliente</param>
    /// <returns>Resultado com novos tokens</returns>
    Task<Result<LoginResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request, string? enderecoIp = null, string? userAgent = null);
    
    /// <summary>
    /// Realiza o logout do usuário
    /// </summary>
    /// <param name="usuarioId">ID do usuário</param>
    /// <param name="refreshToken">Refresh token a ser revogado (opcional)</param>
    /// <returns>Resultado da operação</returns>
    Task<Result<bool>> LogoutAsync(int usuarioId, string? refreshToken = null);
    
    /// <summary>
    /// Altera a senha do usuário
    /// </summary>
    /// <param name="usuarioId">ID do usuário</param>
    /// <param name="request">Dados para alteração de senha</param>
    /// <returns>Resultado da operação</returns>
    Task<Result<bool>> AlterarSenhaAsync(int usuarioId, AlterarSenhaRequestDto request);
    
    /// <summary>
    /// Valida se um token de acesso está válido
    /// </summary>
    /// <param name="token">Token a ser validado</param>
    /// <returns>True se o token é válido</returns>
    Task<bool> ValidarTokenAsync(string token);
    
    /// <summary>
    /// Revoga todos os refresh tokens de um usuário
    /// </summary>
    /// <param name="usuarioId">ID do usuário</param>
    /// <returns>Resultado da operação</returns>
    Task<Result<bool>> RevogarTodosTokensAsync(int usuarioId);
}