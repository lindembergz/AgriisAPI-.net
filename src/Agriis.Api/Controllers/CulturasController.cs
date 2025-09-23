using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Agriis.Culturas.Aplicacao.DTOs;
using Agriis.Culturas.Aplicacao.Interfaces;

namespace Agriis.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class CulturasController : ControllerBase
{
    private readonly ICulturaService _culturaService;
    private readonly ILogger<CulturasController> _logger;

    public CulturasController(ICulturaService culturaService, ILogger<CulturasController> logger)
    {
        _culturaService = culturaService;
        _logger = logger;
    }

    /// <summary>
    /// Obtém todas as culturas
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ObterTodas()
    {
        var resultado = await _culturaService.ObterTodasAsync();
        
        if (!resultado.IsSuccess)
        {
            return BadRequest(new { error_description = resultado.Error });
        }

        return Ok(resultado.Value);
    }

    /// <summary>
    /// Obtém apenas as culturas ativas
    /// </summary>
    [HttpGet("ativas")]
    public async Task<IActionResult> ObterAtivas()
    {
        var resultado = await _culturaService.ObterAtivasAsync();
        
        if (!resultado.IsSuccess)
        {
            return BadRequest(new { error_description = resultado.Error });
        }

        return Ok(resultado.Value);
    }

    /// <summary>
    /// Obtém uma cultura por ID
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var resultado = await _culturaService.ObterPorIdAsync(id);
        
        if (!resultado.IsSuccess)
        {
            return NotFound(new { error_description = resultado.Error });
        }

        return Ok(resultado.Value);
    }

    /// <summary>
    /// Obtém uma cultura por nome
    /// </summary>
    [HttpGet("nome/{nome}")]
    public async Task<IActionResult> ObterPorNome(string nome)
    {
        var resultado = await _culturaService.ObterPorNomeAsync(nome);
        
        if (!resultado.IsSuccess)
        {
            return NotFound(new { error_description = resultado.Error });
        }

        return Ok(resultado.Value);
    }

    /// <summary>
    /// Cria uma nova cultura
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarCulturaDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var resultado = await _culturaService.CriarAsync(dto);
        
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
    /// Atualiza uma cultura existente
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarCulturaDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var resultado = await _culturaService.AtualizarAsync(id, dto);
        
        if (!resultado.IsSuccess)
        {
            return BadRequest(new { error_description = resultado.Error });
        }

        return Ok(resultado.Value);
    }

    /// <summary>
    /// Remove uma cultura
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int id)
    {
        var resultado = await _culturaService.RemoverAsync(id);
        
        if (!resultado.IsSuccess)
        {
            return BadRequest(new { error_description = resultado.Error });
        }

        return NoContent();
    }
}