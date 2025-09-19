using Agriis.Compartilhado.Dominio.Interfaces;
using Agriis.Produtores.Dominio.Entidades;

namespace Agriis.Produtores.Dominio.Interfaces;

/// <summary>
/// Interface do repositório de relacionamentos usuário-produtor
/// </summary>
public interface IUsuarioProdutorRepository : IRepository<UsuarioProdutor>
{
    /// <summary>
    /// Obtém relacionamentos por usuário
    /// </summary>
    /// <param name="usuarioId">ID do usuário</param>
    /// <param name="apenasAtivos">Se deve retornar apenas relacionamentos ativos</param>
    /// <returns>Lista de relacionamentos</returns>
    Task<IEnumerable<UsuarioProdutor>> ObterPorUsuarioAsync(int usuarioId, bool apenasAtivos = true);
    
    /// <summary>
    /// Obtém relacionamentos por produtor
    /// </summary>
    /// <param name="produtorId">ID do produtor</param>
    /// <param name="apenasAtivos">Se deve retornar apenas relacionamentos ativos</param>
    /// <returns>Lista de relacionamentos</returns>
    Task<IEnumerable<UsuarioProdutor>> ObterPorProdutorAsync(int produtorId, bool apenasAtivos = true);
    
    /// <summary>
    /// Obtém um relacionamento específico
    /// </summary>
    /// <param name="usuarioId">ID do usuário</param>
    /// <param name="produtorId">ID do produtor</param>
    /// <returns>Relacionamento encontrado ou null</returns>
    Task<UsuarioProdutor?> ObterPorUsuarioEProdutorAsync(int usuarioId, int produtorId);
    
    /// <summary>
    /// Obtém o proprietário principal de um produtor
    /// </summary>
    /// <param name="produtorId">ID do produtor</param>
    /// <returns>Relacionamento do proprietário ou null</returns>
    Task<UsuarioProdutor?> ObterProprietarioPrincipalAsync(int produtorId);
    
    /// <summary>
    /// Verifica se um usuário tem acesso a um produtor
    /// </summary>
    /// <param name="usuarioId">ID do usuário</param>
    /// <param name="produtorId">ID do produtor</param>
    /// <returns>True se tem acesso</returns>
    Task<bool> UsuarioTemAcessoAoProdutorAsync(int usuarioId, int produtorId);
    
    /// <summary>
    /// Obtém todos os produtores que um usuário tem acesso
    /// </summary>
    /// <param name="usuarioId">ID do usuário</param>
    /// <returns>Lista de produtores</returns>
    Task<IEnumerable<Produtor>> ObterProdutoresDoUsuarioAsync(int usuarioId);
}