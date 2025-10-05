using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Agriis.Produtores.Aplicacao.DTOs;
using Agriis.Produtores.Aplicacao.Interfaces;

namespace Agriis.Api.Controllers;

/// <summary>
/// Controller para gerenciamento de produtores
/// </summary>
[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class ProdutoresController : ControllerBase
{
    private readonly IProdutorService _produtorService;
    private readonly ILogger<ProdutoresController> _logger;

    public ProdutoresController(
        IProdutorService produtorService,
        ILogger<ProdutoresController> logger)
    {
        _produtorService = produtorService ?? throw new ArgumentNullException(nameof(produtorService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Obtém um produtor por ID
    /// </summary>
    /// <param name="id">ID do produtor</param>
    /// <returns>Dados do produtor</returns>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProdutorDto>> ObterPorId(int id)
    {
        try
        {
            var produtor = await _produtorService.ObterPorIdAsync(id);
            
            if (produtor == null)
                return NotFound(new { error_code = "PRODUTOR_NAO_ENCONTRADO", error_description = "Produtor não encontrado" });

            return Ok(produtor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter produtor por ID: {Id}", id);
            return StatusCode(500, new { error_code = "ERRO_INTERNO", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém um produtor por CPF
    /// </summary>
    /// <param name="cpf">CPF do produtor</param>
    /// <returns>Dados do produtor</returns>
    [HttpGet("cpf/{cpf}")]
    public async Task<ActionResult<ProdutorDto>> ObterPorCpf(string cpf)
    {
        try
        {
            var produtor = await _produtorService.ObterPorCpfAsync(cpf);
            
            if (produtor == null)
                return NotFound(new { error_code = "PRODUTOR_NAO_ENCONTRADO", error_description = "Produtor não encontrado" });

            return Ok(produtor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter produtor por CPF: {Cpf}", cpf);
            return StatusCode(500, new { error_code = "ERRO_INTERNO", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém um produtor por CNPJ
    /// </summary>
    /// <param name="cnpj">CNPJ do produtor</param>
    /// <returns>Dados do produtor</returns>
    [HttpGet("cnpj/{cnpj}")]
    public async Task<ActionResult<ProdutorDto>> ObterPorCnpj(string cnpj)
    {
        try
        {
            var produtor = await _produtorService.ObterPorCnpjAsync(cnpj);
            
            if (produtor == null)
                return NotFound(new { error_code = "PRODUTOR_NAO_ENCONTRADO", error_description = "Produtor não encontrado" });

            return Ok(produtor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter produtor por CNPJ: {Cnpj}", cnpj);
            return StatusCode(500, new { error_code = "ERRO_INTERNO", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém produtores paginados com filtros
    /// </summary>
    /// <param name="filtros">Filtros de busca</param>
    /// <returns>Lista paginada de produtores</returns>
    [HttpGet]
    public async Task<ActionResult<object>> ObterPaginado([FromQuery] FiltrosProdutorDto filtros)
    {
        try
        {
            var resultado = await _produtorService.ObterPaginadoAsync(filtros);
            
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
            _logger.LogError(ex, "Erro ao obter produtores paginados");
            return StatusCode(500, new { error_code = "ERRO_INTERNO", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Cria um novo produtor
    /// </summary>
    /// <param name="dto">Dados do produtor</param>
    /// <returns>Produtor criado</returns>
    [HttpPost]
    public async Task<ActionResult<ProdutorDto>> Criar([FromBody] CriarProdutorDto dto)
    {
        try
        {
            var resultado = await _produtorService.CriarAsync(dto);
            
            if (!resultado.IsSuccess)
                return BadRequest(new { error_code = "ERRO_VALIDACAO", error_description = resultado.Error });

            return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Value!.Id }, resultado.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar produtor");
            return StatusCode(500, new { error_code = "ERRO_INTERNO", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Cria um novo produtor com estrutura completa (usuário master e relacionamentos)
    /// </summary>
    /// <param name="request">Dados completos do produtor</param>
    /// <returns>Produtor criado</returns>
    [HttpPost("completo")]
    public async Task<ActionResult<ProdutorDto>> CriarCompleto([FromBody] CriarProdutorCompletoRequest request)
    {
        try
        {
            var resultado = await _produtorService.CriarCompletoAsync(request);
            
            if (!resultado.IsSuccess)
                return BadRequest(new { error_code = "ERRO_VALIDACAO", error_description = resultado.Error });

            return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Value!.Id }, resultado.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar produtor completo");
            return StatusCode(500, new { error_code = "ERRO_INTERNO", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Atualiza um produtor existente
    /// </summary>
    /// <param name="id">ID do produtor</param>
    /// <param name="dto">Dados atualizados</param>
    /// <returns>Produtor atualizado</returns>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProdutorDto>> Atualizar(int id, [FromBody] AtualizarProdutorDto dto)
    {
        try
        {
            var resultado = await _produtorService.AtualizarAsync(id, dto);
            
            if (!resultado.IsSuccess)
            {
                if (resultado.Error!.Contains("não encontrado"))
                    return NotFound(new { error_code = "PRODUTOR_NAO_ENCONTRADO", error_description = resultado.Error });
                
                return BadRequest(new { error_code = "ERRO_VALIDACAO", error_description = resultado.Error });
            }

            return Ok(resultado.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar produtor: {Id}", id);
            return StatusCode(500, new { error_code = "ERRO_INTERNO", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Remove um produtor
    /// </summary>
    /// <param name="id">ID do produtor</param>
    /// <returns>Resultado da operação</returns>
    [HttpDelete("{id:int}")]
    //[Authorize(Roles = "RoleAdmin")]
    public async Task<ActionResult> Remover(int id)
    {
        try
        {
            var resultado = await _produtorService.RemoverAsync(id);
            
            if (!resultado.IsSuccess)
            {
                if (resultado.Error!.Contains("não encontrado"))
                    return NotFound(new { error_code = "PRODUTOR_NAO_ENCONTRADO", error_description = resultado.Error });
                
                return BadRequest(new { error_code = "ERRO_VALIDACAO", error_description = resultado.Error });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover produtor: {Id}", id);
            return StatusCode(500, new { error_code = "ERRO_INTERNO", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Valida um produtor automaticamente via SERPRO
    /// </summary>
    /// <param name="id">ID do produtor</param>
    /// <returns>Resultado da validação</returns>
    [HttpPost("{id:int}/validar-automaticamente")]
    [Authorize(Roles = "RoleAdmin,RoleFornecedorWebAdmin")]
    public async Task<ActionResult<ProdutorDto>> ValidarAutomaticamente(int id)
    {
        try
        {
            var resultado = await _produtorService.ValidarAutomaticamenteAsync(id);
            
            if (!resultado.IsSuccess)
            {
                if (resultado.Error!.Contains("não encontrado"))
                    return NotFound(new { error_code = "PRODUTOR_NAO_ENCONTRADO", error_description = resultado.Error });
                
                return BadRequest(new { error_code = "ERRO_VALIDACAO", error_description = resultado.Error });
            }

            return Ok(resultado.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar produtor automaticamente: {Id}", id);
            return StatusCode(500, new { error_code = "ERRO_INTERNO", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Autoriza um produtor manualmente
    /// </summary>
    /// <param name="id">ID do produtor</param>
    /// <returns>Resultado da operação</returns>
    [HttpPost("{id:int}/autorizar")]
    [Authorize(Roles = "RoleAdmin")]
    public async Task<ActionResult<ProdutorDto>> AutorizarManualmente(int id)
    {
        try
        {
            // Obtém o ID do usuário atual do token JWT
            var usuarioIdClaim = User.FindFirst("user_id")?.Value;
            if (!int.TryParse(usuarioIdClaim, out var usuarioId))
                return Unauthorized(new { error_code = "TOKEN_INVALIDO", error_description = "Token inválido" });

            var resultado = await _produtorService.AutorizarManualmenteAsync(id, usuarioId);
            
            if (!resultado.IsSuccess)
            {
                if (resultado.Error!.Contains("não encontrado"))
                    return NotFound(new { error_code = "PRODUTOR_NAO_ENCONTRADO", error_description = resultado.Error });
                
                return BadRequest(new { error_code = "ERRO_VALIDACAO", error_description = resultado.Error });
            }

            return Ok(resultado.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao autorizar produtor manualmente: {Id}", id);
            return StatusCode(500, new { error_code = "ERRO_INTERNO", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Nega um produtor
    /// </summary>
    /// <param name="id">ID do produtor</param>
    /// <returns>Resultado da operação</returns>
    [HttpPost("{id:int}/negar")]
    [Authorize(Roles = "RoleAdmin")]
    public async Task<ActionResult<ProdutorDto>> Negar(int id)
    {
        try
        {
            // Obtém o ID do usuário atual do token JWT
            var usuarioIdClaim = User.FindFirst("user_id")?.Value;
            if (!int.TryParse(usuarioIdClaim, out var usuarioId))
                return Unauthorized(new { error_code = "TOKEN_INVALIDO", error_description = "Token inválido" });

            var resultado = await _produtorService.NegarAsync(id, usuarioId);
            
            if (!resultado.IsSuccess)
            {
                if (resultado.Error!.Contains("não encontrado"))
                    return NotFound(new { error_code = "PRODUTOR_NAO_ENCONTRADO", error_description = resultado.Error });
                
                return BadRequest(new { error_code = "ERRO_VALIDACAO", error_description = resultado.Error });
            }

            return Ok(resultado.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao negar produtor: {Id}", id);
            return StatusCode(500, new { error_code = "ERRO_INTERNO", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém estatísticas dos produtores
    /// </summary>
    /// <returns>Estatísticas</returns>
    [HttpGet("estatisticas")]
    [Authorize(Roles = "RoleAdmin,RoleFornecedorWebAdmin")]
    public async Task<ActionResult<ProdutorEstatisticasDto>> ObterEstatisticas()
    {
        try
        {
            var estatisticas = await _produtorService.ObterEstatisticasAsync();
            return Ok(estatisticas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter estatísticas de produtores");
            return StatusCode(500, new { error_code = "ERRO_INTERNO", error_description = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém produtores por fornecedor
    /// </summary>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <returns>Lista de produtores</returns>
    [HttpGet("fornecedor/{fornecedorId:int}")]
    public async Task<ActionResult<IEnumerable<ProdutorDto>>> ObterPorFornecedor(int fornecedorId)
    {
        try
        {
            var produtores = await _produtorService.ObterPorFornecedorAsync(fornecedorId);
            return Ok(produtores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter produtores por fornecedor: {FornecedorId}", fornecedorId);
            return StatusCode(500, new { error_code = "ERRO_INTERNO", error_description = "Erro interno do servidor" });
        }
    }
}