using Serilog.Core;
using Serilog.Events;

namespace Agriis.Compartilhado.Infraestrutura.Logging;

/// <summary>
/// Enriquecedor que adiciona o ID de correlação aos logs
/// </summary>
public class CorrelationIdEnricher : ILogEventEnricher
{
    private readonly ILoggingContext _loggingContext;

    public CorrelationIdEnricher(ILoggingContext loggingContext)
    {
        _loggingContext = loggingContext;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (!string.IsNullOrEmpty(_loggingContext.CorrelationId))
        {
            var correlationIdProperty = propertyFactory.CreateProperty("CorrelationId", _loggingContext.CorrelationId);
            logEvent.AddPropertyIfAbsent(correlationIdProperty);
        }

        if (!string.IsNullOrEmpty(_loggingContext.RequestPath))
        {
            var requestPathProperty = propertyFactory.CreateProperty("RequestPath", _loggingContext.RequestPath);
            logEvent.AddPropertyIfAbsent(requestPathProperty);
        }

        if (!string.IsNullOrEmpty(_loggingContext.RequestMethod))
        {
            var requestMethodProperty = propertyFactory.CreateProperty("RequestMethod", _loggingContext.RequestMethod);
            logEvent.AddPropertyIfAbsent(requestMethodProperty);
        }

        if (!string.IsNullOrEmpty(_loggingContext.RemoteIpAddress))
        {
            var remoteIpProperty = propertyFactory.CreateProperty("RemoteIpAddress", _loggingContext.RemoteIpAddress);
            logEvent.AddPropertyIfAbsent(remoteIpProperty);
        }

        if (!string.IsNullOrEmpty(_loggingContext.UserAgent))
        {
            var userAgentProperty = propertyFactory.CreateProperty("UserAgent", _loggingContext.UserAgent);
            logEvent.AddPropertyIfAbsent(userAgentProperty);
        }
    }
}