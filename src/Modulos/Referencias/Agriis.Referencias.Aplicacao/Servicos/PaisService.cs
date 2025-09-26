using AutoMapper;
using Agriis.Referencias.Aplicacao.DTOs;
using Agriis.Referencias.Aplicacao.Interfaces;
using Agriis.Referencias.Dominio.Entidades;
using Agriis.Referencias.Dominio.Interfaces;
using Microsoft.Extensions.Logging;

namespace Agriis.Referencias.Aplicacao.Servicos;

/// <summary>
/// Serviço para gerenciamento de países
/// </summary>
public class PaisService : ReferenciaServiceBase<Pais, PaisDto, CriarPaisDto, AtualizarPaisDto>, IPaisService
{
    private readonly IPaisRepository _paisRepository;
    private readonly IUfRepository _ufRepository;

    public PaisService(
        IPaisRepository repository,
        IUfRepository ufRepository,
        IMapper mapper,
        ILogger<PaisService> logger) : base(repository, mapper, logger)
    {
        _paisRepository = repository;
        _ufRepository = ufRepository;
    }

    /// <summary>
    /// Verifica se existe um país com o código especificado
    /// </summary>
    public async Task<bool> ExisteCodigoAsync(string codigo, int? idExcluir = null, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Verificando se existe país com código {Codigo}", codigo);
            
            var existe = await _paisRepository.ExisteCodigoAsync(codigo, idExcluir, cancellationToken);
            
            Logger.LogDebug("Código {Codigo} {Existe}", codigo, existe ? "já existe" : "não existe");
            return existe;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao verificar se existe país com código {Codigo}", codigo);
            throw;
        }
    }

    /// <summary>
    /// Verifica se existe um país com o nome especificado
    /// </summary>
    public async Task<bool> ExisteNomeAsync(string nome, int? idExcluir = null, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Verificando se existe país com nome {Nome}", nome);
            
            var existe = await _paisRepository.ExisteNomeAsync(nome, idExcluir, cancellationToken);
            
            Logger.LogDebug("Nome {Nome} {Existe}", nome, existe ? "já existe" : "não existe");
            return existe;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao verificar se existe país com nome {Nome}", nome);
            throw;
        }
    }

    /// <summary>
    /// Obtém as UFs de um país
    /// </summary>
    public async Task<IEnumerable<UfDto>> ObterUfsPorPaisAsync(int paisId, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Obtendo UFs do país {PaisId}", paisId);
            
            var ufs = await _ufRepository.ObterPorPaisAsync(paisId, cancellationToken);
            var dtos = Mapper.Map<IEnumerable<UfDto>>(ufs);
            
            Logger.LogDebug("Obtidas {Quantidade} UFs do país {PaisId}", dtos.Count(), paisId);
            return dtos;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao obter UFs do país {PaisId}", paisId);
            throw;
        }
    }

    /// <summary>
    /// Verifica se uma entidade pode ser removida
    /// </summary>
    public override async Task<bool> PodeRemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Verificando se país {Id} pode ser removido", id);
            
            // Verificar se há UFs associadas
            var ufs = await _ufRepository.ObterPorPaisAsync(id, cancellationToken);
            var podeRemover = !ufs.Any();
            
            Logger.LogDebug("País {Id} {PodeRemover} - {QuantidadeUfs} UFs associadas", 
                id, podeRemover ? "pode ser removido" : "não pode ser removido", ufs.Count());
            
            return podeRemover;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao verificar se país {Id} pode ser removido", id);
            throw;
        }
    }

    /// <summary>
    /// Valida os dados antes da criação
    /// </summary>
    protected override async Task ValidarCriacaoAsync(CriarPaisDto dto, CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("Validando criação de país com código {Codigo}", dto.Codigo);

        // Validar se código já existe
        if (await ExisteCodigoAsync(dto.Codigo, null, cancellationToken))
        {
            Logger.LogWarning("Tentativa de criar país com código {Codigo} que já existe", dto.Codigo);
            throw new ArgumentException($"Já existe um país com o código '{dto.Codigo}'", nameof(dto.Codigo));
        }

        // Validar se nome já existe
        if (await ExisteNomeAsync(dto.Nome, null, cancellationToken))
        {
            Logger.LogWarning("Tentativa de criar país com nome {Nome} que já existe", dto.Nome);
            throw new ArgumentException($"Já existe um país com o nome '{dto.Nome}'", nameof(dto.Nome));
        }

        Logger.LogDebug("Validação de criação de país concluída com sucesso");
    }

    /// <summary>
    /// Valida os dados antes da atualização
    /// </summary>
    protected override async Task ValidarAtualizacaoAsync(int id, AtualizarPaisDto dto, CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("Validando atualização de país com ID {Id}", id);

        // Validar se nome já existe (excluindo o próprio país)
        if (await ExisteNomeAsync(dto.Nome, id, cancellationToken))
        {
            Logger.LogWarning("Tentativa de atualizar país com nome {Nome} que já existe", dto.Nome);
            throw new ArgumentException($"Já existe um país com o nome '{dto.Nome}'", nameof(dto.Nome));
        }

        Logger.LogDebug("Validação de atualização de país concluída com sucesso");
    }

    /// <summary>
    /// Aplica regras de negócio específicas durante a criação
    /// </summary>
    protected override async Task AplicarRegrasNegocioCriacaoAsync(Pais entidade, CriarPaisDto dto, CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("Aplicando regras de negócio para criação de país");

        // Países são criados ativos por padrão
        entidade.Ativar();

        Logger.LogDebug("Regras de negócio aplicadas com sucesso");
        await Task.CompletedTask;
    }

    /// <summary>
    /// Aplica regras de negócio específicas durante a atualização
    /// </summary>
    protected override async Task AplicarRegrasNegocioAtualizacaoAsync(Pais entidade, AtualizarPaisDto dto, CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("Aplicando regras de negócio para atualização de país");

        // Aplicar status ativo/inativo
        if (dto.Ativo)
            entidade.Ativar();
        else
            entidade.Desativar();

        Logger.LogDebug("Regras de negócio aplicadas com sucesso");
        await Task.CompletedTask;
    }

    /// <summary>
    /// Obtém um país por código
    /// </summary>
    public async Task<PaisDto?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Obtendo país por código {Codigo}", codigo);
            
            var pais = await _paisRepository.ObterPorCodigoAsync(codigo, cancellationToken);
            var dto = Mapper.Map<PaisDto>(pais);
            
            Logger.LogDebug("País {Codigo} {Encontrado}", codigo, dto != null ? "encontrado" : "não encontrado");
            return dto;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao obter país por código {Codigo}", codigo);
            throw;
        }
    }
}