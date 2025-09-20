using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Agriis.Pedidos.Aplicacao.Interfaces;
using Agriis.Pedidos.Aplicacao.DTOs;

namespace Agriis.Api.Controllers;

/// <summary>
/// Controller para operações de carrinho de compras
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CarrinhoController : ControllerBase
{
    private readonly IPedidoService _pedidoService;
    private readonly ILogger<CarrinhoController> _logger;

    public CarrinhoController(IPedidoService pedidoService, ILogger<CarrinhoController> logger)
    {
        _pedidoService = pedidoService;
        _logger = logger;
    }

    /// <summary>
    /// Adiciona um item ao carrinho de compras
    /// </summary>
    /// <param name="pedidoId">ID do pedido (carrinho)</param>
    /// <param name="dto">Dados do item a ser adicionado</param>
    /// <returns>Item adicionado</returns>
    [HttpPost("{pedidoId}/itens")]
    public async Task<ActionResult<PedidoItemDto>> AdicionarItem(int pedidoId, [FromBody] AdicionarItemCarrinhoDto dto)
    {
        try
        {
            var criarItemDto = new CriarPedidoItemDto
            {
                PedidoId = pedidoId,
                ProdutoId = dto.ProdutoId,
                Quantidade = dto.Quantidade,
                Observacoes = dto.Observacoes
            };

            var item = await _pedidoService.AdicionarItemCarrinhoAsync(pedidoId, criarItemDto, dto.CatalogoId);
            return Ok(item);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro ao adicionar item ao carrinho {PedidoId}", pedidoId);
            return BadRequest(new { error_code = "INVALID_OPERATION", error_description = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno ao adicionar item ao carrinho {PedidoId}", pedidoId);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Remove um item do carrinho de compras
    /// </summary>
    /// <param name="pedidoId">ID do pedido (carrinho)</param>
    /// <param name="itemId">ID do item a ser removido</param>
    /// <returns>Resultado da operação</returns>
    [HttpDelete("{pedidoId}/itens/{itemId}")]
    public async Task<ActionResult> RemoverItem(int pedidoId, int itemId)
    {
        try
        {
            await _pedidoService.RemoverItemCarrinhoAsync(pedidoId, itemId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro ao remover item {ItemId} do carrinho {PedidoId}", itemId, pedidoId);
            return BadRequest(new { error_code = "INVALID_OPERATION", error_description = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno ao remover item {ItemId} do carrinho {PedidoId}", itemId, pedidoId);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Atualiza a quantidade de um item no carrinho
    /// </summary>
    /// <param name="pedidoId">ID do pedido (carrinho)</param>
    /// <param name="itemId">ID do item</param>
    /// <param name="dto">Nova quantidade</param>
    /// <returns>Item atualizado</returns>
    [HttpPut("{pedidoId}/itens/{itemId}/quantidade")]
    public async Task<ActionResult<PedidoItemDto>> AtualizarQuantidade(int pedidoId, int itemId, [FromBody] AtualizarQuantidadeItemDto dto)
    {
        try
        {
            var item = await _pedidoService.AtualizarQuantidadeItemAsync(pedidoId, itemId, dto.Quantidade);
            return Ok(item);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro ao atualizar quantidade do item {ItemId} no carrinho {PedidoId}", itemId, pedidoId);
            return BadRequest(new { error_code = "INVALID_OPERATION", error_description = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno ao atualizar quantidade do item {ItemId} no carrinho {PedidoId}", itemId, pedidoId);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Recalcula os totais do carrinho
    /// </summary>
    /// <param name="pedidoId">ID do pedido (carrinho)</param>
    /// <returns>Pedido com totais atualizados</returns>
    [HttpPost("{pedidoId}/recalcular-totais")]
    public async Task<ActionResult<PedidoDto>> RecalcularTotais(int pedidoId)
    {
        try
        {
            var pedido = await _pedidoService.RecalcularTotaisAsync(pedidoId);
            return Ok(pedido);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro ao recalcular totais do carrinho {PedidoId}", pedidoId);
            return BadRequest(new { error_code = "INVALID_OPERATION", error_description = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno ao recalcular totais do carrinho {PedidoId}", pedidoId);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Atualiza o prazo limite de interação do carrinho
    /// </summary>
    /// <param name="pedidoId">ID do pedido (carrinho)</param>
    /// <param name="novosDias">Novos dias a partir de agora</param>
    /// <returns>Pedido com prazo atualizado</returns>
    [HttpPut("{pedidoId}/prazo-limite")]
    public async Task<ActionResult<PedidoDto>> AtualizarPrazoLimite(int pedidoId, [FromBody] int novosDias)
    {
        try
        {
            var pedido = await _pedidoService.AtualizarPrazoLimiteAsync(pedidoId, novosDias);
            return Ok(pedido);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro ao atualizar prazo limite do carrinho {PedidoId}", pedidoId);
            return BadRequest(new { error_code = "INVALID_OPERATION", error_description = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Argumento inválido ao atualizar prazo limite do carrinho {PedidoId}", pedidoId);
            return BadRequest(new { error_code = "INVALID_ARGUMENT", error_description = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno ao atualizar prazo limite do carrinho {PedidoId}", pedidoId);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém um carrinho com todos os seus itens
    /// </summary>
    /// <param name="pedidoId">ID do pedido (carrinho)</param>
    /// <returns>Carrinho completo</returns>
    [HttpGet("{pedidoId}")]
    public async Task<ActionResult<PedidoDto>> ObterCarrinho(int pedidoId)
    {
        try
        {
            var pedido = await _pedidoService.ObterComItensAsync(pedidoId);
            if (pedido == null)
            {
                return NotFound(new { error_code = "NOT_FOUND", error_description = "Carrinho não encontrado" });
            }

            return Ok(pedido);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno ao obter carrinho {PedidoId}", pedidoId);
            return StatusCode(500, new { error_code = "INTERNAL_ERROR", error_description = "Erro interno do servidor" });
        }
    }
}