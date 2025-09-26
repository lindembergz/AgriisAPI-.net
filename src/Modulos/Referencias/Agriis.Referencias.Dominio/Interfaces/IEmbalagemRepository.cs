using Agriis.Referencias.Dominio.Entidades;

namespace Agriis.Referencias.Dominio.Interfaces;

/// <summary>
/// Interface para repositório de embalagens
/// </summary>
public interface IEmbalagemRepository : IReferenciaRepository<Embalagem>
{
    /// <summary>
    /// Verifica se existe uma embalagem com o nome especificado
    /// </summary>
    /// <param name="nome">Nome a ser verificado</param>
    /// <param name="idExcluir">ID a ser excluído da verificação (para updates)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se existe</returns>
    Task<bool> ExisteNomeAsync(string nome, int? idExcluir = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém embalagens por unidade de medida
    /// </summary>
    /// <param name="unidadeMedidaId">ID da unidade de medida</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de embalagens da unidade de medida</returns>
    Task<IEnumerable<Embalagem>> ObterPorUnidadeMedidaAsync(int unidadeMedidaId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém embalagens ativas por unidade de medida
    /// </summary>
    /// <param name="unidadeMedidaId">ID da unidade de medida</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de embalagens ativas da unidade de medida</returns>
    Task<IEnumerable<Embalagem>> ObterAtivasPorUnidadeMedidaAsync(int unidadeMedidaId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém uma embalagem por nome
    /// </summary>
    /// <param name="nome">Nome da embalagem</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Embalagem encontrada ou null</returns>
    Task<Embalagem?> ObterPorNomeAsync(string nome, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Busca embalagens por nome (busca parcial)
    /// </summary>
    /// <param name="nome">Nome para busca</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de embalagens encontradas</returns>
    Task<IEnumerable<Embalagem>> BuscarPorNomeAsync(string nome, CancellationToken cancellationToken = default);
}