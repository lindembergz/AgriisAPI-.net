using Serilog.Core;
using Serilog.Events;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Agriis.Compartilhado.Infraestrutura.Logging;

/// <summary>
/// Enriquecedor que adiciona informações do usuário aos logs
/// </summary>
public class UserContextEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContextEnricher(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true) return;

        // User ID
        var userId = httpContext.User.FindFirst("user_id")?.Value ?? 
                    httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            var userIdProperty = propertyFactory.CreateProperty("UserId", userId);
            logEvent.AddPropertyIfAbsent(userIdProperty);
        }

        // User Email
        var userEmail = httpContext.User.FindFirst("email")?.Value ?? 
                       httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
        if (!string.IsNullOrEmpty(userEmail))
        {
            var userEmailProperty = propertyFactory.CreateProperty("UserEmail", userEmail);
            logEvent.AddPropertyIfAbsent(userEmailProperty);
        }

        // User Name
        var userName = httpContext.User.FindFirst("name")?.Value ?? 
                      httpContext.User.FindFirst(ClaimTypes.Name)?.Value;
        if (!string.IsNullOrEmpty(userName))
        {
            var userNameProperty = propertyFactory.CreateProperty("UserName", userName);
            logEvent.AddPropertyIfAbsent(userNameProperty);
        }

        // User Role
        var userRole = httpContext.User.FindFirst("role")?.Value ?? 
                      httpContext.User.FindFirst(ClaimTypes.Role)?.Value;
        if (!string.IsNullOrEmpty(userRole))
        {
            var userRoleProperty = propertyFactory.CreateProperty("UserRole", userRole);
            logEvent.AddPropertyIfAbsent(userRoleProperty);
        }
    }
}