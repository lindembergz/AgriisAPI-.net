using System.Diagnostics;
using System.Text;
using Agriis.Compartilhado.Infraestrutura.Logging;
using Microsoft.Extensions.Options;

namespace Agriis.Api.Middleware;

/// <summary>
/// Configurações para o middleware de logging de requisições
/// </summary>
public class RequestLoggingOptions
{
    public bool Enabled { get; set; } = true;
    public bool LogRequestBody { get; set; } = false;
    public bool LogResponseBody { get; set; } = false;
    public int MaxBodySizeKB { get; set; } = 10;
    public List<string> SensitiveHeaders { get; set; } = new();
    public List<string> SensitiveFields { get; set; } = new();
}

/// <summary>
/// Middleware para logging detalhado de requisições com contexto estruturado
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private readonly ILoggingContext _loggingContext;
    private readonly RequestLoggingOptions _options;

    public RequestLoggingMiddleware(
        RequestDelegate next, 
        ILogger<RequestLoggingMiddleware> logger,
        ILoggingContext loggingContext,
        IOptions<RequestLoggingOptions> options)
    {
        _next = next;
        _logger = logger;
        _loggingContext = loggingContext;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_options.Enabled)
        {
            await _next(context);
            return;
        }

        var correlationId = GetOrCreateCorrelationId(context);
        var stopwatch = Stopwatch.StartNew();

        // Configurar contexto de logging
        SetupLoggingContext(context, correlationId);

        // Log da requisição de entrada
        await LogRequestAsync(context, correlationId);

        // Capturar a resposta original
        var originalResponseBodyStream = context.Response.Body;
        using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Log da exceção com contexto
            _logger.LogStructuredError(ex, "Unhandled exception during request processing", new
            {
                CorrelationId = correlationId,
                RequestPath = context.Request.Path.Value,
                RequestMethod = context.Request.Method,
                UserId = context.User?.FindFirst("user_id")?.Value,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds
            });
            throw;
        }
        finally
        {
            stopwatch.Stop();

            // Log da resposta
            await LogResponseAsync(context, correlationId, stopwatch.ElapsedMilliseconds);

            // Restaurar o stream original e copiar o conteúdo
            await responseBodyStream.CopyToAsync(originalResponseBodyStream);
            context.Response.Body = originalResponseBodyStream;

            // Limpar contexto de logging
            _loggingContext.Clear();
        }
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        const string correlationIdHeader = "X-Correlation-ID";
        
        if (context.Request.Headers.TryGetValue(correlationIdHeader, out var correlationId))
        {
            return correlationId.ToString();
        }

        var newCorrelationId = Guid.NewGuid().ToString();
        context.Request.Headers[correlationIdHeader] = newCorrelationId;
        context.Response.Headers[correlationIdHeader] = newCorrelationId;
        
        return newCorrelationId;
    }

    private void SetupLoggingContext(HttpContext context, string correlationId)
    {
        _loggingContext.CorrelationId = correlationId;
        _loggingContext.RequestPath = context.Request.Path.Value;
        _loggingContext.RequestMethod = context.Request.Method;
        _loggingContext.RemoteIpAddress = context.Connection.RemoteIpAddress?.ToString();
        _loggingContext.UserAgent = context.Request.Headers.UserAgent.ToString();

        if (context.User?.Identity?.IsAuthenticated == true)
        {
            _loggingContext.UserId = context.User.FindFirst("user_id")?.Value;
            _loggingContext.UserEmail = context.User.FindFirst("email")?.Value;
        }
    }

    private async Task LogRequestAsync(HttpContext context, string correlationId)
    {
        var request = context.Request;
        
        var requestInfo = new
        {
            CorrelationId = correlationId,
            Method = request.Method,
            Path = request.Path.Value,
            QueryString = request.QueryString.Value,
            Headers = GetSafeHeaders(request.Headers.Select(h => new KeyValuePair<string, IEnumerable<string>>(h.Key, h.Value.AsEnumerable()))),
            UserAgent = request.Headers.UserAgent.ToString(),
            RemoteIpAddress = context.Connection.RemoteIpAddress?.ToString(),
            UserId = context.User?.FindFirst("user_id")?.Value,
            UserEmail = context.User?.FindFirst("email")?.Value,
            ContentType = request.ContentType,
            ContentLength = request.ContentLength
        };

        _logger.LogInformation("Incoming Request: {@RequestInfo}", requestInfo);

        // Log do body apenas para métodos que podem ter conteúdo e se habilitado
        if (_options.LogRequestBody && 
            (HttpMethods.IsPost(request.Method) || 
             HttpMethods.IsPut(request.Method) || 
             HttpMethods.IsPatch(request.Method)))
        {
            await LogRequestBodyAsync(request, correlationId);
        }
    }

    private async Task LogRequestBodyAsync(HttpRequest request, string correlationId)
    {
        var maxSizeBytes = _options.MaxBodySizeKB * 1024;
        
        if (request.ContentLength > 0 && request.ContentLength < maxSizeBytes)
        {
            request.EnableBuffering();
            
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;

            if (!string.IsNullOrWhiteSpace(body))
            {
                // Mascarar dados sensíveis
                var sanitizedBody = SanitizeSensitiveData(body);
                
                _logger.LogDebug("Request Body [{CorrelationId}]: {Body}", correlationId, sanitizedBody);
            }
        }
        else if (request.ContentLength >= maxSizeBytes)
        {
            _logger.LogDebug("Request Body [{CorrelationId}]: Body too large ({ContentLength} bytes), skipping log", 
                correlationId, request.ContentLength);
        }
    }

    private async Task LogResponseAsync(HttpContext context, string correlationId, long elapsedMilliseconds)
    {
        var response = context.Response;
        
        var responseInfo = new
        {
            CorrelationId = correlationId,
            StatusCode = response.StatusCode,
            ContentType = response.ContentType,
            ContentLength = response.ContentLength,
            ElapsedMilliseconds = elapsedMilliseconds,
            Headers = GetSafeHeaders(response.Headers.Select(h => new KeyValuePair<string, IEnumerable<string>>(h.Key, h.Value.AsEnumerable())))
        };

        var logLevel = GetLogLevelForStatusCode(response.StatusCode);
        _logger.Log(logLevel, "Outgoing Response: {@ResponseInfo}", responseInfo);

        // Log de performance se a requisição demorou muito
        if (elapsedMilliseconds > 1000) // > 1 segundo
        {
            _logger.LogPerformance("HTTP Request", TimeSpan.FromMilliseconds(elapsedMilliseconds), new
            {
                Path = context.Request.Path.Value,
                Method = context.Request.Method,
                StatusCode = response.StatusCode
            });
        }

        // Log do body da resposta se habilitado e necessário
        if (_options.LogResponseBody && ShouldLogResponseBody(response.StatusCode, response.ContentType))
        {
            await LogResponseBodyAsync(context, correlationId);
        }
    }

    private async Task LogResponseBodyAsync(HttpContext context, string correlationId)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        
        using var reader = new StreamReader(context.Response.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        context.Response.Body.Seek(0, SeekOrigin.Begin);

        var maxSizeBytes = _options.MaxBodySizeKB * 1024;
        
        if (!string.IsNullOrWhiteSpace(body) && body.Length < maxSizeBytes)
        {
            var sanitizedBody = SanitizeSensitiveData(body);
            _logger.LogDebug("Response Body [{CorrelationId}]: {Body}", correlationId, sanitizedBody);
        }
        else if (body.Length >= maxSizeBytes)
        {
            _logger.LogDebug("Response Body [{CorrelationId}]: Body too large ({BodyLength} bytes), skipping log", 
                correlationId, body.Length);
        }
    }

    private Dictionary<string, string> GetSafeHeaders(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
    {
        var sensitiveHeaders = new HashSet<string>(_options.SensitiveHeaders, StringComparer.OrdinalIgnoreCase);
        
        // Adicionar headers sensíveis padrão se não estiverem na configuração
        var defaultSensitiveHeaders = new[] { "Authorization", "Cookie", "Set-Cookie", "X-API-Key", "X-Auth-Token" };
        foreach (var header in defaultSensitiveHeaders)
        {
            sensitiveHeaders.Add(header);
        }

        return headers
            .Where(h => !sensitiveHeaders.Contains(h.Key))
            .ToDictionary(
                h => h.Key, 
                h => string.Join(", ", h.Value),
                StringComparer.OrdinalIgnoreCase);
    }

    private string SanitizeSensitiveData(string body)
    {
        var sensitiveFields = _options.SensitiveFields.Any() 
            ? _options.SensitiveFields 
            : new List<string> { "password", "senha", "token", "secret", "key", "cpf", "cnpj" };
        
        var sanitized = body;
        
        foreach (var field in sensitiveFields)
        {
            // Regex para mascarar valores de campos sensíveis em JSON
            var pattern = $@"""({field}"":\s*"")[^""]*("")";
            sanitized = System.Text.RegularExpressions.Regex.Replace(
                sanitized, 
                pattern, 
                $"$1***$2", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }
        
        return sanitized;
    }

    private static LogLevel GetLogLevelForStatusCode(int statusCode)
    {
        return statusCode switch
        {
            >= 500 => LogLevel.Error,
            >= 400 => LogLevel.Warning,
            _ => LogLevel.Information
        };
    }

    private static bool ShouldLogResponseBody(int statusCode, string? contentType)
    {
        // Log response body apenas para erros ou em desenvolvimento
        return statusCode >= 400 && 
               contentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true;
    }
}