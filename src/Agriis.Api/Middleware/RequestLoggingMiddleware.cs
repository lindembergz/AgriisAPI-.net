using System.Diagnostics;
using System.Text;

namespace Agriis.Api.Middleware;

/// <summary>
/// Middleware para logging detalhado de requisições
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);
        var stopwatch = Stopwatch.StartNew();

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
        finally
        {
            stopwatch.Stop();

            // Log da resposta
            await LogResponseAsync(context, correlationId, stopwatch.ElapsedMilliseconds);

            // Restaurar o stream original e copiar o conteúdo
            await responseBodyStream.CopyToAsync(originalResponseBodyStream);
            context.Response.Body = originalResponseBodyStream;
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
            UserEmail = context.User?.FindFirst("email")?.Value
        };

        _logger.LogInformation("Incoming Request: {@RequestInfo}", requestInfo);

        // Log do body apenas para métodos que podem ter conteúdo
        if (HttpMethods.IsPost(request.Method) || 
            HttpMethods.IsPut(request.Method) || 
            HttpMethods.IsPatch(request.Method))
        {
            await LogRequestBodyAsync(request, correlationId);
        }
    }

    private async Task LogRequestBodyAsync(HttpRequest request, string correlationId)
    {
        if (request.ContentLength > 0 && request.ContentLength < 10000) // Limitar a 10KB
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

        // Log do body da resposta apenas em desenvolvimento e para erros
        if (ShouldLogResponseBody(response.StatusCode, response.ContentType))
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

        if (!string.IsNullOrWhiteSpace(body) && body.Length < 5000) // Limitar a 5KB
        {
            _logger.LogDebug("Response Body [{CorrelationId}]: {Body}", correlationId, body);
        }
    }

    private static Dictionary<string, string> GetSafeHeaders(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
    {
        var sensitiveHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Authorization", "Cookie", "Set-Cookie", "X-API-Key", "X-Auth-Token"
        };

        return headers
            .Where(h => !sensitiveHeaders.Contains(h.Key))
            .ToDictionary(
                h => h.Key, 
                h => string.Join(", ", h.Value),
                StringComparer.OrdinalIgnoreCase);
    }

    private static string SanitizeSensitiveData(string body)
    {
        // Lista de campos sensíveis que devem ser mascarados
        var sensitiveFields = new[] { "password", "senha", "token", "secret", "key", "cpf", "cnpj" };
        
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