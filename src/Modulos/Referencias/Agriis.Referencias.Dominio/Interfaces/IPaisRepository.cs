using Agriis.Referencias.Dominio.Entidades;

namespace Agriis.Referencias.Dominio.Interfaces;

/// <summary>
/// Interface para repositório de países
/// </summary>
public interface IPaisRepository : IReferenciaRepository<Pais>
{
    /// <summary>
    /// Verifica se existe um país com o nome especificado
    /// </summary>
    /// <param name="nome">Nome a ser verificado</param>
    /// <param name="idExcluir">ID a ser excluído da verificação (para updates)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se existe</returns>
    Task<bool> ExisteNomeAsync(string nome, int? idExcluir = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se o país possui UFs cadastradas
    /// </summary>
    /// <param name="paisId">ID do país</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se possui UFs</returns>
    Task<bool> PossuiUfsAsync(int paisId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém um país por código
    /// </summary>
    /// <param name="codigo">Código do país</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>País encontrado ou null</returns>
    Task<Pais?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken = default);
}