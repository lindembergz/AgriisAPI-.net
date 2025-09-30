using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Agriis.Pagamentos.Aplicacao.Interfaces;
using Agriis.Pagamentos.Aplicacao.DTOs;

namespace Agriis.Api.Controllers;

/// <summary>
/// Controller para gerenciamento de formas de pagamento
/// </summary>
[ApiController]
[Route("api/v1/pagamentos")]
//[Authorize]
public class PagamentosController : ControllerBase
{
    private readonly IFormaPagamentoService _formaPagamentoService;
    private readonly ICulturaFormaPagamentoService _culturaFormaPagamentoService;

    public PagamentosController(
        IFormaPagamentoService formaPagamentoService,
        ICulturaFormaPagamentoService culturaFormaPagamentoService)
    {
        _formaPagamentoService = formaPagamentoService;
        _culturaFormaPagamentoService = culturaFormaPagamentoService;
    }

    /// <summary>
    /// Lista todas as formas de pagamento ativas
    /// </summary>
    /// <returns>Lista de formas de pagamento</returns>
    [HttpGet("formas")]
    public async Task<IActionResult> ListarFormasPagamento()
    {
        var resultado = await _formaPagamentoService.ObterAtivasAsync();
        
        if (!resultado.IsSuccess)
            return BadRequest(new { error_description = resultado.Error });
            
        return Ok(resultado.Value);
    }

    /// <summary>
    /// Obtém formas de pagamento disponíveis para um pedido
    /// </summary>
    /// <param name="pedidoId">ID do pedido</param>
    /// <returns>Lista de formas de pagamento disponíveis</returns>
    [HttpGet("pedido/{pedidoId}/formas")]
    public async Task<IActionResult> ListarFormasPagamentoPorPedido(int pedidoId)
    {
        var resultado = await _formaPagamentoService.ObterPorPedidoAsync(pedidoId);
        
        if (!resultado.IsSuccess)
            return BadRequest(new { error_description = resultado.Error });
            
        return Ok(resultado.Value);
    }

    /// <summary>
    /// Obtém uma forma de pagamento por ID
    /// </summary>
    /// <param name="id">ID da forma de pagamento</param>
    /// <returns>Forma de pagamento</returns>
    [HttpGet("formas/{id}")]
    public async Task<IActionResult> ObterFormaPagamento(int id)
    {
        var resultado = await _formaPagamentoService.ObterPorIdAsync(id);
        
        if (!resultado.IsSuccess)
            return NotFound(new { error_description = resultado.Error });
            
        return Ok(resultado.Value);
    }

    /// <summary>
    /// Cria uma nova forma de pagamento
    /// </summary>
    /// <param name="dto">Dados da forma de pagamento</param>
    /// <returns>Forma de pagamento criada</returns>
    [HttpPost("formas")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> CriarFormaPagamento([FromBody] CriarFormaPagamentoDto dto)
    {
        var resultado = await _formaPagamentoService.CriarAsync(dto);
        
        if (!resultado.IsSuccess)
            return BadRequest(new { error_description = resultado.Error });
            
        return CreatedAtAction(
            nameof(ObterFormaPagamento), 
            new { id = resultado.Value!.Id }, 
            resultado.Value);
    }

    /// <summary>
    /// Atualiza uma forma de pagamento
    /// </summary>
    /// <param name="id">ID da forma de pagamento</param>
    /// <param name="dto">Dados atualizados</param>
    /// <returns>Forma de pagamento atualizada</returns>
    [HttpPut("formas/{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> AtualizarFormaPagamento(int id, [FromBody] AtualizarFormaPagamentoDto dto)
    {
        var resultado = await _formaPagamentoService.AtualizarAsync(id, dto);
        
        if (!resultado.IsSuccess)
            return BadRequest(new { error_description = resultado.Error });
            
        return Ok(resultado.Value);
    }

    /// <summary>
    /// Remove uma forma de pagamento
    /// </summary>
    /// <param name="id">ID da forma de pagamento</param>
    /// <returns>Resultado da operação</returns>
    [HttpDelete("formas/{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> RemoverFormaPagamento(int id)
    {
        var resultado = await _formaPagamentoService.RemoverAsync(id);
        
        if (!resultado.IsSuccess)
            return BadRequest(new { error_description = resultado.Error });
            
        return NoContent();
    }

    /// <summary>
    /// Cria associação entre cultura, fornecedor e forma de pagamento
    /// </summary>
    /// <param name="dto">Dados da associação</param>
    /// <returns>Associação criada</returns>
    [HttpPost("cultura_forma_pagamento")]
    [Authorize(Roles = "FORNECEDOR_WEB_ADMIN,FORNECEDOR_WEB_REPRESENTANTE")]
    public async Task<IActionResult> CriarCulturaFormaPagamento([FromBody] CriarCulturaFormaPagamentoDto dto)
    {
        var resultado = await _culturaFormaPagamentoService.CriarAsync(dto);
        
        if (!resultado.IsSuccess)
            return BadRequest(new { error_description = resultado.Error });
            
        return Created($"/api/v1/pagamentos/cultura_forma_pagamento/{resultado.Value!.Id}", resultado.Value);
    }

    /// <summary>
    /// Remove associação entre cultura, fornecedor e forma de pagamento
    /// </summary>
    /// <param name="culturaFormaPagamentoId">ID da associação</param>
    /// <returns>Resultado da operação</returns>
    [HttpDelete("cultura_forma_pagamento/{culturaFormaPagamentoId}")]
    [Authorize(Roles = "FORNECEDOR_WEB_ADMIN,FORNECEDOR_WEB_REPRESENTANTE")]
    public async Task<IActionResult> RemoverCulturaFormaPagamento(int culturaFormaPagamentoId)
    {
        var resultado = await _culturaFormaPagamentoService.RemoverAsync(culturaFormaPagamentoId);
        
        if (!resultado.IsSuccess)
            return BadRequest(new { error_description = resultado.Error });
            
        return NoContent();
    }

    /// <summary>
    /// Obtém formas de pagamento disponíveis para fornecedor e cultura
    /// </summary>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <param name="culturaId">ID da cultura</param>
    /// <returns>Lista de formas de pagamento disponíveis</returns>
    [HttpGet("fornecedor/{fornecedorId}/cultura/{culturaId}/formas")]
    public async Task<IActionResult> ObterFormasPagamentoPorFornecedorCultura(int fornecedorId, int culturaId)
    {
        var resultado = await _culturaFormaPagamentoService
            .ObterFormasPagamentoPorFornecedorCulturaAsync(fornecedorId, culturaId);
        
        if (!resultado.IsSuccess)
            return BadRequest(new { error_description = resultado.Error });
            
        return Ok(resultado.Value);
    }

    /// <summary>
    /// Obtém associações de um fornecedor
    /// </summary>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <returns>Lista de associações do fornecedor</returns>
    [HttpGet("fornecedor/{fornecedorId}/associacoes")]
    [Authorize(Roles = "FORNECEDOR_WEB_ADMIN,FORNECEDOR_WEB_REPRESENTANTE")]
    public async Task<IActionResult> ObterAssociacoesPorFornecedor(int fornecedorId)
    {
        var resultado = await _culturaFormaPagamentoService.ObterPorFornecedorAsync(fornecedorId);
        
        if (!resultado.IsSuccess)
            return BadRequest(new { error_description = resultado.Error });
            
        return Ok(resultado.Value);
    }
}