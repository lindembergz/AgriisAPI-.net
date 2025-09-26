using Microsoft.AspNetCore.Mvc;
using Agriis.Referencias.Aplicacao.DTOs;
using Agriis.Referencias.Aplicacao.Interfaces;

namespace Agriis.Api.Controllers;

/// <summary>
/// Controlador para gerenciamento de UFs (Unidades Federativas)
/// </summary>
[ApiController]
[Route("api/referencias/ufs")]
public class UfsController : ReferenciaControllerBase<UfDto, CriarUfDto, AtualizarUfDto>
{
    private readonly IUfService _ufService;
    private readonly IMunicipioService _municipioService;

    public UfsController(
        IUfService ufService,
        IMunicipioService municipioService,
        ILogger<UfsController> logger) : base(ufService, logger)
    {
        _ufService = ufService;
        _municipioService = municipioService;
    }

    /// <summary>
    /// Verifica se existe uma UF com o código especificado
    /// </summary>
    /// <param name="codigo">Código da UF (2 caracteres)</param>
    /// <param name="idExcluir">ID da UF a ser excluída da verificação (opcional)</param>
    [HttpGet("existe-codigo/{codigo}")]
    public async Task<IActionResult> ExisteCodigo(string codigo, [FromQuery] int? idExcluir = null)
    {
        try
        {
            Logger.LogDebug("Verificando se existe UF com código {Codigo}", codigo);
            
            var existe = await _ufService.ExisteCodigoAsync(codigo, idExcluir);
            
            return Ok(new { Existe = existe });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao verificar se existe UF com código {Codigo}", codigo);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Verifica se existe uma UF com o nome especificado
    /// </summary>
    /// <param name="nome">Nome da UF</param>
    /// <param name="idExcluir">ID da UF a ser excluída da verificação (opcional)</param>
    [HttpGet("existe-nome/{nome}")]
    public async Task<IActionResult> ExisteNome(string nome, [FromQuery] int? idExcluir = null)
    {
        try
        {
            Logger.LogDebug("Verificando se existe UF com nome {Nome}", nome);
            
            var existe = await _ufService.ExisteNomeAsync(nome, idExcluir);
            
            return Ok(new { Existe = existe });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao verificar se existe UF com nome {Nome}", nome);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Obtém uma UF por código
    /// </summary>
    /// <param name="codigo">Código da UF (2 caracteres)</param>
    [HttpGet("codigo/{codigo}")]
    public async Task<IActionResult> ObterPorCodigo(string codigo)
    {
        try
        {
            Logger.LogDebug("Obtendo UF com código {Codigo}", codigo);
            
            var uf = await _ufService.ExisteCodigoAsync(codigo);
            
            if (uf == null)
            {
                Logger.LogWarning("UF com código {Codigo} não encontrada", codigo);
                return NotFound(new { 
                    ErrorCode = "ENTITY_NOT_FOUND", 
                    ErrorDescription = "UF não encontrada",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            
            return Ok(uf);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao obter UF com código {Codigo}", codigo);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Obtém UFs por país
    /// </summary>
    /// <param name="paisId">ID do país</param>
    [HttpGet("pais/{paisId:int}")]
    public async Task<IActionResult> ObterPorPais(int paisId)
    {
        try
        {
            Logger.LogDebug("Obtendo UFs do país com ID {PaisId}", paisId);
            
            var ufs = await _ufService.ObterPorPaisAsync(paisId);
            
            Logger.LogDebug("Encontradas {Count} UFs para o país {PaisId}", ufs.Count(), paisId);
            
            return Ok(ufs);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao obter UFs do país com ID {PaisId}", paisId);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Obtém os municípios de uma UF específica
    /// </summary>
    /// <param name="ufId">ID da UF</param>
    [HttpGet("{ufId:int}/municipios")]
    public async Task<IActionResult> ObterMunicipiosPorUf(int ufId)
    {
        try
        {
            Logger.LogDebug("Obtendo municípios da UF com ID {UfId}", ufId);
            
            // Verificar se a UF existe
            var uf = await _ufService.ObterPorIdAsync(ufId);
            if (uf == null)
            {
                Logger.LogWarning("UF com ID {UfId} não encontrada", ufId);
                return NotFound(new { 
                    ErrorCode = "ENTITY_NOT_FOUND", 
                    ErrorDescription = "UF não encontrada",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            
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
    /// Obtém os municípios ativos de uma UF específica (otimizado para dropdowns)
    /// </summary>
    /// <param name="ufId">ID da UF</param>
    [HttpGet("{ufId:int}/municipios/dropdown")]
    public async Task<IActionResult> ObterMunicipiosDropdownPorUf(int ufId)
    {
        try
        {
            Logger.LogDebug("Obtendo municípios ativos da UF com ID {UfId} para dropdown", ufId);
            
            // Verificar se a UF existe
            var uf = await _ufService.ObterPorIdAsync(ufId);
            if (uf == null)
            {
                Logger.LogWarning("UF com ID {UfId} não encontrada", ufId);
                return NotFound(new { 
                    ErrorCode = "ENTITY_NOT_FOUND", 
                    ErrorDescription = "UF não encontrada",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            
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