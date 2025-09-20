using System.Net;
using System.Text.Json;
using FluentValidation;
using Agriis.Compartilhado.Dominio.Exceptions;

namespace Agriis.Api.Middleware;

/// <summary>
/// Middleware global para tratamento de exceções
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next, 
        ILogger<GlobalExceptionMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não tratado: {Message} - Path: {Path} - Method: {Method}", 
                ex.Message, context.Request.Path, context.Request.Method);
            
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = CreateErrorResponse(exception);
        response.StatusCode = GetStatusCode(exception);

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = _environment.IsDevelopment()
        };

        var jsonResponse = JsonSerializer.Serialize(errorResponse, jsonOptions);
        await response.WriteAsync(jsonResponse);
    }

    private object CreateErrorResponse(Exception exception)
    {
        return exception switch
        {
            DomainException domainEx => new
            {
                error_code = domainEx.ErrorCode,
                error_description = domainEx.Message,
                timestamp = DateTime.UtcNow
            },
            ValidationException validationEx => new
            {
                error_code = "VALIDATION_ERROR",
                error_description = "Dados de entrada inválidos",
                errors = validationEx.Errors.Select(e => new
                {
                    field = e.PropertyName,
                    message = e.ErrorMessage
                }),
                timestamp = DateTime.UtcNow
            },
            UnauthorizedAccessException => new
            {
                error_code = "UNAUTHORIZED",
                error_description = "Acesso não autorizado",
                timestamp = DateTime.UtcNow
            },
            ArgumentException argEx => new
            {
                error_code = "INVALID_ARGUMENT",
                error_description = argEx.Message,
                parameter = argEx.ParamName,
                timestamp = DateTime.UtcNow
            },
            InvalidOperationException invalidOpEx => new
            {
                error_code = "INVALID_OPERATION",
                error_description = invalidOpEx.Message,
                timestamp = DateTime.UtcNow
            },
            _ => CreateGenericErrorResponse(exception)
        };
    }

    private object CreateGenericErrorResponse(Exception exception)
    {
        if (_environment.IsDevelopment())
        {
            return new
            {
                error_code = "INTERNAL_ERROR",
                error_description = exception.Message,
                stack_trace = exception.StackTrace,
                inner_exception = exception.InnerException?.Message,
                timestamp = DateTime.UtcNow
            };
        }

        return new
        {
            error_code = "INTERNAL_ERROR",
            error_description = "Erro interno do servidor",
            timestamp = DateTime.UtcNow
        };
    }

    private static int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            DomainException => (int)HttpStatusCode.BadRequest,
            ValidationException => (int)HttpStatusCode.UnprocessableEntity,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            ArgumentException => (int)HttpStatusCode.BadRequest,
            InvalidOperationException => (int)HttpStatusCode.BadRequest,
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            _ => (int)HttpStatusCode.InternalServerError
        };
    }
}