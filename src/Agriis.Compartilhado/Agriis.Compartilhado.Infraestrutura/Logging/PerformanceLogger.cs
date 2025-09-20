using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Agriis.Compartilhado.Infraestrutura.Logging;

/// <summary>
/// Logger especializado para monitoramento de performance
/// </summary>
public interface IPerformanceLogger
{
    /// <summary>
    /// Inicia o monitoramento de uma operação
    /// </summary>
    IDisposable BeginOperation(string operationName, object? context = null);

    /// <summary>
    /// Log de performance de operação de banco de dados
    /// </summary>
    void LogDatabaseOperation(string operation, TimeSpan elapsed, int? recordCount = null, object? context = null);

    /// <summary>
    /// Log de performance de operação de API externa
    /// </summary>
    void LogExternalApiCall(string apiName, string endpoint, TimeSpan elapsed, bool success, object? context = null);

    /// <summary>
    /// Log de performance de operação de negócio
    /// </summary>
    void LogBusinessOperation(string operation, TimeSpan elapsed, object? context = null);

    /// <summary>
    /// Log de uso de memória
    /// </summary>
    void LogMemoryUsage(string operation, long memoryBefore, long memoryAfter, object? context = null);
}

/// <summary>
/// Implementação do logger de performance
/// </summary>
public class PerformanceLogger : IPerformanceLogger
{
    private readonly ILogger<PerformanceLogger> _logger;
    private readonly ILoggingContext _loggingContext;

    public PerformanceLogger(ILogger<PerformanceLogger> logger, ILoggingContext loggingContext)
    {
        _logger = logger;
        _loggingContext = loggingContext;
    }

    public IDisposable BeginOperation(string operationName, object? context = null)
    {
        return new PerformanceScope(_logger, _loggingContext, operationName, context);
    }

    public void LogDatabaseOperation(string operation, TimeSpan elapsed, int? recordCount = null, object? context = null)
    {
        var logLevel = GetLogLevelForDatabaseOperation(elapsed);
        
        _logger.Log(logLevel, 
            "DATABASE_PERFORMANCE: {Operation} completed in {ElapsedMilliseconds}ms with {RecordCount} records {@Context} {CorrelationId}",
            operation, elapsed.TotalMilliseconds, recordCount, context, _loggingContext.CorrelationId);

        // Log warning para operações muito lentas
        if (elapsed.TotalMilliseconds > 5000)
        {
            _logger.LogWarning(
                "SLOW_DATABASE_OPERATION: {Operation} took {ElapsedMilliseconds}ms - consider optimization {@Context}",
                operation, elapsed.TotalMilliseconds, context);
        }
    }

    public void LogExternalApiCall(string apiName, string endpoint, TimeSpan elapsed, bool success, object? context = null)
    {
        var logLevel = success ? LogLevel.Information : LogLevel.Warning;
        
        _logger.Log(logLevel,
            "EXTERNAL_API_PERFORMANCE: {ApiName} call to {Endpoint} completed in {ElapsedMilliseconds}ms with success={Success} {@Context} {CorrelationId}",
            apiName, endpoint, elapsed.TotalMilliseconds, success, context, _loggingContext.CorrelationId);

        // Log warning para chamadas muito lentas
        if (elapsed.TotalMilliseconds > 10000)
        {
            _logger.LogWarning(
                "SLOW_EXTERNAL_API: {ApiName} call to {Endpoint} took {ElapsedMilliseconds}ms {@Context}",
                apiName, endpoint, elapsed.TotalMilliseconds, context);
        }
    }

    public void LogBusinessOperation(string operation, TimeSpan elapsed, object? context = null)
    {
        var logLevel = GetLogLevelForBusinessOperation(elapsed);
        
        _logger.Log(logLevel,
            "BUSINESS_PERFORMANCE: {Operation} completed in {ElapsedMilliseconds}ms {@Context} {CorrelationId}",
            operation, elapsed.TotalMilliseconds, context, _loggingContext.CorrelationId);

        // Log warning para operações de negócio muito lentas
        if (elapsed.TotalMilliseconds > 3000)
        {
            _logger.LogWarning(
                "SLOW_BUSINESS_OPERATION: {Operation} took {ElapsedMilliseconds}ms - review business logic {@Context}",
                operation, elapsed.TotalMilliseconds, context);
        }
    }

    public void LogMemoryUsage(string operation, long memoryBefore, long memoryAfter, object? context = null)
    {
        var memoryDiff = memoryAfter - memoryBefore;
        var memoryDiffMB = memoryDiff / (1024.0 * 1024.0);

        var logLevel = Math.Abs(memoryDiffMB) > 100 ? LogLevel.Warning : LogLevel.Information;

        _logger.Log(logLevel,
            "MEMORY_USAGE: {Operation} changed memory by {MemoryDiffMB:F2}MB (before: {MemoryBeforeMB:F2}MB, after: {MemoryAfterMB:F2}MB) {@Context} {CorrelationId}",
            operation, memoryDiffMB, memoryBefore / (1024.0 * 1024.0), memoryAfter / (1024.0 * 1024.0), context, _loggingContext.CorrelationId);
    }

    private static LogLevel GetLogLevelForDatabaseOperation(TimeSpan elapsed)
    {
        return elapsed.TotalMilliseconds switch
        {
            > 5000 => LogLevel.Warning,  // > 5 segundos
            > 1000 => LogLevel.Information, // > 1 segundo
            _ => LogLevel.Debug
        };
    }

    private static LogLevel GetLogLevelForBusinessOperation(TimeSpan elapsed)
    {
        return elapsed.TotalMilliseconds switch
        {
            > 3000 => LogLevel.Warning,  // > 3 segundos
            > 1000 => LogLevel.Information, // > 1 segundo
            _ => LogLevel.Debug
        };
    }
}

/// <summary>
/// Scope para monitoramento automático de performance
/// </summary>
internal class PerformanceScope : IDisposable
{
    private readonly ILogger _logger;
    private readonly ILoggingContext _loggingContext;
    private readonly string _operationName;
    private readonly object? _context;
    private readonly Stopwatch _stopwatch;
    private readonly long _memoryBefore;
    private bool _disposed;

    public PerformanceScope(ILogger logger, ILoggingContext loggingContext, string operationName, object? context)
    {
        _logger = logger;
        _loggingContext = loggingContext;
        _operationName = operationName;
        _context = context;
        _stopwatch = Stopwatch.StartNew();
        _memoryBefore = GC.GetTotalMemory(false);

        _logger.LogDebug("PERFORMANCE_START: {Operation} started {@Context} {CorrelationId}",
            _operationName, _context, _loggingContext.CorrelationId);
    }

    public void Dispose()
    {
        if (_disposed) return;

        _stopwatch.Stop();
        var memoryAfter = GC.GetTotalMemory(false);
        var elapsed = _stopwatch.Elapsed;

        var logLevel = elapsed.TotalMilliseconds switch
        {
            > 5000 => LogLevel.Warning,
            > 1000 => LogLevel.Information,
            _ => LogLevel.Debug
        };

        _logger.Log(logLevel,
            "PERFORMANCE_END: {Operation} completed in {ElapsedMilliseconds}ms (memory change: {MemoryChangeMB:F2}MB) {@Context} {CorrelationId}",
            _operationName, elapsed.TotalMilliseconds, (memoryAfter - _memoryBefore) / (1024.0 * 1024.0), _context, _loggingContext.CorrelationId);

        _disposed = true;
    }
}