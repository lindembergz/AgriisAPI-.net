using Agriis.Autenticacao.Dominio.Interfaces;
using System.Security.Claims;

namespace Agriis.Api.Middleware;

/// <summary>
/// Middleware para autenticação JWT
/// </summary>
public class JwtAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtAuthenticationMiddleware> _logger;

    public JwtAuthenticationMiddleware(RequestDelegate next, ILogger<JwtAuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITokenService tokenService)
    {
        try
        {
            var token = ExtractTokenFromHeader(context);
            
            if (!string.IsNullOrEmpty(token))
            {
                var principal = tokenService.ValidarToken(token);
                
                if (principal != null)
                {
                    context.User = principal;
                    
                    // Log do usuário autenticado
                    var userId = principal.FindFirst("user_id")?.Value;
                    var email = principal.FindFirst("email")?.Value;
                    
                    _logger.LogDebug("Usuário autenticado: {UserId} - {Email}", userId, email);
                }
                else
                {
                    _logger.LogWarning("Token JWT inválido recebido");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar token JWT");
        }

        await _next(context);
    }

    private static string? ExtractTokenFromHeader(HttpContext context)
    {
        var authorizationHeader = context.Request.Headers.Authorization.FirstOrDefault();
        
        if (string.IsNullOrEmpty(authorizationHeader))
            return null;

        if (authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return authorizationHeader["Bearer ".Length..].Trim();
        }

        return null;
    }
}