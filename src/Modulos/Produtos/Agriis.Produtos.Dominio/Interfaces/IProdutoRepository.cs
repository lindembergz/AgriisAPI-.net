using Agriis.Compartilhado.Dominio.Interfaces;
using Agriis.Produtos.Dominio.Entidades;
using Agriis.Produtos.Dominio.Enums;

namespace Agriis.Produtos.Dominio.Interfaces;

/// <summary>
/// Interface do repositório de produtos
/// </summary>
public interface IProdutoRepository : IRepository<Produto>
{
    /// <summary>
    /// Obtém um produto por código
    /// </summary>
    Task<Produto?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém produtos por fornecedor
    /// </summary>
    Task<IEnumerable<Produto>> ObterPorFornecedorAsync(int fornecedorId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém produtos por categoria
    /// </summary>
    Task<IEnumerable<Produto>> ObterPorCategoriaAsync(int categoriaId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém produtos compatíveis com uma cultura
    /// </summary>
    Task<IEnumerable<Produto>> ObterPorCulturaAsync(int culturaId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém produtos por tipo (Fabricante/Revendedor)
    /// </summary>
    Task<IEnumerable<Produto>> ObterPorTipoAsync(TipoProduto tipo, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém produtos ativos
    /// </summary>
    Task<IEnumerable<Produto>> ObterAtivosAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém produtos fabricantes (sem produto pai)
    /// </summary>
    Task<IEnumerable<Produto>> ObterFabricantesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém produtos filhos de um produto pai
    /// </summary>
    Task<IEnumerable<Produto>> ObterProdutosFilhosAsync(int produtoPaiId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém produtos restritos
    /// </summary>
    Task<IEnumerable<Produto>> ObterRestritosAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Busca produtos por nome ou código
    /// </summary>
    Task<IEnumerable<Produto>> BuscarPorNomeOuCodigoAsync(string termo, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se existe produto com o código especificado
    /// </summary>
    Task<bool> ExisteComCodigoAsync(string codigo, int? idExcluir = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém produtos com suas culturas
    /// </summary>
    Task<IEnumerable<Produto>> ObterComCulturasAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém produtos por múltiplas culturas
    /// </summary>
    Task<IEnumerable<Produto>> ObterPorCulturasAsync(IEnumerable<int> culturasIds, CancellationToken cancellationToken = default);
}