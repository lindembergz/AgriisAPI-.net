namespace Agriis.Compartilhado.Dominio.Exceptions;

/// <summary>
/// Exceção base para violações de regras de domínio
/// </summary>
public class DomainException : Exception
{
    public string ErrorCode { get; }

    public DomainException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode ?? throw new ArgumentNullException(nameof(errorCode));
    }

    public DomainException(string errorCode, string message, Exception innerException) 
        : base(message, innerException)
    {
        ErrorCode = errorCode ?? throw new ArgumentNullException(nameof(errorCode));
    }
}

/// <summary>
/// Exceção para entidades não encontradas
/// </summary>
public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, object id) 
        : base("ENTITY_NOT_FOUND", $"{entityName} com ID '{id}' não foi encontrado(a)")
    {
    }
}

/// <summary>
/// Exceção para violações de regras de negócio
/// </summary>
public class BusinessRuleException : DomainException
{
    public BusinessRuleException(string rule, string message) 
        : base($"BUSINESS_RULE_{rule}", message)
    {
    }
}

/// <summary>
/// Exceção para conflitos de dados
/// </summary>
public class ConflictException : DomainException
{
    public ConflictException(string message) 
        : base("CONFLICT", message)
    {
    }
}