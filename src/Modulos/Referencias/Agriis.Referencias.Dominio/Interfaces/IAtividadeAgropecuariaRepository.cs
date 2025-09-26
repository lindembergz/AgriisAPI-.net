using Agriis.Referencias.Dominio.Entidades;
using Agriis.Referencias.Dominio.Enums;

namespace Agriis.Referencias.Dominio.Interfaces;

/// <summary>
/// Interface para repositório de atividades agropecuárias
/// </summary>
public interface IAtividadeAgropecuariaRepository : IReferenciaRepository<AtividadeAgropecuaria>
{
    /// <summary>
    /// Verifica se existe uma atividade com a descrição especificada
    /// </summary>
    /// <param name="descricao">Descrição a ser verificada</param>
    /// <param name="idExcluir">ID a ser excluído da verificação (para updates)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se existe</returns>
    Task<bool> ExisteDescricaoAsync(string descricao, int? idExcluir = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém atividades por tipo
    /// </summary>
    /// <param name="tipo">Tipo da atividade</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de atividades do tipo especificado</returns>
    Task<IEnumerable<AtividadeAgropecuaria>> ObterPorTipoAsync(TipoAtividadeAgropecuaria tipo, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém atividades ativas por tipo
    /// </summary>
    /// <param name="tipo">Tipo da atividade</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de atividades ativas do tipo especificado</returns>
    Task<IEnumerable<AtividadeAgropecuaria>> ObterAtivasPorTipoAsync(TipoAtividadeAgropecuaria tipo, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém uma atividade agropecuária por código
    /// </summary>
    /// <param name="codigo">Código da atividade</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Atividade encontrada ou null</returns>
    Task<AtividadeAgropecuaria?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken = default);
}