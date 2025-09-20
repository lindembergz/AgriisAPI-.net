using Agriis.Compartilhado.Dominio.Interfaces;
using Agriis.Segmentacoes.Dominio.Entidades;

namespace Agriis.Segmentacoes.Dominio.Interfaces;

/// <summary>
/// Interface para repositório de grupos de segmentação
/// </summary>
public interface IGrupoRepository : IRepository<Grupo>
{
    /// <summary>
    /// Obtém todos os grupos de uma segmentação
    /// </summary>
    /// <param name="segmentacaoId">ID da segmentação</param>
    /// <returns>Lista de grupos</returns>
    Task<IEnumerable<Grupo>> ObterPorSegmentacaoAsync(int segmentacaoId);
    
    /// <summary>
    /// Obtém grupos ativos de uma segmentação
    /// </summary>
    /// <param name="segmentacaoId">ID da segmentação</param>
    /// <returns>Lista de grupos ativos</returns>
    Task<IEnumerable<Grupo>> ObterAtivosPorSegmentacaoAsync(int segmentacaoId);
    
    /// <summary>
    /// Obtém grupo que se enquadra para uma área específica
    /// </summary>
    /// <param name="segmentacaoId">ID da segmentação</param>
    /// <param name="area">Área em hectares</param>
    /// <returns>Grupo que se enquadra ou null</returns>
    Task<Grupo?> ObterPorAreaAsync(int segmentacaoId, decimal area);
    
    /// <summary>
    /// Obtém grupo com descontos carregados
    /// </summary>
    /// <param name="id">ID do grupo</param>
    /// <returns>Grupo completo</returns>
    Task<Grupo?> ObterComDescontosAsync(int id);
    
    /// <summary>
    /// Verifica se existe sobreposição de faixas de área
    /// </summary>
    /// <param name="segmentacaoId">ID da segmentação</param>
    /// <param name="areaMinima">Área mínima</param>
    /// <param name="areaMaxima">Área máxima</param>
    /// <param name="excluirId">ID do grupo a excluir da verificação</param>
    /// <returns>True se existe sobreposição</returns>
    Task<bool> ExisteSobreposicaoAsync(int segmentacaoId, decimal areaMinima, decimal? areaMaxima, int? excluirId = null);
}