using Agriis.Produtores.Dominio.Entidades;
using Agriis.Produtores.Dominio.Enums;
using Agriis.Produtores.Dominio.Interfaces;

namespace Agriis.Produtores.Dominio.Servicos;

/// <summary>
/// Serviço de domínio para regras de negócio dos produtores
/// </summary>
public class ProdutorDomainService
{
    private readonly ISerproService _serproService;
    
    public ProdutorDomainService(ISerproService serproService)
    {
        _serproService = serproService ?? throw new ArgumentNullException(nameof(serproService));
    }
    
    /// <summary>
    /// Valida um produtor automaticamente via SERPRO
    /// </summary>
    /// <param name="produtor">Produtor a ser validado</param>
    /// <returns>Resultado da validação</returns>
    public async Task<ValidacaoAutomaticaResult> ValidarAutomaticamenteAsync(Produtor produtor)
    {
        if (produtor == null)
            throw new ArgumentNullException(nameof(produtor));
            
        try
        {
            SerproValidationResult resultado;
            
            if (produtor.EhPessoaFisica())
            {
                resultado = await _serproService.ValidarCpfAsync(produtor.Cpf!.Valor);
            }
            else
            {
                resultado = await _serproService.ValidarCnpjAsync(produtor.Cnpj!.Valor);
            }
            
            // Armazena o retorno da API
            if (resultado.DadosRetorno != null)
            {
                produtor.ArmazenarRetornosApiCheck(resultado.DadosRetorno);
            }
            
            // Define o status baseado no resultado
            if (resultado.Sucesso && resultado.DocumentoValido)
            {
                produtor.AtualizarStatus(StatusProdutor.AutorizadoAutomaticamente);
                return new ValidacaoAutomaticaResult
                {
                    Sucesso = true,
                    StatusResultante = StatusProdutor.AutorizadoAutomaticamente,
                    Mensagem = "Produtor autorizado automaticamente"
                };
            }
            else if (resultado.Sucesso && !resultado.DocumentoValido)
            {
                produtor.AtualizarStatus(StatusProdutor.Negado);
                return new ValidacaoAutomaticaResult
                {
                    Sucesso = false,
                    StatusResultante = StatusProdutor.Negado,
                    Mensagem = "Documento inválido no SERPRO"
                };
            }
            else
            {
                // Erro na consulta - deixa para validação manual
                produtor.AtualizarStatus(StatusProdutor.PendenteValidacaoManual);
                return new ValidacaoAutomaticaResult
                {
                    Sucesso = false,
                    StatusResultante = StatusProdutor.PendenteValidacaoManual,
                    Mensagem = "Erro na validação automática. Será necessária validação manual."
                };
            }
        }
        catch (Exception ex)
        {
            // Em caso de erro, deixa para validação manual
            produtor.AtualizarStatus(StatusProdutor.PendenteValidacaoManual);
            return new ValidacaoAutomaticaResult
            {
                Sucesso = false,
                StatusResultante = StatusProdutor.PendenteValidacaoManual,
                Mensagem = $"Erro na validação automática: {ex.Message}"
            };
        }
    }
    
    /// <summary>
    /// Verifica se um produtor pode ser editado
    /// </summary>
    /// <param name="produtor">Produtor a verificar</param>
    /// <returns>True se pode ser editado</returns>
    public bool PodeSerEditado(Produtor produtor)
    {
        if (produtor == null)
            return false;
            
        // Produtores negados não podem ser editados
        return produtor.Status != StatusProdutor.Negado;
    }
    
    /// <summary>
    /// Verifica se um produtor pode fazer pedidos
    /// </summary>
    /// <param name="produtor">Produtor a verificar</param>
    /// <returns>True se pode fazer pedidos</returns>
    public bool PodeFazerPedidos(Produtor produtor)
    {
        if (produtor == null)
            return false;
            
        return produtor.EstaAutorizado();
    }
    
    /// <summary>
    /// Calcula a área total de plantio baseada nas propriedades
    /// </summary>
    /// <param name="produtor">Produtor</param>
    /// <param name="areasPropriedades">Áreas das propriedades</param>
    /// <returns>Área total calculada</returns>
    public decimal CalcularAreaTotalPlantio(Produtor produtor, IEnumerable<decimal> areasPropriedades)
    {
        if (produtor == null || areasPropriedades == null)
            return 0;
            
        var areaTotal = areasPropriedades.Sum();
        return Math.Round(areaTotal, 4);
    }
}

/// <summary>
/// Resultado da validação automática
/// </summary>
public class ValidacaoAutomaticaResult
{
    public bool Sucesso { get; set; }
    public StatusProdutor StatusResultante { get; set; }
    public string Mensagem { get; set; } = string.Empty;
}