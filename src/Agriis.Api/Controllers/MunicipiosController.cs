using Microsoft.AspNetCore.Mvc;
using Agriis.Referencias.Aplicacao.DTOs;
using Agriis.Referencias.Aplicacao.Interfaces;

namespace Agriis.Api.Controllers;

/// <summary>
/// Controlador para gerenciamento de municípios
/// </summary>
[ApiController]
[Route("api/referencias/municipios")]
public class MunicipiosController : ReferenciaControllerBase<MunicipioDto, CriarMunicipioDto, AtualizarMunicipioDto>
{
    private readonly IMunicipioService _municipioService;

    public MunicipiosController(
        IMunicipioService municipioService,
        ILogger<MunicipiosController> logger) : base(municipioService, logger)
    {
        _municipioService = municipioService;
    }

    /// <summary>
    /// Verifica se existe um município com o nome especificado na UF
    /// </summary>
    /// <param name="nome">Nome do município</param>
    /// <param name="ufId">ID da UF</param>
    /// <param name="idExcluir">ID do município a ser excluído da verificação (opcional)</param>
    [HttpGet("existe-nome/{nome}/uf/{ufId:int}")]
    public async Task<IActionResult> ExisteNomeNaUf(string nome, int ufId, [FromQuery] int? idExcluir = null)
    {
        try
        {
            Logger.LogDebug("Verificando se existe município com nome {Nome} na UF {UfId}", nome, ufId);
            
            var existe = await _municipioService.ExisteNomeNaUfAsync(nome, ufId, idExcluir);
            
            return Ok(new { Existe = existe });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao verificar se existe município com nome {Nome} na UF {UfId}", nome, ufId);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Verifica se existe um município com o código IBGE especificado
    /// </summary>
    /// <param name="codigoIbge">Código IBGE do município</param>
    /// <param name="idExcluir">ID do município a ser excluído da verificação (opcional)</param>
    [HttpGet("existe-codigo-ibge/{codigoIbge}")]
    public async Task<IActionResult> ExisteCodigoIbge(string codigoIbge, [FromQuery] int? idExcluir = null)
    {
        try
        {
            Logger.LogDebug("Verificando se existe município com código IBGE {CodigoIbge}", codigoIbge);
            
            var existe = await _municipioService.ExisteCodigoIbgeAsync(codigoIbge, idExcluir);
            
            return Ok(new { Existe = existe });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao verificar se existe município com código IBGE {CodigoIbge}", codigoIbge);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Obtém um município por código IBGE
    /// </summary>
    /// <param name="codigoIbge">Código IBGE do município</param>
    [HttpGet("codigo-ibge/{codigoIbge}")]
    public async Task<IActionResult> ObterPorCodigoIbge(string codigoIbge)
    {
        try
        {
            Logger.LogDebug("Obtendo município com código IBGE {CodigoIbge}", codigoIbge);
            
            var municipio = await _municipioService.ObterPorCodigoIbgeAsync(codigoIbge);
            
            if (municipio == null)
            {
                Logger.LogWarning("Município com código IBGE {CodigoIbge} não encontrado", codigoIbge);
                return NotFound(new { 
                    ErrorCode = "ENTITY_NOT_FOUND", 
                    ErrorDescription = "Município não encontrado",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            
            return Ok(municipio);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao obter município com código IBGE {CodigoIbge}", codigoIbge);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Obtém municípios por UF
    /// </summary>
    /// <param name="ufId">ID da UF</param>
    [HttpGet("uf/{ufId:int}")]
    public async Task<IActionResult> ObterPorUf(int ufId)
    {
        try
        {
            Logger.LogDebug("Obtendo municípios da UF com ID {UfId}", ufId);
            
            var municipios = await _municipioService.ObterPorUfAsync(ufId);
            
            Logger.LogDebug("Encontrados {Count} municípios para a UF {UfId}", municipios.Count(), ufId);
            
            return Ok(municipios);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao obter municípios da UF com ID {UfId}", ufId);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Busca municípios por nome (busca parcial)
    /// </summary>
    /// <param name="nome">Nome ou parte do nome do município</param>
    /// <param name="ufId">ID da UF para filtrar (opcional)</param>
    /// <param name="limite">Limite de resultados (padrão: 50)</param>
    [HttpGet("buscar")]
    public async Task<IActionResult> BuscarPorNome([FromQuery] string nome, [FromQuery] int? ufId = null, [FromQuery] int limite = 50)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(nome) || nome.Length < 2)
            {
                return BadRequest(new { 
                    ErrorCode = "VALIDATION_ERROR", 
                    ErrorDescription = "O nome deve ter pelo menos 2 caracteres",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }

            Logger.LogDebug("Buscando municípios com nome {Nome} na UF {UfId}", nome, ufId);
            
            var municipios = await _municipioService.BuscarPorNomeAsync(nome, ufId);
            
            Logger.LogDebug("Encontrados {Count} municípios na busca por {Nome}", municipios.Count(), nome);
            
            return Ok(municipios);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao buscar municípios com nome {Nome}", nome);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Obtém municípios ativos por UF (otimizado para dropdowns)
    /// </summary>
    /// <param name="ufId">ID da UF</param>
    [HttpGet("uf/{ufId:int}/dropdown")]
    public async Task<IActionResult> ObterDropdownPorUf(int ufId)
    {
        try
        {
            Logger.LogDebug("Obtendo municípios ativos da UF com ID {UfId} para dropdown", ufId);
            
            var municipios = await _municipioService.ObterPorUfAsync(ufId);
            
            // Filtrar apenas municípios ativos e retornar apenas ID e Nome para otimizar
            var municipiosDropdown = municipios
                .Where(m => m.Ativo)
                .Select(m => new { m.Id, m.Nome, m.CodigoIbge })
                .OrderBy(m => m.Nome);
            
            Logger.LogDebug("Encontrados {Count} municípios ativos para dropdown da UF {UfId}", municipiosDropdown.Count(), ufId);
            
            return Ok(municipiosDropdown);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao obter municípios dropdown da UF com ID {UfId}", ufId);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}