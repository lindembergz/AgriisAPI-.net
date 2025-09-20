using Agriis.Produtos.Aplicacao.DTOs;
using Agriis.Produtos.Dominio.Enums;

namespace Agriis.Produtos.Aplicacao.Interfaces;

/// <summary>
/// Interface do serviço de produtos
/// </summary>
public interface IProdutoService
{
    /// <summary>
    /// Obtém um produto por ID
    /// </summary>
    Task<ProdutoDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém um produto por código
    /// </summary>
    Task<ProdutoDto?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém todos os produtos
    /// </summary>
    Task<IEnumerable<ProdutoDto>> ObterTodosAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém produtos ativos
    /// </summary>
    Task<IEnumerable<ProdutoDto>> ObterAtivosAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém produtos por fornecedor
    /// </summary>
    Task<IEnumerable<ProdutoDto>> ObterPorFornecedorAsync(int fornecedorId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém produtos por categoria
    /// </summary>
    Task<IEnumerable<ProdutoDto>> ObterPorCategoriaAsync(int categoriaId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém produtos compatíveis com uma cultura
    /// </summary>
    Task<IEnumerable<ProdutoDto>> ObterPorCulturaAsync(int culturaId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém produtos por tipo
    /// </summary>
    Task<IEnumerable<ProdutoDto>> ObterPorTipoAsync(TipoProduto tipo, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém produtos fabricantes
    /// </summary>
    Task<IEnumerable<ProdutoDto>> ObterFabricantesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém produtos filhos de um produto pai
    /// </summary>
    Task<IEnumerable<ProdutoDto>> ObterProdutosFilhosAsync(int produtoPaiId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém produtos restritos
    /// </summary>
    Task<IEnumerable<ProdutoDto>> ObterRestritosAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Busca produtos por nome ou código
    /// </summary>
    Task<IEnumerable<ProdutoDto>> BuscarAsync(string termo, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém produtos compatíveis com múltiplas culturas
    /// </summary>
    Task<IEnumerable<ProdutoDto>> ObterPorCulturasAsync(IEnumerable<int> culturasIds, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cria um novo produto
    /// </summary>
    Task<ProdutoDto> CriarAsync(CriarProdutoDto dto, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Atualiza um produto existente
    /// </summary>
    Task<ProdutoDto> AtualizarAsync(int id, AtualizarProdutoDto dto, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Ativa um produto
    /// </summary>
    Task AtivarAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Inativa um produto
    /// </summary>
    Task InativarAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Descontinua um produto
    /// </summary>
    Task DescontinuarAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Remove um produto
    /// </summary>
    Task RemoverAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Adiciona uma cultura ao produto
    /// </summary>
    Task AdicionarCulturaAsync(int produtoId, int culturaId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Remove uma cultura do produto
    /// </summary>
    Task RemoverCulturaAsync(int produtoId, int culturaId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se existe produto com o código especificado
    /// </summary>
    Task<bool> ExisteComCodigoAsync(string codigo, int? idExcluir = null, CancellationToken cancellationToken = default);
}