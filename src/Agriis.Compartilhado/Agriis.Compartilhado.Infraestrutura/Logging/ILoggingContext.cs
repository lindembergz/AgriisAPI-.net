namespace Agriis.Compartilhado.Infraestrutura.Logging;

/// <summary>
/// Interface para contexto de logging estruturado
/// </summary>
public interface ILoggingContext
{
    /// <summary>
    /// ID de correlação da requisição atual
    /// </summary>
    string? CorrelationId { get; set; }

    /// <summary>
    /// ID do usuário autenticado
    /// </summary>
    string? UserId { get; set; }

    /// <summary>
    /// Email do usuário autenticado
    /// </summary>
    string? UserEmail { get; set; }

    /// <summary>
    /// Caminho da requisição atual
    /// </summary>
    string? RequestPath { get; set; }

    /// <summary>
    /// Método HTTP da requisição atual
    /// </summary>
    string? RequestMethod { get; set; }

    /// <summary>
    /// IP remoto da requisição
    /// </summary>
    string? RemoteIpAddress { get; set; }

    /// <summary>
    /// User Agent da requisição
    /// </summary>
    string? UserAgent { get; set; }

    /// <summary>
    /// Adiciona propriedade customizada ao contexto
    /// </summary>
    void AddProperty(string key, object? value);

    /// <summary>
    /// Remove propriedade do contexto
    /// </summary>
    void RemoveProperty(string key);

    /// <summary>
    /// Obtém todas as propriedades do contexto
    /// </summary>
    IDictionary<string, object?> GetProperties();

    /// <summary>
    /// Limpa o contexto
    /// </summary>
    void Clear();
}