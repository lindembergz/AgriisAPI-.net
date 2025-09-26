using Agriis.Referencias.Dominio.Entidades;

namespace Agriis.Referencias.Dominio.Interfaces;

/// <summary>
/// Interface para repositório de UFs
/// </summary>
public interface IUfRepository : IReferenciaRepository<Uf>
{
    /// <summary>
    /// Verifica se existe uma UF com o nome especificado
    /// </summary>
    /// <param name="nome">Nome a ser verificado</param>
    /// <param name="idExcluir">ID a ser excluído da verificação (para updates)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se existe</returns>
    Task<bool> ExisteNomeAsync(string nome, int? idExcluir = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém UFs por país
    /// </summary>
    /// <param name="paisId">ID do país</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de UFs do país</returns>
    Task<IEnumerable<Uf>> ObterPorPaisAsync(int paisId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém UFs ativas por país
    /// </summary>
    /// <param name="paisId">ID do país</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de UFs ativas do país</returns>
    Task<IEnumerable<Uf>> ObterAtivasPorPaisAsync(int paisId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se a UF possui municípios cadastrados
    /// </summary>
    /// <param name="ufId">ID da UF</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se possui municípios</returns>
    Task<bool> PossuiMunicipiosAsync(int ufId, CancellationToken cancellationToken = default);
}