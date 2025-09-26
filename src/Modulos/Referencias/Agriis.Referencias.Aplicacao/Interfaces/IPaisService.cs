using Agriis.Referencias.Aplicacao.DTOs;

namespace Agriis.Referencias.Aplicacao.Interfaces;

/// <summary>
/// Interface para serviço de países
/// </summary>
public interface IPaisService : IReferenciaService<PaisDto, CriarPaisDto, AtualizarPaisDto>
{
    /// <summary>
    /// Verifica se existe um país com o código especificado
    /// </summary>
    /// <param name="codigo">Código a ser verificado</param>
    /// <param name="idExcluir">ID a ser excluído da verificação (para updates)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se existe</returns>
    Task<bool> ExisteCodigoAsync(string codigo, int? idExcluir = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se existe um país com o nome especificado
    /// </summary>
    /// <param name="nome">Nome a ser verificado</param>
    /// <param name="idExcluir">ID a ser excluído da verificação (para updates)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se existe</returns>
    Task<bool> ExisteNomeAsync(string nome, int? idExcluir = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém as UFs de um país
    /// </summary>
    /// <param name="paisId">ID do país</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de UFs do país</returns>
    Task<IEnumerable<UfDto>> ObterUfsPorPaisAsync(int paisId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém um país por código
    /// </summary>
    /// <param name="codigo">Código do país</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>País encontrado ou null</returns>
    Task<PaisDto?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken = default);
}