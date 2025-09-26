using Agriis.Referencias.Aplicacao.DTOs;

namespace Agriis.Referencias.Aplicacao.Interfaces;

/// <summary>
/// Interface para serviço de UFs
/// </summary>
public interface IUfService : IReferenciaService<UfDto, CriarUfDto, AtualizarUfDto>
{
    /// <summary>
    /// Verifica se existe uma UF com o código especificado
    /// </summary>
    /// <param name="codigo">Código a ser verificado</param>
    /// <param name="idExcluir">ID a ser excluído da verificação (para updates)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se existe</returns>
    Task<bool> ExisteCodigoAsync(string codigo, int? idExcluir = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se existe uma UF com o nome especificado
    /// </summary>
    /// <param name="nome">Nome a ser verificado</param>
    /// <param name="idExcluir">ID a ser excluído da verificação (para updates)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se existe</returns>
    Task<bool> ExisteNomeAsync(string nome, int? idExcluir = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém as UFs de um país específico
    /// </summary>
    /// <param name="paisId">ID do país</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de UFs do país</returns>
    Task<IEnumerable<UfDto>> ObterPorPaisAsync(int paisId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém os municípios de uma UF
    /// </summary>
    /// <param name="ufId">ID da UF</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de municípios da UF</returns>
    Task<IEnumerable<MunicipioDto>> ObterMunicipiosPorUfAsync(int ufId, CancellationToken cancellationToken = default);
}