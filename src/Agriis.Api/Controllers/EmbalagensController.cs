using Microsoft.AspNetCore.Mvc;
using Agriis.Referencias.Aplicacao.DTOs;
using Agriis.Referencias.Aplicacao.Interfaces;

namespace Agriis.Api.Controllers;

/// <summary>
/// Controlador para gerenciamento de embalagens
/// </summary>
[ApiController]
[Route("api/referencias/embalagens")]
public class EmbalagensController : ReferenciaControllerBase<EmbalagemDto, CriarEmbalagemDto, AtualizarEmbalagemDto>
{
    private readonly IEmbalagemService _embalagemService;

    public EmbalagensController(
        IEmbalagemService embalagemService,
        ILogger<EmbalagensController> logger) : base(embalagemService, logger)
    {
        _embalagemService = embalagemService;
    }

    /// <summary>
    /// Verifica se existe uma embalagem com o nome especificado
    /// </summary>
    /// <param name="nome">Nome da embalagem</param>
    /// <param name="idExcluir">ID da embalagem a ser excluída da verificação (opcional)</param>
    [HttpGet("existe-nome/{nome}")]
    public async Task<IActionResult> ExisteNome(string nome, [FromQuery] int? idExcluir = null)
    {
        try
        {
            Logger.LogDebug("Verificando se existe embalagem com nome {Nome}", nome);
            
            var existe = await _embalagemService.ExisteNomeAsync(nome, idExcluir);
            
            return Ok(new { Existe = existe });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao verificar se existe embalagem com nome {Nome}", nome);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Obtém uma embalagem por nome
    /// </summary>
    /// <param name="nome">Nome da embalagem</param>
    [HttpGet("nome/{nome}")]
    public async Task<IActionResult> ObterPorNome(string nome)
    {
        try
        {
            Logger.LogDebug("Obtendo embalagem com nome {Nome}", nome);
            
            var embalagem = await _embalagemService.ObterPorNomeAsync(nome);


            if (embalagem == null)
            {
                Logger.LogWarning("Embalagem com nome {Nome} não encontrada", nome);
                return NotFound(new { 
                    ErrorCode = "ENTITY_NOT_FOUND", 
                    ErrorDescription = "Embalagem não encontrada",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            
            return Ok(embalagem);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao obter embalagem com nome {Nome}", nome);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Obtém embalagens por unidade de medida
    /// </summary>
    /// <param name="unidadeId">ID da unidade de medida</param>
    [HttpGet("unidade/{unidadeId:int}")]
    public async Task<IActionResult> ObterPorUnidadeMedida(int unidadeId)
    {
        try
        {
            Logger.LogDebug("Obtendo embalagens da unidade de medida com ID {UnidadeId}", unidadeId);
            
            var embalagens = await _embalagemService.ObterPorUnidadeMedidaAsync(unidadeId);
            
            Logger.LogDebug("Encontradas {Count} embalagens para a unidade de medida {UnidadeId}", embalagens.Count(), unidadeId);
            
            return Ok(embalagens);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao obter embalagens da unidade de medida com ID {UnidadeId}", unidadeId);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Obtém embalagens ativas por unidade de medida (otimizado para dropdowns)
    /// </summary>
    /// <param name="unidadeId">ID da unidade de medida</param>
    [HttpGet("unidade/{unidadeId:int}/dropdown")]
    public async Task<IActionResult> ObterDropdownPorUnidadeMedida(int unidadeId)
    {
        try
        {
            Logger.LogDebug("Obtendo embalagens ativas da unidade de medida com ID {UnidadeId} para dropdown", unidadeId);
            
            var embalagens = await _embalagemService.ObterPorUnidadeMedidaAsync(unidadeId);
            
            // Filtrar apenas embalagens ativas e retornar apenas ID e Nome para otimizar
            var embalagensDropdown = embalagens
                .Where(e => e.Ativo)
                .Select(e => new { e.Id, e.Nome, e.Descricao })
                .OrderBy(e => e.Nome);
            
            Logger.LogDebug("Encontradas {Count} embalagens ativas para dropdown da unidade de medida {UnidadeId}", embalagensDropdown.Count(), unidadeId);
            
            return Ok(embalagensDropdown);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao obter embalagens dropdown da unidade de medida com ID {UnidadeId}", unidadeId);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Busca embalagens por nome (busca parcial)
    /// </summary>
    /// <param name="nome">Nome ou parte do nome da embalagem</param>
    /// <param name="unidadeId">ID da unidade de medida para filtrar (opcional)</param>
    /// <param name="limite">Limite de resultados (padrão: 50)</param>
    [HttpGet("buscar")]
    public async Task<IActionResult> BuscarPorNome([FromQuery] string nome, [FromQuery] int? unidadeId = null, [FromQuery] int limite = 50)
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

            Logger.LogDebug("Buscando embalagens com nome {Nome} na unidade {UnidadeId}", nome, unidadeId);
            
            var embalagens = await _embalagemService.BuscarPorNomeAsync(nome);
            
            Logger.LogDebug("Encontrada embalagens na busca por {Nome}", nome);
            
            return Ok(embalagens);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao buscar embalagens com nome {Nome}", nome);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}