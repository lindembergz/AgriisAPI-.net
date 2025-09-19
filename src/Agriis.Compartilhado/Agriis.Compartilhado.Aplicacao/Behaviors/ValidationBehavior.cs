using FluentValidation;
using MediatR;
using Agriis.Compartilhado.Aplicacao.Resultados;

namespace Agriis.Compartilhado.Aplicacao.Behaviors;

/// <summary>
/// Behavior para validação automática usando FluentValidation
/// </summary>
/// <typeparam name="TRequest">Tipo da requisição</typeparam>
/// <typeparam name="TResponse">Tipo da resposta</typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    
    /// <summary>
    /// Construtor
    /// </summary>
    /// <param name="validators">Lista de validadores</param>
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }
    
    /// <summary>
    /// Executa a validação antes do handler
    /// </summary>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }
        
        var context = new ValidationContext<TRequest>(request);
        
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken))
        );
        
        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();
        
        if (failures.Any())
        {
            var errorMessages = failures.Select(f => f.ErrorMessage).ToList();
            
            // Se TResponse é um Result, retorna um resultado de falha
            if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var resultType = typeof(TResponse).GetGenericArguments()[0];
                var failureMethod = typeof(Result<>).MakeGenericType(resultType)
                    .GetMethod(nameof(Result<object>.ValidationFailure), new[] { typeof(IEnumerable<string>) });
                
                return (TResponse)failureMethod!.Invoke(null, new object[] { errorMessages })!;
            }
            
            // Se TResponse é um Result simples
            if (typeof(TResponse) == typeof(Result))
            {
                return (TResponse)(object)Result.ValidationFailure(errorMessages);
            }
            
            // Para outros tipos, lança exceção
            throw new ValidationException(failures);
        }
        
        return await next();
    }
}