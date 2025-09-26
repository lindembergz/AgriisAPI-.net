using AutoMapper;
using Agriis.Referencias.Aplicacao.DTOs;
using Agriis.Referencias.Aplicacao.Interfaces;
using Agriis.Referencias.Dominio.Entidades;
using Agriis.Referencias.Dominio.Interfaces;
using Microsoft.Extensions.Logging;

namespace Agriis.Referencias.Aplicacao.Servicos;

/// <summary>
/// Serviço para gerenciamento de embalagens
/// </summary>
public class EmbalagemService : ReferenciaServiceBase<Embalagem, EmbalagemDto, CriarEmbalagemDto, AtualizarEmbalagemDto>, IEmbalagemService
{
    private readonly IEmbalagemRepository _embalagemRepository;
    private readonly IUnidadeMedidaRepository _unidadeMedidaRepository;

    public EmbalagemService(
        IEmbalagemRepository repository,
        IUnidadeMedidaRepository unidadeMedidaRepository,
        IMapper mapper,
        ILogger<EmbalagemService> logger) : base(repository, mapper, logger)
    {
        _embalagemRepository = repository;
        _unidadeMedidaRepository = unidadeMedidaRepository;
    }

    /// <summary>
    /// Verifica se existe uma embalagem com o nome especificado
    /// </summary>
    public async Task<bool> ExisteNomeAsync(string nome, int? idExcluir = null, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Verificando se existe embalagem com nome {Nome}", nome);
            
            var existe = await _embalagemRepository.ExisteNomeAsync(nome, idExcluir, cancellationToken);
            
            Logger.LogDebug("Nome {Nome} {Existe}", nome, existe ? "já existe" : "não existe");
            return existe;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao verificar se existe embalagem com nome {Nome}", nome);
            throw;
        }
    }

    /// <summary>
    /// Obtém embalagens por unidade de medida
    /// </summary>
    public async Task<IEnumerable<EmbalagemDto>> ObterPorUnidadeMedidaAsync(int unidadeMedidaId, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Obtendo embalagens da unidade de medida {UnidadeMedidaId}", unidadeMedidaId);
            
            var embalagens = await _embalagemRepository.ObterPorUnidadeMedidaAsync(unidadeMedidaId, cancellationToken);
            var dtos = Mapper.Map<IEnumerable<EmbalagemDto>>(embalagens);
            
            Logger.LogDebug("Obtidas {Quantidade} embalagens da unidade de medida {UnidadeMedidaId}", dtos.Count(), unidadeMedidaId);
            return dtos;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao obter embalagens da unidade de medida {UnidadeMedidaId}", unidadeMedidaId);
            throw;
        }
    }

    /// <summary>
    /// Valida os dados antes da criação
    /// </summary>
    protected override async Task ValidarCriacaoAsync(CriarEmbalagemDto dto, CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("Validando criação de embalagem com nome {Nome}", dto.Nome);

        // Validar se nome já existe
        if (await ExisteNomeAsync(dto.Nome, null, cancellationToken))
        {
            Logger.LogWarning("Tentativa de criar embalagem com nome {Nome} que já existe", dto.Nome);
            throw new ArgumentException($"Já existe uma embalagem com o nome '{dto.Nome}'", nameof(dto.Nome));
        }

        // Validar se a unidade de medida existe
        var unidadeExiste = await _unidadeMedidaRepository.ExisteAsync(dto.UnidadeMedidaId, cancellationToken);
        if (!unidadeExiste)
        {
            Logger.LogWarning("Tentativa de criar embalagem com unidade de medida {UnidadeMedidaId} que não existe", dto.UnidadeMedidaId);
            throw new ArgumentException($"Unidade de medida com ID {dto.UnidadeMedidaId} não encontrada", nameof(dto.UnidadeMedidaId));
        }

        Logger.LogDebug("Validação de criação de embalagem concluída com sucesso");
    }

    /// <summary>
    /// Valida os dados antes da atualização
    /// </summary>
    protected override async Task ValidarAtualizacaoAsync(int id, AtualizarEmbalagemDto dto, CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("Validando atualização de embalagem com ID {Id}", id);

        // Validar se nome já existe (excluindo a própria embalagem)
        if (await ExisteNomeAsync(dto.Nome, id, cancellationToken))
        {
            Logger.LogWarning("Tentativa de atualizar embalagem com nome {Nome} que já existe", dto.Nome);
            throw new ArgumentException($"Já existe uma embalagem com o nome '{dto.Nome}'", nameof(dto.Nome));
        }

        // Validar se a unidade de medida existe
        var unidadeExiste = await _unidadeMedidaRepository.ExisteAsync(dto.UnidadeMedidaId, cancellationToken);
        if (!unidadeExiste)
        {
            Logger.LogWarning("Tentativa de atualizar embalagem com unidade de medida {UnidadeMedidaId} que não existe", dto.UnidadeMedidaId);
            throw new ArgumentException($"Unidade de medida com ID {dto.UnidadeMedidaId} não encontrada", nameof(dto.UnidadeMedidaId));
        }

        Logger.LogDebug("Validação de atualização de embalagem concluída com sucesso");
    }

    /// <summary>
    /// Aplica regras de negócio específicas durante a criação
    /// </summary>
    protected override async Task AplicarRegrasNegocioCriacaoAsync(Embalagem entidade, CriarEmbalagemDto dto, CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("Aplicando regras de negócio para criação de embalagem");

        // Embalagens são criadas ativas por padrão
        entidade.Ativar();

        Logger.LogDebug("Regras de negócio aplicadas com sucesso");
        await Task.CompletedTask;
    }

    /// <summary>
    /// Aplica regras de negócio específicas durante a atualização
    /// </summary>
    protected override async Task AplicarRegrasNegocioAtualizacaoAsync(Embalagem entidade, AtualizarEmbalagemDto dto, CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("Aplicando regras de negócio para atualização de embalagem");

        // Aplicar status ativo/inativo
        if (dto.Ativo)
            entidade.Ativar();
        else
            entidade.Desativar();

        Logger.LogDebug("Regras de negócio aplicadas com sucesso");
        await Task.CompletedTask;
    }

    /// <summary>
    /// Obtém uma embalagem por nome
    /// </summary>
    public async Task<EmbalagemDto?> ObterPorNomeAsync(string nome, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Obtendo embalagem por nome {Nome}", nome);
            
            var embalagem = await _embalagemRepository.ObterPorNomeAsync(nome, cancellationToken);
            var dto = Mapper.Map<EmbalagemDto>(embalagem);
            
            Logger.LogDebug("Embalagem {Nome} {Encontrada}", nome, dto != null ? "encontrada" : "não encontrada");
            return dto;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao obter embalagem por nome {Nome}", nome);
            throw;
        }
    }

    /// <summary>
    /// Busca embalagens por nome (busca parcial)
    /// </summary>
    public async Task<IEnumerable<EmbalagemDto>> BuscarPorNomeAsync(string nome, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Buscando embalagens por nome {Nome}", nome);
            
            var embalagens = await _embalagemRepository.BuscarPorNomeAsync(nome, cancellationToken);
            var dtos = Mapper.Map<IEnumerable<EmbalagemDto>>(embalagens);
            
            Logger.LogDebug("Encontradas {Quantidade} embalagens com nome {Nome}", dtos.Count(), nome);
            return dtos;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao buscar embalagens por nome {Nome}", nome);
            throw;
        }
    }
}