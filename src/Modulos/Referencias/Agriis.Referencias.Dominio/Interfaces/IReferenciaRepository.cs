using Agriis.Compartilhado.Dominio.Entidades;
using Agriis.Compartilhado.Dominio.Interfaces;

namespace Agriis.Referencias.Dominio.Interfaces;

/// <summary>
/// Interface base para repositórios de entidades de referência
/// </summary>
/// <typeparam name="T">Tipo da entidade de referência</typeparam>
public interface IReferenciaRepository<T> : IRepository<T> where T : EntidadeBase
{
    /// <summary>
    /// Obtém apenas as entidades ativas
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de entidades ativas</returns>
    Task<IEnumerable<T>> ObterAtivosAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se existe uma entidade com o código especificado
    /// </summary>
    /// <param name="codigo">Código a ser verificado</param>
    /// <param name="idExcluir">ID a ser excluído da verificação (para updates)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se existe</returns>
    Task<bool> ExisteCodigoAsync(string codigo, int? idExcluir = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se existe uma entidade com o nome especificado
    /// </summary>
    /// <param name="nome">Nome a ser verificado</param>
    /// <param name="idExcluir">ID a ser excluído da verificação (para updates)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se existe</returns>
    Task<bool> ExisteNomeAsync(string nome, int? idExcluir = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se uma entidade pode ser removida (não possui dependências)
    /// </summary>
    /// <param name="id">ID da entidade</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se pode ser removida</returns>
    Task<bool> PodeRemoverAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Ativa uma entidade
    /// </summary>
    /// <param name="id">ID da entidade</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task AtivarAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Desativa uma entidade
    /// </summary>
    /// <param name="id">ID da entidade</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task DesativarAsync(int id, CancellationToken cancellationToken = default);
}