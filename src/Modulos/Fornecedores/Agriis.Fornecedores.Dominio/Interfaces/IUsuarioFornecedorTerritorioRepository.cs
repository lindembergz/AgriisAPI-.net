using Agriis.Compartilhado.Dominio.Interfaces;
using Agriis.Fornecedores.Dominio.Entidades;

namespace Agriis.Fornecedores.Dominio.Interfaces;

/// <summary>
/// Interface para repositório de territórios de usuários fornecedores
/// </summary>
public interface IUsuarioFornecedorTerritorioRepository : IRepository<UsuarioFornecedorTerritorio>
{
    /// <summary>
    /// Obtém territórios por associação usuário-fornecedor
    /// </summary>
    /// <param name="usuarioFornecedorId">ID da associação usuário-fornecedor</param>
    /// <param name="apenasAtivos">Se deve retornar apenas ativos</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de territórios</returns>
    Task<IEnumerable<UsuarioFornecedorTerritorio>> ObterPorUsuarioFornecedorAsync(int usuarioFornecedorId, bool apenasAtivos = true, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém territórios por estado
    /// </summary>
    /// <param name="uf">UF do estado</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de territórios que incluem o estado</returns>
    Task<IEnumerable<UsuarioFornecedorTerritorio>> ObterPorEstadoAsync(string uf, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém territórios por município
    /// </summary>
    /// <param name="uf">UF do estado</param>
    /// <param name="municipio">Nome do município</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de territórios que incluem o município</returns>
    Task<IEnumerable<UsuarioFornecedorTerritorio>> ObterPorMunicipioAsync(string uf, string municipio, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém território padrão de uma associação usuário-fornecedor
    /// </summary>
    /// <param name="usuarioFornecedorId">ID da associação usuário-fornecedor</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Território padrão ou null</returns>
    Task<UsuarioFornecedorTerritorio?> ObterTerritorioPadraoAsync(int usuarioFornecedorId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém usuários fornecedores que atendem um território específico
    /// </summary>
    /// <param name="uf">UF do estado</param>
    /// <param name="municipio">Nome do município (opcional)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de usuários fornecedores que atendem o território</returns>
    Task<IEnumerable<UsuarioFornecedor>> ObterUsuariosFornecedoresPorTerritorioAsync(string uf, string? municipio = null, CancellationToken cancellationToken = default);
}