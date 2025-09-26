using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Agriis.Referencias.Aplicacao.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Agriis.Api.Controllers;

/// <summary>
/// Controlador base para entidades de referência
/// </summary>
/// <typeparam name="TDto">DTO de leitura</typeparam>
/// <typeparam name="TCriarDto">DTO de criação</typeparam>
/// <typeparam name="TAtualizarDto">DTO de atualização</typeparam>
[ApiController]
[Route("api/referencias/[controller]")]
[Authorize]
public abstract class ReferenciaControllerBase<TDto, TCriarDto, TAtualizarDto> : ControllerBase
{
    protected readonly IReferenciaService<TDto, TCriarDto, TAtualizarDto> Service;
    protected readonly ILogger Logger;

    protected ReferenciaControllerBase(
        IReferenciaService<TDto, TCriarDto, TAtualizarDto> service,
        ILogger logger)
    {
        Service = service;
        Logger = logger;
    }

    /// <summary>
    /// Obtém todas as entidades
    /// </summary>
    [HttpGet]
    public virtual async Task<IActionResult> ObterTodos()
    {
        try
        {
            Logger.LogDebug("Obtendo todas as entidades de {EntityType}", typeof(TDto).Name);
            
            var entidades = await Service.ObterTodosAsync();
            
            Logger.LogDebug("Encontradas {Count} entidades de {EntityType}", 
                entidades.Count(), typeof(TDto).Name);
            
            return Ok(entidades);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao obter todas as entidades de {EntityType}", typeof(TDto).Name);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Obtém apenas as entidades ativas
    /// </summary>
    [HttpGet("ativos")]
    public virtual async Task<IActionResult> ObterAtivos()
    {
        try
        {
            Logger.LogDebug("Obtendo entidades ativas de {EntityType}", typeof(TDto).Name);
            
            var entidades = await Service.ObterAtivosAsync();
            
            Logger.LogDebug("Encontradas {Count} entidades ativas de {EntityType}", 
                entidades.Count(), typeof(TDto).Name);
            
            return Ok(entidades);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao obter entidades ativas de {EntityType}", typeof(TDto).Name);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Obtém uma entidade por ID
    /// </summary>
    [HttpGet("{id:int}")]
    public virtual async Task<IActionResult> ObterPorId(int id)
    {
        try
        {
            Logger.LogDebug("Obtendo entidade {EntityType} com ID {Id}", typeof(TDto).Name, id);
            
            var entidade = await Service.ObterPorIdAsync(id);
            
            if (entidade == null)
            {
                Logger.LogWarning("Entidade {EntityType} com ID {Id} não encontrada", typeof(TDto).Name, id);
                return NotFound(new { 
                    ErrorCode = "ENTITY_NOT_FOUND", 
                    ErrorDescription = "Entidade não encontrada",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            
            Logger.LogDebug("Entidade {EntityType} com ID {Id} encontrada", typeof(TDto).Name, id);
            return Ok(entidade);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao obter entidade {EntityType} com ID {Id}", typeof(TDto).Name, id);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Cria uma nova entidade
    /// </summary>
    [HttpPost]
    public virtual async Task<IActionResult> Criar([FromBody] TCriarDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                Logger.LogWarning("Dados inválidos para criação de {EntityType}: {ValidationErrors}", 
                    typeof(TDto).Name, 
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                
                return BadRequest(new { 
                    ErrorCode = "VALIDATION_ERROR", 
                    ErrorDescription = "Dados inválidos",
                    ValidationErrors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                    ),
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }

            Logger.LogDebug("Criando nova entidade {EntityType}", typeof(TDto).Name);
            
            var entidadeCriada = await Service.CriarAsync(dto);
            
            Logger.LogInformation("Entidade {EntityType} criada com sucesso", typeof(TDto).Name);
            
            // Tentar obter o ID da entidade criada para o Location header
            var idProperty = typeof(TDto).GetProperty("Id");
            if (idProperty != null)
            {
                var id = idProperty.GetValue(entidadeCriada);
                return CreatedAtAction(nameof(ObterPorId), new { id }, entidadeCriada);
            }
            
            return Created("", entidadeCriada);
        }
        catch (ArgumentException ex)
        {
            Logger.LogWarning(ex, "Erro de validação ao criar {EntityType}: {Message}", typeof(TDto).Name, ex.Message);
            
            var errorCode = ex.ParamName switch
            {
                "Codigo" => "DUPLICATE_CODE",
                "Nome" => "DUPLICATE_NAME",
                "Sigla" => "DUPLICATE_CODE",
                _ => "VALIDATION_ERROR"
            };
            
            return BadRequest(new { 
                ErrorCode = errorCode, 
                ErrorDescription = ex.Message,
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao criar entidade {EntityType}", typeof(TDto).Name);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Atualiza uma entidade existente
    /// </summary>
    [HttpPut("{id:int}")]
    public virtual async Task<IActionResult> Atualizar(int id, [FromBody] TAtualizarDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                Logger.LogWarning("Dados inválidos para atualização de {EntityType} ID {Id}: {ValidationErrors}", 
                    typeof(TDto).Name, id,
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                
                return BadRequest(new { 
                    ErrorCode = "VALIDATION_ERROR", 
                    ErrorDescription = "Dados inválidos",
                    ValidationErrors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                    ),
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }

            Logger.LogDebug("Atualizando entidade {EntityType} com ID {Id}", typeof(TDto).Name, id);
            
            var entidadeAtualizada = await Service.AtualizarAsync(id, dto);
            
            Logger.LogInformation("Entidade {EntityType} com ID {Id} atualizada com sucesso", typeof(TDto).Name, id);
            
            return Ok(entidadeAtualizada);
        }
        catch (ArgumentException ex) when (ex.Message.Contains("não encontrada"))
        {
            Logger.LogWarning("Entidade {EntityType} com ID {Id} não encontrada para atualização", typeof(TDto).Name, id);
            return NotFound(new { 
                ErrorCode = "ENTITY_NOT_FOUND", 
                ErrorDescription = "Entidade não encontrada",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (ArgumentException ex)
        {
            Logger.LogWarning(ex, "Erro de validação ao atualizar {EntityType} ID {Id}: {Message}", typeof(TDto).Name, id, ex.Message);
            
            var errorCode = ex.ParamName switch
            {
                "Codigo" => "DUPLICATE_CODE",
                "Nome" => "DUPLICATE_NAME",
                "Sigla" => "DUPLICATE_CODE",
                _ => "VALIDATION_ERROR"
            };
            
            return BadRequest(new { 
                ErrorCode = errorCode, 
                ErrorDescription = ex.Message,
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("CONCURRENCY_CONFLICT"))
        {
            Logger.LogWarning("Conflito de concorrência ao atualizar {EntityType} ID {Id}", typeof(TDto).Name, id);
            return StatusCode(412, new { 
                ErrorCode = "CONCURRENCY_CONFLICT", 
                ErrorDescription = "Este registro foi modificado por outro usuário. Por favor, recarregue e tente novamente.",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao atualizar entidade {EntityType} com ID {Id}", typeof(TDto).Name, id);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Ativa uma entidade
    /// </summary>
    [HttpPatch("{id:int}/ativar")]
    public virtual async Task<IActionResult> Ativar(int id)
    {
        try
        {
            Logger.LogDebug("Ativando entidade {EntityType} com ID {Id}", typeof(TDto).Name, id);
            
            await Service.AtivarAsync(id);
            
            Logger.LogInformation("Entidade {EntityType} com ID {Id} ativada com sucesso", typeof(TDto).Name, id);
            
            return NoContent();
        }
        catch (ArgumentException ex) when (ex.Message.Contains("não encontrada"))
        {
            Logger.LogWarning("Entidade {EntityType} com ID {Id} não encontrada para ativação", typeof(TDto).Name, id);
            return NotFound(new { 
                ErrorCode = "ENTITY_NOT_FOUND", 
                ErrorDescription = "Entidade não encontrada",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao ativar entidade {EntityType} com ID {Id}", typeof(TDto).Name, id);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Desativa uma entidade
    /// </summary>
    [HttpPatch("{id:int}/desativar")]
    public virtual async Task<IActionResult> Desativar(int id)
    {
        try
        {
            Logger.LogDebug("Desativando entidade {EntityType} com ID {Id}", typeof(TDto).Name, id);
            
            await Service.DesativarAsync(id);
            
            Logger.LogInformation("Entidade {EntityType} com ID {Id} desativada com sucesso", typeof(TDto).Name, id);
            
            return NoContent();
        }
        catch (ArgumentException ex) when (ex.Message.Contains("não encontrada"))
        {
            Logger.LogWarning("Entidade {EntityType} com ID {Id} não encontrada para desativação", typeof(TDto).Name, id);
            return NotFound(new { 
                ErrorCode = "ENTITY_NOT_FOUND", 
                ErrorDescription = "Entidade não encontrada",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao desativar entidade {EntityType} com ID {Id}", typeof(TDto).Name, id);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Remove uma entidade
    /// </summary>
    [HttpDelete("{id:int}")]
    public virtual async Task<IActionResult> Remover(int id)
    {
        try
        {
            Logger.LogDebug("Verificando se entidade {EntityType} com ID {Id} pode ser removida", typeof(TDto).Name, id);
            
            var podeRemover = await Service.PodeRemoverAsync(id);
            if (!podeRemover)
            {
                Logger.LogWarning("Entidade {EntityType} com ID {Id} não pode ser removida pois está sendo referenciada", typeof(TDto).Name, id);
                return Conflict(new { 
                    ErrorCode = "CANNOT_DELETE_REFERENCED", 
                    ErrorDescription = "Não é possível excluir este item pois está sendo usado por outros registros.",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            
            Logger.LogDebug("Removendo entidade {EntityType} com ID {Id}", typeof(TDto).Name, id);
            
            await Service.RemoverAsync(id);
            
            Logger.LogInformation("Entidade {EntityType} com ID {Id} removida com sucesso", typeof(TDto).Name, id);
            
            return NoContent();
        }
        catch (ArgumentException ex) when (ex.Message.Contains("não encontrada"))
        {
            Logger.LogWarning("Entidade {EntityType} com ID {Id} não encontrada para remoção", typeof(TDto).Name, id);
            return NotFound(new { 
                ErrorCode = "ENTITY_NOT_FOUND", 
                ErrorDescription = "Entidade não encontrada",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao remover entidade {EntityType} com ID {Id}", typeof(TDto).Name, id);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Verifica se uma entidade pode ser removida
    /// </summary>
    [HttpGet("{id:int}/pode-remover")]
    public virtual async Task<IActionResult> PodeRemover(int id)
    {
        try
        {
            Logger.LogDebug("Verificando se entidade {EntityType} com ID {Id} pode ser removida", typeof(TDto).Name, id);
            
            var podeRemover = await Service.PodeRemoverAsync(id);
            
            Logger.LogDebug("Entidade {EntityType} com ID {Id} {PodeRemover}", 
                typeof(TDto).Name, id, podeRemover ? "pode ser removida" : "não pode ser removida");
            
            return Ok(new { PodeRemover = podeRemover });
        }
        catch (ArgumentException ex) when (ex.Message.Contains("não encontrada"))
        {
            Logger.LogWarning("Entidade {EntityType} com ID {Id} não encontrada", typeof(TDto).Name, id);
            return NotFound(new { 
                ErrorCode = "ENTITY_NOT_FOUND", 
                ErrorDescription = "Entidade não encontrada",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao verificar se entidade {EntityType} com ID {Id} pode ser removida", typeof(TDto).Name, id);
            return StatusCode(500, new { 
                ErrorCode = "INTERNAL_ERROR", 
                ErrorDescription = "Erro interno do servidor",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}