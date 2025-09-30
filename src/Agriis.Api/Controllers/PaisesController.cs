using Microsoft.AspNetCore.Mvc;
using Agriis.Referencias.Aplicacao.DTOs;
using Agriis.Referencias.Aplicacao.Interfaces;

namespace Agriis.Api.Controllers;

/// <summary>
/// Controlador para gerenciamento de países
/// </summary>
[ApiController]
[Route("api/referencias/paises")]
public class PaisesController : ReferenciaControllerBase<PaisDto, CriarPaisDto, AtualizarPaisDto>
{
    private readonly IPaisService _paisService;
    private readonly IUfService _ufService;

    public PaisesController(
        IPaisService paisService,
        IUfService ufService,
        ILogger<PaisesController> logger) : base(paisService, logger)
    {
        _paisService = paisService;
        _ufService = ufService;
    }

    /// <summary>
    /// Verifica se existe um país com o código especificado
    /// </summary>
    /// <param name="codigo">Código do país (2-3 caracteres ISO)</param>
    /// <param name="idExcluir">ID do país a ser excluído da verificação (opcional)</param>
    [HttpGet("existe-codigo/{codigo}")]
    public async Task<IActionResult> ExisteCodigo(string codigo, [FromQuery] int? idExcluir = null)
    {
        try
        {
            Logger.LogDebug("Verificando se existe país com código {Codigo}", codigo);
            
            var existe = await _paisService.ExisteCodigoAsync(codigo, idExcluir);
            
            return Ok(new { Existe = existe });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao verificar se existe país com código {Codigo}", codigo);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Verifica se existe um país com o nome especificado
    /// </summary>
    /// <param name="nome">Nome do país</param>
    /// <param name="idExcluir">ID do país a ser excluído da verificação (opcional)</param>
    [HttpGet("existe-nome/{nome}")]
    public async Task<IActionResult> ExisteNome(string nome, [FromQuery] int? idExcluir = null)
    {
        try
        {
            Logger.LogDebug("Verificando se existe país com nome {Nome}", nome);
            
            var existe = await _paisService.ExisteNomeAsync(nome, idExcluir);
            
            return Ok(new { Existe = existe });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao verificar se existe país com nome {Nome}", nome);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Obtém um país por código
    /// </summary>
    /// <param name="codigo">Código do país (2-3 caracteres ISO)</param>
    [HttpGet("codigo/{codigo}")]
    public async Task<IActionResult> ObterPorCodigo(string codigo)
    {
        try
        {
            Logger.LogDebug("Obtendo país com código {Codigo}", codigo);
            
            var pais = await _paisService.ObterPorCodigoAsync(codigo);


            if (pais == null)
            {
                Logger.LogWarning("País com código {Codigo} não encontrado", codigo);
                return NotFound(new { 
                    ErrorCode = "ENTITY_NOT_FOUND", 
                    ErrorDescription = "País não encontrado",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            
            return Ok(pais);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao obter país com código {Codigo}", codigo);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Obtém as UFs de um país específico
    /// </summary>
    /// <param name="paisId">ID do país</param>
    [HttpGet("{paisId:int}/ufs")]
    public async Task<IActionResult> ObterUfsPorPais(int paisId)
    {
        try
        {
            Logger.LogDebug("Obtendo UFs do país com ID {PaisId}", paisId);
            
            // Verificar se o país existe
            var pais = await _paisService.ObterPorIdAsync(paisId);
            if (pais == null)
            {
                Logger.LogWarning("País com ID {PaisId} não encontrado", paisId);
                return NotFound(new { 
                    ErrorCode = "ENTITY_NOT_FOUND", 
                    ErrorDescription = "País não encontrado",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            
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
    /// Obtém países com contadores de UFs
    /// </summary>
    [HttpGet("com-contadores")]
    public async Task<IActionResult> ObterComContadores()
    {
        try
        {
            Logger.LogDebug("Obtendo países com contadores de UFs");
            
            var paises = await _paisService.ObterTodosAsync();
            var paisesComContadores = new List<object>();
            
            foreach (var pais in paises)
            {
                var ufs = await _ufService.ObterPorPaisAsync(pais.Id);
                paisesComContadores.Add(new
                {
                    pais.Id,
                    pais.Codigo,
                    pais.Nome,
                    pais.Ativo,
                    pais.DataCriacao,
                    pais.DataAtualizacao,
                    UfsCount = ufs.Count()
                });
            }
            
            Logger.LogDebug("Encontrados {Count} países com contadores", paisesComContadores.Count);
            
            return Ok(paisesComContadores);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao obter países com contadores");
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Obtém países ativos com contadores de UFs
    /// </summary>
    [HttpGet("ativos/com-contadores")]
    public async Task<IActionResult> ObterAtivosComContadores()
    {
        try
        {
            Logger.LogDebug("Obtendo países ativos com contadores de UFs");
            
            var paises = await _paisService.ObterAtivosAsync();
            var paisesComContadores = new List<object>();
            
            foreach (var pais in paises)
            {
                var ufs = await _ufService.ObterPorPaisAsync(pais.Id);
                paisesComContadores.Add(new
                {
                    pais.Id,
                    pais.Codigo,
                    pais.Nome,
                    pais.Ativo,
                    pais.DataCriacao,
                    pais.DataAtualizacao,
                    UfsCount = ufs.Count()
                });
            }
            
            Logger.LogDebug("Encontrados {Count} países ativos com contadores", paisesComContadores.Count);
            
            return Ok(paisesComContadores);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao obter países ativos com contadores");
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Obtém as UFs ativas de um país específico (otimizado para dropdowns)
    /// </summary>
    /// <param name="paisId">ID do país</param>
    [HttpGet("{paisId:int}/ufs/dropdown")]
    public async Task<IActionResult> ObterUfsDropdownPorPais(int paisId)
    {
        try
        {
            Logger.LogDebug("Obtendo UFs ativas do país com ID {PaisId} para dropdown", paisId);
            
            // Verificar se o país existe
            var pais = await _paisService.ObterPorIdAsync(paisId);
            if (pais == null)
            {
                Logger.LogWarning("País com ID {PaisId} não encontrado", paisId);
                return NotFound(new { 
                    ErrorCode = "ENTITY_NOT_FOUND", 
                    ErrorDescription = "País não encontrado",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            
            var ufs = await _ufService.ObterPorPaisAsync(paisId);
            
            // Filtrar apenas UFs ativas e retornar apenas ID e Nome para otimizar
            var ufsDropdown = ufs
                .Where(u => u.Ativo)
                .Select(u => new { u.Id, u.Nome, u.Codigo })
                .OrderBy(u => u.Nome);
            
            Logger.LogDebug("Encontradas {Count} UFs ativas para dropdown do país {PaisId}", ufsDropdown.Count(), paisId);
            
            return Ok(ufsDropdown);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao obter UFs dropdown do país com ID {PaisId}", paisId);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}