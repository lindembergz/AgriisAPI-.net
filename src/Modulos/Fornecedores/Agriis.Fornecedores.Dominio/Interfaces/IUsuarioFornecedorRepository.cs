using Agriis.Compartilhado.Dominio.Interfaces;
using Agriis.Compartilhado.Dominio.Enums;
using Agriis.Fornecedores.Dominio.Entidades;

namespace Agriis.Fornecedores.Dominio.Interfaces;

/// <summary>
/// Interface para repositório de usuários fornecedores
/// </summary>
public interface IUsuarioFornecedorRepository : IRepository<UsuarioFornecedor>
{
    /// <summary>
    /// Obtém associações por usuário
    /// </summary>
    /// <param name="usuarioId">ID do usuário</param>
    /// <param name="apenasAtivos">Se deve retornar apenas ativos</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de associações do usuário</returns>
    Task<IEnumerable<UsuarioFornecedor>> ObterPorUsuarioAsync(int usuarioId, bool apenasAtivos = true, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém associações por fornecedor
    /// </summary>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <param name="apenasAtivos">Se deve retornar apenas ativos</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de associações do fornecedor</returns>
    Task<IEnumerable<UsuarioFornecedor>> ObterPorFornecedorAsync(int fornecedorId, bool apenasAtivos = true, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém uma associação específica usuário-fornecedor
    /// </summary>
    /// <param name="usuarioId">ID do usuário</param>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Associação encontrada ou null</returns>
    Task<UsuarioFornecedor?> ObterPorUsuarioFornecedorAsync(int usuarioId, int fornecedorId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém usuários por role no fornecedor
    /// </summary>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <param name="role">Role desejado</param>
    /// <param name="apenasAtivos">Se deve retornar apenas ativos</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de usuários com o role especificado</returns>
    Task<IEnumerable<UsuarioFornecedor>> ObterPorRoleAsync(int fornecedorId, Roles role, bool apenasAtivos = true, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém usuários com territórios incluindo dados de território
    /// </summary>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de usuários com territórios</returns>
    Task<IEnumerable<UsuarioFornecedor>> ObterComTerritoriosAsync(int fornecedorId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se existe uma associação ativa entre usuário e fornecedor
    /// </summary>
    /// <param name="usuarioId">ID do usuário</param>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se existe associação ativa</returns>
    Task<bool> ExisteAssociacaoAtivaAsync(int usuarioId, int fornecedorId, CancellationToken cancellationToken = default);
}