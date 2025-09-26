using Agriis.Referencias.Aplicacao.DTOs;

namespace Agriis.Referencias.Aplicacao.Interfaces;

/// <summary>
/// Interface para serviço de municípios
/// </summary>
public interface IMunicipioService : IReferenciaService<MunicipioDto, CriarMunicipioDto, AtualizarMunicipioDto>
{
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
    /// Verifica se existe um município com o código IBGE especificado
    /// </summary>
    /// <param name="codigoIbge">Código IBGE a ser verificado</param>
    /// <param name="idExcluir">ID a ser excluído da verificação (para updates)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se existe</returns>
    Task<bool> ExisteCodigoIbgeAsync(string codigoIbge, int? idExcluir = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém os municípios de uma UF específica
    /// </summary>
    /// <param name="ufId">ID da UF</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de municípios da UF</returns>
    Task<IEnumerable<MunicipioDto>> ObterPorUfAsync(int ufId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Busca municípios por nome
    /// </summary>
    /// <param name="nome">Nome ou parte do nome para busca</param>
    /// <param name="ufId">ID da UF (opcional)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de municípios encontrados</returns>
    Task<IEnumerable<MunicipioDto>> BuscarPorNomeAsync(string nome, int? ufId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém um município por código IBGE
    /// </summary>
    /// <param name="codigoIbge">Código IBGE do município</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Município encontrado ou null</returns>
    Task<MunicipioDto?> ObterPorCodigoIbgeAsync(string codigoIbge, CancellationToken cancellationToken = default);
}