using Agriis.Compartilhado.Dominio.Interfaces;
using Agriis.Compartilhado.Dominio.Enums;
using Agriis.Usuarios.Dominio.Entidades;

namespace Agriis.Usuarios.Dominio.Interfaces;

/// <summary>
/// Interface do repositório de usuários
/// </summary>
public interface IUsuarioRepository : IRepository<Usuario>
{
    /// <summary>
    /// Obtém um usuário por email
    /// </summary>
    /// <param name="email">Email do usuário</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Usuário encontrado ou null</returns>
    Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém um usuário por CPF
    /// </summary>
    /// <param name="cpf">CPF do usuário</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Usuário encontrado ou null</returns>
    Task<Usuario?> ObterPorCpfAsync(string cpf, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém usuários por role
    /// </summary>
    /// <param name="role">Role a ser filtrada</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de usuários com a role especificada</returns>
    Task<IEnumerable<Usuario>> ObterPorRoleAsync(Roles role, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém usuários ativos
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de usuários ativos</returns>
    Task<IEnumerable<Usuario>> ObterAtivosAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se existe um usuário com o email especificado
    /// </summary>
    /// <param name="email">Email a ser verificado</param>
    /// <param name="usuarioIdExcluir">ID do usuário a ser excluído da verificação (para updates)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se existe</returns>
    Task<bool> ExisteEmailAsync(string email, int? usuarioIdExcluir = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se existe um usuário com o CPF especificado
    /// </summary>
    /// <param name="cpf">CPF a ser verificado</param>
    /// <param name="usuarioIdExcluir">ID do usuário a ser excluído da verificação (para updates)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se existe</returns>
    Task<bool> ExisteCpfAsync(string cpf, int? usuarioIdExcluir = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém usuários com paginação
    /// </summary>
    /// <param name="pagina">Número da página (base 1)</param>
    /// <param name="tamanhoPagina">Tamanho da página</param>
    /// <param name="filtro">Filtro de busca (nome ou email)</param>
    /// <param name="apenasAtivos">Se deve filtrar apenas usuários ativos</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado paginado de usuários</returns>
    Task<(IEnumerable<Usuario> Usuarios, int Total)> ObterPaginadoAsync(
        int pagina, 
        int tamanhoPagina, 
        string? filtro = null, 
        bool apenasAtivos = true, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém usuário com suas roles carregadas
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Usuário com roles ou null</returns>
    Task<Usuario?> ObterComRolesAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém usuário por email com suas roles carregadas
    /// </summary>
    /// <param name="email">Email do usuário</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Usuário com roles ou null</returns>
    Task<Usuario?> ObterPorEmailComRolesAsync(string email, CancellationToken cancellationToken = default);
}