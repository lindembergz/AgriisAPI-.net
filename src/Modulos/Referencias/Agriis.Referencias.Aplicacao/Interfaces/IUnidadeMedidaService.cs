using Agriis.Referencias.Aplicacao.DTOs;
using Agriis.Referencias.Dominio.Enums;

namespace Agriis.Referencias.Aplicacao.Interfaces;

/// <summary>
/// Interface para serviço de unidades de medida
/// </summary>
public interface IUnidadeMedidaService : IReferenciaService<UnidadeMedidaDto, CriarUnidadeMedidaDto, AtualizarUnidadeMedidaDto>
{
    /// <summary>
    /// Verifica se existe uma unidade com o símbolo especificado
    /// </summary>
    /// <param name="simbolo">Símbolo a ser verificado</param>
    /// <param name="idExcluir">ID a ser excluído da verificação (para updates)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se existe</returns>
    Task<bool> ExisteSimboloAsync(string simbolo, int? idExcluir = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se existe uma unidade com o nome especificado
    /// </summary>
    /// <param name="nome">Nome a ser verificado</param>
    /// <param name="idExcluir">ID a ser excluído da verificação (para updates)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se existe</returns>
    Task<bool> ExisteNomeAsync(string nome, int? idExcluir = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém unidades por tipo
    /// </summary>
    /// <param name="tipo">Tipo de unidade</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de unidades do tipo especificado</returns>
    Task<IEnumerable<UnidadeMedidaDto>> ObterPorTipoAsync(TipoUnidadeMedida tipo, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Calcula conversão entre unidades do mesmo tipo
    /// </summary>
    /// <param name="valor">Valor a ser convertido</param>
    /// <param name="unidadeOrigemId">ID da unidade de origem</param>
    /// <param name="unidadeDestinoId">ID da unidade de destino</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Valor convertido</returns>
    Task<decimal> CalcularConversaoAsync(decimal valor, int unidadeOrigemId, int unidadeDestinoId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Converte quantidade entre unidades do mesmo tipo (alias para CalcularConversaoAsync)
    /// </summary>
    /// <param name="quantidade">Quantidade a ser convertida</param>
    /// <param name="unidadeOrigemId">ID da unidade de origem</param>
    /// <param name="unidadeDestinoId">ID da unidade de destino</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Quantidade convertida</returns>
    Task<decimal> ConverterAsync(decimal quantidade, int unidadeOrigemId, int unidadeDestinoId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém unidade por símbolo
    /// </summary>
    /// <param name="simbolo">Símbolo da unidade</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Unidade encontrada ou null</returns>
    Task<UnidadeMedidaDto?> ObterPorSimboloAsync(string simbolo, CancellationToken cancellationToken = default);
}