using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Agriis.PontosDistribuicao.Aplicacao.DTOs;
using Agriis.PontosDistribuicao.Aplicacao.Servicos;

namespace Agriis.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PontosDistribuicaoController : ControllerBase
{
    private readonly PontoDistribuicaoService _pontoDistribuicaoService;
    private readonly ILogger<PontosDistribuicaoController> _logger;

    public PontosDistribuicaoController(
        PontoDistribuicaoService pontoDistribuicaoService, 
        ILogger<PontosDistribuicaoController> logger)
    {
        _pontoDistribuicaoService = pontoDistribuicaoService;
        _logger = logger;
    }

    /// <summary>
    /// Obtém um ponto de distribuição por ID
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var resultado = await _pontoDistribuicaoService.ObterPorIdAsync(id);
        
        if (!resultado.IsSuccess)
        {
            return NotFound(new { error_description = resultado.Error });
        }

        return Ok(resultado.Value);
    }

    /// <summary>
    /// Obtém pontos de distribuição por fornecedor
    /// </summary>
    [HttpGet("fornecedor/{fornecedorId:int}")]
    public async Task<IActionResult> ObterPorFornecedor(int fornecedorId, [FromQuery] bool apenasAtivos = true)
    {
        var resultado = await _pontoDistribuicaoService.ObterPorFornecedorAsync(fornecedorId, apenasAtivos);
        
        if (!resultado.IsSuccess)
        {
            return BadRequest(new { error_description = resultado.Error });
        }

        return Ok(resultado.Value);
    }

    /// <summary>
    /// Busca pontos de distribuição por localização
    /// </summary>
    [HttpPost("buscar-por-localizacao")]
    public async Task<IActionResult> BuscarPorLocalizacao([FromBody] ConsultaPontosPorLocalizacaoDto consulta)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var resultado = await _pontoDistribuicaoService.BuscarPorLocalizacaoAsync(consulta);
        
        if (!resultado.IsSuccess)
        {
            return BadRequest(new { error_description = resultado.Error });
        }

        return Ok(resultado.Value);
    }

    /// <summary>
    /// Busca pontos de distribuição próximos por coordenadas (GET alternativo)
    /// </summary>
    [HttpGet("proximos")]
    public async Task<IActionResult> BuscarProximos(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] double raioKm = 50,
        [FromQuery] int? fornecedorId = null,
        [FromQuery] bool apenasAtivos = true)
    {
        var consulta = new ConsultaPontosPorLocalizacaoDto
        {
            Latitude = latitude,
            Longitude = longitude,
            RaioKm = raioKm,
            FornecedorId = fornecedorId,
            ApenasAtivos = apenasAtivos
        };

        var resultado = await _pontoDistribuicaoService.BuscarPorLocalizacaoAsync(consulta);
        
        if (!resultado.IsSuccess)
        {
            return BadRequest(new { error_description = resultado.Error });
        }

        return Ok(resultado.Value);
    }

    /// <summary>
    /// Busca pontos de distribuição por estado
    /// </summary>
    [HttpGet("estado/{estadoId:int}")]
    public async Task<IActionResult> BuscarPorEstado(
        int estadoId,
        [FromQuery] int? fornecedorId = null,
        [FromQuery] bool apenasAtivos = true)
    {
        var consulta = new ConsultaPontosPorLocalizacaoDto
        {
            EstadoId = estadoId,
            FornecedorId = fornecedorId,
            ApenasAtivos = apenasAtivos
        };

        var resultado = await _pontoDistribuicaoService.BuscarPorLocalizacaoAsync(consulta);
        
        if (!resultado.IsSuccess)
        {
            return BadRequest(new { error_description = resultado.Error });
        }

        return Ok(resultado.Value);
    }

    /// <summary>
    /// Busca pontos de distribuição por município
    /// </summary>
    [HttpGet("municipio/{municipioId:int}")]
    public async Task<IActionResult> BuscarPorMunicipio(
        int municipioId,
        [FromQuery] int? fornecedorId = null,
        [FromQuery] bool apenasAtivos = true)
    {
        var consulta = new ConsultaPontosPorLocalizacaoDto
        {
            MunicipioId = municipioId,
            FornecedorId = fornecedorId,
            ApenasAtivos = apenasAtivos
        };

        var resultado = await _pontoDistribuicaoService.BuscarPorLocalizacaoAsync(consulta);
        
        if (!resultado.IsSuccess)
        {
            return BadRequest(new { error_description = resultado.Error });
        }

        return Ok(resultado.Value);
    }

    /// <summary>
    /// Cria um novo ponto de distribuição
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarPontoDistribuicaoDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var resultado = await _pontoDistribuicaoService.CriarAsync(dto);
        
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
    /// Atualiza um ponto de distribuição existente
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarPontoDistribuicaoDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var resultado = await _pontoDistribuicaoService.AtualizarAsync(id, dto);
        
        if (!resultado.IsSuccess)
        {
            return BadRequest(new { error_description = resultado.Error });
        }

        return Ok(resultado.Value);
    }

    /// <summary>
    /// Ativa um ponto de distribuição
    /// </summary>
    [HttpPatch("{id:int}/ativar")]
    public async Task<IActionResult> Ativar(int id)
    {
        var resultado = await _pontoDistribuicaoService.AtivarAsync(id);
        
        if (!resultado.IsSuccess)
        {
            return BadRequest(new { error_description = resultado.Error });
        }

        return Ok(new { message = "Ponto de distribuição ativado com sucesso" });
    }

    /// <summary>
    /// Desativa um ponto de distribuição
    /// </summary>
    [HttpPatch("{id:int}/desativar")]
    public async Task<IActionResult> Desativar(int id)
    {
        var resultado = await _pontoDistribuicaoService.DesativarAsync(id);
        
        if (!resultado.IsSuccess)
        {
            return BadRequest(new { error_description = resultado.Error });
        }

        return Ok(new { message = "Ponto de distribuição desativado com sucesso" });
    }

    /// <summary>
    /// Remove um ponto de distribuição
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int id)
    {
        var resultado = await _pontoDistribuicaoService.RemoverAsync(id);
        
        if (!resultado.IsSuccess)
        {
            return BadRequest(new { error_description = resultado.Error });
        }

        return NoContent();
    }

    /// <summary>
    /// Obtém estatísticas de pontos de distribuição por fornecedor
    /// </summary>
    [HttpGet("fornecedor/{fornecedorId:int}/estatisticas")]
    public async Task<IActionResult> ObterEstatisticas(int fornecedorId)
    {
        var resultado = await _pontoDistribuicaoService.ObterEstatisticasAsync(fornecedorId);
        
        if (!resultado.IsSuccess)
        {
            return BadRequest(new { error_description = resultado.Error });
        }

        return Ok(resultado.Value);
    }
}