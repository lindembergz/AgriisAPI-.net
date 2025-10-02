using Agriis.Enderecos.Aplicacao.DTOs;

namespace Agriis.Enderecos.Aplicacao.Interfaces;

/// <summary>
/// Interface para serviços de países
/// </summary>
public interface IPaisService
{
    /// <summary>
    /// Obtém um país por ID
    /// </summary>
    /// <param name="id">ID do país</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>País encontrado</returns>
    Task<PaisDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém um país por código
    /// </summary>
    /// <param name="codigo">Código do país</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>País encontrado</returns>
    Task<PaisDto?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém todos os países ativos
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de países ativos</returns>
    Task<IEnumerable<PaisDto>> ObterAtivosAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca países por nome
    /// </summary>
    /// <param name="nome">Nome ou parte do nome</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de países encontrados</returns>
    Task<IEnumerable<PaisDto>> BuscarPorNomeAsync(string nome, CancellationToken cancellationToken = default);
}