using AutoMapper;
using Agriis.Compartilhado.Dominio.Entidades;
using Agriis.Referencias.Aplicacao.Interfaces;
using Agriis.Referencias.Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Agriis.Referencias.Aplicacao.Servicos;

/// <summary>
/// Implementação base para serviços de entidades de referência
/// </summary>
/// <typeparam name="TEntidade">Tipo da entidade de domínio</typeparam>
/// <typeparam name="TDto">DTO de leitura</typeparam>
/// <typeparam name="TCriarDto">DTO de criação</typeparam>
/// <typeparam name="TAtualizarDto">DTO de atualização</typeparam>
public abstract class ReferenciaServiceBase<TEntidade, TDto, TCriarDto, TAtualizarDto> : IReferenciaService<TDto, TCriarDto, TAtualizarDto>
    where TEntidade : EntidadeBase
{
    protected readonly IReferenciaRepository<TEntidade> Repository;
    protected readonly IMapper Mapper;
    protected readonly ILogger Logger;

    protected ReferenciaServiceBase(
        IReferenciaRepository<TEntidade> repository,
        IMapper mapper,
        ILogger logger)
    {
        Repository = repository ?? throw new ArgumentNullException(nameof(repository));
        Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Obtém todas as entidades
    /// </summary>
    public virtual async Task<IEnumerable<TDto>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Obtendo todas as entidades {TipoEntidade}", typeof(TEntidade).Name);
            
            var entidades = await Repository.ObterTodosAsync(cancellationToken);
            var dtos = Mapper.Map<IEnumerable<TDto>>(entidades);
            
            Logger.LogDebug("Obtidas {Quantidade} entidades {TipoEntidade}", dtos.Count(), typeof(TEntidade).Name);
            return dtos;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao obter todas as entidades {TipoEntidade}", typeof(TEntidade).Name);
            throw;
        }
    }

    /// <summary>
    /// Obtém apenas as entidades ativas
    /// </summary>
    public virtual async Task<IEnumerable<TDto>> ObterAtivosAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Obtendo entidades ativas {TipoEntidade}", typeof(TEntidade).Name);
            
            var entidades = await Repository.ObterAtivosAsync(cancellationToken);
            var dtos = Mapper.Map<IEnumerable<TDto>>(entidades);
            
            Logger.LogDebug("Obtidas {Quantidade} entidades ativas {TipoEntidade}", dtos.Count(), typeof(TEntidade).Name);
            return dtos;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao obter entidades ativas {TipoEntidade}", typeof(TEntidade).Name);
            throw;
        }
    }

    /// <summary>
    /// Obtém uma entidade por ID
    /// </summary>
    public virtual async Task<TDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Obtendo entidade {TipoEntidade} com ID {Id}", typeof(TEntidade).Name, id);
            
            var entidade = await Repository.ObterPorIdAsync(id, cancellationToken);
            if (entidade == null)
            {
                Logger.LogWarning("Entidade {TipoEntidade} com ID {Id} não encontrada", typeof(TEntidade).Name, id);
                return default;
            }

            var dto = Mapper.Map<TDto>(entidade);
            Logger.LogDebug("Entidade {TipoEntidade} com ID {Id} obtida com sucesso", typeof(TEntidade).Name, id);
            return dto;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao obter entidade {TipoEntidade} com ID {Id}", typeof(TEntidade).Name, id);
            throw;
        }
    }

    /// <summary>
    /// Cria uma nova entidade
    /// </summary>
    public virtual async Task<TDto> CriarAsync(TCriarDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Criando nova entidade {TipoEntidade}", typeof(TEntidade).Name);
            
            // Validar dados antes da criação
            await ValidarCriacaoAsync(dto, cancellationToken);
            
            // Mapear DTO para entidade
            var entidade = Mapper.Map<TEntidade>(dto);
            
            // Aplicar regras de negócio específicas
            await AplicarRegrasNegocioCriacaoAsync(entidade, dto, cancellationToken);
            
            // Salvar no repositório
            var entidadeCriada = await Repository.AdicionarAsync(entidade, cancellationToken);
            
            // Mapear de volta para DTO
            var dtoResultado = Mapper.Map<TDto>(entidadeCriada);
            
            Logger.LogInformation("Entidade {TipoEntidade} criada com sucesso. ID: {Id}", typeof(TEntidade).Name, entidadeCriada.Id);
            return dtoResultado;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao criar entidade {TipoEntidade}", typeof(TEntidade).Name);
            throw;
        }
    }

    /// <summary>
    /// Atualiza uma entidade existente
    /// </summary>
    public virtual async Task<TDto> AtualizarAsync(int id, TAtualizarDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Atualizando entidade {TipoEntidade} com ID {Id}", typeof(TEntidade).Name, id);
            
            // Obter entidade existente
            var entidadeExistente = await Repository.ObterPorIdAsync(id, cancellationToken);
            if (entidadeExistente == null)
            {
                Logger.LogWarning("Entidade {TipoEntidade} com ID {Id} não encontrada para atualização", typeof(TEntidade).Name, id);
                throw new ArgumentException($"Entidade com ID {id} não encontrada", nameof(id));
            }
            
            // Validar dados antes da atualização
            await ValidarAtualizacaoAsync(id, dto, cancellationToken);
            
            // Mapear alterações do DTO para a entidade existente
            Mapper.Map(dto, entidadeExistente);
            
            // Aplicar regras de negócio específicas
            await AplicarRegrasNegocioAtualizacaoAsync(entidadeExistente, dto, cancellationToken);
            
            // Salvar alterações
            await Repository.AtualizarAsync(entidadeExistente, cancellationToken);
            
            // Mapear de volta para DTO
            var dtoResultado = Mapper.Map<TDto>(entidadeExistente);
            
            Logger.LogInformation("Entidade {TipoEntidade} com ID {Id} atualizada com sucesso", typeof(TEntidade).Name, id);
            return dtoResultado;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            Logger.LogWarning(ex, "Conflito de concorrência ao atualizar entidade {TipoEntidade} com ID {Id}", typeof(TEntidade).Name, id);
            throw new InvalidOperationException("A entidade foi modificada por outro usuário. Por favor, recarregue os dados e tente novamente.", ex);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao atualizar entidade {TipoEntidade} com ID {Id}", typeof(TEntidade).Name, id);
            throw;
        }
    }

    /// <summary>
    /// Ativa uma entidade
    /// </summary>
    public virtual async Task AtivarAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Ativando entidade {TipoEntidade} com ID {Id}", typeof(TEntidade).Name, id);
            
            await Repository.AtivarAsync(id, cancellationToken);
            
            Logger.LogInformation("Entidade {TipoEntidade} com ID {Id} ativada com sucesso", typeof(TEntidade).Name, id);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao ativar entidade {TipoEntidade} com ID {Id}", typeof(TEntidade).Name, id);
            throw;
        }
    }

    /// <summary>
    /// Desativa uma entidade
    /// </summary>
    public virtual async Task DesativarAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Desativando entidade {TipoEntidade} com ID {Id}", typeof(TEntidade).Name, id);
            
            await Repository.DesativarAsync(id, cancellationToken);
            
            Logger.LogInformation("Entidade {TipoEntidade} com ID {Id} desativada com sucesso", typeof(TEntidade).Name, id);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao desativar entidade {TipoEntidade} com ID {Id}", typeof(TEntidade).Name, id);
            throw;
        }
    }

    /// <summary>
    /// Remove uma entidade
    /// </summary>
    public virtual async Task RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Removendo entidade {TipoEntidade} com ID {Id}", typeof(TEntidade).Name, id);
            
            // Verificar se pode remover
            var podeRemover = await Repository.PodeRemoverAsync(id, cancellationToken);
            if (!podeRemover)
            {
                Logger.LogWarning("Tentativa de remover entidade {TipoEntidade} com ID {Id} que possui dependências", typeof(TEntidade).Name, id);
                throw new InvalidOperationException("Não é possível remover esta entidade pois ela está sendo referenciada por outros registros.");
            }
            
            await Repository.RemoverAsync(id, cancellationToken);
            
            Logger.LogInformation("Entidade {TipoEntidade} com ID {Id} removida com sucesso", typeof(TEntidade).Name, id);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao remover entidade {TipoEntidade} com ID {Id}", typeof(TEntidade).Name, id);
            throw;
        }
    }

    /// <summary>
    /// Verifica se uma entidade pode ser removida
    /// </summary>
    public virtual async Task<bool> PodeRemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Verificando se entidade {TipoEntidade} com ID {Id} pode ser removida", typeof(TEntidade).Name, id);
            
            var podeRemover = await Repository.PodeRemoverAsync(id, cancellationToken);
            
            Logger.LogDebug("Entidade {TipoEntidade} com ID {Id} {PodeRemover}", typeof(TEntidade).Name, id, podeRemover ? "pode ser removida" : "não pode ser removida");
            return podeRemover;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao verificar se entidade {TipoEntidade} com ID {Id} pode ser removida", typeof(TEntidade).Name, id);
            throw;
        }
    }

    /// <summary>
    /// Valida os dados antes da criação
    /// Classes filhas podem sobrescrever para implementar validações específicas
    /// </summary>
    protected virtual async Task ValidarCriacaoAsync(TCriarDto dto, CancellationToken cancellationToken = default)
    {
        // Implementação padrão - sem validações específicas
        await Task.CompletedTask;
    }

    /// <summary>
    /// Valida os dados antes da atualização
    /// Classes filhas podem sobrescrever para implementar validações específicas
    /// </summary>
    protected virtual async Task ValidarAtualizacaoAsync(int id, TAtualizarDto dto, CancellationToken cancellationToken = default)
    {
        // Implementação padrão - sem validações específicas
        await Task.CompletedTask;
    }

    /// <summary>
    /// Aplica regras de negócio específicas durante a criação
    /// Classes filhas podem sobrescrever para implementar regras específicas
    /// </summary>
    protected virtual async Task AplicarRegrasNegocioCriacaoAsync(TEntidade entidade, TCriarDto dto, CancellationToken cancellationToken = default)
    {
        // Implementação padrão - sem regras específicas
        await Task.CompletedTask;
    }

    /// <summary>
    /// Aplica regras de negócio específicas durante a atualização
    /// Classes filhas podem sobrescrever para implementar regras específicas
    /// </summary>
    protected virtual async Task AplicarRegrasNegocioAtualizacaoAsync(TEntidade entidade, TAtualizarDto dto, CancellationToken cancellationToken = default)
    {
        // Implementação padrão - sem regras específicas
        await Task.CompletedTask;
    }
}