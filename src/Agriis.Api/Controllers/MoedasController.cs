using Microsoft.AspNetCore.Mvc;
using Agriis.Referencias.Aplicacao.DTOs;
using Agriis.Referencias.Aplicacao.Interfaces;
using Agriis.Api.Controllers;

namespace Agriis.Api.Controllers;

/// <summary>
/// Controlador para gerenciamento de moedas
/// </summary>
// Temporariamente desabilitado devido à unificação das tabelas geográficas

[ApiController]
[Route("api/moedas")]
public class MoedasController : ReferenciaControllerBase<MoedaDto, CriarMoedaDto, AtualizarMoedaDto>
{
    private readonly IMoedaService _moedaService;

    public MoedasController(
        IMoedaService moedaService,
        ILogger<MoedasController> logger) : base(moedaService, logger)
    {
        _moedaService = moedaService;
    }

    /// <summary>
    /// Verifica se existe uma moeda com o código especificado
    /// </summary>
    /// <param name="codigo">Código da moeda (3 caracteres)</param>
    /// <param name="idExcluir">ID da moeda a ser excluída da verificação (opcional)</param>
    [HttpGet("existe-codigo/{codigo}")]
    public async Task<IActionResult> ExisteCodigo(string codigo, [FromQuery] int? idExcluir = null)
    {
        try
        {
            Logger.LogDebug("Verificando se existe moeda com código {Codigo}", codigo);
            
            var existe = await _moedaService.ExisteCodigoAsync(codigo, idExcluir);
            
            return Ok(new { Existe = existe });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao verificar se existe moeda com código {Codigo}", codigo);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Verifica se existe uma moeda com o nome especificado
    /// </summary>
    /// <param name="nome">Nome da moeda</param>
    /// <param name="idExcluir">ID da moeda a ser excluída da verificação (opcional)</param>
    [HttpGet("existe-nome/{nome}")]
    public async Task<IActionResult> ExisteNome(string nome, [FromQuery] int? idExcluir = null)
    {
        try
        {
            Logger.LogDebug("Verificando se existe moeda com nome {Nome}", nome);
            
            var existe = await _moedaService.ExisteNomeAsync(nome, idExcluir);
            
            return Ok(new { Existe = existe });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao verificar se existe moeda com nome {Nome}", nome);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Obtém uma moeda por código
    /// </summary>
    /// <param name="codigo">Código da moeda (3 caracteres)</param>
    [HttpGet("codigo/{codigo}")]
    public async Task<IActionResult> ObterPorCodigo(string codigo)
    {
        try
        {
            Logger.LogDebug("Obtendo moeda com código {Codigo}", codigo);
            
            var moeda = await _moedaService.ObterPorCodigoAsync(codigo);
            
            if (moeda == null)
            {
                Logger.LogWarning("Moeda com código {Codigo} não encontrada", codigo);
                return NotFound(new { 
                    ErrorCode = "ENTITY_NOT_FOUND", 
                    ErrorDescription = "Moeda não encontrada",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            
            return Ok(moeda);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao obter moeda com código {Codigo}", codigo);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}