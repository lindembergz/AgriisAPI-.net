using AutoMapper;
using Agriis.Referencias.Aplicacao.DTOs;
using Agriis.Referencias.Aplicacao.Interfaces;
using Agriis.Referencias.Dominio.Entidades;
using Agriis.Referencias.Dominio.Interfaces;
using Microsoft.Extensions.Logging;

namespace Agriis.Referencias.Aplicacao.Servicos;

/// <summary>
/// Serviço para gerenciamento de municípios
/// </summary>
public class MunicipioService : ReferenciaServiceBase<Municipio, MunicipioDto, CriarMunicipioDto, AtualizarMunicipioDto>, IMunicipioService
{
    private readonly IMunicipioRepository _municipioRepository;
    private readonly IUfRepository _ufRepository;

    public MunicipioService(
        IMunicipioRepository repository,
        IUfRepository ufRepository,
        IMapper mapper,
        ILogger<MunicipioService> logger) : base(repository, mapper, logger)
    {
        _municipioRepository = repository;
        _ufRepository = ufRepository;
    }

    /// <summary>
    /// Verifica se existe um município com o nome especificado na UF
    /// </summary>
    public async Task<bool> ExisteNomeNaUfAsync(string nome, int ufId, int? idExcluir = null, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Verificando se existe município com nome {Nome} na UF {UfId}", nome, ufId);
            
            var existe = await _municipioRepository.ExisteNomeNaUfAsync(nome, ufId, idExcluir, cancellationToken);
            
            Logger.LogDebug("Nome {Nome} na UF {UfId} {Existe}", nome, ufId, existe ? "já existe" : "não existe");
            return existe;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao verificar se existe município com nome {Nome} na UF {UfId}", nome, ufId);
            throw;
        }
    }

    /// <summary>
    /// Verifica se existe um município com o código IBGE especificado
    /// </summary>
    public async Task<bool> ExisteCodigoIbgeAsync(string codigoIbge, int? idExcluir = null, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Verificando se existe município com código IBGE {CodigoIbge}", codigoIbge);
            
            var existe = await _municipioRepository.ExisteCodigoIbgeAsync(codigoIbge, idExcluir, cancellationToken);
            
            Logger.LogDebug("Código IBGE {CodigoIbge} {Existe}", codigoIbge, existe ? "já existe" : "não existe");
            return existe;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao verificar se existe município com código IBGE {CodigoIbge}", codigoIbge);
            throw;
        }
    }

    /// <summary>
    /// Obtém os municípios de uma UF específica
    /// </summary>
    public async Task<IEnumerable<MunicipioDto>> ObterPorUfAsync(int ufId, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Obtendo municípios da UF {UfId}", ufId);
            
            var municipios = await _municipioRepository.ObterPorUfAsync(ufId, cancellationToken);
            var dtos = Mapper.Map<IEnumerable<MunicipioDto>>(municipios);
            
            Logger.LogDebug("Obtidos {Quantidade} municípios da UF {UfId}", dtos.Count(), ufId);
            return dtos;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao obter municípios da UF {UfId}", ufId);
            throw;
        }
    }

    /// <summary>
    /// Busca municípios por nome
    /// </summary>
    public async Task<IEnumerable<MunicipioDto>> BuscarPorNomeAsync(string nome, int? ufId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Buscando municípios por nome {Nome} na UF {UfId}", nome, ufId);
            
            var municipios = await _municipioRepository.BuscarPorNomeAsync(nome, ufId, cancellationToken);
            var dtos = Mapper.Map<IEnumerable<MunicipioDto>>(municipios);
            
            Logger.LogDebug("Encontrados {Quantidade} municípios com nome {Nome}", dtos.Count(), nome);
            return dtos;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao buscar municípios por nome {Nome}", nome);
            throw;
        }
    }

    /// <summary>
    /// Valida os dados antes da criação
    /// </summary>
    protected override async Task ValidarCriacaoAsync(CriarMunicipioDto dto, CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("Validando criação de município com nome {Nome}", dto.Nome);

        // Validar se a UF existe
        var ufExiste = await _ufRepository.ExisteAsync(dto.UfId, cancellationToken);
        if (!ufExiste)
        {
            Logger.LogWarning("Tentativa de criar município com UF {UfId} que não existe", dto.UfId);
            throw new ArgumentException($"UF com ID {dto.UfId} não encontrada", nameof(dto.UfId));
        }

        // Validar se nome já existe na UF
        if (await ExisteNomeNaUfAsync(dto.Nome, dto.UfId, null, cancellationToken))
        {
            Logger.LogWarning("Tentativa de criar município com nome {Nome} que já existe na UF {UfId}", dto.Nome, dto.UfId);
            throw new ArgumentException($"Já existe um município com o nome '{dto.Nome}' nesta UF", nameof(dto.Nome));
        }

        // Validar se código IBGE já existe
        if (dto.CodigoIbge > 0 && await ExisteCodigoIbgeAsync(dto.CodigoIbge.ToString(), null, cancellationToken))
        {
            Logger.LogWarning("Tentativa de criar município com código IBGE {CodigoIbge} que já existe", dto.CodigoIbge);
            throw new ArgumentException($"Já existe um município com o código IBGE '{dto.CodigoIbge}'", nameof(dto.CodigoIbge));
        }

        Logger.LogDebug("Validação de criação de município concluída com sucesso");
    }

    /// <summary>
    /// Valida os dados antes da atualização
    /// </summary>
    protected override async Task ValidarAtualizacaoAsync(int id, AtualizarMunicipioDto dto, CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("Validando atualização de município com ID {Id}", id);

        // Obter município atual para validar UF
        var municipioAtual = await _municipioRepository.ObterPorIdAsync(id, cancellationToken);
        if (municipioAtual == null)
        {
            Logger.LogWarning("Município com ID {Id} não encontrado para atualização", id);
            throw new ArgumentException($"Município com ID {id} não encontrado", nameof(id));
        }

        // Validar se nome já existe na UF (excluindo o próprio município)
        if (await ExisteNomeNaUfAsync(dto.Nome, municipioAtual.UfId, id, cancellationToken))
        {
            Logger.LogWarning("Tentativa de atualizar município com nome {Nome} que já existe na UF", dto.Nome);
            throw new ArgumentException($"Já existe um município com o nome '{dto.Nome}' nesta UF", nameof(dto.Nome));
        }

        Logger.LogDebug("Validação de atualização de município concluída com sucesso");
    }

    /// <summary>
    /// Aplica regras de negócio específicas durante a criação
    /// </summary>
    protected override async Task AplicarRegrasNegocioCriacaoAsync(Municipio entidade, CriarMunicipioDto dto, CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("Aplicando regras de negócio para criação de município");

        // Municípios são criados ativos por padrão
        entidade.Ativar();

        Logger.LogDebug("Regras de negócio aplicadas com sucesso");
        await Task.CompletedTask;
    }

    /// <summary>
    /// Aplica regras de negócio específicas durante a atualização
    /// </summary>
    protected override async Task AplicarRegrasNegocioAtualizacaoAsync(Municipio entidade, AtualizarMunicipioDto dto, CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("Aplicando regras de negócio para atualização de município");

        // Aplicar status ativo/inativo
        if (dto.Ativo)
            entidade.Ativar();
        else
            entidade.Desativar();

        Logger.LogDebug("Regras de negócio aplicadas com sucesso");
        await Task.CompletedTask;
    }

    /// <summary>
    /// Obtém um município por código IBGE
    /// </summary>
    public async Task<MunicipioDto?> ObterPorCodigoIbgeAsync(string codigoIbge, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Obtendo município por código IBGE {CodigoIbge}", codigoIbge);
            
            var municipio = await _municipioRepository.ObterPorCodigoIbgeAsync(codigoIbge, cancellationToken);
            var dto = Mapper.Map<MunicipioDto>(municipio);
            
            Logger.LogDebug("Município {CodigoIbge} {Encontrado}", codigoIbge, dto != null ? "encontrado" : "não encontrado");
            return dto;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao obter município por código IBGE {CodigoIbge}", codigoIbge);
            throw;
        }
    }
}