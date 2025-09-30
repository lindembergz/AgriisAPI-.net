using Agriis.Catalogos.Aplicacao.DTOs;
using Agriis.Catalogos.Aplicacao.Interfaces;
using Agriis.Compartilhado.Dominio.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Agriis.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class CatalogosController : ControllerBase
{
    private readonly ICatalogoService _catalogoService;

    public CatalogosController(ICatalogoService catalogoService)
    {
        _catalogoService = catalogoService;
    }

    [HttpGet]
    public async Task<IActionResult> ObterPaginado(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 10,
        [FromQuery] int? safraId = null,
        [FromQuery] int? pontoDistribuicaoId = null,
        [FromQuery] int? culturaId = null,
        [FromQuery] int? categoriaId = null,
        [FromQuery] Moeda? moeda = null,
        [FromQuery] bool? ativo = null)
    {
        var resultado = await _catalogoService.ObterPaginadoAsync(
            pagina, tamanhoPagina, safraId, pontoDistribuicaoId, culturaId, categoriaId, moeda, ativo);

        if (!resultado.IsSuccess)
            return BadRequest(new { error_description = resultado.Error });

        return Ok(resultado.Value);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var resultado = await _catalogoService.ObterPorIdAsync(id);

        if (!resultado.IsSuccess)
            return NotFound(new { error_description = resultado.Error });

        return Ok(resultado.Value);
    }

    [HttpGet("vigentes")]
    public async Task<IActionResult> ObterVigentes([FromQuery] DateTime? data = null)
    {
        var dataConsulta = data ?? DateTime.UtcNow;
        var resultado = await _catalogoService.ObterVigentesAsync(dataConsulta);

        if (!resultado.IsSuccess)
            return BadRequest(new { error_description = resultado.Error });

        return Ok(resultado.Value);
    }

    [HttpGet("chave-unica")]
    public async Task<IActionResult> ObterPorChaveUnica(
        [FromQuery] int safraId,
        [FromQuery] int pontoDistribuicaoId,
        [FromQuery] int culturaId,
        [FromQuery] int categoriaId)
    {
        var resultado = await _catalogoService.ObterPorChaveUnicaAsync(safraId, pontoDistribuicaoId, culturaId, categoriaId);

        if (!resultado.IsSuccess)
            return NotFound(new { error_description = resultado.Error });

        return Ok(resultado.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarCatalogoDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var resultado = await _catalogoService.CriarAsync(dto);

        if (!resultado.IsSuccess)
            return BadRequest(new { error_description = resultado.Error });

        return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Value.Id }, resultado.Value);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarCatalogoDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var resultado = await _catalogoService.AtualizarAsync(id, dto);

        if (!resultado.IsSuccess)
            return BadRequest(new { error_description = resultado.Error });

        return Ok(resultado.Value);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Remover(int id)
    {
        var resultado = await _catalogoService.RemoverAsync(id);

        if (!resultado.IsSuccess)
            return BadRequest(new { error_description = resultado.Error });

        return NoContent();
    }

    // Endpoints para itens do catálogo
    [HttpPost("{catalogoId}/itens")]
    public async Task<IActionResult> AdicionarItem(int catalogoId, [FromBody] CriarCatalogoItemDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var resultado = await _catalogoService.AdicionarItemAsync(catalogoId, dto);

        if (!resultado.IsSuccess)
            return BadRequest(new { error_description = resultado.Error });

        return CreatedAtAction(nameof(ObterPorId), new { id = catalogoId }, resultado.Value);
    }

    [HttpPut("{catalogoId}/itens/{itemId}")]
    public async Task<IActionResult> AtualizarItem(int catalogoId, int itemId, [FromBody] AtualizarCatalogoItemDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var resultado = await _catalogoService.AtualizarItemAsync(catalogoId, itemId, dto);

        if (!resultado.IsSuccess)
            return BadRequest(new { error_description = resultado.Error });

        return Ok(resultado.Value);
    }

    [HttpDelete("{catalogoId}/itens/{itemId}")]
    public async Task<IActionResult> RemoverItem(int catalogoId, int itemId)
    {
        var resultado = await _catalogoService.RemoverItemAsync(catalogoId, itemId);

        if (!resultado.IsSuccess)
            return BadRequest(new { error_description = resultado.Error });

        return NoContent();
    }

    [HttpPost("{catalogoId}/produtos/{produtoId}/preco")]
    public async Task<IActionResult> ConsultarPreco(int catalogoId, int produtoId, [FromBody] ConsultarPrecoDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var resultado = await _catalogoService.ConsultarPrecoAsync(catalogoId, produtoId, dto);

        if (!resultado.IsSuccess)
            return BadRequest(new { error_description = resultado.Error });

        return Ok(new { preco = resultado.Value });
    }
}