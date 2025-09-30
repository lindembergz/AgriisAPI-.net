using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Agriis.Combos.Aplicacao.DTOs;
using Agriis.Combos.Aplicacao.Interfaces;
using Agriis.Combos.Dominio.Enums;

namespace Agriis.Api.Controllers;

/// <summary>
/// Controller para gerenciamento de combos promocionais
/// </summary>
[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class CombosController : ControllerBase
{
    private readonly IComboService _comboService;

    public CombosController(IComboService comboService)
    {
        _comboService = comboService;
    }

    /// <summary>
    /// Cria um novo combo
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarComboDto dto)
    {
        var resultado = await _comboService.CriarAsync(dto);
        
        if (!resultado.IsSuccess)
        {
            return BadRequest(new { error_description = resultado.Error });
        }

        return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Value.Id }, resultado.Value);
    }

    /// <summary>
    /// Obtém um combo por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var resultado = await _comboService.ObterPorIdAsync(id);
        
        if (!resultado.IsSuccess)
        {
            return NotFound(new { error_description = resultado.Error });
        }

        return Ok(resultado.Value);
    }

    /// <summary>
    /// Atualiza um combo existente
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarComboDto dto)
    {
        var resultado = await _comboService.AtualizarAsync(id, dto);
        
        if (!resultado.IsSuccess)
        {
            return BadRequest(new { error_description = resultado.Error });
        }

        return Ok(resultado.Value);
    }

    /// <summary>
    /// Obtém combos por fornecedor
    /// </summary>
    [HttpGet("fornecedor/{fornecedorId}")]
    public async Task<IActionResult> ObterPorFornecedor(int fornecedorId)
    {
        var resultado = await _comboService.ObterPorFornecedorAsync(fornecedorId);
        
        if (!resultado.IsSuccess)
        {
            return BadRequest(new { error_description = resultado.Error });
        }

        return Ok(resultado.Value);
    }

    /// <summary>
    /// Obtém combos vigentes
    /// </summary>
    [HttpGet("vigentes")]
    public async Task<IActionResult> ObterCombosVigentes()
    {
        var resultado = await _comboService.ObterCombosVigentesAsync();
        
        if (!resultado.IsSuccess)
        {
            return BadRequest(new { error_description = resultado.Error });
        }

        return Ok(resultado.Value);
    }

    /// <summary>
    /// Obtém combos válidos para um produtor
    /// </summary>
    [HttpGet("produtor/{produtorId}")]
    public async Task<IActionResult> ObterCombosValidosParaProdutor(
        int produtorId, 
        [FromQuery] decimal hectareProdutor, 
        [FromQuery] int municipioId)
    {
        var resultado = await _comboService.ObterCombosValidosParaProdutorAsync(
            produtorId, 
            hectareProdutor, 
            municipioId);
        
        if (!resultado.IsSuccess)
        {
            return BadRequest(new { error_description = resultado.Error });
        }

        return Ok(resultado.Value);
    }

    /// <summary>
    /// Atualiza o status de um combo
    /// </summary>
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> AtualizarStatus(int id, [FromBody] StatusCombo status)
    {
        var resultado = await _comboService.AtualizarStatusAsync(id, status);
        
        if (!resultado.IsSuccess)
        {
            return BadRequest(new { error_description = resultado.Error });
        }

        return NoContent();
    }

    /// <summary>
    /// Remove um combo
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Remover(int id)
    {
        var resultado = await _comboService.RemoverAsync(id);
        
        if (!resultado.IsSuccess)
        {
            return BadRequest(new { error_description = resultado.Error });
        }

        return NoContent();
    }

    /// <summary>
    /// Adiciona um item ao combo
    /// </summary>
    [HttpPost("{comboId}/itens")]
    public async Task<IActionResult> AdicionarItem(int comboId, [FromBody] CriarComboItemDto dto)
    {
        var resultado = await _comboService.AdicionarItemAsync(comboId, dto);
        
        if (!resultado.IsSuccess)
        {
            return BadRequest(new { error_description = resultado.Error });
        }

        return CreatedAtAction(nameof(ObterPorId), new { id = comboId }, resultado.Value);
    }

    /// <summary>
    /// Atualiza um item do combo
    /// </summary>
    [HttpPut("{comboId}/itens/{itemId}")]
    public async Task<IActionResult> AtualizarItem(
        int comboId, 
        int itemId, 
        [FromBody] AtualizarComboItemDto dto)
    {
        var resultado = await _comboService.AtualizarItemAsync(comboId, itemId, dto);
        
        if (!resultado.IsSuccess)
        {
            return BadRequest(new { error_description = resultado.Error });
        }

        return Ok(resultado.Value);
    }

    /// <summary>
    /// Remove um item do combo
    /// </summary>
    [HttpDelete("{comboId}/itens/{itemId}")]
    public async Task<IActionResult> RemoverItem(int comboId, int itemId)
    {
        var resultado = await _comboService.RemoverItemAsync(comboId, itemId);
        
        if (!resultado.IsSuccess)
        {
            return BadRequest(new { error_description = resultado.Error });
        }

        return NoContent();
    }

    /// <summary>
    /// Adiciona um local de recebimento ao combo
    /// </summary>
    [HttpPost("{comboId}/locais-recebimento")]
    public async Task<IActionResult> AdicionarLocalRecebimento(
        int comboId, 
        [FromBody] CriarComboLocalRecebimentoDto dto)
    {
        var resultado = await _comboService.AdicionarLocalRecebimentoAsync(comboId, dto);
        
        if (!resultado.IsSuccess)
        {
            return BadRequest(new { error_description = resultado.Error });
        }

        return CreatedAtAction(nameof(ObterPorId), new { id = comboId }, resultado.Value);
    }

    /// <summary>
    /// Adiciona uma categoria de desconto ao combo
    /// </summary>
    [HttpPost("{comboId}/categorias-desconto")]
    public async Task<IActionResult> AdicionarCategoriaDesconto(
        int comboId, 
        [FromBody] CriarComboCategoriaDescontoDto dto)
    {
        var resultado = await _comboService.AdicionarCategoriaDescontoAsync(comboId, dto);
        
        if (!resultado.IsSuccess)
        {
            return BadRequest(new { error_description = resultado.Error });
        }

        return CreatedAtAction(nameof(ObterPorId), new { id = comboId }, resultado.Value);
    }

    /// <summary>
    /// Valida se um combo é aplicável para um produtor
    /// </summary>
    [HttpGet("{comboId}/validar-produtor/{produtorId}")]
    public async Task<IActionResult> ValidarComboParaProdutor(
        int comboId, 
        int produtorId, 
        [FromQuery] decimal hectareProdutor, 
        [FromQuery] int municipioId)
    {
        var resultado = await _comboService.ValidarComboParaProdutorAsync(
            comboId, 
            produtorId, 
            hectareProdutor, 
            municipioId);
        
        if (!resultado.IsSuccess)
        {
            return BadRequest(new { error_description = resultado.Error });
        }

        return Ok(new { valido = resultado.Value });
    }
}