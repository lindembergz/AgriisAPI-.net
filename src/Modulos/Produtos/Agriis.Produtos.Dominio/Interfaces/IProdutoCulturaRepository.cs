using Agriis.Compartilhado.Dominio.Interfaces;
using Agriis.Produtos.Dominio.Entidades;

namespace Agriis.Produtos.Dominio.Interfaces;

/// <summary>
/// Interface do repositório de relacionamento produto-cultura
/// </summary>
public interface IProdutoCulturaRepository : IRepository<ProdutoCultura>
{
    /// <summary>
    /// Obtém relacionamentos por produto
    /// </summary>
    Task<IEnumerable<ProdutoCultura>> ObterPorProdutoAsync(int produtoId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém relacionamentos por cultura
    /// </summary>
    Task<IEnumerable<ProdutoCultura>> ObterPorCulturaAsync(int culturaId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém relacionamentos ativos por produto
    /// </summary>
    Task<IEnumerable<ProdutoCultura>> ObterAtivosPorProdutoAsync(int produtoId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém relacionamentos ativos por cultura
    /// </summary>
    Task<IEnumerable<ProdutoCultura>> ObterAtivosPorCulturaAsync(int culturaId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém um relacionamento específico
    /// </summary>
    Task<ProdutoCultura?> ObterPorProdutoECulturaAsync(int produtoId, int culturaId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se existe relacionamento entre produto e cultura
    /// </summary>
    Task<bool> ExisteRelacionamentoAsync(int produtoId, int culturaId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Remove todos os relacionamentos de um produto
    /// </summary>
    Task RemoverPorProdutoAsync(int produtoId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Remove todos os relacionamentos de uma cultura
    /// </summary>
    Task RemoverPorCulturaAsync(int culturaId, CancellationToken cancellationToken = default);
}