using Agriis.Compartilhado.Dominio.Interfaces;
using Agriis.Segmentacoes.Dominio.Entidades;

namespace Agriis.Segmentacoes.Dominio.Interfaces;

/// <summary>
/// Interface para repositório de grupos de segmentação (descontos por categoria)
/// </summary>
public interface IGrupoSegmentacaoRepository : IRepository<GrupoSegmentacao>
{
    /// <summary>
    /// Obtém todos os descontos de um grupo
    /// </summary>
    /// <param name="grupoId">ID do grupo</param>
    /// <returns>Lista de descontos</returns>
    Task<IEnumerable<GrupoSegmentacao>> ObterPorGrupoAsync(int grupoId);
    
    /// <summary>
    /// Obtém descontos ativos de um grupo
    /// </summary>
    /// <param name="grupoId">ID do grupo</param>
    /// <returns>Lista de descontos ativos</returns>
    Task<IEnumerable<GrupoSegmentacao>> ObterAtivosPorGrupoAsync(int grupoId);
    
    /// <summary>
    /// Obtém desconto específico por grupo e categoria
    /// </summary>
    /// <param name="grupoId">ID do grupo</param>
    /// <param name="categoriaId">ID da categoria</param>
    /// <returns>Desconto ou null</returns>
    Task<GrupoSegmentacao?> ObterPorGrupoECategoriaAsync(int grupoId, int categoriaId);
    
    /// <summary>
    /// Obtém todos os descontos de uma categoria
    /// </summary>
    /// <param name="categoriaId">ID da categoria</param>
    /// <returns>Lista de descontos</returns>
    Task<IEnumerable<GrupoSegmentacao>> ObterPorCategoriaAsync(int categoriaId);
    
    /// <summary>
    /// Verifica se já existe desconto para a combinação grupo/categoria
    /// </summary>
    /// <param name="grupoId">ID do grupo</param>
    /// <param name="categoriaId">ID da categoria</param>
    /// <param name="excluirId">ID do desconto a excluir da verificação</param>
    /// <returns>True se já existe</returns>
    Task<bool> ExisteAsync(int grupoId, int categoriaId, int? excluirId = null);
}