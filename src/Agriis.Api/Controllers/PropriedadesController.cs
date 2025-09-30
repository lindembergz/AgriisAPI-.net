using Agriis.Propriedades.Aplicacao.DTOs;
using Agriis.Propriedades.Aplicacao.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Agriis.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class PropriedadesController : ControllerBase
{
    private readonly IPropriedadeService _propriedadeService;
    private readonly ITalhaoService _talhaoService;
    private readonly IPropriedadeCulturaService _propriedadeCulturaService;

    public PropriedadesController(
        IPropriedadeService propriedadeService,
        ITalhaoService talhaoService,
        IPropriedadeCulturaService propriedadeCulturaService)
    {
        _propriedadeService = propriedadeService;
        _talhaoService = talhaoService;
        _propriedadeCulturaService = propriedadeCulturaService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var resultado = await _propriedadeService.ObterPorIdAsync(id);
        
        if (!resultado.IsSuccess)
            return BadRequest(new { error_description = resultado.Error });

        return Ok(resultado.Value);
    }

    [HttpGet("completa/{id}")]
    public async Task<IActionResult> ObterCompleta(int id)
    {
        var resultado = await _propriedadeService.ObterCompletaAsync(id);
        
        if (!resultado.IsSuccess)
            return BadRequest(new { error_description = resultado.Error });

        return Ok(resultado.Value);
    }

    [HttpGet("produtor/{produtorId}")]
    public async Task<IActionResult> ObterPorProdutor(int produtorId)
    {
        var resultado = await _propriedadeService.ObterPorProdutorAsync(produtorId);
        
        if (!resultado.IsSuccess)
            return BadRequest(new { error_description = resultado.Error });

        return Ok(resultado.Value);
    }

    [HttpGet("cultura/{culturaId}")]
    public async Task<IActionResult> ObterPorCultura(int culturaId)
    {
        var resultado = await _propriedadeService.ObterPorCulturaAsync(culturaId);
        
        if (!resultado.IsSuccess)
            return BadRequest(new { error_description = resultado.Error });

        return Ok(resultado.Value);
    }

    [HttpGet("proximas")]
    public async Task<IActionResult> BuscarPropriedadesProximas(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] double raioKm = 50)
    {
        var resultado = await _propriedadeService.BuscarPropriedadesProximasAsync(latitude, longitude, raioKm);
        
        if (!resultado.IsSuccess)
            return BadRequest(new { error_description = resultado.Error });

        return Ok(resultado.Value);
    }

    [HttpGet("area-total/produtor/{produtorId}")]
    public async Task<IActionResult> CalcularAreaTotalPorProdutor(int produtorId)
    {
        var resultado = await _propriedadeService.CalcularAreaTotalPorProdutorAsync(produtorId);
        
        if (!resultado.IsSuccess)
            return BadRequest(new { error_description = resultado.Error });

        return Ok(new { area_total = resultado.Value });
    }

    [HttpGet("estatisticas-culturas/produtor/{produtorId}")]
    public async Task<IActionResult> ObterEstatisticasCulturasPorProdutor(int produtorId)
    {
        var resultado = await _propriedadeService.ObterEstatisticasCulturasPorProdutorAsync(produtorId);
        
        if (!resultado.IsSuccess)
            return BadRequest(new { error_description = resultado.Error });

        return Ok(resultado.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] PropriedadeCreateDto dto)
    {
        var resultado = await _propriedadeService.CriarAsync(dto);
        
        if (!resultado.IsSuccess)
            return BadRequest(new { error_description = resultado.Error });

        return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Value.Id }, resultado.Value);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] PropriedadeUpdateDto dto)
    {
        var resultado = await _propriedadeService.AtualizarAsync(id, dto);
        
        if (!resultado.IsSuccess)
            return BadRequest(new { error_description = resultado.Error });

        return Ok(resultado.Value);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Remover(int id)
    {
        var resultado = await _propriedadeService.RemoverAsync(id);
        
        if (!resultado.IsSuccess)
            return BadRequest(new { error_description = resultado.Error });

        return NoContent();
    }

    // Endpoints para Talhões
    [HttpGet("{propriedadeId}/talhoes")]
    public async Task<IActionResult> ObterTalhoesPorPropriedade(int propriedadeId)
    {
        var resultado = await _talhaoService.ObterPorPropriedadeAsync(propriedadeId);
        
        if (!resultado.IsSuccess)
            return BadRequest(new { error_description = resultado.Error });

        return Ok(resultado.Value);
    }

    [HttpGet("talhoes/{id}")]
    public async Task<IActionResult> ObterTalhaoPorId(int id)
    {
        var resultado = await _talhaoService.ObterPorIdAsync(id);
        
        if (!resultado.IsSuccess)
            return BadRequest(new { error_description = resultado.Error });

        return Ok(resultado.Value);
    }

    [HttpGet("talhoes/proximos")]
    public async Task<IActionResult> BuscarTalhoesProximos(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] double raioKm = 50)
    {
        var resultado = await _talhaoService.BuscarTalhoesProximosAsync(latitude, longitude, raioKm);
        
        if (!resultado.IsSuccess)
            return BadRequest(new { error_description = resultado.Error });

        return Ok(resultado.Value);
    }

    [HttpPost("talhoes")]
    public async Task<IActionResult> CriarTalhao([FromBody] TalhaoCreateDto dto)
    {
        var resultado = await _talhaoService.CriarAsync(dto);
        
        if (!resultado.IsSuccess)
            return BadRequest(new { error_description = resultado.Error });

        return CreatedAtAction(nameof(ObterTalhaoPorId), new { id = resultado.Value.Id }, resultado.Value);
    }

    [HttpPut("talhoes/{id}")]
    public async Task<IActionResult> AtualizarTalhao(int id, [FromBody] TalhaoUpdateDto dto)
    {
        var resultado = await _talhaoService.AtualizarAsync(id, dto);
        
        if (!resultado.IsSuccess)
            return BadRequest(new { error_description = resultado.Error });

        return Ok(resultado.Value);
    }

    [HttpDelete("talhoes/{id}")]
    public async Task<IActionResult> RemoverTalhao(int id)
    {
        var resultado = await _talhaoService.RemoverAsync(id);
        
        if (!resultado.IsSuccess)
            return BadRequest(new { error_description = resultado.Error });

        return NoContent();
    }

    // Endpoints para Culturas da Propriedade
    [HttpGet("{propriedadeId}/culturas")]
    public async Task<IActionResult> ObterCulturasPorPropriedade(int propriedadeId)
    {
        var resultado = await _propriedadeCulturaService.ObterPorPropriedadeAsync(propriedadeId);
        
        if (!resultado.IsSuccess)
            return BadRequest(new { error_description = resultado.Error });

        return Ok(resultado.Value);
    }

    [HttpGet("culturas/{id}")]
    public async Task<IActionResult> ObterPropriedadeCulturaPorId(int id)
    {
        var resultado = await _propriedadeCulturaService.ObterPorIdAsync(id);
        
        if (!resultado.IsSuccess)
            return BadRequest(new { error_description = resultado.Error });

        return Ok(resultado.Value);
    }

    [HttpGet("culturas/cultura/{culturaId}")]
    public async Task<IActionResult> ObterPropriedadesPorCultura(int culturaId)
    {
        var resultado = await _propriedadeCulturaService.ObterPorCulturaAsync(culturaId);
        
        if (!resultado.IsSuccess)
            return BadRequest(new { error_description = resultado.Error });

        return Ok(resultado.Value);
    }

    [HttpGet("culturas/periodo-plantio")]
    public async Task<IActionResult> ObterCulturasEmPeriodoPlantio()
    {
        var resultado = await _propriedadeCulturaService.ObterEmPeriodoPlantioAsync();
        
        if (!resultado.IsSuccess)
            return BadRequest(new { error_description = resultado.Error });

        return Ok(resultado.Value);
    }

    [HttpPost("culturas")]
    public async Task<IActionResult> CriarPropriedadeCultura([FromBody] PropriedadeCulturaCreateDto dto)
    {
        var resultado = await _propriedadeCulturaService.CriarAsync(dto);
        
        if (!resultado.IsSuccess)
            return BadRequest(new { error_description = resultado.Error });

        return CreatedAtAction(nameof(ObterPropriedadeCulturaPorId), new { id = resultado.Value.Id }, resultado.Value);
    }

    [HttpPut("culturas/{id}")]
    public async Task<IActionResult> AtualizarPropriedadeCultura(int id, [FromBody] PropriedadeCulturaUpdateDto dto)
    {
        var resultado = await _propriedadeCulturaService.AtualizarAsync(id, dto);
        
        if (!resultado.IsSuccess)
            return BadRequest(new { error_description = resultado.Error });

        return Ok(resultado.Value);
    }

    [HttpDelete("culturas/{id}")]
    public async Task<IActionResult> RemoverPropriedadeCultura(int id)
    {
        var resultado = await _propriedadeCulturaService.RemoverAsync(id);
        
        if (!resultado.IsSuccess)
            return BadRequest(new { error_description = resultado.Error });

        return NoContent();
    }
}