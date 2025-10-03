using Agriis.Compartilhado.Dominio.Interfaces;
using Agriis.Compartilhado.Aplicacao.Resultados;
using Agriis.Fornecedores.Dominio.Entidades;

namespace Agriis.Fornecedores.Dominio.Interfaces;

/// <summary>
/// Interface para repositório de fornecedores
/// </summary>
public interface IFornecedorRepository : IRepository<Fornecedor>
{
    /// <summary>
    /// Obtém um fornecedor por CNPJ
    /// </summary>
    /// <param name="cnpj">CNPJ do fornecedor</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Fornecedor encontrado ou null</returns>
    /// 

    Task<PagedResult<Fornecedor>> ObterPaginadoAsync(
        int pagina,
        int tamanhoPagina,
        string? filtro = null);
    Task<Fornecedor?> ObterPorCnpjAsync(string cnpj, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém fornecedores ativos
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de fornecedores ativos</returns>
    Task<IEnumerable<Fornecedor>> ObterAtivosAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém fornecedores por território (estado/município)
    /// </summary>
    /// <param name="uf">UF do estado</param>
    /// <param name="municipio">Nome do município (opcional)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de fornecedores que atendem o território</returns>
    Task<IEnumerable<Fornecedor>> ObterPorTerritorioAsync(string uf, string? municipio = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém fornecedores com filtros avançados
    /// </summary>
    /// <param name="nome">Filtro por nome (opcional)</param>
    /// <param name="cnpj">Filtro por CNPJ (opcional)</param>
    /// <param name="ativo">Filtro por status ativo (opcional)</param>
    /// <param name="moedaPadrao">Filtro por moeda padrão (opcional)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de fornecedores filtrados</returns>
    Task<IEnumerable<Fornecedor>> ObterComFiltrosAsync(
        string? nome = null,
        string? cnpj = null,
        bool? ativo = null,
        int? moedaPadrao = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se existe um fornecedor com o CNPJ especificado
    /// </summary>
    /// <param name="cnpj">CNPJ a verificar</param>
    /// <param name="fornecedorIdExcluir">ID do fornecedor a excluir da verificação (para updates)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se existe</returns>
    Task<bool> ExisteCnpjAsync(string cnpj, int? fornecedorIdExcluir = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém dados geográficos para enriquecer os fornecedores
    /// </summary>
    /// <param name="ufIds">IDs das UFs</param>
    /// <param name="municipioIds">IDs dos municípios</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dicionário com dados geográficos</returns>
    Task<Dictionary<string, object>> ObterDadosGeograficosAsync(IEnumerable<int> ufIds, IEnumerable<int> municipioIds, CancellationToken cancellationToken = default);
}