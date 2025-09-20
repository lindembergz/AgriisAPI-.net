using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Agriis.Safras.Aplicacao.DTOs;
using Agriis.Safras.Aplicacao.Interfaces;

namespace Agriis.Api.Controllers;

/// <summary>
/// Controller para gerenciamento de Safras
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SafrasController : ControllerBase
{
    private readonly ISafraService _safraService;
    private readonly ILogger<SafrasController> _logger;

    public SafrasController(ISafraService safraService, ILogger<SafrasController> logger)
    {
        _safraService = safraService;
        _logger = logger;
    }

    /// <summary>
    /// Obtém todas as safras ordenadas por data de plantio
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ObterTodas()
    {
        var resultado = await _safraService.ObterTodasAsync();
        
        if (!resultado.IsSuccess)
        {
            return BadRequest(new { error_description = resultado.Error });
        }

        return Ok(resultado.Value);
    }

    /// <summary>
    /// Obtém a safra atual (ativa)
    /// </summary>
    [HttpGet("atual")]
    [AllowAnonymous] // Permitir acesso sem autenticação como no sistema Python
    public async Task<IActionResult> ObterSafraAtual()
    {
        var resultado = await _safraService.ObterSafraAtualAsync();
        
        if (!resultado.IsSuccess)
        {
            return BadRequest(new { error_description = resultado.Error });
        }

        if (resultado.Value == null)
        {
            return NotFound(new { error_description = "Nenhuma safra ativa encontrada" });
        }

        return Ok(resultado.Value);
    }

    /// <summary>
    /// Obtém uma safra por ID
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var resultado = await _safraService.ObterPorIdAsync(id);
        
        if (!resultado.IsSuccess)
        {
            return NotFound(new { error_description = resultado.Error });
        }

        return Ok(resultado.Value);
    }

    /// <summary>
    /// Obtém safras por ano de colheita
    /// </summary>
    [HttpGet("ano-colheita/{anoColheita:int}")]
    public async Task<IActionResult> ObterPorAnoColheita(int anoColheita)
    {
        var resultado = await _safraService.ObterPorAnoColheitaAsync(anoColheita);
        
        if (!resultado.IsSuccess)
        {
            return BadRequest(new { error_description = resultado.Error });
        }

        return Ok(resultado.Value);
    }

    /// <summary>
    /// Cria uma nova safra
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarSafraDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var resultado = await _safraService.CriarAsync(dto);
        
        if (!resultado.IsSuccess)
        {
            return BadRequest(new { error_description = resultado.Error });
        }

        return CreatedAtAction(
            nameof(ObterPorId), 
            new { id = resultado.Value!.Id }, 
            resultado.Value);
    }

    /// <summary>
    /// Atualiza uma safra existente
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarSafraDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var resultado = await _safraService.AtualizarAsync(id, dto);
        
        if (!resultado.IsSuccess)
        {
            return BadRequest(new { error_description = resultado.Error });
        }

        return Ok(resultado.Value);
    }

    /// <summary>
    /// Remove uma safra
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int id)
    {
        var resultado = await _safraService.RemoverAsync(id);
        
        if (!resultado.IsSuccess)
        {
            return BadRequest(new { error_description = resultado.Error });
        }

        return NoContent();
    }
}