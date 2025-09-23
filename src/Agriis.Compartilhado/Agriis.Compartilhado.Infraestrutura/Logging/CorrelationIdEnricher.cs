using Serilog.Core;
using Serilog.Events;
using Microsoft.AspNetCore.Http;

namespace Agriis.Compartilhado.Infraestrutura.Logging;

/// <summary>
/// Enriquecedor que adiciona o ID de correlação aos logs
/// </summary>
public class CorrelationIdEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CorrelationIdEnricher(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return;

        // Correlation ID
        if (httpContext.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
        {
            var correlationIdProperty = propertyFactory.CreateProperty("CorrelationId", correlationId.ToString());
            logEvent.AddPropertyIfAbsent(correlationIdProperty);
        }

        // Request Path
        var requestPath = httpContext.Request.Path.Value;
        if (!string.IsNullOrEmpty(requestPath))
        {
            var requestPathProperty = propertyFactory.CreateProperty("RequestPath", requestPath);
            logEvent.AddPropertyIfAbsent(requestPathProperty);
        }

        // Request Method
        var requestMethod = httpContext.Request.Method;
        if (!string.IsNullOrEmpty(requestMethod))
        {
            var requestMethodProperty = propertyFactory.CreateProperty("RequestMethod", requestMethod);
            logEvent.AddPropertyIfAbsent(requestMethodProperty);
        }

        // Remote IP Address
        var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString();
        if (!string.IsNullOrEmpty(remoteIp))
        {
            var remoteIpProperty = propertyFactory.CreateProperty("RemoteIpAddress", remoteIp);
            logEvent.AddPropertyIfAbsent(remoteIpProperty);
        }

        // User Agent
        var userAgent = httpContext.Request.Headers.UserAgent.FirstOrDefault();
        if (!string.IsNullOrEmpty(userAgent))
        {
            var userAgentProperty = propertyFactory.CreateProperty("UserAgent", userAgent);
            logEvent.AddPropertyIfAbsent(userAgentProperty);
        }
    }
}