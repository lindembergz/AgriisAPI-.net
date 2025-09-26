using AutoMapper;
using Agriis.Referencias.Aplicacao.DTOs;
using Agriis.Referencias.Aplicacao.Interfaces;
using Agriis.Referencias.Dominio.Entidades;
using Agriis.Referencias.Dominio.Interfaces;
using Microsoft.Extensions.Logging;

namespace Agriis.Referencias.Aplicacao.Servicos;

/// <summary>
/// Serviço para gerenciamento de moedas
/// </summary>
public class MoedaService : ReferenciaServiceBase<Moeda, MoedaDto, CriarMoedaDto, AtualizarMoedaDto>, IMoedaService
{
    private readonly IMoedaRepository _moedaRepository;

    public MoedaService(
        IMoedaRepository repository,
        IMapper mapper,
        ILogger<MoedaService> logger) : base(repository, mapper, logger)
    {
        _moedaRepository = repository;
    }

    /// <summary>
    /// Verifica se existe uma moeda com o código especificado
    /// </summary>
    public async Task<bool> ExisteCodigoAsync(string codigo, int? idExcluir = null, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Verificando se existe moeda com código {Codigo}", codigo);
            
            var existe = await _moedaRepository.ExisteCodigoAsync(codigo, idExcluir, cancellationToken);
            
            Logger.LogDebug("Código {Codigo} {Existe}", codigo, existe ? "já existe" : "não existe");
            return existe;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao verificar se existe moeda com código {Codigo}", codigo);
            throw;
        }
    }

    /// <summary>
    /// Verifica se existe uma moeda com o nome especificado
    /// </summary>
    public async Task<bool> ExisteNomeAsync(string nome, int? idExcluir = null, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Verificando se existe moeda com nome {Nome}", nome);
            
            var existe = await _moedaRepository.ExisteNomeAsync(nome, idExcluir, cancellationToken);
            
            Logger.LogDebug("Nome {Nome} {Existe}", nome, existe ? "já existe" : "não existe");
            return existe;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao verificar se existe moeda com nome {Nome}", nome);
            throw;
        }
    }

    /// <summary>
    /// Verifica se existe uma moeda com o símbolo especificado
    /// </summary>
    public async Task<bool> ExisteSimboloAsync(string simbolo, int? idExcluir = null, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Verificando se existe moeda com símbolo {Simbolo}", simbolo);
            
            var existe = await _moedaRepository.ExisteSimboloAsync(simbolo, idExcluir, cancellationToken);
            
            Logger.LogDebug("Símbolo {Simbolo} {Existe}", simbolo, existe ? "já existe" : "não existe");
            return existe;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao verificar se existe moeda com símbolo {Simbolo}", simbolo);
            throw;
        }
    }

    /// <summary>
    /// Valida os dados antes da criação
    /// </summary>
    protected override async Task ValidarCriacaoAsync(CriarMoedaDto dto, CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("Validando criação de moeda com código {Codigo}", dto.Codigo);

        // Validar se código já existe
        if (await ExisteCodigoAsync(dto.Codigo, null, cancellationToken))
        {
            Logger.LogWarning("Tentativa de criar moeda com código {Codigo} que já existe", dto.Codigo);
            throw new ArgumentException($"Já existe uma moeda com o código '{dto.Codigo}'", nameof(dto.Codigo));
        }

        // Validar se nome já existe
        if (await ExisteNomeAsync(dto.Nome, null, cancellationToken))
        {
            Logger.LogWarning("Tentativa de criar moeda com nome {Nome} que já existe", dto.Nome);
            throw new ArgumentException($"Já existe uma moeda com o nome '{dto.Nome}'", nameof(dto.Nome));
        }

        // Validar se símbolo já existe
        if (await ExisteSimboloAsync(dto.Simbolo, null, cancellationToken))
        {
            Logger.LogWarning("Tentativa de criar moeda com símbolo {Simbolo} que já existe", dto.Simbolo);
            throw new ArgumentException($"Já existe uma moeda com o símbolo '{dto.Simbolo}'", nameof(dto.Simbolo));
        }

        Logger.LogDebug("Validação de criação de moeda concluída com sucesso");
    }

    /// <summary>
    /// Valida os dados antes da atualização
    /// </summary>
    protected override async Task ValidarAtualizacaoAsync(int id, AtualizarMoedaDto dto, CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("Validando atualização de moeda com ID {Id}", id);

        // Validar se nome já existe (excluindo a própria moeda)
        if (await ExisteNomeAsync(dto.Nome, id, cancellationToken))
        {
            Logger.LogWarning("Tentativa de atualizar moeda com nome {Nome} que já existe", dto.Nome);
            throw new ArgumentException($"Já existe uma moeda com o nome '{dto.Nome}'", nameof(dto.Nome));
        }

        // Validar se símbolo já existe (excluindo a própria moeda)
        if (await ExisteSimboloAsync(dto.Simbolo, id, cancellationToken))
        {
            Logger.LogWarning("Tentativa de atualizar moeda com símbolo {Simbolo} que já existe", dto.Simbolo);
            throw new ArgumentException($"Já existe uma moeda com o símbolo '{dto.Simbolo}'", nameof(dto.Simbolo));
        }

        Logger.LogDebug("Validação de atualização de moeda concluída com sucesso");
    }

    /// <summary>
    /// Aplica regras de negócio específicas durante a criação
    /// </summary>
    protected override async Task AplicarRegrasNegocioCriacaoAsync(Moeda entidade, CriarMoedaDto dto, CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("Aplicando regras de negócio para criação de moeda");

        // Normalizar código para maiúsculo
        entidade.AtualizarInformacoes(dto.Nome, dto.Simbolo);
        
        // Moedas são criadas ativas por padrão
        entidade.Ativar();

        Logger.LogDebug("Regras de negócio aplicadas com sucesso");
        await Task.CompletedTask;
    }

    /// <summary>
    /// Aplica regras de negócio específicas durante a atualização
    /// </summary>
    protected override async Task AplicarRegrasNegocioAtualizacaoAsync(Moeda entidade, AtualizarMoedaDto dto, CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("Aplicando regras de negócio para atualização de moeda");

        // Atualizar informações da moeda
        entidade.AtualizarInformacoes(dto.Nome, dto.Simbolo);
        
        // Aplicar status ativo/inativo
        if (dto.Ativo)
            entidade.Ativar();
        else
            entidade.Desativar();

        Logger.LogDebug("Regras de negócio aplicadas com sucesso");
        await Task.CompletedTask;
    }

    /// <summary>
    /// Obtém uma moeda por código
    /// </summary>
    public async Task<MoedaDto?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Obtendo moeda por código {Codigo}", codigo);
            
            var moeda = await _moedaRepository.ObterPorCodigoAsync(codigo, cancellationToken);
            var dto = Mapper.Map<MoedaDto>(moeda);
            
            Logger.LogDebug("Moeda {Codigo} {Encontrada}", codigo, dto != null ? "encontrada" : "não encontrada");
            return dto;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao obter moeda por código {Codigo}", codigo);
            throw;
        }
    }
}