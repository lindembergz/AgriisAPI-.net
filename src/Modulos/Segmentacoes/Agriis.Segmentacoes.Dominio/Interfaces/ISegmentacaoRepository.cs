using Agriis.Compartilhado.Dominio.Interfaces;
using Agriis.Segmentacoes.Dominio.Entidades;

namespace Agriis.Segmentacoes.Dominio.Interfaces;

/// <summary>
/// Interface para repositório de segmentações
/// </summary>
public interface ISegmentacaoRepository : IRepository<Segmentacao>
{
    /// <summary>
    /// Obtém todas as segmentações de um fornecedor
    /// </summary>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <returns>Lista de segmentações</returns>
    Task<IEnumerable<Segmentacao>> ObterPorFornecedorAsync(int fornecedorId);
    
    /// <summary>
    /// Obtém a segmentação padrão de um fornecedor
    /// </summary>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <returns>Segmentação padrão ou null</returns>
    Task<Segmentacao?> ObterPadraoAsync(int fornecedorId);
    
    /// <summary>
    /// Obtém segmentações ativas de um fornecedor
    /// </summary>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <returns>Lista de segmentações ativas</returns>
    Task<IEnumerable<Segmentacao>> ObterAtivasPorFornecedorAsync(int fornecedorId);
    
    /// <summary>
    /// Verifica se existe uma segmentação padrão para o fornecedor
    /// </summary>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <param name="excluirId">ID da segmentação a excluir da verificação</param>
    /// <returns>True se existe segmentação padrão</returns>
    Task<bool> ExistePadraoAsync(int fornecedorId, int? excluirId = null);
    
    /// <summary>
    /// Obtém segmentação com grupos e descontos carregados
    /// </summary>
    /// <param name="id">ID da segmentação</param>
    /// <returns>Segmentação completa</returns>
    Task<Segmentacao?> ObterCompletaAsync(int id);
}