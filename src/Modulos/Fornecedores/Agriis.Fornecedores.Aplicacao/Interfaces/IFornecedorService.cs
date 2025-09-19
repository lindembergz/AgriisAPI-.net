using Agriis.Fornecedores.Aplicacao.DTOs;

namespace Agriis.Fornecedores.Aplicacao.Interfaces;

/// <summary>
/// Interface para serviços de aplicação de fornecedores
/// </summary>
public interface IFornecedorService
{
    /// <summary>
    /// Obtém todos os fornecedores
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de fornecedores</returns>
    Task<IEnumerable<FornecedorDto>> ObterTodosAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém um fornecedor por ID
    /// </summary>
    /// <param name="id">ID do fornecedor</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Fornecedor encontrado ou null</returns>
    Task<FornecedorDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém um fornecedor por CNPJ
    /// </summary>
    /// <param name="cnpj">CNPJ do fornecedor</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Fornecedor encontrado ou null</returns>
    Task<FornecedorDto?> ObterPorCnpjAsync(string cnpj, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém fornecedores ativos
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de fornecedores ativos</returns>
    Task<IEnumerable<FornecedorDto>> ObterAtivosAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém fornecedores por território
    /// </summary>
    /// <param name="uf">UF do estado</param>
    /// <param name="municipio">Nome do município (opcional)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de fornecedores que atendem o território</returns>
    Task<IEnumerable<FornecedorDto>> ObterPorTerritorioAsync(string uf, string? municipio = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém fornecedores com filtros avançados
    /// </summary>
    /// <param name="nome">Filtro por nome (opcional)</param>
    /// <param name="cnpj">Filtro por CNPJ (opcional)</param>
    /// <param name="ativo">Filtro por status ativo (opcional)</param>
    /// <param name="moedaPadrao">Filtro por moeda padrão (opcional)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de fornecedores filtrados</returns>
    Task<IEnumerable<FornecedorDto>> ObterComFiltrosAsync(
        string? nome = null,
        string? cnpj = null,
        bool? ativo = null,
        int? moedaPadrao = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cria um novo fornecedor
    /// </summary>
    /// <param name="request">Dados do fornecedor</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Fornecedor criado</returns>
    Task<FornecedorDto> CriarAsync(CriarFornecedorRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Atualiza um fornecedor existente
    /// </summary>
    /// <param name="id">ID do fornecedor</param>
    /// <param name="request">Novos dados do fornecedor</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Fornecedor atualizado</returns>
    Task<FornecedorDto> AtualizarAsync(int id, AtualizarFornecedorRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Ativa um fornecedor
    /// </summary>
    /// <param name="id">ID do fornecedor</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task AtivarAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Desativa um fornecedor
    /// </summary>
    /// <param name="id">ID do fornecedor</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task DesativarAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Define a logo do fornecedor
    /// </summary>
    /// <param name="id">ID do fornecedor</param>
    /// <param name="logoUrl">URL da logo</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    Task DefinirLogoAsync(int id, string? logoUrl, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica se um CNPJ está disponível
    /// </summary>
    /// <param name="cnpj">CNPJ a verificar</param>
    /// <param name="fornecedorIdExcluir">ID do fornecedor a excluir da verificação</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se disponível</returns>
    Task<bool> VerificarCnpjDisponivelAsync(string cnpj, int? fornecedorIdExcluir = null, CancellationToken cancellationToken = default);
}