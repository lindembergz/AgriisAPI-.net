using Agriis.Produtores.Dominio.Entidades;

namespace Agriis.Produtores.Dominio.Interfaces;

/// <summary>
/// Interface para repositório de relacionamento usuário-produtor
/// </summary>
public interface IUsuarioProdutorRepository
{
    /// <summary>
    /// Adiciona um novo relacionamento usuário-produtor
    /// </summary>
    /// <param name="usuarioProdutor">Relacionamento a ser adicionado</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Relacionamento adicionado</returns>
    Task<UsuarioProdutor> AdicionarAsync(UsuarioProdutor usuarioProdutor, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém um relacionamento por ID
    /// </summary>
    /// <param name="id">ID do relacionamento</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Relacionamento encontrado ou null</returns>
    Task<UsuarioProdutor?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém relacionamentos por usuário
    /// </summary>
    /// <param name="usuarioId">ID do usuário</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de relacionamentos</returns>
    Task<IEnumerable<UsuarioProdutor>> ObterPorUsuarioAsync(int usuarioId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém relacionamentos por produtor
    /// </summary>
    /// <param name="produtorId">ID do produtor</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de relacionamentos</returns>
    Task<IEnumerable<UsuarioProdutor>> ObterPorProdutorAsync(int produtorId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém relacionamento específico entre usuário e produtor
    /// </summary>
    /// <param name="usuarioId">ID do usuário</param>
    /// <param name="produtorId">ID do produtor</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Relacionamento encontrado ou null</returns>
    Task<UsuarioProdutor?> ObterPorUsuarioEProdutorAsync(int usuarioId, int produtorId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Atualiza um relacionamento
    /// </summary>
    /// <param name="usuarioProdutor">Relacionamento a ser atualizado</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Task</returns>
    Task AtualizarAsync(UsuarioProdutor usuarioProdutor, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Remove um relacionamento
    /// </summary>
    /// <param name="id">ID do relacionamento</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Task</returns>
    Task RemoverAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se existe relacionamento entre usuário e produtor
    /// </summary>
    /// <param name="usuarioId">ID do usuário</param>
    /// <param name="produtorId">ID do produtor</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se existe</returns>
    Task<bool> ExisteRelacionamentoAsync(int usuarioId, int produtorId, CancellationToken cancellationToken = default);
}