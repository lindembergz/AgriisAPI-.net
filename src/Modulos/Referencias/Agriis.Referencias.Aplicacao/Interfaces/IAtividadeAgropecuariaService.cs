using Agriis.Referencias.Aplicacao.DTOs;
using Agriis.Referencias.Dominio.Enums;

namespace Agriis.Referencias.Aplicacao.Interfaces;

/// <summary>
/// Interface para serviço de atividades agropecuárias
/// </summary>
public interface IAtividadeAgropecuariaService : IReferenciaService<AtividadeAgropecuariaDto, CriarAtividadeAgropecuariaDto, AtualizarAtividadeAgropecuariaDto>
{
    /// <summary>
    /// Verifica se existe uma atividade com o código especificado
    /// </summary>
    /// <param name="codigo">Código a ser verificado</param>
    /// <param name="idExcluir">ID a ser excluído da verificação (para updates)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se existe</returns>
    Task<bool> ExisteCodigoAsync(string codigo, int? idExcluir = null, CancellationToken cancellationToken = default);
    
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
    /// <param name="tipo">Tipo de atividade</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de atividades do tipo especificado</returns>
    Task<IEnumerable<AtividadeAgropecuariaDto>> ObterPorTipoAsync(TipoAtividadeAgropecuaria tipo, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém uma atividade por código
    /// </summary>
    /// <param name="codigo">Código da atividade</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Atividade encontrada ou null</returns>
    Task<AtividadeAgropecuariaDto?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken = default);
}