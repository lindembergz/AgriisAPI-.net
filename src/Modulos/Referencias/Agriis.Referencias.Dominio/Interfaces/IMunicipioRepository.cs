using Agriis.Referencias.Dominio.Entidades;

namespace Agriis.Referencias.Dominio.Interfaces;

/// <summary>
/// Interface para repositório de municípios
/// </summary>
public interface IMunicipioRepository : IReferenciaRepository<Municipio>
{
    /// <summary>
    /// Verifica se existe um município com o nome especificado
    /// </summary>
    /// <param name="nome">Nome a ser verificado</param>
    /// <param name="idExcluir">ID a ser excluído da verificação (para updates)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se existe</returns>
    Task<bool> ExisteNomeAsync(string nome, int? idExcluir = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se existe um município com o código IBGE especificado
    /// </summary>
    /// <param name="codigoIbge">Código IBGE a ser verificado</param>
    /// <param name="idExcluir">ID a ser excluído da verificação (para updates)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se existe</returns>
    Task<bool> ExisteCodigoIbgeAsync(int codigoIbge, int? idExcluir = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se existe um município com o código IBGE especificado (string - compatibilidade)
    /// </summary>
    /// <param name="codigoIbge">Código IBGE a ser verificado como string</param>
    /// <param name="idExcluir">ID a ser excluído da verificação (para updates)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se existe</returns>
    Task<bool> ExisteCodigoIbgeAsync(string codigoIbge, int? idExcluir = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se existe um município com o nome especificado na UF
    /// </summary>
    /// <param name="nome">Nome a ser verificado</param>
    /// <param name="ufId">ID da UF</param>
    /// <param name="idExcluir">ID a ser excluído da verificação (para updates)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se existe</returns>
    Task<bool> ExisteNomeNaUfAsync(string nome, int ufId, int? idExcluir = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém municípios por UF
    /// </summary>
    /// <param name="ufId">ID da UF</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de municípios da UF</returns>
    Task<IEnumerable<Municipio>> ObterPorUfAsync(int ufId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém municípios ativos por UF
    /// </summary>
    /// <param name="ufId">ID da UF</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de municípios ativos da UF</returns>
    Task<IEnumerable<Municipio>> ObterAtivosPorUfAsync(int ufId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Busca municípios por nome (busca parcial)
    /// </summary>
    /// <param name="nome">Nome ou parte do nome</param>
    /// <param name="ufId">ID da UF (opcional para filtrar)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de municípios que contêm o nome</returns>
    Task<IEnumerable<Municipio>> BuscarPorNomeAsync(string nome, int? ufId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém um município por código IBGE
    /// </summary>
    /// <param name="codigoIbge">Código IBGE do município</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Município encontrado ou null</returns>
    Task<Municipio?> ObterPorCodigoIbgeAsync(int codigoIbge, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém um município por código IBGE (string - compatibilidade)
    /// </summary>
    /// <param name="codigoIbge">Código IBGE do município como string</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Município encontrado ou null</returns>
    Task<Municipio?> ObterPorCodigoIbgeAsync(string codigoIbge, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Busca municípios por código IBGE (busca parcial por string)
    /// </summary>
    /// <param name="codigoIbgeParcial">Parte do código IBGE para busca</param>
    /// <param name="ufId">ID da UF (opcional para filtrar)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de municípios que contêm o código</returns>
    Task<IEnumerable<Municipio>> BuscarPorCodigoIbgeAsync(string codigoIbgeParcial, int? ufId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém municípios por região (baseado no prefixo do código IBGE)
    /// </summary>
    /// <param name="prefixoCodigoIbge">Prefixo do código IBGE da região</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de municípios da região</returns>
    Task<IEnumerable<Municipio>> ObterPorRegiaoAsync(string prefixoCodigoIbge, CancellationToken cancellationToken = default);
}