using Agriis.Compartilhado.Dominio.Interfaces;
using Agriis.Produtos.Dominio.Entidades;
using Agriis.Produtos.Dominio.Enums;

namespace Agriis.Produtos.Dominio.Interfaces;

/// <summary>
/// Interface do repositório de categorias
/// </summary>
public interface ICategoriaRepository : IRepository<Categoria>
{
    /// <summary>
    /// Obtém uma categoria por nome
    /// </summary>
    Task<Categoria?> ObterPorNomeAsync(string nome, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém categorias ativas
    /// </summary>
    Task<IEnumerable<Categoria>> ObterAtivasAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém categorias por tipo
    /// </summary>
    Task<IEnumerable<Categoria>> ObterPorTipoAsync(CategoriaProduto tipo, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém categorias raiz (sem categoria pai)
    /// </summary>
    Task<IEnumerable<Categoria>> ObterCategoriasRaizAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém subcategorias de uma categoria pai
    /// </summary>
    Task<IEnumerable<Categoria>> ObterSubCategoriasAsync(int categoriaPaiId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém categorias com suas subcategorias
    /// </summary>
    Task<IEnumerable<Categoria>> ObterComSubCategoriasAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém categorias ordenadas por ordem de exibição
    /// </summary>
    Task<IEnumerable<Categoria>> ObterOrdenadasAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se existe categoria com o nome especificado
    /// </summary>
    Task<bool> ExisteComNomeAsync(string nome, int? idExcluir = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se a categoria tem produtos associados
    /// </summary>
    Task<bool> TemProdutosAsync(int categoriaId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se a categoria tem subcategorias
    /// </summary>
    Task<bool> TemSubCategoriasAsync(int categoriaId, CancellationToken cancellationToken = default);
}