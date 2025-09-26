using Agriis.Produtos.Aplicacao.DTOs;
using Agriis.Produtos.Aplicacao.Interfaces;
using Agriis.Produtos.Dominio.Enums;
using Agriis.Referencias.Aplicacao.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Agriis.Api.Controllers;

/// <summary>
/// Controller para gerenciamento de produtos
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProdutosController : ControllerBase
{
    private readonly IProdutoService _produtoService;
    private readonly IUnidadeMedidaService _unidadeMedidaService;
    private readonly IEmbalagemService _embalagemService;
    private readonly IAtividadeAgropecuariaService _atividadeAgropecuariaService;
    private readonly ILogger<ProdutosController> _logger;

    public ProdutosController(
        IProdutoService produtoService,
        IUnidadeMedidaService unidadeMedidaService,
        IEmbalagemService embalagemService,
        IAtividadeAgropecuariaService atividadeAgropecuariaService,
        ILogger<ProdutosController> logger)
    {
        _produtoService = produtoService;
        _unidadeMedidaService = unidadeMedidaService;
        _embalagemService = embalagemService;
        _atividadeAgropecuariaService = atividadeAgropecuariaService;
        _logger = logger;
    }

    /// <summary>
    /// Obtém todos os produtos
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProdutoDto>>> ObterTodos(CancellationToken cancellationToken = default)
    {
        try
        {
            var produtos = await _produtoService.ObterTodosAsync(cancellationToken);
            return Ok(produtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter produtos");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obtém produtos ativos
    /// </summary>
    [HttpGet("ativos")]
    public async Task<ActionResult<IEnumerable<ProdutoDto>>> ObterAtivos(CancellationToken cancellationToken = default)
    {
        try
        {
            var produtos = await _produtoService.ObterAtivosAsync(cancellationToken);
            return Ok(produtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter produtos ativos");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obtém um produto por ID
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProdutoDto>> ObterPorId(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var produto = await _produtoService.ObterPorIdAsync(id, cancellationToken);
            
            if (produto == null)
                return NotFound($"Produto com ID {id} não encontrado");

            return Ok(produto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter produto {ProdutoId}", id);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obtém um produto por código
    /// </summary>
    [HttpGet("codigo/{codigo}")]
    public async Task<ActionResult<ProdutoDto>> ObterPorCodigo(string codigo, CancellationToken cancellationToken = default)
    {
        try
        {
            var produto = await _produtoService.ObterPorCodigoAsync(codigo, cancellationToken);
            
            if (produto == null)
                return NotFound($"Produto com código {codigo} não encontrado");

            return Ok(produto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter produto por código {Codigo}", codigo);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obtém produtos por fornecedor
    /// </summary>
    [HttpGet("fornecedor/{fornecedorId:int}")]
    public async Task<ActionResult<IEnumerable<ProdutoDto>>> ObterPorFornecedor(int fornecedorId, CancellationToken cancellationToken = default)
    {
        try
        {
            var produtos = await _produtoService.ObterPorFornecedorAsync(fornecedorId, cancellationToken);
            return Ok(produtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter produtos do fornecedor {FornecedorId}", fornecedorId);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obtém produtos por categoria
    /// </summary>
    [HttpGet("categoria/{categoriaId:int}")]
    public async Task<ActionResult<IEnumerable<ProdutoDto>>> ObterPorCategoria(int categoriaId, CancellationToken cancellationToken = default)
    {
        try
        {
            var produtos = await _produtoService.ObterPorCategoriaAsync(categoriaId, cancellationToken);
            return Ok(produtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter produtos da categoria {CategoriaId}", categoriaId);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obtém produtos por cultura
    /// </summary>
    [HttpGet("cultura/{culturaId:int}")]
    public async Task<ActionResult<IEnumerable<ProdutoDto>>> ObterPorCultura(int culturaId, CancellationToken cancellationToken = default)
    {
        try
        {
            var produtos = await _produtoService.ObterPorCulturaAsync(culturaId, cancellationToken);
            return Ok(produtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter produtos da cultura {CulturaId}", culturaId);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obtém produtos por tipo
    /// </summary>
    [HttpGet("tipo/{tipo}")]
    public async Task<ActionResult<IEnumerable<ProdutoDto>>> ObterPorTipo(TipoProduto tipo, CancellationToken cancellationToken = default)
    {
        try
        {
            var produtos = await _produtoService.ObterPorTipoAsync(tipo, cancellationToken);
            return Ok(produtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter produtos do tipo {Tipo}", tipo);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obtém produtos fabricantes
    /// </summary>
    [HttpGet("fabricantes")]
    public async Task<ActionResult<IEnumerable<ProdutoDto>>> ObterFabricantes(CancellationToken cancellationToken = default)
    {
        try
        {
            var produtos = await _produtoService.ObterFabricantesAsync(cancellationToken);
            return Ok(produtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter produtos fabricantes");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obtém produtos filhos de um produto pai
    /// </summary>
    [HttpGet("{produtoPaiId:int}/filhos")]
    public async Task<ActionResult<IEnumerable<ProdutoDto>>> ObterProdutosFilhos(int produtoPaiId, CancellationToken cancellationToken = default)
    {
        try
        {
            var produtos = await _produtoService.ObterProdutosFilhosAsync(produtoPaiId, cancellationToken);
            return Ok(produtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter produtos filhos do produto {ProdutoPaiId}", produtoPaiId);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obtém produtos restritos
    /// </summary>
    [HttpGet("restritos")]
    public async Task<ActionResult<IEnumerable<ProdutoDto>>> ObterRestritos(CancellationToken cancellationToken = default)
    {
        try
        {
            var produtos = await _produtoService.ObterRestritosAsync(cancellationToken);
            return Ok(produtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter produtos restritos");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Busca produtos por nome ou código
    /// </summary>
    [HttpGet("buscar")]
    public async Task<ActionResult<IEnumerable<ProdutoDto>>> Buscar([FromQuery] string termo, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(termo))
                return BadRequest("Termo de busca é obrigatório");

            var produtos = await _produtoService.BuscarAsync(termo, cancellationToken);
            return Ok(produtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar produtos com termo {Termo}", termo);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Cria um novo produto
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ProdutoDto>> Criar([FromBody] CriarProdutoDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validar referências antes de criar
            var validationResult = await ValidarReferenciasAsync(dto.UnidadeMedidaId, dto.EmbalagemId, dto.AtividadeAgropecuariaId, cancellationToken);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.ErrorMessage);

            var produto = await _produtoService.CriarAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(ObterPorId), new { id = produto.Id }, produto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao criar produto");
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro de operação ao criar produto");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar produto");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Atualiza um produto existente
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProdutoDto>> Atualizar(int id, [FromBody] AtualizarProdutoDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validar referências antes de atualizar
            var validationResult = await ValidarReferenciasAsync(dto.UnidadeMedidaId, dto.EmbalagemId, dto.AtividadeAgropecuariaId, cancellationToken);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.ErrorMessage);

            var produto = await _produtoService.AtualizarAsync(id, dto, cancellationToken);
            return Ok(produto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao atualizar produto {ProdutoId}", id);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro de operação ao atualizar produto {ProdutoId}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar produto {ProdutoId}", id);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Ativa um produto
    /// </summary>
    [HttpPatch("{id:int}/ativar")]
    public async Task<ActionResult> Ativar(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _produtoService.AtivarAsync(id, cancellationToken);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Produto {ProdutoId} não encontrado para ativação", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao ativar produto {ProdutoId}", id);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Inativa um produto
    /// </summary>
    [HttpPatch("{id:int}/inativar")]
    public async Task<ActionResult> Inativar(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _produtoService.InativarAsync(id, cancellationToken);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Produto {ProdutoId} não encontrado para inativação", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao inativar produto {ProdutoId}", id);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Descontinua um produto
    /// </summary>
    [HttpPatch("{id:int}/descontinuar")]
    public async Task<ActionResult> Descontinuar(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _produtoService.DescontinuarAsync(id, cancellationToken);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Produto {ProdutoId} não encontrado para descontinuação", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao descontinuar produto {ProdutoId}", id);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Remove um produto
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Remover(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _produtoService.RemoverAsync(id, cancellationToken);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Produto {ProdutoId} não encontrado para remoção", id);
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro de operação ao remover produto {ProdutoId}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover produto {ProdutoId}", id);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Adiciona uma cultura ao produto
    /// </summary>
    [HttpPost("{produtoId:int}/culturas/{culturaId:int}")]
    public async Task<ActionResult> AdicionarCultura(int produtoId, int culturaId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _produtoService.AdicionarCulturaAsync(produtoId, culturaId, cancellationToken);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Erro ao adicionar cultura {CulturaId} ao produto {ProdutoId}", culturaId, produtoId);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao adicionar cultura {CulturaId} ao produto {ProdutoId}", culturaId, produtoId);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Remove uma cultura do produto
    /// </summary>
    [HttpDelete("{produtoId:int}/culturas/{culturaId:int}")]
    public async Task<ActionResult> RemoverCultura(int produtoId, int culturaId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _produtoService.RemoverCulturaAsync(produtoId, culturaId, cancellationToken);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Erro ao remover cultura {CulturaId} do produto {ProdutoId}", culturaId, produtoId);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover cultura {CulturaId} do produto {ProdutoId}", culturaId, produtoId);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Valida se todas as entidades de referência existem e estão ativas
    /// </summary>
    private async Task<ValidationResult> ValidarReferenciasAsync(int unidadeMedidaId, int? embalagemId, int? atividadeAgropecuariaId, CancellationToken cancellationToken = default)
    {
        // Validar se unidade de medida existe e está ativa
        var unidadeMedida = await _unidadeMedidaService.ObterPorIdAsync(unidadeMedidaId, cancellationToken);
        if (unidadeMedida == null)
            return ValidationResult.Invalid("Unidade de medida não encontrada");
        
        if (!unidadeMedida.Ativo)
            return ValidationResult.Invalid("Unidade de medida está inativa");

        // Validar se embalagem existe e está ativa (se especificada)
        if (embalagemId.HasValue)
        {
            var embalagem = await _embalagemService.ObterPorIdAsync(embalagemId.Value, cancellationToken);
            if (embalagem == null)
                return ValidationResult.Invalid("Embalagem não encontrada");
            
            if (!embalagem.Ativo)
                return ValidationResult.Invalid("Embalagem está inativa");
        }

        // Validar se atividade agropecuária existe e está ativa (se especificada)
        if (atividadeAgropecuariaId.HasValue)
        {
            var atividade = await _atividadeAgropecuariaService.ObterPorIdAsync(atividadeAgropecuariaId.Value, cancellationToken);
            if (atividade == null)
                return ValidationResult.Invalid("Atividade agropecuária não encontrada");
            
            if (!atividade.Ativo)
                return ValidationResult.Invalid("Atividade agropecuária está inativa");
        }

        return ValidationResult.Valid();
    }

    /// <summary>
    /// Resultado da validação de referências
    /// </summary>
    private class ValidationResult
    {
        public bool IsValid { get; private set; }
        public string? ErrorMessage { get; private set; }

        private ValidationResult(bool isValid, string? errorMessage = null)
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
        }

        public static ValidationResult Valid() => new(true);
        public static ValidationResult Invalid(string errorMessage) => new(false, errorMessage);
    }
}