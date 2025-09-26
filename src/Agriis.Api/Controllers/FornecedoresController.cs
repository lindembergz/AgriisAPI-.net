using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Agriis.Fornecedores.Aplicacao.DTOs;
using Agriis.Fornecedores.Aplicacao.Interfaces;
using Agriis.Produtores.Aplicacao.DTOs;

namespace Agriis.Api.Controllers;

/// <summary>
/// Controller para gerenciamento de fornecedores
/// </summary>
[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class FornecedoresController : ControllerBase
{
    private readonly IFornecedorService _fornecedorService;
    private readonly ILogger<FornecedoresController> _logger;

    public FornecedoresController(
        IFornecedorService fornecedorService,
        ILogger<FornecedoresController> logger)
    {
        _fornecedorService = fornecedorService ?? throw new ArgumentNullException(nameof(fornecedorService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Obtém todos os fornecedores
    /// </summary>
    /// <returns>Lista de fornecedores</returns>
    /*[HttpGet]
    public async Task<ActionResult<IEnumerable<FornecedorDto>>> ObterTodos()
    {
        try
        {
            var fornecedores = await _fornecedorService.ObterTodosAsync();
            return Ok(fornecedores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter todos os fornecedores");
            return StatusCode(500, new { error_code = "ERRO_INTERNO", error_description = "Erro interno do servidor" });
        }
    }*/

    [HttpGet]
    public async Task<ActionResult<object>> ObterPaginado([FromQuery] FiltrosFornecedorDto filtros)
    {
        try
        {
            var resultado = await _fornecedorService.ObterPaginadoAsync(filtros);

            return Ok(new
            {
                items = resultado.Items,
                total_items = resultado.TotalCount,
                page = resultado.PageNumber,
                page_size = resultado.PageSize,
                total_pages = resultado.TotalPages
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter fornecedores paginados");
            return StatusCode(500, new { error_code = "ERRO_INTERNO", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém um fornecedor por ID
    /// </summary>
    /// <param name="id">ID do fornecedor</param>
    /// <returns>Dados do fornecedor</returns>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<FornecedorDto>> ObterPorId(int id)
    {
        try
        {
            var fornecedor = await _fornecedorService.ObterPorIdAsync(id);
            
            if (fornecedor == null)
                return NotFound(new { error_code = "FORNECEDOR_NAO_ENCONTRADO", error_description = "Fornecedor não encontrado" });

            return Ok(fornecedor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter fornecedor por ID: {Id}", id);
            return StatusCode(500, new { error_code = "ERRO_INTERNO", error_description = "Erro interno do servidor" });
        }
    }   
 /// <summary>
    /// Obtém um fornecedor por CNPJ
    /// </summary>
    /// <param name="cnpj">CNPJ do fornecedor</param>
    /// <returns>Dados do fornecedor</returns>
    [HttpGet("cnpj/{cnpj}")]
    public async Task<ActionResult<FornecedorDto>> ObterPorCnpj(string cnpj)
    {
        try
        {
            var fornecedor = await _fornecedorService.ObterPorCnpjAsync(cnpj);
            
            if (fornecedor == null)
                return NotFound(new { error_code = "FORNECEDOR_NAO_ENCONTRADO", error_description = "Fornecedor não encontrado" });

            return Ok(fornecedor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter fornecedor por CNPJ: {Cnpj}", cnpj);
            return StatusCode(500, new { error_code = "ERRO_INTERNO", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém fornecedores ativos
    /// </summary>
    /// <returns>Lista de fornecedores ativos</returns>
    [HttpGet("ativos")]
    public async Task<ActionResult<IEnumerable<FornecedorDto>>> ObterAtivos()
    {
        try
        {
            var fornecedores = await _fornecedorService.ObterAtivosAsync();
            return Ok(fornecedores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter fornecedores ativos");
            return StatusCode(500, new { error_code = "ERRO_INTERNO", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém fornecedores por território
    /// </summary>
    /// <param name="uf">UF do estado</param>
    /// <param name="municipio">Nome do município (opcional)</param>
    /// <returns>Lista de fornecedores que atendem o território</returns>
    [HttpGet("territorio")]
    public async Task<ActionResult<IEnumerable<FornecedorDto>>> ObterPorTerritorio([FromQuery] string uf, [FromQuery] string? municipio = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(uf))
                return BadRequest(new { error_code = "UF_OBRIGATORIA", error_description = "UF é obrigatória" });

            var fornecedores = await _fornecedorService.ObterPorTerritorioAsync(uf, municipio);
            return Ok(fornecedores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter fornecedores por território: UF={Uf}, Município={Municipio}", uf, municipio);
            return StatusCode(500, new { error_code = "ERRO_INTERNO", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém fornecedores com filtros avançados
    /// </summary>
    /// <param name="nome">Filtro por nome (opcional)</param>
    /// <param name="cnpj">Filtro por CNPJ (opcional)</param>
    /// <param name="ativo">Filtro por status ativo (opcional)</param>
    /// <param name="moedaPadrao">Filtro por moeda padrão (opcional)</param>
    /// <returns>Lista de fornecedores filtrados</returns>
    [HttpGet("filtros")]
    public async Task<ActionResult<IEnumerable<FornecedorDto>>> ObterComFiltros(
        [FromQuery] string? nome = null,
        [FromQuery] string? cnpj = null,
        [FromQuery] bool? ativo = null,
        [FromQuery] int? moedaPadrao = null)
    {
        try
        {
            var fornecedores = await _fornecedorService.ObterComFiltrosAsync(nome, cnpj, ativo, moedaPadrao);
            return Ok(fornecedores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter fornecedores com filtros");
            return StatusCode(500, new { error_code = "ERRO_INTERNO", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Cria um novo fornecedor com estrutura completa (frontend)
    /// </summary>
    /// <param name="request">Dados completos do fornecedor</param>
    /// <returns>Fornecedor criado</returns>
    [HttpPost("completo")]
    public async Task<ActionResult<FornecedorDto>> CriarCompleto([FromBody] CriarFornecedorCompletoRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var fornecedor = await _fornecedorService.CriarCompletoAsync(request);
            return CreatedAtAction(nameof(ObterPorId), new { id = fornecedor.Id }, fornecedor);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao criar fornecedor completo");
            return BadRequest(new { error_code = "VALIDACAO_ERRO", error_description = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar fornecedor completo");
            return StatusCode(500, new { error_code = "ERRO_INTERNO", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Cria um novo fornecedor
    /// </summary>
    /// <param name="request">Dados do fornecedor</param>
    /// <returns>Fornecedor criado</returns>
    [HttpPost]
    public async Task<ActionResult<FornecedorDto>> Criar([FromBody] CriarFornecedorRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var fornecedor = await _fornecedorService.CriarAsync(request);
            return CreatedAtAction(nameof(ObterPorId), new { id = fornecedor.Id }, fornecedor);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao criar fornecedor");
            return BadRequest(new { error_code = "VALIDACAO_ERRO", error_description = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar fornecedor");
            return StatusCode(500, new { error_code = "ERRO_INTERNO", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Atualiza um fornecedor existente
    /// </summary>
    /// <param name="id">ID do fornecedor</param>
    /// <param name="request">Novos dados do fornecedor</param>
    /// <returns>Fornecedor atualizado</returns>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<FornecedorDto>> Atualizar(int id, [FromBody] AtualizarFornecedorRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var fornecedor = await _fornecedorService.AtualizarAsync(id, request);
            return Ok(fornecedor);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao atualizar fornecedor: {Id}", id);
            return BadRequest(new { error_code = "VALIDACAO_ERRO", error_description = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar fornecedor: {Id}", id);
            return StatusCode(500, new { error_code = "ERRO_INTERNO", error_description = "Erro interno do servidor" });
        }
    }    
/// <summary>
    /// Ativa um fornecedor
    /// </summary>
    /// <param name="id">ID do fornecedor</param>
    /// <returns>Resultado da operação</returns>
    [HttpPatch("{id:int}/ativar")]
    public async Task<ActionResult> Ativar(int id)
    {
        try
        {
            await _fornecedorService.AtivarAsync(id);
            return Ok(new { message = "Fornecedor ativado com sucesso" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro ao ativar fornecedor: {Id}", id);
            return NotFound(new { error_code = "FORNECEDOR_NAO_ENCONTRADO", error_description = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao ativar fornecedor: {Id}", id);
            return StatusCode(500, new { error_code = "ERRO_INTERNO", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Desativa um fornecedor
    /// </summary>
    /// <param name="id">ID do fornecedor</param>
    /// <returns>Resultado da operação</returns>
    [HttpPatch("{id:int}/desativar")]
    public async Task<ActionResult> Desativar(int id)
    {
        try
        {
            await _fornecedorService.DesativarAsync(id);
            return Ok(new { message = "Fornecedor desativado com sucesso" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro ao desativar fornecedor: {Id}", id);
            return NotFound(new { error_code = "FORNECEDOR_NAO_ENCONTRADO", error_description = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao desativar fornecedor: {Id}", id);
            return StatusCode(500, new { error_code = "ERRO_INTERNO", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Define a logo do fornecedor
    /// </summary>
    /// <param name="id">ID do fornecedor</param>
    /// <param name="request">URL da logo</param>
    /// <returns>Resultado da operação</returns>
    [HttpPatch("{id:int}/logo")]
    public async Task<ActionResult> DefinirLogo(int id, [FromBody] DefinirLogoRequest request)
    {
        try
        {
            await _fornecedorService.DefinirLogoAsync(id, request.LogoUrl);
            return Ok(new { message = "Logo definida com sucesso" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro ao definir logo do fornecedor: {Id}", id);
            return NotFound(new { error_code = "FORNECEDOR_NAO_ENCONTRADO", error_description = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao definir logo do fornecedor: {Id}", id);
            return StatusCode(500, new { error_code = "ERRO_INTERNO", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Verifica se um CNPJ está disponível
    /// </summary>
    /// <param name="cnpj">CNPJ a verificar</param>
    /// <param name="fornecedorIdExcluir">ID do fornecedor a excluir da verificação</param>
    /// <returns>Resultado da verificação</returns>
    [HttpGet("cnpj/{cnpj}/disponivel")]
    public async Task<ActionResult<bool>> VerificarCnpjDisponivel(string cnpj, [FromQuery] int? fornecedorIdExcluir = null)
    {
        try
        {
            var disponivel = await _fornecedorService.VerificarCnpjDisponivelAsync(cnpj, fornecedorIdExcluir);
            return Ok(new { disponivel });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar disponibilidade do CNPJ: {Cnpj}", cnpj);
            return StatusCode(500, new { error_code = "ERRO_INTERNO", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém fornecedores filtrados por UF
    /// </summary>
    /// <param name="ufId">ID da UF</param>
    /// <returns>Lista de fornecedores da UF</returns>
    [HttpGet("uf/{ufId:int}")]
    public async Task<ActionResult<IEnumerable<FornecedorDto>>> ObterPorUf(int ufId)
    {
        try
        {
            var fornecedores = await _fornecedorService.ObterComFiltrosAsync(ativo: true);
            var fornecedoresFiltrados = fornecedores.Where(f => f.UfId == ufId);
            return Ok(fornecedoresFiltrados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter fornecedores por UF: {UfId}", ufId);
            return StatusCode(500, new { error_code = "ERRO_INTERNO", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém fornecedores filtrados por município
    /// </summary>
    /// <param name="municipioId">ID do município</param>
    /// <returns>Lista de fornecedores do município</returns>
    [HttpGet("municipio/{municipioId:int}")]
    public async Task<ActionResult<IEnumerable<FornecedorDto>>> ObterPorMunicipio(int municipioId)
    {
        try
        {
            var fornecedores = await _fornecedorService.ObterComFiltrosAsync(ativo: true);
            var fornecedoresFiltrados = fornecedores.Where(f => f.MunicipioId == municipioId);
            return Ok(fornecedoresFiltrados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter fornecedores por município: {MunicipioId}", municipioId);
            return StatusCode(500, new { error_code = "ERRO_INTERNO", error_description = "Erro interno do servidor" });
        }
    }
}

/// <summary>
/// Request para definir logo do fornecedor
/// </summary>
public class DefinirLogoRequest
{
    /// <summary>
    /// URL da logo
    /// </summary>
    public string? LogoUrl { get; set; }
}