using Agriis.Produtos.Aplicacao.DTOs;
using Agriis.Produtos.Dominio.Enums;

namespace Agriis.Produtos.Aplicacao.Interfaces;

/// <summary>
/// Interface do serviço de categorias
/// </summary>
public interface ICategoriaService
{
    /// <summary>
    /// Obtém uma categoria por ID
    /// </summary>
    Task<CategoriaDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém uma categoria por nome
    /// </summary>
    Task<CategoriaDto?> ObterPorNomeAsync(string nome, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém todas as categorias
    /// </summary>
    Task<IEnumerable<CategoriaDto>> ObterTodosAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém categorias ativas
    /// </summary>
    Task<IEnumerable<CategoriaDto>> ObterAtivasAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém categorias por tipo
    /// </summary>
    Task<IEnumerable<CategoriaDto>> ObterPorTipoAsync(CategoriaProduto tipo, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém categorias raiz (sem categoria pai)
    /// </summary>
    Task<IEnumerable<CategoriaDto>> ObterCategoriasRaizAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém subcategorias de uma categoria pai
    /// </summary>
    Task<IEnumerable<CategoriaDto>> ObterSubCategoriasAsync(int categoriaPaiId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém categorias com hierarquia completa
    /// </summary>
    Task<IEnumerable<CategoriaDto>> ObterComHierarquiaAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém categorias ordenadas
    /// </summary>
    Task<IEnumerable<CategoriaDto>> ObterOrdenadasAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cria uma nova categoria
    /// </summary>
    Task<CategoriaDto> CriarAsync(CriarCategoriaDto dto, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Atualiza uma categoria existente
    /// </summary>
    Task<CategoriaDto> AtualizarAsync(int id, AtualizarCategoriaDto dto, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Ativa uma categoria
    /// </summary>
    Task AtivarAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Desativa uma categoria
    /// </summary>
    Task DesativarAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Remove uma categoria
    /// </summary>
    Task RemoverAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se existe categoria com o nome especificado
    /// </summary>
    Task<bool> ExisteComNomeAsync(string nome, int? idExcluir = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se a categoria pode ser removida
    /// </summary>
    Task<bool> PodeRemoverAsync(int id, CancellationToken cancellationToken = default);
}