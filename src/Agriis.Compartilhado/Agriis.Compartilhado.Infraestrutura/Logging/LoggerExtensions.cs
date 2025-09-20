using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace Agriis.Compartilhado.Infraestrutura.Logging;

/// <summary>
/// Extensões para logging estruturado
/// </summary>
public static class LoggerExtensions
{
    /// <summary>
    /// Log de início de operação
    /// </summary>
    public static IDisposable BeginOperation(this ILogger logger, string operationName, object? parameters = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        var operationId = Guid.NewGuid().ToString("N")[..8];
        
        logger.LogInformation("Starting operation {OperationName} with ID {OperationId} at {MemberName} in {SourceFile}:{SourceLine} {@Parameters}",
            operationName, operationId, memberName, Path.GetFileName(sourceFilePath), sourceLineNumber, parameters);

        return new OperationScope(logger, operationName, operationId);
    }

    /// <summary>
    /// Log de erro com contexto estruturado
    /// </summary>
    public static void LogStructuredError(this ILogger logger, Exception exception, string message, object? context = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        logger.LogError(exception, 
            "Error in {MemberName} at {SourceFile}:{SourceLine} - {Message} {@Context}",
            memberName, Path.GetFileName(sourceFilePath), sourceLineNumber, message, context);
    }

    /// <summary>
    /// Log de warning com contexto estruturado
    /// </summary>
    public static void LogStructuredWarning(this ILogger logger, string message, object? context = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        logger.LogWarning(
            "Warning in {MemberName} at {SourceFile}:{SourceLine} - {Message} {@Context}",
            memberName, Path.GetFileName(sourceFilePath), sourceLineNumber, message, context);
    }

    /// <summary>
    /// Log de informação com contexto estruturado
    /// </summary>
    public static void LogStructuredInformation(this ILogger logger, string message, object? context = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        logger.LogInformation(
            "Info in {MemberName} at {SourceFile}:{SourceLine} - {Message} {@Context}",
            memberName, Path.GetFileName(sourceFilePath), sourceLineNumber, message, context);
    }

    /// <summary>
    /// Log de debug com contexto estruturado
    /// </summary>
    public static void LogStructuredDebug(this ILogger logger, string message, object? context = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        logger.LogDebug(
            "Debug in {MemberName} at {SourceFile}:{SourceLine} - {Message} {@Context}",
            memberName, Path.GetFileName(sourceFilePath), sourceLineNumber, message, context);
    }

    /// <summary>
    /// Log de performance para operações demoradas
    /// </summary>
    public static void LogPerformance(this ILogger logger, string operationName, TimeSpan elapsed, object? context = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        var logLevel = elapsed.TotalMilliseconds switch
        {
            > 5000 => LogLevel.Warning, // > 5 segundos
            > 1000 => LogLevel.Information, // > 1 segundo
            _ => LogLevel.Debug
        };

        logger.Log(logLevel,
            "Performance: {OperationName} completed in {ElapsedMilliseconds}ms at {MemberName} in {SourceFile}:{SourceLine} {@Context}",
            operationName, elapsed.TotalMilliseconds, memberName, Path.GetFileName(sourceFilePath), sourceLineNumber, context);
    }

    /// <summary>
    /// Log de auditoria para ações importantes
    /// </summary>
    public static void LogAudit(this ILogger logger, string action, string? userId = null, object? details = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        logger.LogInformation(
            "AUDIT: {Action} by user {UserId} at {MemberName} in {SourceFile}:{SourceLine} {@Details}",
            action, userId ?? "Anonymous", memberName, Path.GetFileName(sourceFilePath), sourceLineNumber, details);
    }

    /// <summary>
    /// Log de segurança para eventos relacionados à segurança
    /// </summary>
    public static void LogSecurity(this ILogger logger, string securityEvent, string? userId = null, string? ipAddress = null, object? details = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        logger.LogWarning(
            "SECURITY: {SecurityEvent} by user {UserId} from IP {IpAddress} at {MemberName} in {SourceFile}:{SourceLine} {@Details}",
            securityEvent, userId ?? "Anonymous", ipAddress ?? "Unknown", memberName, Path.GetFileName(sourceFilePath), sourceLineNumber, details);
    }

    /// <summary>
    /// Log de business events para eventos de negócio importantes
    /// </summary>
    public static void LogBusinessEvent(this ILogger logger, string eventName, object eventData,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        logger.LogInformation(
            "BUSINESS_EVENT: {EventName} at {MemberName} in {SourceFile}:{SourceLine} {@EventData}",
            eventName, memberName, Path.GetFileName(sourceFilePath), sourceLineNumber, eventData);
    }
}

/// <summary>
/// Scope para operações com logging automático de início e fim
/// </summary>
internal class OperationScope : IDisposable
{
    private readonly ILogger _logger;
    private readonly string _operationName;
    private readonly string _operationId;
    private readonly DateTime _startTime;
    private bool _disposed;

    public OperationScope(ILogger logger, string operationName, string operationId)
    {
        _logger = logger;
        _operationName = operationName;
        _operationId = operationId;
        _startTime = DateTime.UtcNow;
    }

    public void Dispose()
    {
        if (_disposed) return;

        var elapsed = DateTime.UtcNow - _startTime;
        _logger.LogInformation("Completed operation {OperationName} with ID {OperationId} in {ElapsedMilliseconds}ms",
            _operationName, _operationId, elapsed.TotalMilliseconds);

        _disposed = true;
    }
}