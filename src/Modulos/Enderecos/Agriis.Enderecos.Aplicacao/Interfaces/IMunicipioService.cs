using Agriis.Enderecos.Aplicacao.DTOs;

namespace Agriis.Enderecos.Aplicacao.Interfaces;

/// <summary>
/// Interface para serviços de municípios
/// </summary>
public interface IMunicipioService
{
    /// <summary>
    /// Obtém um município por ID
    /// </summary>
    /// <param name="id">ID do município</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Município encontrado</returns>
    Task<MunicipioDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém todos os municípios de um estado
    /// </summary>
    /// <param name="estadoId">ID do estado</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de municípios</returns>
    Task<IEnumerable<MunicipioDto>> ObterPorEstadoAsync(int estadoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca municípios por nome
    /// </summary>
    /// <param name="nome">Nome ou parte do nome</param>
    /// <param name="estadoId">ID do estado (opcional)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de municípios encontrados</returns>
    Task<IEnumerable<MunicipioDto>> BuscarPorNomeAsync(string nome, int? estadoId = null, CancellationToken cancellationToken = default);
}