using Agriis.Compartilhado.Aplicacao.Resultados;
using Agriis.Compartilhado.Dominio.Interfaces;
using Agriis.Produtores.Dominio.Entidades;
using Agriis.Produtores.Dominio.Enums;

namespace Agriis.Produtores.Dominio.Interfaces;

/// <summary>
/// Interface do repositório de produtores
/// </summary>
public interface IProdutorRepository : IRepository<Produtor>
{
    /// <summary>
    /// Obtém um produtor pelo CPF
    /// </summary>
    /// <param name="cpf">CPF do produtor</param>
    /// <returns>Produtor encontrado ou null</returns>
    Task<Produtor?> ObterPorCpfAsync(string cpf);
    
    /// <summary>
    /// Obtém um produtor pelo CNPJ
    /// </summary>
    /// <param name="cnpj">CNPJ do produtor</param>
    /// <returns>Produtor encontrado ou null</returns>
    Task<Produtor?> ObterPorCnpjAsync(string cnpj);
    
    /// <summary>
    /// Obtém produtores por fornecedor (baseado na área de atuação territorial)
    /// </summary>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <returns>Lista de produtores</returns>
    Task<IEnumerable<Produtor>> ObterPorFornecedorAsync(int fornecedorId);
    
    /// <summary>
    /// Obtém produtores paginados com filtros
    /// </summary>
    /// <param name="pagina">Número da página</param>
    /// <param name="tamanhoPagina">Tamanho da página</param>
    /// <param name="filtro">Filtro de busca (nome, CPF, CNPJ)</param>
    /// <param name="status">Status do produtor (opcional)</param>
    /// <param name="culturaId">ID da cultura (opcional)</param>
    /// <returns>Resultado paginado</returns>
    Task<PagedResult<Produtor>> ObterPaginadoAsync(
        int pagina, 
        int tamanhoPagina, 
        string? filtro = null,
        StatusProdutor? status = null,
        int? culturaId = null);
    
    /// <summary>
    /// Obtém produtores por status
    /// </summary>
    /// <param name="status">Status do produtor</param>
    /// <returns>Lista de produtores</returns>
    Task<IEnumerable<Produtor>> ObterPorStatusAsync(StatusProdutor status);
    
    /// <summary>
    /// Obtém produtores por cultura
    /// </summary>
    /// <param name="culturaId">ID da cultura</param>
    /// <returns>Lista de produtores</returns>
    Task<IEnumerable<Produtor>> ObterPorCulturaAsync(int culturaId);
    
    /// <summary>
    /// Obtém produtores por área de plantio (faixa)
    /// </summary>
    /// <param name="areaMinima">Área mínima em hectares</param>
    /// <param name="areaMaxima">Área máxima em hectares</param>
    /// <returns>Lista de produtores</returns>
    Task<IEnumerable<Produtor>> ObterPorFaixaAreaAsync(decimal areaMinima, decimal areaMaxima);
    
    /// <summary>
    /// Verifica se existe um produtor com o CPF informado
    /// </summary>
    /// <param name="cpf">CPF a verificar</param>
    /// <param name="produtorIdExcluir">ID do produtor a excluir da verificação (para updates)</param>
    /// <returns>True se existe</returns>
    Task<bool> ExistePorCpfAsync(string cpf, int? produtorIdExcluir = null);
    
    /// <summary>
    /// Verifica se existe um produtor com o CNPJ informado
    /// </summary>
    /// <param name="cnpj">CNPJ a verificar</param>
    /// <param name="produtorIdExcluir">ID do produtor a excluir da verificação (para updates)</param>
    /// <returns>True se existe</returns>
    Task<bool> ExistePorCnpjAsync(string cnpj, int? produtorIdExcluir = null);
    
    /// <summary>
    /// Obtém estatísticas dos produtores
    /// </summary>
    /// <returns>Estatísticas</returns>
    Task<ProdutorEstatisticas> ObterEstatisticasAsync();
}

/// <summary>
/// Estatísticas dos produtores
/// </summary>
public class ProdutorEstatisticas
{
    public int TotalProdutores { get; set; }
    public int ProdutoresAutorizados { get; set; }
    public int ProdutoresPendentes { get; set; }
    public int ProdutoresNegados { get; set; }
    public decimal AreaTotalPlantio { get; set; }
    public decimal AreaMediaPlantio { get; set; }
}