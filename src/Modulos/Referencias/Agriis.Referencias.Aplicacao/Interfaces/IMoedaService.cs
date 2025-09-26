using Agriis.Referencias.Aplicacao.DTOs;

namespace Agriis.Referencias.Aplicacao.Interfaces;

/// <summary>
/// Interface para serviço de moedas
/// </summary>
public interface IMoedaService : IReferenciaService<MoedaDto, CriarMoedaDto, AtualizarMoedaDto>
{
    /// <summary>
    /// Verifica se existe uma moeda com o código especificado
    /// </summary>
    /// <param name="codigo">Código a ser verificado</param>
    /// <param name="idExcluir">ID a ser excluído da verificação (para updates)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se existe</returns>
    Task<bool> ExisteCodigoAsync(string codigo, int? idExcluir = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se existe uma moeda com o nome especificado
    /// </summary>
    /// <param name="nome">Nome a ser verificado</param>
    /// <param name="idExcluir">ID a ser excluído da verificação (para updates)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se existe</returns>
    Task<bool> ExisteNomeAsync(string nome, int? idExcluir = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se existe uma moeda com o símbolo especificado
    /// </summary>
    /// <param name="simbolo">Símbolo a ser verificado</param>
    /// <param name="idExcluir">ID a ser excluído da verificação (para updates)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se existe</returns>
    Task<bool> ExisteSimboloAsync(string simbolo, int? idExcluir = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém uma moeda por código
    /// </summary>
    /// <param name="codigo">Código da moeda</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Moeda encontrada ou null</returns>
    Task<MoedaDto?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken = default);
}