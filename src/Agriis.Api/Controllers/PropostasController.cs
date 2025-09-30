using Agriis.Pedidos.Aplicacao.DTOs;
using Agriis.Pedidos.Aplicacao.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Agriis.Api.Controllers;

/// <summary>
/// Controller para gerenciamento de propostas
/// </summary>
[ApiController]
[Route("api/pedidos/{pedidoId}/propostas")]
//[Authorize]
public class PropostasController : ControllerBase
{
    private readonly IPropostaService _propostaService;
    private readonly ILogger<PropostasController> _logger;
    
    /// <summary>
    /// Construtor do controller de propostas
    /// </summary>
    public PropostasController(IPropostaService propostaService, ILogger<PropostasController> logger)
    {
        _propostaService = propostaService;
        _logger = logger;
    }
    
    /// <summary>
    /// Cria uma nova proposta para um pedido
    /// </summary>
    /// <param name="pedidoId">ID do pedido</param>
    /// <param name="dto">Dados da proposta</param>
    /// <returns>Resultado da operação</returns>
    [HttpPost]
    public async Task<IActionResult> CriarProposta(int pedidoId, [FromBody] CriarPropostaDto dto)
    {
        try
        {
            var usuarioId = ObterUsuarioId();
            var clientId = ObterClientId();
            
            if (usuarioId == 0)
            {
                return Unauthorized(new { error_code = "UNAUTHORIZED", error_description = "Usuário não autenticado" });
            }
            
            if (string.IsNullOrEmpty(clientId))
            {
                return BadRequest(new { error_code = "INVALID_CLIENT", error_description = "Tipo de cliente não identificado" });
            }
            
            var resultado = await _propostaService.CriarPropostaAsync(pedidoId, usuarioId, clientId, dto);
            
            if (!resultado.IsSuccess)
            {
                return BadRequest(new { error_code = "BUSINESS_ERROR", error_description = resultado.Error });
            }
            
            return Ok(new { message = "Proposta criada com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar proposta para pedido {PedidoId}", pedidoId);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }
    
    /// <summary>
    /// Lista todas as propostas de um pedido
    /// </summary>
    /// <param name="pedidoId">ID do pedido</param>
    /// <param name="dto">Parâmetros de listagem</param>
    /// <returns>Lista paginada de propostas</returns>
    [HttpPost("all")]
    public async Task<IActionResult> ListarPropostas(int pedidoId, [FromBody] ListarPropostasDto dto)
    {
        try
        {
            var resultado = await _propostaService.ListarPropostasAsync(pedidoId, dto);
            
            if (!resultado.IsSuccess)
            {
                return BadRequest(new { error_code = "BUSINESS_ERROR", error_description = resultado.Error });
            }
            
            return Ok(resultado.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar propostas do pedido {PedidoId}", pedidoId);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }
    
    /// <summary>
    /// Obtém a última proposta de um pedido
    /// </summary>
    /// <param name="pedidoId">ID do pedido</param>
    /// <returns>Última proposta</returns>
    [HttpGet("ultima")]
    public async Task<IActionResult> ObterUltimaProposta(int pedidoId)
    {
        try
        {
            var resultado = await _propostaService.ObterUltimaPropostaAsync(pedidoId);
            
            if (!resultado.IsSuccess)
            {
                return BadRequest(new { error_code = "BUSINESS_ERROR", error_description = resultado.Error });
            }
            
            return Ok(resultado.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter última proposta do pedido {PedidoId}", pedidoId);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }
    
    /// <summary>
    /// Obtém o ID do usuário do token JWT
    /// </summary>
    /// <returns>ID do usuário</returns>
    private int ObterUsuarioId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
    
    /// <summary>
    /// Obtém o ID do cliente do token JWT
    /// </summary>
    /// <returns>ID do cliente</returns>
    private string? ObterClientId()
    {
        return User.FindFirst("client_id")?.Value;
    }
}