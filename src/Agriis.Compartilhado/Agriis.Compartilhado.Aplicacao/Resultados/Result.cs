namespace Agriis.Compartilhado.Aplicacao.Resultados;

/// <summary>
/// Representa o resultado de uma operação que pode falhar
/// </summary>
public class Result
{
    /// <summary>
    /// Indica se a operação foi bem-sucedida
    /// </summary>
    public bool IsSuccess { get; protected set; }
    
    /// <summary>
    /// Indica se a operação falhou
    /// </summary>
    public bool IsFailure => !IsSuccess;
    
    /// <summary>
    /// Mensagem de erro (se houver)
    /// </summary>
    public string? Error { get; protected set; }
    
    /// <summary>
    /// Código do erro (se houver)
    /// </summary>
    public string? ErrorCode { get; protected set; }
    
    /// <summary>
    /// Lista de erros de validação (se houver)
    /// </summary>
    public IReadOnlyList<string> ValidationErrors { get; protected set; } = new List<string>();
    
    /// <summary>
    /// Construtor protegido
    /// </summary>
    protected Result(bool isSuccess, string? error, string? errorCode = null, IEnumerable<string>? validationErrors = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorCode = errorCode;
        ValidationErrors = validationErrors?.ToList() ?? new List<string>();
    }
    
    /// <summary>
    /// Cria um resultado de sucesso
    /// </summary>
    public static Result Success() => new(true, null);
    
    /// <summary>
    /// Cria um resultado de falha
    /// </summary>
    /// <param name="error">Mensagem de erro</param>
    /// <param name="errorCode">Código do erro</param>
    public static Result Failure(string error, string? errorCode = null) => new(false, error, errorCode);
    
    /// <summary>
    /// Cria um resultado de falha com erros de validação
    /// </summary>
    /// <param name="validationErrors">Lista de erros de validação</param>
    public static Result ValidationFailure(IEnumerable<string> validationErrors) => 
        new(false, "Erro de validação", "VALIDATION_ERROR", validationErrors);
    
    /// <summary>
    /// Cria um resultado de falha com um único erro de validação
    /// </summary>
    /// <param name="validationError">Erro de validação</param>
    public static Result ValidationFailure(string validationError) => 
        ValidationFailure(new[] { validationError });
    
    /// <summary>
    /// Combina múltiplos resultados em um único resultado
    /// </summary>
    /// <param name="results">Resultados a serem combinados</param>
    public static Result Combine(params Result[] results)
    {
        var failures = results.Where(r => r.IsFailure).ToList();
        
        if (!failures.Any())
            return Success();
            
        var errors = failures.SelectMany(f => f.ValidationErrors).ToList();
        if (errors.Any())
            return ValidationFailure(errors);
            
        var firstFailure = failures.First();
        return Failure(firstFailure.Error!, firstFailure.ErrorCode);
    }
    
    /// <summary>
    /// Executa uma ação se o resultado for bem-sucedido
    /// </summary>
    /// <param name="action">Ação a ser executada</param>
    public Result OnSuccess(Action action)
    {
        if (IsSuccess)
            action();
        return this;
    }
    
    /// <summary>
    /// Executa uma ação se o resultado falhou
    /// </summary>
    /// <param name="action">Ação a ser executada</param>
    public Result OnFailure(Action<string?> action)
    {
        if (IsFailure)
            action(Error);
        return this;
    }
}

/// <summary>
/// Representa o resultado de uma operação que pode falhar e retorna um valor
/// </summary>
/// <typeparam name="T">Tipo do valor retornado</typeparam>
public class Result<T> : Result
{
    /// <summary>
    /// Valor retornado pela operação (se bem-sucedida)
    /// </summary>
    public T? Value { get; private set; }
    
    /// <summary>
    /// Construtor protegido
    /// </summary>
    protected Result(bool isSuccess, T? value, string? error, string? errorCode = null, IEnumerable<string>? validationErrors = null)
        : base(isSuccess, error, errorCode, validationErrors)
    {
        Value = value;
    }
    
    /// <summary>
    /// Cria um resultado de sucesso com valor
    /// </summary>
    /// <param name="value">Valor a ser retornado</param>
    public static Result<T> Success(T value) => new(true, value, null);
    
    /// <summary>
    /// Cria um resultado de falha
    /// </summary>
    /// <param name="error">Mensagem de erro</param>
    /// <param name="errorCode">Código do erro</param>
    public static new Result<T> Failure(string error, string? errorCode = null) => new(false, default, error, errorCode);
    
    /// <summary>
    /// Cria um resultado de falha com erros de validação
    /// </summary>
    /// <param name="validationErrors">Lista de erros de validação</param>
    public static new Result<T> ValidationFailure(IEnumerable<string> validationErrors) => 
        new(false, default, "Erro de validação", "VALIDATION_ERROR", validationErrors);
    
    /// <summary>
    /// Cria um resultado de falha com um único erro de validação
    /// </summary>
    /// <param name="validationError">Erro de validação</param>
    public static new Result<T> ValidationFailure(string validationError) => 
        ValidationFailure(new[] { validationError });
    
    /// <summary>
    /// Converte um Result simples para Result<T>
    /// </summary>
    /// <param name="result">Result a ser convertido</param>
    public static Result<T> FromResult(Result result)
    {
        if (result.IsSuccess)
            throw new InvalidOperationException("Não é possível converter um Result de sucesso sem valor para Result<T>");
            
        return new Result<T>(false, default, result.Error, result.ErrorCode, result.ValidationErrors);
    }
    
    /// <summary>
    /// Mapeia o valor do resultado para outro tipo
    /// </summary>
    /// <typeparam name="TNew">Novo tipo</typeparam>
    /// <param name="mapper">Função de mapeamento</param>
    public Result<TNew> Map<TNew>(Func<T, TNew> mapper)
    {
        if (IsFailure)
            return Result<TNew>.Failure(Error!, ErrorCode);
            
        try
        {
            var mappedValue = mapper(Value!);
            return Result<TNew>.Success(mappedValue);
        }
        catch (Exception ex)
        {
            return Result<TNew>.Failure($"Erro no mapeamento: {ex.Message}", "MAPPING_ERROR");
        }
    }
    
    /// <summary>
    /// Executa uma ação se o resultado for bem-sucedido
    /// </summary>
    /// <param name="action">Ação a ser executada</param>
    public Result<T> OnSuccess(Action<T> action)
    {
        if (IsSuccess && Value != null)
            action(Value);
        return this;
    }
    
    /// <summary>
    /// Executa uma função se o resultado for bem-sucedido
    /// </summary>
    /// <param name="func">Função a ser executada</param>
    public Result<TNew> OnSuccess<TNew>(Func<T, Result<TNew>> func)
    {
        if (IsFailure)
            return Result<TNew>.Failure(Error!, ErrorCode);
            
        return func(Value!);
    }
    
    /// <summary>
    /// Operador implícito para converter valor em Result<T>
    /// </summary>
    public static implicit operator Result<T>(T value) => Success(value);
}