using AutoMapper;
using Agriis.Referencias.Aplicacao.DTOs;
using Agriis.Referencias.Aplicacao.Interfaces;
using Agriis.Referencias.Dominio.Entidades;
using Agriis.Referencias.Dominio.Interfaces;
using Microsoft.Extensions.Logging;

namespace Agriis.Referencias.Aplicacao.Servicos;

/// <summary>
/// Serviço para gerenciamento de UFs
/// </summary>
public class UfService : ReferenciaServiceBase<Uf, UfDto, CriarUfDto, AtualizarUfDto>, IUfService
{
    private readonly IUfRepository _ufRepository;
    private readonly IPaisRepository _paisRepository;
    private readonly IMunicipioRepository _municipioRepository;

    public UfService(
        IUfRepository repository,
        IPaisRepository paisRepository,
        IMunicipioRepository municipioRepository,
        IMapper mapper,
        ILogger<UfService> logger) : base(repository, mapper, logger)
    {
        _ufRepository = repository;
        _paisRepository = paisRepository;
        _municipioRepository = municipioRepository;
    }

    /// <summary>
    /// Verifica se existe uma UF com o código especificado
    /// </summary>
    public async Task<bool> ExisteCodigoAsync(string codigo, int? idExcluir = null, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Verificando se existe UF com código {Codigo}", codigo);
            
            var existe = await _ufRepository.ExisteCodigoAsync(codigo, idExcluir, cancellationToken);
            
            Logger.LogDebug("Código {Codigo} {Existe}", codigo, existe ? "já existe" : "não existe");
            return existe;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao verificar se existe UF com código {Codigo}", codigo);
            throw;
        }
    }

    /// <summary>
    /// Verifica se existe uma UF com o nome especificado
    /// </summary>
    public async Task<bool> ExisteNomeAsync(string nome, int? idExcluir = null, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Verificando se existe UF com nome {Nome}", nome);
            
            var existe = await _ufRepository.ExisteNomeAsync(nome, idExcluir, cancellationToken);
            
            Logger.LogDebug("Nome {Nome} {Existe}", nome, existe ? "já existe" : "não existe");
            return existe;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao verificar se existe UF com nome {Nome}", nome);
            throw;
        }
    }

    /// <summary>
    /// Obtém as UFs de um país específico
    /// </summary>
    public async Task<IEnumerable<UfDto>> ObterPorPaisAsync(int paisId, CancellationToken cancellationToken = default)
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
    /// Obtém os municípios de uma UF
    /// </summary>
    public async Task<IEnumerable<MunicipioDto>> ObterMunicipiosPorUfAsync(int ufId, CancellationToken cancellationToken = default)
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
    /// Verifica se uma entidade pode ser removida
    /// </summary>
    public override async Task<bool> PodeRemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Verificando se UF {Id} pode ser removida", id);
            
            // Verificar se há municípios associados
            var municipios = await _municipioRepository.ObterPorUfAsync(id, cancellationToken);
            var podeRemover = !municipios.Any();
            
            Logger.LogDebug("UF {Id} {PodeRemover} - {QuantidadeMunicipios} municípios associados", 
                id, podeRemover ? "pode ser removida" : "não pode ser removida", municipios.Count());
            
            return podeRemover;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao verificar se UF {Id} pode ser removida", id);
            throw;
        }
    }

    /// <summary>
    /// Valida os dados antes da criação
    /// </summary>
    protected override async Task ValidarCriacaoAsync(CriarUfDto dto, CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("Validando criação de UF com código {Codigo}", dto.Codigo);

        // Validar se código já existe
        if (await ExisteCodigoAsync(dto.Codigo, null, cancellationToken))
        {
            Logger.LogWarning("Tentativa de criar UF com código {Codigo} que já existe", dto.Codigo);
            throw new ArgumentException($"Já existe uma UF com o código '{dto.Codigo}'", nameof(dto.Codigo));
        }

        // Validar se nome já existe
        if (await ExisteNomeAsync(dto.Nome, null, cancellationToken))
        {
            Logger.LogWarning("Tentativa de criar UF com nome {Nome} que já existe", dto.Nome);
            throw new ArgumentException($"Já existe uma UF com o nome '{dto.Nome}'", nameof(dto.Nome));
        }

        // Validar se o país existe
        var paisExiste = await _paisRepository.ExisteAsync(dto.PaisId, cancellationToken);
        if (!paisExiste)
        {
            Logger.LogWarning("Tentativa de criar UF com país {PaisId} que não existe", dto.PaisId);
            throw new ArgumentException($"País com ID {dto.PaisId} não encontrado", nameof(dto.PaisId));
        }

        Logger.LogDebug("Validação de criação de UF concluída com sucesso");
    }

    /// <summary>
    /// Valida os dados antes da atualização
    /// </summary>
    protected override async Task ValidarAtualizacaoAsync(int id, AtualizarUfDto dto, CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("Validando atualização de UF com ID {Id}", id);

        // Validar se nome já existe (excluindo a própria UF)
        if (await ExisteNomeAsync(dto.Nome, id, cancellationToken))
        {
            Logger.LogWarning("Tentativa de atualizar UF com nome {Nome} que já existe", dto.Nome);
            throw new ArgumentException($"Já existe uma UF com o nome '{dto.Nome}'", nameof(dto.Nome));
        }

        Logger.LogDebug("Validação de atualização de UF concluída com sucesso");
    }

    /// <summary>
    /// Aplica regras de negócio específicas durante a criação
    /// </summary>
    protected override async Task AplicarRegrasNegocioCriacaoAsync(Uf entidade, CriarUfDto dto, CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("Aplicando regras de negócio para criação de UF");

        // UFs são criadas ativas por padrão
        entidade.Ativar();

        Logger.LogDebug("Regras de negócio aplicadas com sucesso");
        await Task.CompletedTask;
    }

    /// <summary>
    /// Aplica regras de negócio específicas durante a atualização
    /// </summary>
    protected override async Task AplicarRegrasNegocioAtualizacaoAsync(Uf entidade, AtualizarUfDto dto, CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("Aplicando regras de negócio para atualização de UF");

        // Aplicar status ativo/inativo
        if (dto.Ativo)
            entidade.Ativar();
        else
            entidade.Desativar();

        Logger.LogDebug("Regras de negócio aplicadas com sucesso");
        await Task.CompletedTask;
    }
}