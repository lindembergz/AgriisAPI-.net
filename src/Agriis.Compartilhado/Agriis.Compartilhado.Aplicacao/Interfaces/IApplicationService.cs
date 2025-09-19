namespace Agriis.Compartilhado.Aplicacao.Interfaces;

/// <summary>
/// Interface base para serviços de aplicação
/// Define o contrato básico para serviços que coordenam operações de domínio
/// </summary>
public interface IApplicationService
{
    // Marker interface - serviços específicos herdarão desta interface
    // e definirão seus próprios métodos conforme necessário
}

/// <summary>
/// Interface genérica para serviços de aplicação com operações CRUD básicas
/// </summary>
/// <typeparam name="TDto">Tipo do DTO</typeparam>
/// <typeparam name="TKey">Tipo da chave primária</typeparam>
public interface IApplicationService<TDto, TKey> : IApplicationService
    where TDto : class
    where TKey : struct
{
    /// <summary>
    /// Obtém um item por ID
    /// </summary>
    /// <param name="id">ID do item</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>DTO do item ou null se não encontrado</returns>
    Task<TDto?> ObterPorIdAsync(TKey id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém todos os itens
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de DTOs</returns>
    Task<IEnumerable<TDto>> ObterTodosAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cria um novo item
    /// </summary>
    /// <param name="dto">DTO com os dados do item</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>DTO do item criado</returns>
    Task<TDto> CriarAsync(TDto dto, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Atualiza um item existente
    /// </summary>
    /// <param name="id">ID do item</param>
    /// <param name="dto">DTO com os novos dados</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>DTO do item atualizado</returns>
    Task<TDto> AtualizarAsync(TKey id, TDto dto, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Remove um item
    /// </summary>
    /// <param name="id">ID do item</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task RemoverAsync(TKey id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se um item existe
    /// </summary>
    /// <param name="id">ID do item</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se existe</returns>
    Task<bool> ExisteAsync(TKey id, CancellationToken cancellationToken = default);
}