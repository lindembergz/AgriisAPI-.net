using System.Text.Json;

namespace Agriis.Produtores.Dominio.Interfaces;

/// <summary>
/// Interface para integração com SERPRO
/// </summary>
public interface ISerproService
{
    /// <summary>
    /// Valida um CPF no SERPRO
    /// </summary>
    /// <param name="cpf">CPF a ser validado</param>
    /// <returns>Resultado da validação</returns>
    Task<SerproValidationResult> ValidarCpfAsync(string cpf);
    
    /// <summary>
    /// Valida um CNPJ no SERPRO
    /// </summary>
    /// <param name="cnpj">CNPJ a ser validado</param>
    /// <returns>Resultado da validação</returns>
    Task<SerproValidationResult> ValidarCnpjAsync(string cnpj);
}

/// <summary>
/// Resultado da validação no SERPRO
/// </summary>
public class SerproValidationResult
{
    /// <summary>
    /// Indica se a validação foi bem-sucedida
    /// </summary>
    public bool Sucesso { get; set; }
    
    /// <summary>
    /// Indica se o documento é válido
    /// </summary>
    public bool DocumentoValido { get; set; }
    
    /// <summary>
    /// Mensagem de erro (se houver)
    /// </summary>
    public string? MensagemErro { get; set; }
    
    /// <summary>
    /// Dados retornados pela API do SERPRO
    /// </summary>
    public JsonDocument? DadosRetorno { get; set; }
    
    /// <summary>
    /// Nome da pessoa/empresa (se disponível)
    /// </summary>
    public string? Nome { get; set; }
    
    /// <summary>
    /// Situação cadastral (se disponível)
    /// </summary>
    public string? SituacaoCadastral { get; set; }
}