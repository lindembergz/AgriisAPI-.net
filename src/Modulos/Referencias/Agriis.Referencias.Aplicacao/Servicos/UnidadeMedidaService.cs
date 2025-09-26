using AutoMapper;
using Agriis.Referencias.Aplicacao.DTOs;
using Agriis.Referencias.Aplicacao.Interfaces;
using Agriis.Referencias.Dominio.Entidades;
using Agriis.Referencias.Dominio.Enums;
using Agriis.Referencias.Dominio.Interfaces;
using Microsoft.Extensions.Logging;

namespace Agriis.Referencias.Aplicacao.Servicos;

/// <summary>
/// Serviço para gerenciamento de unidades de medida
/// </summary>
public class UnidadeMedidaService : ReferenciaServiceBase<UnidadeMedida, UnidadeMedidaDto, CriarUnidadeMedidaDto, AtualizarUnidadeMedidaDto>, IUnidadeMedidaService
{
    private readonly IUnidadeMedidaRepository _unidadeRepository;

    public UnidadeMedidaService(
        IUnidadeMedidaRepository repository,
        IMapper mapper,
        ILogger<UnidadeMedidaService> logger) : base(repository, mapper, logger)
    {
        _unidadeRepository = repository;
    }

    /// <summary>
    /// Verifica se existe uma unidade com o símbolo especificado
    /// </summary>
    public async Task<bool> ExisteSimboloAsync(string simbolo, int? idExcluir = null, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Verificando se existe unidade de medida com símbolo {Simbolo}", simbolo);
            
            var existe = await _unidadeRepository.ExisteSimboloAsync(simbolo, idExcluir, cancellationToken);
            
            Logger.LogDebug("Símbolo {Simbolo} {Existe}", simbolo, existe ? "já existe" : "não existe");
            return existe;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao verificar se existe unidade de medida com símbolo {Simbolo}", simbolo);
            throw;
        }
    }

    /// <summary>
    /// Verifica se existe uma unidade com o nome especificado
    /// </summary>
    public async Task<bool> ExisteNomeAsync(string nome, int? idExcluir = null, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Verificando se existe unidade de medida com nome {Nome}", nome);
            
            var existe = await _unidadeRepository.ExisteNomeAsync(nome, idExcluir, cancellationToken);
            
            Logger.LogDebug("Nome {Nome} {Existe}", nome, existe ? "já existe" : "não existe");
            return existe;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao verificar se existe unidade de medida com nome {Nome}", nome);
            throw;
        }
    }

    /// <summary>
    /// Obtém unidades por tipo
    /// </summary>
    public async Task<IEnumerable<UnidadeMedidaDto>> ObterPorTipoAsync(TipoUnidadeMedida tipo, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Obtendo unidades de medida do tipo {Tipo}", tipo);
            
            var unidades = await _unidadeRepository.ObterPorTipoAsync(tipo, cancellationToken);
            var dtos = Mapper.Map<IEnumerable<UnidadeMedidaDto>>(unidades);
            
            Logger.LogDebug("Obtidas {Quantidade} unidades do tipo {Tipo}", dtos.Count(), tipo);
            return dtos;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao obter unidades de medida do tipo {Tipo}", tipo);
            throw;
        }
    }

    /// <summary>
    /// Calcula conversão entre unidades do mesmo tipo
    /// </summary>
    public async Task<decimal> CalcularConversaoAsync(decimal valor, int unidadeOrigemId, int unidadeDestinoId, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Calculando conversão de {Valor} da unidade {OrigemId} para {DestinoId}", valor, unidadeOrigemId, unidadeDestinoId);
            
            // Se são a mesma unidade, retorna o valor original
            if (unidadeOrigemId == unidadeDestinoId)
            {
                Logger.LogDebug("Unidades são iguais, retornando valor original");
                return valor;
            }

            // Obter unidades
            var unidadeOrigem = await _unidadeRepository.ObterPorIdAsync(unidadeOrigemId, cancellationToken);
            var unidadeDestino = await _unidadeRepository.ObterPorIdAsync(unidadeDestinoId, cancellationToken);

            if (unidadeOrigem == null)
            {
                Logger.LogWarning("Unidade de origem {OrigemId} não encontrada", unidadeOrigemId);
                throw new ArgumentException($"Unidade de origem com ID {unidadeOrigemId} não encontrada", nameof(unidadeOrigemId));
            }

            if (unidadeDestino == null)
            {
                Logger.LogWarning("Unidade de destino {DestinoId} não encontrada", unidadeDestinoId);
                throw new ArgumentException($"Unidade de destino com ID {unidadeDestinoId} não encontrada", nameof(unidadeDestinoId));
            }

            // Verificar se são do mesmo tipo
            if (unidadeOrigem.Tipo != unidadeDestino.Tipo)
            {
                Logger.LogWarning("Tentativa de conversão entre tipos diferentes: {TipoOrigem} -> {TipoDestino}", unidadeOrigem.Tipo, unidadeDestino.Tipo);
                throw new ArgumentException($"Não é possível converter entre tipos diferentes: {unidadeOrigem.Tipo} -> {unidadeDestino.Tipo}");
            }

            // Calcular conversão usando fatores de conversão
            var fatorOrigem = unidadeOrigem.FatorConversao ?? 1m;
            var fatorDestino = unidadeDestino.FatorConversao ?? 1m;
            
            var valorConvertido = valor * fatorOrigem / fatorDestino;
            
            Logger.LogDebug("Conversão calculada: {Valor} {OrigemSimbolo} = {ValorConvertido} {DestinoSimbolo}", 
                valor, unidadeOrigem.Simbolo, valorConvertido, unidadeDestino.Simbolo);
            
            return valorConvertido;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao calcular conversão entre unidades {OrigemId} -> {DestinoId}", unidadeOrigemId, unidadeDestinoId);
            throw;
        }
    }

    /// <summary>
    /// Valida os dados antes da criação
    /// </summary>
    protected override async Task ValidarCriacaoAsync(CriarUnidadeMedidaDto dto, CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("Validando criação de unidade de medida com símbolo {Simbolo}", dto.Simbolo);

        // Validar se símbolo já existe
        if (await ExisteSimboloAsync(dto.Simbolo, null, cancellationToken))
        {
            Logger.LogWarning("Tentativa de criar unidade de medida com símbolo {Simbolo} que já existe", dto.Simbolo);
            throw new ArgumentException($"Já existe uma unidade de medida com o símbolo '{dto.Simbolo}'", nameof(dto.Simbolo));
        }

        // Validar se nome já existe
        if (await ExisteNomeAsync(dto.Nome, null, cancellationToken))
        {
            Logger.LogWarning("Tentativa de criar unidade de medida com nome {Nome} que já existe", dto.Nome);
            throw new ArgumentException($"Já existe uma unidade de medida com o nome '{dto.Nome}'", nameof(dto.Nome));
        }

        // Validar tipo de unidade
        if (!Enum.IsDefined(typeof(TipoUnidadeMedida), dto.Tipo))
        {
            Logger.LogWarning("Tentativa de criar unidade de medida com tipo inválido {Tipo}", dto.Tipo);
            throw new ArgumentException($"Tipo de unidade de medida '{dto.Tipo}' é inválido", nameof(dto.Tipo));
        }

        // Validar fator de conversão
        if (dto.FatorConversao.HasValue && dto.FatorConversao.Value <= 0)
        {
            Logger.LogWarning("Tentativa de criar unidade de medida com fator de conversão inválido {Fator}", dto.FatorConversao);
            throw new ArgumentException("Fator de conversão deve ser maior que zero", nameof(dto.FatorConversao));
        }

        Logger.LogDebug("Validação de criação de unidade de medida concluída com sucesso");
    }

    /// <summary>
    /// Valida os dados antes da atualização
    /// </summary>
    protected override async Task ValidarAtualizacaoAsync(int id, AtualizarUnidadeMedidaDto dto, CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("Validando atualização de unidade de medida com ID {Id}", id);

        // Validar se nome já existe (excluindo a própria unidade)
        if (await ExisteNomeAsync(dto.Nome, id, cancellationToken))
        {
            Logger.LogWarning("Tentativa de atualizar unidade de medida com nome {Nome} que já existe", dto.Nome);
            throw new ArgumentException($"Já existe uma unidade de medida com o nome '{dto.Nome}'", nameof(dto.Nome));
        }

        // Validar fator de conversão
        if (dto.FatorConversao.HasValue && dto.FatorConversao.Value <= 0)
        {
            Logger.LogWarning("Tentativa de atualizar unidade de medida com fator de conversão inválido {Fator}", dto.FatorConversao);
            throw new ArgumentException("Fator de conversão deve ser maior que zero", nameof(dto.FatorConversao));
        }

        Logger.LogDebug("Validação de atualização de unidade de medida concluída com sucesso");
    }

    /// <summary>
    /// Aplica regras de negócio específicas durante a criação
    /// </summary>
    protected override async Task AplicarRegrasNegocioCriacaoAsync(UnidadeMedida entidade, CriarUnidadeMedidaDto dto, CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("Aplicando regras de negócio para criação de unidade de medida");

        // Unidades são criadas ativas por padrão
        entidade.Ativar();

        // Se não foi especificado fator de conversão, usar 1 como padrão
        if (!dto.FatorConversao.HasValue)
        {
            entidade.AtualizarInformacoes(entidade.Nome, 1m);
        }

        Logger.LogDebug("Regras de negócio aplicadas com sucesso");
        await Task.CompletedTask;
    }

    /// <summary>
    /// Aplica regras de negócio específicas durante a atualização
    /// </summary>
    protected override async Task AplicarRegrasNegocioAtualizacaoAsync(UnidadeMedida entidade, AtualizarUnidadeMedidaDto dto, CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("Aplicando regras de negócio para atualização de unidade de medida");

        // Aplicar status ativo/inativo
        if (dto.Ativo)
            entidade.Ativar();
        else
            entidade.Desativar();

        // Atualizar fator de conversão se especificado
        if (dto.FatorConversao.HasValue)
        {
            entidade.AtualizarInformacoes(entidade.Nome, dto.FatorConversao.Value);
        }

        Logger.LogDebug("Regras de negócio aplicadas com sucesso");
        await Task.CompletedTask;
    }

    /// <summary>
    /// Converte quantidade entre unidades (alias para CalcularConversaoAsync)
    /// </summary>
    public async Task<decimal> ConverterAsync(decimal quantidade, int unidadeOrigemId, int unidadeDestinoId, CancellationToken cancellationToken = default)
    {
        return await CalcularConversaoAsync(quantidade, unidadeOrigemId, unidadeDestinoId, cancellationToken);
    }

    /// <summary>
    /// Obtém unidade por símbolo
    /// </summary>
    public async Task<UnidadeMedidaDto?> ObterPorSimboloAsync(string simbolo, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Obtendo unidade de medida com símbolo {Simbolo}", simbolo);
            
            var unidade = await _unidadeRepository.ObterPorSimboloAsync(simbolo, cancellationToken);
            
            if (unidade == null)
            {
                Logger.LogDebug("Unidade de medida com símbolo {Simbolo} não encontrada", simbolo);
                return null;
            }
            
            var dto = Mapper.Map<UnidadeMedidaDto>(unidade);
            
            Logger.LogDebug("Unidade de medida com símbolo {Simbolo} encontrada: {Nome}", simbolo, dto.Nome);
            return dto;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao obter unidade de medida com símbolo {Simbolo}", simbolo);
            throw;
        }
    }
}