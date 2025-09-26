using AutoMapper;
using Agriis.Referencias.Aplicacao.DTOs;
using Agriis.Referencias.Aplicacao.Interfaces;
using Agriis.Referencias.Dominio.Entidades;
using Agriis.Referencias.Dominio.Enums;
using Agriis.Referencias.Dominio.Interfaces;
using Microsoft.Extensions.Logging;

namespace Agriis.Referencias.Aplicacao.Servicos;

/// <summary>
/// Serviço para gerenciamento de atividades agropecuárias
/// </summary>
public class AtividadeAgropecuariaService : ReferenciaServiceBase<AtividadeAgropecuaria, AtividadeAgropecuariaDto, CriarAtividadeAgropecuariaDto, AtualizarAtividadeAgropecuariaDto>, IAtividadeAgropecuariaService
{
    private readonly IAtividadeAgropecuariaRepository _atividadeRepository;

    public AtividadeAgropecuariaService(
        IAtividadeAgropecuariaRepository repository,
        IMapper mapper,
        ILogger<AtividadeAgropecuariaService> logger) : base(repository, mapper, logger)
    {
        _atividadeRepository = repository;
    }

    /// <summary>
    /// Verifica se existe uma atividade com o código especificado
    /// </summary>
    public async Task<bool> ExisteCodigoAsync(string codigo, int? idExcluir = null, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Verificando se existe atividade agropecuária com código {Codigo}", codigo);
            
            var existe = await _atividadeRepository.ExisteCodigoAsync(codigo, idExcluir, cancellationToken);
            
            Logger.LogDebug("Código {Codigo} {Existe}", codigo, existe ? "já existe" : "não existe");
            return existe;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao verificar se existe atividade agropecuária com código {Codigo}", codigo);
            throw;
        }
    }

    /// <summary>
    /// Verifica se existe uma atividade com a descrição especificada
    /// </summary>
    public async Task<bool> ExisteDescricaoAsync(string descricao, int? idExcluir = null, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Verificando se existe atividade agropecuária com descrição {Descricao}", descricao);
            
            var existe = await _atividadeRepository.ExisteDescricaoAsync(descricao, idExcluir, cancellationToken);
            
            Logger.LogDebug("Descrição {Descricao} {Existe}", descricao, existe ? "já existe" : "não existe");
            return existe;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao verificar se existe atividade agropecuária com descrição {Descricao}", descricao);
            throw;
        }
    }

    /// <summary>
    /// Obtém atividades por tipo
    /// </summary>
    public async Task<IEnumerable<AtividadeAgropecuariaDto>> ObterPorTipoAsync(TipoAtividadeAgropecuaria tipo, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Obtendo atividades agropecuárias do tipo {Tipo}", tipo);
            
            var atividades = await _atividadeRepository.ObterPorTipoAsync(tipo, cancellationToken);
            var dtos = Mapper.Map<IEnumerable<AtividadeAgropecuariaDto>>(atividades);
            
            Logger.LogDebug("Obtidas {Quantidade} atividades do tipo {Tipo}", dtos.Count(), tipo);
            return dtos;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao obter atividades agropecuárias do tipo {Tipo}", tipo);
            throw;
        }
    }

    /// <summary>
    /// Valida os dados antes da criação
    /// </summary>
    protected override async Task ValidarCriacaoAsync(CriarAtividadeAgropecuariaDto dto, CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("Validando criação de atividade agropecuária com código {Codigo}", dto.Codigo);

        // Validar se código já existe
        if (await ExisteCodigoAsync(dto.Codigo, null, cancellationToken))
        {
            Logger.LogWarning("Tentativa de criar atividade agropecuária com código {Codigo} que já existe", dto.Codigo);
            throw new ArgumentException($"Já existe uma atividade agropecuária com o código '{dto.Codigo}'", nameof(dto.Codigo));
        }

        // Validar se descrição já existe
        if (await ExisteDescricaoAsync(dto.Descricao, null, cancellationToken))
        {
            Logger.LogWarning("Tentativa de criar atividade agropecuária com descrição {Descricao} que já existe", dto.Descricao);
            throw new ArgumentException($"Já existe uma atividade agropecuária com a descrição '{dto.Descricao}'", nameof(dto.Descricao));
        }

        // Validar tipo de atividade
        if (!Enum.IsDefined(typeof(TipoAtividadeAgropecuaria), dto.Tipo))
        {
            Logger.LogWarning("Tentativa de criar atividade agropecuária com tipo inválido {Tipo}", dto.Tipo);
            throw new ArgumentException($"Tipo de atividade agropecuária '{dto.Tipo}' é inválido", nameof(dto.Tipo));
        }

        Logger.LogDebug("Validação de criação de atividade agropecuária concluída com sucesso");
    }

    /// <summary>
    /// Valida os dados antes da atualização
    /// </summary>
    protected override async Task ValidarAtualizacaoAsync(int id, AtualizarAtividadeAgropecuariaDto dto, CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("Validando atualização de atividade agropecuária com ID {Id}", id);

        // Validar se descrição já existe (excluindo a própria atividade)
        if (await ExisteDescricaoAsync(dto.Descricao, id, cancellationToken))
        {
            Logger.LogWarning("Tentativa de atualizar atividade agropecuária com descrição {Descricao} que já existe", dto.Descricao);
            throw new ArgumentException($"Já existe uma atividade agropecuária com a descrição '{dto.Descricao}'", nameof(dto.Descricao));
        }

        // Validar tipo de atividade
        if (!Enum.IsDefined(typeof(TipoAtividadeAgropecuaria), dto.Tipo))
        {
            Logger.LogWarning("Tentativa de atualizar atividade agropecuária com tipo inválido {Tipo}", dto.Tipo);
            throw new ArgumentException($"Tipo de atividade agropecuária '{dto.Tipo}' é inválido", nameof(dto.Tipo));
        }

        Logger.LogDebug("Validação de atualização de atividade agropecuária concluída com sucesso");
    }

    /// <summary>
    /// Aplica regras de negócio específicas durante a criação
    /// </summary>
    protected override async Task AplicarRegrasNegocioCriacaoAsync(AtividadeAgropecuaria entidade, CriarAtividadeAgropecuariaDto dto, CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("Aplicando regras de negócio para criação de atividade agropecuária");

        // Atividades são criadas ativas por padrão
        entidade.Ativar();

        Logger.LogDebug("Regras de negócio aplicadas com sucesso");
        await Task.CompletedTask;
    }

    /// <summary>
    /// Aplica regras de negócio específicas durante a atualização
    /// </summary>
    protected override async Task AplicarRegrasNegocioAtualizacaoAsync(AtividadeAgropecuaria entidade, AtualizarAtividadeAgropecuariaDto dto, CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("Aplicando regras de negócio para atualização de atividade agropecuária");

        // Aplicar status ativo/inativo
        if (dto.Ativo)
            entidade.Ativar();
        else
            entidade.Desativar();

        Logger.LogDebug("Regras de negócio aplicadas com sucesso");
        await Task.CompletedTask;
    }

    /// <summary>
    /// Obtém uma atividade por código
    /// </summary>
    public async Task<AtividadeAgropecuariaDto?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Obtendo atividade agropecuária com código {Codigo}", codigo);
            
            var atividade = await _atividadeRepository.ObterPorCodigoAsync(codigo, cancellationToken);
            if (atividade == null)
            {
                Logger.LogDebug("Atividade agropecuária com código {Codigo} não encontrada", codigo);
                return null;
            }
            
            var dto = Mapper.Map<AtividadeAgropecuariaDto>(atividade);
            Logger.LogDebug("Atividade agropecuária com código {Codigo} encontrada", codigo);
            return dto;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao obter atividade agropecuária com código {Codigo}", codigo);
            throw;
        }
    }
}