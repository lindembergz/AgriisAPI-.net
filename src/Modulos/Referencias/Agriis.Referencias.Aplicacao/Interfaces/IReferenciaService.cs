namespace Agriis.Referencias.Aplicacao.Interfaces;

/// <summary>
/// Interface base para serviços de entidades de referência
/// </summary>
/// <typeparam name="TDto">DTO de leitura</typeparam>
/// <typeparam name="TCriarDto">DTO de criação</typeparam>
/// <typeparam name="TAtualizarDto">DTO de atualização</typeparam>
public interface IReferenciaService<TDto, TCriarDto, TAtualizarDto>
{
    /// <summary>
    /// Obtém todas as entidades
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de entidades</returns>
    Task<IEnumerable<TDto>> ObterTodosAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém apenas as entidades ativas
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de entidades ativas</returns>
    Task<IEnumerable<TDto>> ObterAtivosAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém uma entidade por ID
    /// </summary>
    /// <param name="id">ID da entidade</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Entidade encontrada ou null</returns>
    Task<TDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cria uma nova entidade
    /// </summary>
    /// <param name="dto">DTO de criação</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Entidade criada</returns>
    Task<TDto> CriarAsync(TCriarDto dto, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Atualiza uma entidade existente
    /// </summary>
    /// <param name="id">ID da entidade</param>
    /// <param name="dto">DTO de atualização</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Entidade atualizada</returns>
    Task<TDto> AtualizarAsync(int id, TAtualizarDto dto, CancellationToken cancellationToken = default);
    
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
    
    /// <summary>
    /// Remove uma entidade
    /// </summary>
    /// <param name="id">ID da entidade</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task RemoverAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se uma entidade pode ser removida
    /// </summary>
    /// <param name="id">ID da entidade</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se pode ser removida</returns>
    Task<bool> PodeRemoverAsync(int id, CancellationToken cancellationToken = default);
}