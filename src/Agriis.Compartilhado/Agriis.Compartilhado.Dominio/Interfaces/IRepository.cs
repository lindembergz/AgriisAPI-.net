using Agriis.Compartilhado.Dominio.Entidades;
using System.Linq.Expressions;

namespace Agriis.Compartilhado.Dominio.Interfaces;

/// <summary>
/// Interface base para repositórios genéricos
/// </summary>
/// <typeparam name="T">Tipo da entidade</typeparam>
public interface IRepository<T> where T : EntidadeBase
{
    /// <summary>
    /// Obtém uma entidade por seu ID
    /// </summary>
    /// <param name="id">ID da entidade</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Entidade encontrada ou null</returns>
    Task<T?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém todas as entidades
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de entidades</returns>
    Task<IEnumerable<T>> ObterTodosAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém entidades que atendem a uma condição
    /// </summary>
    /// <param name="predicate">Condição de filtro</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de entidades filtradas</returns>
    Task<IEnumerable<T>> ObterPorCondicaoAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém uma única entidade que atende a uma condição
    /// </summary>
    /// <param name="predicate">Condição de filtro</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Entidade encontrada ou null</returns>
    Task<T?> ObterPrimeiroAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Adiciona uma nova entidade
    /// </summary>
    /// <param name="entidade">Entidade a ser adicionada</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Entidade adicionada</returns>
    Task<T> AdicionarAsync(T entidade, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Adiciona múltiplas entidades
    /// </summary>
    /// <param name="entidades">Entidades a serem adicionadas</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task AdicionarVariasAsync(IEnumerable<T> entidades, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Atualiza uma entidade existente
    /// </summary>
    /// <param name="entidade">Entidade a ser atualizada</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task AtualizarAsync(T entidade, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Remove uma entidade por ID
    /// </summary>
    /// <param name="id">ID da entidade a ser removida</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task RemoverAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Remove uma entidade
    /// </summary>
    /// <param name="entidade">Entidade a ser removida</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task RemoverAsync(T entidade, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Remove múltiplas entidades
    /// </summary>
    /// <param name="entidades">Entidades a serem removidas</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task RemoverVariasAsync(IEnumerable<T> entidades, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se existe uma entidade com o ID especificado
    /// </summary>
    /// <param name="id">ID da entidade</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se existe</returns>
    Task<bool> ExisteAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se existe uma entidade que atende a uma condição
    /// </summary>
    /// <param name="predicate">Condição de filtro</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se existe</returns>
    Task<bool> ExisteAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Conta o número de entidades
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Número total de entidades</returns>
    Task<int> ContarAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Conta o número de entidades que atendem a uma condição
    /// </summary>
    /// <param name="predicate">Condição de filtro</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Número de entidades que atendem à condição</returns>
    Task<int> ContarAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
}