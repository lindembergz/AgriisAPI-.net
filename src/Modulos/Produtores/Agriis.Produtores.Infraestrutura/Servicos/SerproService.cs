using System.Text.Json;
using Microsoft.Extensions.Logging;
using Agriis.Produtores.Dominio.Interfaces;

namespace Agriis.Produtores.Infraestrutura.Servicos;

/// <summary>
/// Implementação do serviço de integração com SERPRO
/// </summary>
public class SerproService : ISerproService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SerproService> _logger;

    public SerproService(HttpClient httpClient, ILogger<SerproService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<SerproValidationResult> ValidarCpfAsync(string cpf)
    {
        try
        {
            _logger.LogInformation("Iniciando validação de CPF no SERPRO: {Cpf}", cpf);

            // Por enquanto, implementação simulada
            // Em produção, seria feita a chamada real para a API do SERPRO
            await Task.Delay(1000); // Simula latência da API

            // Simulação de validação baseada em regras simples
            var cpfLimpo = cpf.Replace(".", "").Replace("-", "");
            
            // CPFs de teste que sempre passam
            var cpfsValidos = new[] { "11111111111", "22222222222", "33333333333" };
            var cpfsInvalidos = new[] { "00000000000", "99999999999" };

            if (cpfsInvalidos.Contains(cpfLimpo))
            {
                return new SerproValidationResult
                {
                    Sucesso = true,
                    DocumentoValido = false,
                    MensagemErro = "CPF inválido no SERPRO",
                    DadosRetorno = JsonDocument.Parse(JsonSerializer.Serialize(new { cpf = cpfLimpo, situacao = "INATIVO" }))
                };
            }

            if (cpfsValidos.Contains(cpfLimpo) || ValidarCpfAlgoritmo(cpfLimpo))
            {
                return new SerproValidationResult
                {
                    Sucesso = true,
                    DocumentoValido = true,
                    Nome = "Nome do Produtor Simulado",
                    SituacaoCadastral = "ATIVO",
                    DadosRetorno = JsonDocument.Parse(JsonSerializer.Serialize(new 
                    { 
                        cpf = cpfLimpo, 
                        nome = "Nome do Produtor Simulado",
                        situacao = "ATIVO"
                    }))
                };
            }

            return new SerproValidationResult
            {
                Sucesso = true,
                DocumentoValido = false,
                MensagemErro = "CPF não encontrado no SERPRO",
                DadosRetorno = JsonDocument.Parse(JsonSerializer.Serialize(new { cpf = cpfLimpo, situacao = "NAO_ENCONTRADO" }))
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar CPF no SERPRO: {Cpf}", cpf);
            
            return new SerproValidationResult
            {
                Sucesso = false,
                DocumentoValido = false,
                MensagemErro = $"Erro na consulta SERPRO: {ex.Message}"
            };
        }
    }

    /// <inheritdoc />
    public async Task<SerproValidationResult> ValidarCnpjAsync(string cnpj)
    {
        try
        {
            _logger.LogInformation("Iniciando validação de CNPJ no SERPRO: {Cnpj}", cnpj);

            // Por enquanto, implementação simulada
            // Em produção, seria feita a chamada real para a API do SERPRO
            await Task.Delay(1000); // Simula latência da API

            // Simulação de validação baseada em regras simples
            var cnpjLimpo = cnpj.Replace(".", "").Replace("/", "").Replace("-", "");
            
            // CNPJs de teste que sempre passam
            var cnpjsValidos = new[] { "11111111000111", "22222222000122", "33333333000133" };
            var cnpjsInvalidos = new[] { "00000000000000", "99999999000199" };

            if (cnpjsInvalidos.Contains(cnpjLimpo))
            {
                return new SerproValidationResult
                {
                    Sucesso = true,
                    DocumentoValido = false,
                    MensagemErro = "CNPJ inválido no SERPRO",
                    DadosRetorno = JsonDocument.Parse(JsonSerializer.Serialize(new { cnpj = cnpjLimpo, situacao = "INATIVO" }))
                };
            }

            if (cnpjsValidos.Contains(cnpjLimpo) || ValidarCnpjAlgoritmo(cnpjLimpo))
            {
                return new SerproValidationResult
                {
                    Sucesso = true,
                    DocumentoValido = true,
                    Nome = "Empresa Produtora Simulada LTDA",
                    SituacaoCadastral = "ATIVO",
                    DadosRetorno = JsonDocument.Parse(JsonSerializer.Serialize(new 
                    { 
                        cnpj = cnpjLimpo, 
                        razao_social = "Empresa Produtora Simulada LTDA",
                        situacao = "ATIVO"
                    }))
                };
            }

            return new SerproValidationResult
            {
                Sucesso = true,
                DocumentoValido = false,
                MensagemErro = "CNPJ não encontrado no SERPRO",
                DadosRetorno = JsonDocument.Parse(JsonSerializer.Serialize(new { cnpj = cnpjLimpo, situacao = "NAO_ENCONTRADO" }))
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar CNPJ no SERPRO: {Cnpj}", cnpj);
            
            return new SerproValidationResult
            {
                Sucesso = false,
                DocumentoValido = false,
                MensagemErro = $"Erro na consulta SERPRO: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Valida CPF usando o algoritmo oficial (simplificado)
    /// </summary>
    private static bool ValidarCpfAlgoritmo(string cpf)
    {
        if (cpf.Length != 11 || cpf.All(c => c == cpf[0]))
            return false;

        // Implementação simplificada - em produção usaria a validação completa
        return true;
    }

    /// <summary>
    /// Valida CNPJ usando o algoritmo oficial (simplificado)
    /// </summary>
    private static bool ValidarCnpjAlgoritmo(string cnpj)
    {
        if (cnpj.Length != 14 || cnpj.All(c => c == cnpj[0]))
            return false;

        // Implementação simplificada - em produção usaria a validação completa
        return true;
    }
}