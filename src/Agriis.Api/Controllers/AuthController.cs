using Agriis.Autenticacao.Aplicacao.DTOs;
using Agriis.Autenticacao.Aplicacao.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Agriis.Api.Controllers;

/// <summary>
/// Controller para autenticação e autorização
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAutenticacaoService _autenticacaoService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAutenticacaoService autenticacaoService, ILogger<AuthController> logger)
    {
        _autenticacaoService = autenticacaoService;
        _logger = logger;
    }

    /// <summary>
    /// Realiza o login do usuário
    /// </summary>
    /// <param name="request">Dados de login</param>
    /// <returns>Tokens de acesso e dados do usuário</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { 
                    error_code = "VALIDATION_ERROR", 
                    error_description = "Dados inválidos",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var enderecoIp = ObterEnderecoIp();
            var userAgent = Request.Headers.UserAgent.ToString();

            var resultado = await _autenticacaoService.LoginAsync(request, enderecoIp, userAgent);

            if (resultado.IsSuccess)
            {
                _logger.LogInformation("Login realizado com sucesso para: {Email}", request.Email);
                return Ok(resultado.Value);
            }

            _logger.LogWarning("Falha no login para: {Email} - {Error}", request.Email, resultado.Error);
            return BadRequest(new { 
                error_code = "LOGIN_FAILED", 
                error_description = resultado.Error 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno no login para: {Email}", request.Email);
            return StatusCode(500, new { 
                error_code = "INTERNAL_ERROR", 
                error_description = "Erro interno do servidor" 
            });
        }
    }

    /// <summary>
    /// Renova o token de acesso usando refresh token
    /// </summary>
    /// <param name="request">Dados para renovação</param>
    /// <returns>Novos tokens de acesso</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { 
                    error_code = "VALIDATION_ERROR", 
                    error_description = "Dados inválidos",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var enderecoIp = ObterEnderecoIp();
            var userAgent = Request.Headers.UserAgent.ToString();

            var resultado = await _autenticacaoService.RefreshTokenAsync(request, enderecoIp, userAgent);

            if (resultado.IsSuccess)
            {
                _logger.LogInformation("Token renovado com sucesso");
                return Ok(resultado.Value);
            }

            _logger.LogWarning("Falha na renovação de token: {Error}", resultado.Error);
            return BadRequest(new { 
                error_code = "REFRESH_FAILED", 
                error_description = resultado.Error 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno na renovação de token");
            return StatusCode(500, new { 
                error_code = "INTERNAL_ERROR", 
                error_description = "Erro interno do servidor" 
            });
        }
    }

    /// <summary>
    /// Realiza o logout do usuário
    /// </summary>
    /// <param name="refreshToken">Refresh token a ser revogado (opcional)</param>
    /// <returns>Resultado da operação</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Logout([FromBody] string? refreshToken = null)
    {
        try
        {
            var usuarioId = ObterUsuarioId();
            if (usuarioId == null)
            {
                return Unauthorized(new { 
                    error_code = "UNAUTHORIZED", 
                    error_description = "Usuário não autenticado" 
                });
            }

            var resultado = await _autenticacaoService.LogoutAsync(usuarioId.Value, refreshToken);

            if (resultado.IsSuccess)
            {
                _logger.LogInformation("Logout realizado com sucesso para usuário: {UsuarioId}", usuarioId);
                return Ok(new { message = "Logout realizado com sucesso" });
            }

            return BadRequest(new { 
                error_code = "LOGOUT_FAILED", 
                error_description = resultado.Error 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno no logout");
            return StatusCode(500, new { 
                error_code = "INTERNAL_ERROR", 
                error_description = "Erro interno do servidor" 
            });
        }
    }

    /// <summary>
    /// Altera a senha do usuário autenticado
    /// </summary>
    /// <param name="request">Dados para alteração de senha</param>
    /// <returns>Resultado da operação</returns>
    [HttpPost("alterar-senha")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AlterarSenha([FromBody] AlterarSenhaRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { 
                    error_code = "VALIDATION_ERROR", 
                    error_description = "Dados inválidos",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var usuarioId = ObterUsuarioId();
            if (usuarioId == null)
            {
                return Unauthorized(new { 
                    error_code = "UNAUTHORIZED", 
                    error_description = "Usuário não autenticado" 
                });
            }

            var resultado = await _autenticacaoService.AlterarSenhaAsync(usuarioId.Value, request);

            if (resultado.IsSuccess)
            {
                _logger.LogInformation("Senha alterada com sucesso para usuário: {UsuarioId}", usuarioId);
                return Ok(new { message = "Senha alterada com sucesso" });
            }

            return BadRequest(new { 
                error_code = "PASSWORD_CHANGE_FAILED", 
                error_description = resultado.Error 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno na alteração de senha");
            return StatusCode(500, new { 
                error_code = "INTERNAL_ERROR", 
                error_description = "Erro interno do servidor" 
            });
        }
    }

    /// <summary>
    /// Obtém informações do usuário autenticado
    /// </summary>
    /// <returns>Dados do usuário</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public IActionResult ObterUsuarioAtual()
    {
        try
        {
            var usuarioId = ObterUsuarioId();
            var email = User.FindFirst("email")?.Value ?? User.FindFirst(ClaimTypes.Email)?.Value;
            var nome = User.FindFirst("name")?.Value ?? User.FindFirst(ClaimTypes.Name)?.Value;
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            if (usuarioId == null)
            {
                return Unauthorized(new { 
                    error_code = "UNAUTHORIZED", 
                    error_description = "Usuário não autenticado" 
                });
            }

            return Ok(new
            {
                id = usuarioId,
                email,
                nome,
                roles
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter dados do usuário atual");
            return StatusCode(500, new { 
                error_code = "INTERNAL_ERROR", 
                error_description = "Erro interno do servidor" 
            });
        }
    }

    /// <summary>
    /// Revoga todos os refresh tokens do usuário
    /// </summary>
    /// <returns>Resultado da operação</returns>
    [HttpPost("revogar-tokens")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RevogarTodosTokens()
    {
        try
        {
            var usuarioId = ObterUsuarioId();
            if (usuarioId == null)
            {
                return Unauthorized(new { 
                    error_code = "UNAUTHORIZED", 
                    error_description = "Usuário não autenticado" 
                });
            }

            var resultado = await _autenticacaoService.RevogarTodosTokensAsync(usuarioId.Value);

            if (resultado.IsSuccess)
            {
                _logger.LogInformation("Todos os tokens revogados para usuário: {UsuarioId}", usuarioId);
                return Ok(new { message = "Todos os tokens foram revogados com sucesso" });
            }

            return BadRequest(new { 
                error_code = "REVOKE_FAILED", 
                error_description = resultado.Error 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno ao revogar tokens");
            return StatusCode(500, new { 
                error_code = "INTERNAL_ERROR", 
                error_description = "Erro interno do servidor" 
            });
        }
    }

    /// <summary>
    /// Obtém o ID do usuário autenticado
    /// </summary>
    /// <returns>ID do usuário ou null</returns>
    private int? ObterUsuarioId()
    {
        var userIdClaim = User.FindFirst("user_id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (int.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        return null;
    }

    /// <summary>
    /// Obtém o endereço IP do cliente
    /// </summary>
    /// <returns>Endereço IP</returns>
    private string? ObterEnderecoIp()
    {
        return Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? 
               Request.Headers["X-Real-IP"].FirstOrDefault() ?? 
               HttpContext.Connection.RemoteIpAddress?.ToString();
    }
}