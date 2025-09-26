using Microsoft.AspNetCore.Mvc;
using Agriis.Referencias.Aplicacao.DTOs;
using Agriis.Referencias.Aplicacao.Interfaces;
using Agriis.Referencias.Dominio.Enums;

namespace Agriis.Api.Controllers;

/// <summary>
/// Controlador para gerenciamento de unidades de medida
/// </summary>
[ApiController]
[Route("api/referencias/unidades-medida")]
public class UnidadesMedidaController : ReferenciaControllerBase<UnidadeMedidaDto, CriarUnidadeMedidaDto, AtualizarUnidadeMedidaDto>
{
    private readonly IUnidadeMedidaService _unidadeService;

    public UnidadesMedidaController(
        IUnidadeMedidaService unidadeService,
        ILogger<UnidadesMedidaController> logger) : base(unidadeService, logger)
    {
        _unidadeService = unidadeService;
    }

    /// <summary>
    /// Verifica se existe uma unidade de medida com o símbolo especificado
    /// </summary>
    /// <param name="simbolo">Símbolo da unidade de medida</param>
    /// <param name="idExcluir">ID da unidade a ser excluída da verificação (opcional)</param>
    [HttpGet("existe-simbolo/{simbolo}")]
    public async Task<IActionResult> ExisteSimbolo(string simbolo, [FromQuery] int? idExcluir = null)
    {
        try
        {
            Logger.LogDebug("Verificando se existe unidade de medida com símbolo {Simbolo}", simbolo);
            
            var existe = await _unidadeService.ExisteSimboloAsync(simbolo, idExcluir);
            
            return Ok(new { Existe = existe });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao verificar se existe unidade de medida com símbolo {Simbolo}", simbolo);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Verifica se existe uma unidade de medida com o nome especificado
    /// </summary>
    /// <param name="nome">Nome da unidade de medida</param>
    /// <param name="idExcluir">ID da unidade a ser excluída da verificação (opcional)</param>
    [HttpGet("existe-nome/{nome}")]
    public async Task<IActionResult> ExisteNome(string nome, [FromQuery] int? idExcluir = null)
    {
        try
        {
            Logger.LogDebug("Verificando se existe unidade de medida com nome {Nome}", nome);
            
            var existe = await _unidadeService.ExisteNomeAsync(nome, idExcluir);
            
            return Ok(new { Existe = existe });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao verificar se existe unidade de medida com nome {Nome}", nome);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Obtém uma unidade de medida por símbolo
    /// </summary>
    /// <param name="simbolo">Símbolo da unidade de medida</param>
    [HttpGet("simbolo/{simbolo}")]
    public async Task<IActionResult> ObterPorSimbolo(string simbolo)
    {
        try
        {
            Logger.LogDebug("Obtendo unidade de medida com símbolo {Simbolo}", simbolo);
            
            var unidade = await _unidadeService.ObterPorSimboloAsync(simbolo);
            
            if (unidade == null)
            {
                Logger.LogWarning("Unidade de medida com símbolo {Simbolo} não encontrada", simbolo);
                return NotFound(new { 
                    ErrorCode = "ENTITY_NOT_FOUND", 
                    ErrorDescription = "Unidade de medida não encontrada",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            
            return Ok(unidade);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao obter unidade de medida com símbolo {Simbolo}", simbolo);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Obtém unidades de medida por tipo
    /// </summary>
    /// <param name="tipo">Tipo da unidade (Peso, Volume, Area, Unidade)</param>
    [HttpGet("tipo/{tipo}")]
    public async Task<IActionResult> ObterPorTipo(TipoUnidadeMedida tipo)
    {
        try
        {
            Logger.LogDebug("Obtendo unidades de medida do tipo {Tipo}", tipo);
            
            var unidades = await _unidadeService.ObterPorTipoAsync(tipo);
            
            Logger.LogDebug("Encontradas {Count} unidades de medida do tipo {Tipo}", unidades.Count(), tipo);
            
            return Ok(unidades);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao obter unidades de medida do tipo {Tipo}", tipo);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Obtém unidades de medida ativas por tipo (otimizado para dropdowns)
    /// </summary>
    /// <param name="tipo">Tipo da unidade (Peso, Volume, Area, Unidade)</param>
    [HttpGet("tipo/{tipo}/dropdown")]
    public async Task<IActionResult> ObterDropdownPorTipo(TipoUnidadeMedida tipo)
    {
        try
        {
            Logger.LogDebug("Obtendo unidades de medida ativas do tipo {Tipo} para dropdown", tipo);
            
            var unidades = await _unidadeService.ObterPorTipoAsync(tipo);
            
            // Filtrar apenas unidades ativas e retornar apenas ID, Simbolo e Nome para otimizar
            var unidadesDropdown = unidades
                .Where(u => u.Ativo)
                .Select(u => new { u.Id, u.Simbolo, u.Nome })
                .OrderBy(u => u.Nome);
            
            Logger.LogDebug("Encontradas {Count} unidades de medida ativas para dropdown do tipo {Tipo}", unidadesDropdown.Count(), tipo);
            
            return Ok(unidadesDropdown);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao obter unidades de medida dropdown do tipo {Tipo}", tipo);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Obtém todos os tipos de unidade de medida disponíveis
    /// </summary>
    [HttpGet("tipos")]
    public IActionResult ObterTipos()
    {
        try
        {
            Logger.LogDebug("Obtendo tipos de unidade de medida");
            
            var tipos = Enum.GetValues<TipoUnidadeMedida>()
                .Select(t => new { 
                    Valor = (int)t, 
                    Nome = t.ToString(),
                    Descricao = t switch
                    {
                        TipoUnidadeMedida.Peso => "Peso",
                        TipoUnidadeMedida.Volume => "Volume",
                        TipoUnidadeMedida.Area => "Área",
                        TipoUnidadeMedida.Unidade => "Unidade",
                        _ => t.ToString()
                    }
                });
            
            return Ok(tipos);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao obter tipos de unidade de medida");
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Converte uma quantidade de uma unidade para outra do mesmo tipo
    /// </summary>
    /// <param name="quantidade">Quantidade a ser convertida</param>
    /// <param name="unidadeOrigemId">ID da unidade de origem</param>
    /// <param name="unidadeDestinoId">ID da unidade de destino</param>
    [HttpGet("converter")]
    public async Task<IActionResult> Converter(
        [FromQuery] decimal quantidade, 
        [FromQuery] int unidadeOrigemId, 
        [FromQuery] int unidadeDestinoId)
    {
        try
        {
            Logger.LogDebug("Convertendo {Quantidade} da unidade {UnidadeOrigem} para unidade {UnidadeDestino}", 
                quantidade, unidadeOrigemId, unidadeDestinoId);
            
            var resultado = await _unidadeService.ConverterAsync(quantidade, unidadeOrigemId, unidadeDestinoId);
            
            Logger.LogDebug("Conversão realizada: {Quantidade} -> {Resultado}", quantidade, resultado);
            
            return Ok(new { 
                QuantidadeOriginal = quantidade,
                UnidadeOrigemId = unidadeOrigemId,
                UnidadeDestinoId = unidadeDestinoId,
                QuantidadeConvertida = resultado
            });
        }
        catch (ArgumentException ex)
        {
            Logger.LogWarning(ex, "Erro de validação na conversão: {Message}", ex.Message);
            return BadRequest(new { 
                ErrorCode = "VALIDATION_ERROR", 
                ErrorDescription = ex.Message,
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (InvalidOperationException ex)
        {
            Logger.LogWarning(ex, "Erro de operação na conversão: {Message}", ex.Message);
            return BadRequest(new { 
                ErrorCode = "CONVERSION_ERROR", 
                ErrorDescription = ex.Message,
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao converter unidades");
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}