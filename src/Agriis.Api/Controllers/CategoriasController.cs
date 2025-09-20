using Agriis.Produtos.Aplicacao.DTOs;
using Agriis.Produtos.Aplicacao.Interfaces;
using Agriis.Produtos.Dominio.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Agriis.Api.Controllers;

/// <summary>
/// Controller para gerenciamento de categorias de produtos
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CategoriasController : ControllerBase
{
    private readonly ICategoriaService _categoriaService;
    private readonly ILogger<CategoriasController> _logger;

    public CategoriasController(ICategoriaService categoriaService, ILogger<CategoriasController> logger)
    {
        _categoriaService = categoriaService;
        _logger = logger;
    }

    /// <summary>
    /// Obtém todas as categorias
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoriaDto>>> ObterTodos(CancellationToken cancellationToken = default)
    {
        try
        {
            var categorias = await _categoriaService.ObterTodosAsync(cancellationToken);
            return Ok(categorias);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter categorias");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obtém categorias ativas
    /// </summary>
    [HttpGet("ativas")]
    public async Task<ActionResult<IEnumerable<CategoriaDto>>> ObterAtivas(CancellationToken cancellationToken = default)
    {
        try
        {
            var categorias = await _categoriaService.ObterAtivasAsync(cancellationToken);
            return Ok(categorias);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter categorias ativas");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obtém uma categoria por ID
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoriaDto>> ObterPorId(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var categoria = await _categoriaService.ObterPorIdAsync(id, cancellationToken);
            
            if (categoria == null)
                return NotFound($"Categoria com ID {id} não encontrada");

            return Ok(categoria);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter categoria {CategoriaId}", id);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obtém uma categoria por nome
    /// </summary>
    [HttpGet("nome/{nome}")]
    public async Task<ActionResult<CategoriaDto>> ObterPorNome(string nome, CancellationToken cancellationToken = default)
    {
        try
        {
            var categoria = await _categoriaService.ObterPorNomeAsync(nome, cancellationToken);
            
            if (categoria == null)
                return NotFound($"Categoria com nome {nome} não encontrada");

            return Ok(categoria);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter categoria por nome {Nome}", nome);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obtém categorias por tipo
    /// </summary>
    [HttpGet("tipo/{tipo}")]
    public async Task<ActionResult<IEnumerable<CategoriaDto>>> ObterPorTipo(CategoriaProduto tipo, CancellationToken cancellationToken = default)
    {
        try
        {
            var categorias = await _categoriaService.ObterPorTipoAsync(tipo, cancellationToken);
            return Ok(categorias);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter categorias do tipo {Tipo}", tipo);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obtém categorias raiz (sem categoria pai)
    /// </summary>
    [HttpGet("raiz")]
    public async Task<ActionResult<IEnumerable<CategoriaDto>>> ObterCategoriasRaiz(CancellationToken cancellationToken = default)
    {
        try
        {
            var categorias = await _categoriaService.ObterCategoriasRaizAsync(cancellationToken);
            return Ok(categorias);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter categorias raiz");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obtém subcategorias de uma categoria pai
    /// </summary>
    [HttpGet("{categoriaPaiId:int}/subcategorias")]
    public async Task<ActionResult<IEnumerable<CategoriaDto>>> ObterSubCategorias(int categoriaPaiId, CancellationToken cancellationToken = default)
    {
        try
        {
            var categorias = await _categoriaService.ObterSubCategoriasAsync(categoriaPaiId, cancellationToken);
            return Ok(categorias);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter subcategorias da categoria {CategoriaPaiId}", categoriaPaiId);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obtém categorias com hierarquia completa
    /// </summary>
    [HttpGet("hierarquia")]
    public async Task<ActionResult<IEnumerable<CategoriaDto>>> ObterComHierarquia(CancellationToken cancellationToken = default)
    {
        try
        {
            var categorias = await _categoriaService.ObterComHierarquiaAsync(cancellationToken);
            return Ok(categorias);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter categorias com hierarquia");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obtém categorias ordenadas
    /// </summary>
    [HttpGet("ordenadas")]
    public async Task<ActionResult<IEnumerable<CategoriaDto>>> ObterOrdenadas(CancellationToken cancellationToken = default)
    {
        try
        {
            var categorias = await _categoriaService.ObterOrdenadasAsync(cancellationToken);
            return Ok(categorias);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter categorias ordenadas");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Cria uma nova categoria
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CategoriaDto>> Criar([FromBody] CriarCategoriaDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var categoria = await _categoriaService.CriarAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(ObterPorId), new { id = categoria.Id }, categoria);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao criar categoria");
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro de operação ao criar categoria");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar categoria");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Atualiza uma categoria existente
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoriaDto>> Atualizar(int id, [FromBody] AtualizarCategoriaDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var categoria = await _categoriaService.AtualizarAsync(id, dto, cancellationToken);
            return Ok(categoria);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao atualizar categoria {CategoriaId}", id);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro de operação ao atualizar categoria {CategoriaId}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar categoria {CategoriaId}", id);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Ativa uma categoria
    /// </summary>
    [HttpPatch("{id:int}/ativar")]
    public async Task<ActionResult> Ativar(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _categoriaService.AtivarAsync(id, cancellationToken);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Categoria {CategoriaId} não encontrada para ativação", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao ativar categoria {CategoriaId}", id);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Desativa uma categoria
    /// </summary>
    [HttpPatch("{id:int}/desativar")]
    public async Task<ActionResult> Desativar(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _categoriaService.DesativarAsync(id, cancellationToken);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Categoria {CategoriaId} não encontrada para desativação", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao desativar categoria {CategoriaId}", id);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Remove uma categoria
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Remover(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _categoriaService.RemoverAsync(id, cancellationToken);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Categoria {CategoriaId} não encontrada para remoção", id);
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro de operação ao remover categoria {CategoriaId}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover categoria {CategoriaId}", id);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Verifica se uma categoria pode ser removida
    /// </summary>
    [HttpGet("{id:int}/pode-remover")]
    public async Task<ActionResult<bool>> PodeRemover(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var podeRemover = await _categoriaService.PodeRemoverAsync(id, cancellationToken);
            return Ok(podeRemover);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar se categoria {CategoriaId} pode ser removida", id);
            return StatusCode(500, "Erro interno do servidor");
        }
    }
}