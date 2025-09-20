using Serilog.Core;
using Serilog.Events;

namespace Agriis.Compartilhado.Infraestrutura.Logging;

/// <summary>
/// Enriquecedor que adiciona informações do usuário aos logs
/// </summary>
public class UserContextEnricher : ILogEventEnricher
{
    private readonly ILoggingContext _loggingContext;

    public UserContextEnricher(ILoggingContext loggingContext)
    {
        _loggingContext = loggingContext;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (!string.IsNullOrEmpty(_loggingContext.UserId))
        {
            var userIdProperty = propertyFactory.CreateProperty("UserId", _loggingContext.UserId);
            logEvent.AddPropertyIfAbsent(userIdProperty);
        }

        if (!string.IsNullOrEmpty(_loggingContext.UserEmail))
        {
            var userEmailProperty = propertyFactory.CreateProperty("UserEmail", _loggingContext.UserEmail);
            logEvent.AddPropertyIfAbsent(userEmailProperty);
        }

        // Adicionar propriedades customizadas
        var customProperties = _loggingContext.GetProperties();
        foreach (var kvp in customProperties)
        {
            if (!string.IsNullOrEmpty(kvp.Key) && kvp.Value != null)
            {
                // Evitar duplicar propriedades já adicionadas
                if (kvp.Key != nameof(_loggingContext.CorrelationId) &&
                    kvp.Key != nameof(_loggingContext.UserId) &&
                    kvp.Key != nameof(_loggingContext.UserEmail) &&
                    kvp.Key != nameof(_loggingContext.RequestPath) &&
                    kvp.Key != nameof(_loggingContext.RequestMethod) &&
                    kvp.Key != nameof(_loggingContext.RemoteIpAddress) &&
                    kvp.Key != nameof(_loggingContext.UserAgent))
                {
                    var customProperty = propertyFactory.CreateProperty(kvp.Key, kvp.Value);
                    logEvent.AddPropertyIfAbsent(customProperty);
                }
            }
        }
    }
}