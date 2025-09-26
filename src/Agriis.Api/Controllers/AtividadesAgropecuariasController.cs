using Microsoft.AspNetCore.Mvc;
using Agriis.Referencias.Aplicacao.DTOs;
using Agriis.Referencias.Aplicacao.Interfaces;
using Agriis.Referencias.Dominio.Enums;

namespace Agriis.Api.Controllers;

/// <summary>
/// Controlador para gerenciamento de atividades agropecuárias
/// </summary>
[ApiController]
[Route("api/referencias/atividades-agropecuarias")]
public class AtividadesAgropecuariasController : ReferenciaControllerBase<AtividadeAgropecuariaDto, CriarAtividadeAgropecuariaDto, AtualizarAtividadeAgropecuariaDto>
{
    private readonly IAtividadeAgropecuariaService _atividadeService;

    public AtividadesAgropecuariasController(
        IAtividadeAgropecuariaService atividadeService,
        ILogger<AtividadesAgropecuariasController> logger) : base(atividadeService, logger)
    {
        _atividadeService = atividadeService;
    }

    /// <summary>
    /// Verifica se existe uma atividade agropecuária com o código especificado
    /// </summary>
    /// <param name="codigo">Código da atividade agropecuária</param>
    /// <param name="idExcluir">ID da atividade a ser excluída da verificação (opcional)</param>
    [HttpGet("existe-codigo/{codigo}")]
    public async Task<IActionResult> ExisteCodigo(string codigo, [FromQuery] int? idExcluir = null)
    {
        try
        {
            Logger.LogDebug("Verificando se existe atividade agropecuária com código {Codigo}", codigo);
            
            var existe = await _atividadeService.ExisteCodigoAsync(codigo, idExcluir);
            
            return Ok(new { Existe = existe });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao verificar se existe atividade agropecuária com código {Codigo}", codigo);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Verifica se existe uma atividade agropecuária com a descrição especificada
    /// </summary>
    /// <param name="descricao">Descrição da atividade agropecuária</param>
    /// <param name="idExcluir">ID da atividade a ser excluída da verificação (opcional)</param>
    [HttpGet("existe-descricao/{descricao}")]
    public async Task<IActionResult> ExisteDescricao(string descricao, [FromQuery] int? idExcluir = null)
    {
        try
        {
            Logger.LogDebug("Verificando se existe atividade agropecuária com descrição {Descricao}", descricao);
            
            var existe = await _atividadeService.ExisteDescricaoAsync(descricao, idExcluir);
            
            return Ok(new { Existe = existe });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao verificar se existe atividade agropecuária com descrição {Descricao}", descricao);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Obtém uma atividade agropecuária por código
    /// </summary>
    /// <param name="codigo">Código da atividade agropecuária</param>
    [HttpGet("codigo/{codigo}")]
    public async Task<IActionResult> ObterPorCodigo(string codigo)
    {
        try
        {
            Logger.LogDebug("Obtendo atividade agropecuária com código {Codigo}", codigo);
            
            var atividade = await _atividadeService.ObterPorCodigoAsync(codigo);
            
            if (atividade == null)
            {
                Logger.LogWarning("Atividade agropecuária com código {Codigo} não encontrada", codigo);
                return NotFound(new { 
                    ErrorCode = "ENTITY_NOT_FOUND", 
                    ErrorDescription = "Atividade agropecuária não encontrada",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            
            return Ok(atividade);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao obter atividade agropecuária com código {Codigo}", codigo);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Obtém atividades agropecuárias por tipo
    /// </summary>
    /// <param name="tipo">Tipo da atividade (Agricultura, Pecuaria, Mista)</param>
    [HttpGet("tipo/{tipo}")]
    public async Task<IActionResult> ObterPorTipo(TipoAtividadeAgropecuaria tipo)
    {
        try
        {
            Logger.LogDebug("Obtendo atividades agropecuárias do tipo {Tipo}", tipo);
            
            var atividades = await _atividadeService.ObterPorTipoAsync(tipo);
            
            Logger.LogDebug("Encontradas {Count} atividades agropecuárias do tipo {Tipo}", atividades.Count(), tipo);
            
            return Ok(atividades);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao obter atividades agropecuárias do tipo {Tipo}", tipo);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Obtém atividades agropecuárias ativas por tipo (otimizado para dropdowns)
    /// </summary>
    /// <param name="tipo">Tipo da atividade (Agricultura, Pecuaria, Mista)</param>
    [HttpGet("tipo/{tipo}/dropdown")]
    public async Task<IActionResult> ObterDropdownPorTipo(TipoAtividadeAgropecuaria tipo)
    {
        try
        {
            Logger.LogDebug("Obtendo atividades agropecuárias ativas do tipo {Tipo} para dropdown", tipo);
            
            var atividades = await _atividadeService.ObterPorTipoAsync(tipo);
            
            // Filtrar apenas atividades ativas e retornar apenas ID, Codigo e Descricao para otimizar
            var atividadesDropdown = atividades
                .Where(a => a.Ativo)
                .Select(a => new { a.Id, a.Codigo, a.Descricao })
                .OrderBy(a => a.Descricao);
            
            Logger.LogDebug("Encontradas {Count} atividades agropecuárias ativas para dropdown do tipo {Tipo}", atividadesDropdown.Count(), tipo);
            
            return Ok(atividadesDropdown);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao obter atividades agropecuárias dropdown do tipo {Tipo}", tipo);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Obtém todos os tipos de atividade agropecuária disponíveis
    /// </summary>
    [HttpGet("tipos")]
    public IActionResult ObterTipos()
    {
        try
        {
            Logger.LogDebug("Obtendo tipos de atividade agropecuária");
            
            var tipos = Enum.GetValues<TipoAtividadeAgropecuaria>()
                .Select(t => new { 
                    Valor = (int)t, 
                    Nome = t.ToString(),
                    Descricao = t switch
                    {
                        TipoAtividadeAgropecuaria.Agricultura => "Agricultura",
                        TipoAtividadeAgropecuaria.Pecuaria => "Pecuária",
                        TipoAtividadeAgropecuaria.Mista => "Mista",
                        _ => t.ToString()
                    }
                });
            
            return Ok(tipos);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao obter tipos de atividade agropecuária");
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}