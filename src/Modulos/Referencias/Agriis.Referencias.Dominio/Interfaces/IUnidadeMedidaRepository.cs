using Agriis.Referencias.Dominio.Entidades;
using Agriis.Referencias.Dominio.Enums;

namespace Agriis.Referencias.Dominio.Interfaces;

/// <summary>
/// Interface para repositório de unidades de medida
/// </summary>
public interface IUnidadeMedidaRepository : IReferenciaRepository<UnidadeMedida>
{
    /// <summary>
    /// Verifica se existe uma unidade com o símbolo especificado
    /// </summary>
    /// <param name="simbolo">Símbolo a ser verificado</param>
    /// <param name="idExcluir">ID a ser excluído da verificação (para updates)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se existe</returns>
    Task<bool> ExisteSimboloAsync(string simbolo, int? idExcluir = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se existe uma unidade com o nome especificado
    /// </summary>
    /// <param name="nome">Nome a ser verificado</param>
    /// <param name="idExcluir">ID a ser excluído da verificação (para updates)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se existe</returns>
    Task<bool> ExisteNomeAsync(string nome, int? idExcluir = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém unidades por tipo
    /// </summary>
    /// <param name="tipo">Tipo da unidade</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de unidades do tipo especificado</returns>
    Task<IEnumerable<UnidadeMedida>> ObterPorTipoAsync(TipoUnidadeMedida tipo, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém unidades ativas por tipo
    /// </summary>
    /// <param name="tipo">Tipo da unidade</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de unidades ativas do tipo especificado</returns>
    Task<IEnumerable<UnidadeMedida>> ObterAtivasPorTipoAsync(TipoUnidadeMedida tipo, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se a unidade possui embalagens cadastradas
    /// </summary>
    /// <param name="unidadeId">ID da unidade</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se possui embalagens</returns>
    Task<bool> PossuiEmbalagensAsync(int unidadeId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém unidade por símbolo
    /// </summary>
    /// <param name="simbolo">Símbolo da unidade</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Unidade encontrada ou null</returns>
    Task<UnidadeMedida?> ObterPorSimboloAsync(string simbolo, CancellationToken cancellationToken = default);
}