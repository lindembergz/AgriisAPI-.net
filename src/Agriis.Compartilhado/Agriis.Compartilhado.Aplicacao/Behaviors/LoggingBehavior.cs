using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Agriis.Compartilhado.Aplicacao.Behaviors;

/// <summary>
/// Behavior para logging automático de requisições
/// </summary>
/// <typeparam name="TRequest">Tipo da requisição</typeparam>
/// <typeparam name="TResponse">Tipo da resposta</typeparam>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    
    /// <summary>
    /// Construtor
    /// </summary>
    /// <param name="logger">Logger</param>
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }
    
    /// <summary>
    /// Executa o logging antes e depois do handler
    /// </summary>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestId = Guid.NewGuid();
        
        _logger.LogInformation(
            "Iniciando requisição {RequestName} com ID {RequestId}",
            requestName, requestId);
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var response = await next();
            
            stopwatch.Stop();
            
            _logger.LogInformation(
                "Requisição {RequestName} com ID {RequestId} concluída em {ElapsedMilliseconds}ms",
                requestName, requestId, stopwatch.ElapsedMilliseconds);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            _logger.LogError(ex,
                "Erro na requisição {RequestName} com ID {RequestId} após {ElapsedMilliseconds}ms",
                requestName, requestId, stopwatch.ElapsedMilliseconds);
            
            throw;
        }
    }
}